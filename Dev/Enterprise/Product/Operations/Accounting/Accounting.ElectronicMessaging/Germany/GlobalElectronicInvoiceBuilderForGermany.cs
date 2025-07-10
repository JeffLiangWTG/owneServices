using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing.FeatureConfiguration;
using Enterprise.Accounting.Business.EInvoicing.Germany;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Germany
{
	public class GlobalElectronicInvoiceBuilderForGermany : IGlobalElectronicInvoiceBuilder
	{
		readonly EInvoicingFeatureSettingsReader settingsReader;

		[EInvoiceMessageSubType]
		public const string WS_REQUEST = EInvoiceAPICommandList.Codes.Request;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is an internal constant")]
		const string GERMANY_ELECTRONIC_INVOICING = "Germany electronic invoicing system";

		GlbBranch geiMessageBranch;
		readonly bool isB2GinXTEnabled;
		readonly bool isB2BEnabled;

		public GlobalElectronicInvoiceBuilderForGermany(string batchNumber, UniversalTransactionBatch universalTransactionBatch, AdditionalTransactionInfoForGermanyEInvoice additionalTransactionInfoForGermany)
		{
			Argument.NotNull(universalTransactionBatch, "universalTransactionBatch");
			Argument.NotNullOrEmpty(batchNumber, "batchNumber");

			BatchNumber = batchNumber;
			AdditionalTransactionInfoForGermany = additionalTransactionInfoForGermany;

			UniversalTransaction = universalTransactionBatch.TransactionCollection.First();

			var featureControlManager = ObjectFactory.Get<IFeatureControlManager>();
			settingsReader = new EInvoicingFeatureSettingsReader(CountryCodes.Germany, featureControlManager);
			isB2GinXTEnabled = settingsReader.HasFeature(GermanyEInvoicingFeatureFlags.B2GinXT);
			isB2BEnabled = settingsReader.HasFeature(GermanyEInvoicingFeatureFlags.B2B);
		}

		TransactionInfo UniversalTransaction { get; }

		string BranchCode => UniversalTransaction.Branch.Code;

		string CompanyCode => GEIMessageBranch.Company.GC_Code;

		GlbBranch GEIMessageBranch => geiMessageBranch ?? (geiMessageBranch = new ReadOnlyBusinessObjectFactory().LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, BranchCode ?? string.Empty));

		string BatchNumber { get; }

		readonly AdditionalTransactionInfoForGermanyEInvoice AdditionalTransactionInfoForGermany;

		public (GlobalElectronicInvoicing EInvoice, INotifications ValidationErrors, INotifications ValidationWarnings) Create()
		{
			var errorNotifications = new Logger();
			var warningNotifications = new Logger();

			var eInvoice = new GlobalElectronicInvoicing()
			{
				Header = new GlobalElectronicInvoicingHeader()
				{
					ElectronicInvoiceBatchRequest = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest()
					{
						BranchCode = BranchCode,
						CompanyCode = CompanyCode,
						IsProductionSystem = GEIMessageHelper.GetIsProductionSystem(GEIMessageBranch.Company),
						IsProductionSystemSpecified = true,
						MessagingSystem = GERMANY_ELECTRONIC_INVOICING,
						BatchNumber = BatchNumber,
					}
				},
				Transaction = UniversalTransaction.ToBase64EncodedXmlFragment(),
				TransactionBatchSpecified = false,
			};

			// The system reaches this line of code only if the transaction is eligible for eInvoicing, either B2B or B2G
			// A B2B transaction is eligible if the B2B feature is enabled.
			// A B2G transaction is eligible independent of the B2GinXT feature enabled or not.
			if (isB2GinXTEnabled || (isB2BEnabled && (AdditionalTransactionInfoForGermany.TransactionCategory == OrgConstants.Category.Business))
)
			{
				AddAdditionalDataItems(eInvoice.Header.ElectronicInvoiceBatchRequest, errorNotifications, UniversalTransaction);
				eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType = EInvoiceAPICommandList.Codes.GenerateInvoiceRequest;
			}
			else
			{
				// While the legacy implementation of B2G transactions on xTrade is active, provide
				// additional data as part of the payload. After migration this will be removed and
				// everything will be passed using AdditionalDataItems like B2B above.
				eInvoice.Payload = GetPayloadXML(errorNotifications, UniversalTransaction);
				eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType = EInvoiceAPICommandList.Codes.Request;
			}

			return (eInvoice, errorNotifications, warningNotifications);
		}

		void AddAdditionalDataItems(GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest batchRequest, INotifications errorNotifications, TransactionInfo transaction)
		{
			batchRequest.AdditionalDataItems ??= [];

			batchRequest.AdditionalDataItems.Add(
				new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem()
				{
					Key = GermanyEInvoicingDataItems.TransactionCategory,
					Value = AdditionalTransactionInfoForGermany.TransactionCategory,
				}
			);

			if (string.IsNullOrEmpty(AdditionalTransactionInfoForGermany.IBANNumber))
			{
				errorNotifications.AddError(Res.GetString(
					"42F74FC8-4C34-49F3-AFA6-B075EA001259",
					"The IBAN number of the invoice sender cannot be determined."));
			}
			else
			{
				batchRequest.AdditionalDataItems.Add(
					new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem()
					{
						Key = GermanyEInvoicingDataItems.SellerIBAN,
						Value = AdditionalTransactionInfoForGermany.IBANNumber,
					}
				);
			}

			var buyersReference = GetLeitwegID(errorNotifications, transaction);
			if (!string.IsNullOrEmpty(buyersReference))
			{
				batchRequest.AdditionalDataItems.Add(
					new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem()
					{
						Key = GermanyEInvoicingDataItems.BuyersReference,
						Value = (ZString)buyersReference,
					}
				);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This word is wrong either in UK or US. Have to trick the system now to be able to get through the process")]
		ZString GetPayloadXML(INotifications errorNotifications, TransactionInfo transaction)
		{
			using (var memoryStream = new MemoryStream())
			{
				var ibanNumber = AdditionalTransactionInfoForGermany.IBANNumber;

				if (string.IsNullOrEmpty(ibanNumber))
				{
					errorNotifications.AddError(Res.GetString(
						"42F74FC8-4C34-49F3-AFA6-B075EA001259",
						"The IBAN number of the invoice sender cannot be determined."));
					return "";
				}

				var buyersReference = GetLeitwegID(errorNotifications, transaction);
				if (!string.IsNullOrEmpty(buyersReference) && !string.IsNullOrEmpty(ibanNumber))
				{
					var doc =
					 new XDocument(
					   new XElement("MissingInXUT",
									new XElement("BuyersReference", buyersReference),
									new XElement("SellersIBANNumber", ibanNumber))
					 );
					return doc.ToString();
				}
			}

			return string.Empty;
		}

		ZString? GetLeitwegID(INotifications errorNotifications, TransactionInfo transaction)
		{
			if (AdditionalTransactionInfoForGermany.TransactionCategory != OrgConstants.Category.Government)
			{
				return string.Empty;
			}

			var regNumbers = transaction.OrganizationAddress?.RegistrationNumberCollection ?? new List<RegistrationNumber>();
			var leitwegIDs = regNumbers.Where(x =>
				x.Type.Code.HasValue && x.Type.Code.Value == GermanyOrgCusCodeInfo.OrgCusCodes.LID &&
				x.CountryOfIssue.Code.HasValue && x.CountryOfIssue.Code.Value == Core.Constants.CountryCodes.Germany)?.ToArray();

			if (!leitwegIDs.Any())
			{
				errorNotifications.AddError(Res.GetString(
					"324705BD-3EB8-426C-9FBD-D4AF37776A7D",
					"There is no {0} specified for the invoice receiver organization {1}. See Organization > Details > Config > Registration Numbers / Codes.",
					GermanyOrgCusCodeInfo.OrgCusCodes.LID,
					transaction.OrganizationAddress?.OrganizationCode));
				return "";
			}

			if (leitwegIDs.Length > 1)
			{
				errorNotifications.AddError(Res.GetString(
					"0BDC7B34-ACE5-47C1-A5E6-5BE1DBA6F6F7",
					"There is more than one {0} specified for the invoice receiver organization {1}. See Organization > Details > Config > Registration Numbers / Codes.",
					GermanyOrgCusCodeInfo.OrgCusCodes.LID,
					transaction.OrganizationAddress?.OrganizationCode));
				return "";
			}

				var leitwegID = leitwegIDs.First();
				var buyersReference = leitwegID.Value;

			return buyersReference;
		}
	}
}
