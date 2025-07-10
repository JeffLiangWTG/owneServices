using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.Accounting.ElectronicMessaging.Hungary
{
	/// <summary>
	/// Class to capture all data required for Hungary ManageInvoiceRequest XML (API Code 'GEN').
	/// </summary>
	public class ManageInvoiceRequestModel
	{
		public ManageInvoiceRequestModel(TransactionInfo transactionInfo, HungaryTransactionExtraInfo transactionExtraInfo, string batchNumber, INotifications notifications)
		{
			Transaction = Argument.NotNull(transactionInfo, nameof(transactionInfo));
			TransactionExtraInfo = Argument.NotNull(transactionExtraInfo, nameof(transactionExtraInfo));
			BatchNumber = Argument.NotNullOrEmpty(batchNumber, nameof(batchNumber));
			Notifications = Argument.NotNull(notifications, nameof(notifications));
			UtcTimestamp = ZDateTime.UtcNow;
		}

		#region Properties

		public INotifications Notifications { get; }

		public TransactionInfo Transaction { get; }

		public HungaryTransactionExtraInfo TransactionExtraInfo { get; }

		public ZDateTime? DeliveryDate => isDeliveryDateCalculated ? deliveryDate : (deliveryDate = GetDeliveryDate());
		ZDateTime? deliveryDate;
		bool isDeliveryDateCalculated;

		ZDateTime? GetDeliveryDate()
		{
			ZDateTime? maxTaxDate = null;
			var linesWithTaxDates = Transaction?.PostingJournalCollection?.Where(pj => pj.TaxDate.HasValue && pj.TaxDate.Value.IsValid);
			if (linesWithTaxDates?.Any() ?? false)
			{
				maxTaxDate = linesWithTaxDates.Max(jl => jl.TaxDate);
			}

			ZDateTime? result = null;
			if (maxTaxDate != null && (!Transaction.TransactionDate.HasValue || Transaction.TransactionDate.Value > maxTaxDate))
			{
				result = maxTaxDate;
			}
			else
			{
				result = Transaction?.TransactionDate;
			}
			isDeliveryDateCalculated = true;
			return result;
		}

		public ZString? CustomerCountry => Transaction?.OrganizationAddress?.Country?.Code;
		public bool IsCustomerInEUCountry { get; private set; }

		public bool IsCustomerPrivatePerson => TransactionExtraInfo.IsDebtorPrivatePerson;

		public InvoiceDeliveryMethod? InvoiceDeliveryMethod => TransactionExtraInfo.InvoiceDeliveryMethod;

		public ZString? SupplierCountry => Transaction?.BranchAddress?.Country?.Code;
		public bool IsSupplierInEUCountry { get; private set; }

		/// <summary>
		/// Collection of previous transactions, in order of submission to government. Does not include the current transaction.
		/// </summary>
		public IReadOnlyCollection<PreviousInvoice> PreviousInvoices { get; private set; }

		public ZDateTime UtcTimestamp { get; }

		public string BatchNumber { get; }
		public string BranchCode { get; private set; }
		public Guid CompanyPK { get; private set; }
		public string CompanyCode { get; private set; }
		public bool CompanyIsReciprocal { get; private set; }

		public string LoginId { get; private set; }
		public string PasswordHash { get; private set; }
		public string SignatureKey { get; private set; }
		public string ReplacementKey { get; private set; }

		public string SoftwareVersion { get; private set; }

		public string RequestVersion => "3.0";

		public string TaxPayerRegistrationNumber => GetSupplierRegistrationNumber(OrgCusCode.CodeTypes.VATCode);

		public string InvoiceOperation
			=> Transaction?.TransactionType == TransactionType.INV && !PreviousInvoices.Any() ? "CREATE"
			:  Transaction?.TransactionType == TransactionType.INV && PreviousInvoices.Any()  ? "MODIFY"
			:  Transaction?.TransactionType == TransactionType.CRD                            ? "MODIFY"
			:  string.Empty;

		#endregion

		#region LoadData()

		/// <summary>
		/// Loads all required data from CW1 database and maps any fields required in XML.
		/// Errors may be logged in Notifications property.
		/// </summary>
		public void LoadData(BusinessObjectFactory bizoFactory, bool prettyPrintInvoiceDataXml = false)
		{
			var factory = bizoFactory ?? new BusinessObjectFactory();

			var branchCode = Transaction.Branch?.Code ?? ZString.Empty;
			if (branchCode.IsEmpty)
			{
				Notifications.AddError(Res.GetString("7f9e9b52-a934-4987-9325-13d712c0db28", "Unable to determine Branch from Universal Transaction Batch. This transaction will not be processed."));
				return;
			}

			var branch = factory.LoadFromUniqueKey<GlbBranch>(GlbBranchSchema.GB_Code, branchCode);
			if (branch == null)
			{
				Notifications.AddError(Res.GetString("bd612171-96ab-49ef-8442-491953643185", "Unable to load Branch based on Universal Transaction Batch. This transaction will not be processed."));
				return;
			}

			if (branch.Company == null)
			{
				Notifications.AddError(Res.GetString("b3741657-560a-41c4-b5fc-6918aaa282dd", "Unable to load Company based on Universal Transaction Batch. This transaction will not be processed."));
				return;
			}

			var credentials = GlbCompanyExternalPasswordHUI.LoadForCompany(factory, branch.Company);
			if (credentials != null)
			{
				LoginId = credentials.Login;
				PasswordHash = credentials.PasswordHash;
				SignatureKey = credentials.SignatureKey;
				ReplacementKey = credentials.ReplacementKey;
			}

			BranchCode = branchCode;
			CompanyCode = branch.Company.GC_Code;
			CompanyPK = branch.Company.PK.ToGuid();
			CompanyIsReciprocal = branch.Company.GC_IsReciprocal;

			SoftwareVersion = new EnterpriseInformationRetriever().VersionNumber;

			PreviousInvoices = GetPreviousInvoices(factory, branch);

			IsCustomerInEUCountry = IsEUCountry(CustomerCountry);
			IsSupplierInEUCountry = IsEUCountry(SupplierCountry);
		}

		public InvoiceDataModel GetInvoiceDataModel() => new InvoiceDataModel(this);

#if DEBUG
		/// <summary>
		/// Only to be used when testing the XML Builder to avoid calling Load().
		/// DO NOT USE THIS WHEN TESTING THIS CLASS
		/// </summary>
		public void SetData_ForTestOnly(
			string branchCode = null,
			string companyCode = null,
			Guid? companyPK = null,
			bool? companyIsReciprocal = null,
			string loginId = null,
			string passwordHash = null,
			string signatureKey = null,
			string replacementKey = null,
			string softwareVersion = null,
			IReadOnlyCollection<PreviousInvoice> previousInvoices = null)
		{
			BranchCode = branchCode;
			CompanyCode = companyCode;
			CompanyPK = companyPK ?? Guid.Empty;
			CompanyIsReciprocal = companyIsReciprocal ?? false;

			LoginId = loginId;
			PasswordHash = passwordHash;
			SignatureKey = signatureKey;
			ReplacementKey = replacementKey;

			SoftwareVersion = softwareVersion;

			PreviousInvoices = previousInvoices ?? Array.Empty<PreviousInvoice>();
		}
#endif

		#endregion

		public ZString? GetDebtorRegistrationNumber(string code)
		{
			var regNumber = Transaction?.OrganizationAddress?.RegistrationNumberCollection?
				.FirstOrDefault(x => (x.Type?.Code ?? ZString.Empty) == code
								&& (x.CountryOfIssue?.Code ?? ZString.Empty) == Core.Constants.CountryCodes.Hungary);
			return CleanRegistrationNumber(regNumber?.Value);
		}

		public ZString? GetSupplierRegistrationNumber(string code)
		{
			var regNumber = Transaction?.BranchAddress?.RegistrationNumberCollection?
				.FirstOrDefault(x => (x.Type?.Code ?? ZString.Empty) == code
								&& (x.CountryOfIssue?.Code ?? ZString.Empty) == Core.Constants.CountryCodes.Hungary);
			return CleanRegistrationNumber(regNumber?.Value);
		}

		public ZString? GetNonHungarySupplierTaxID(string countryCode)
		{
			ZString? result = null;
			if (countryCode != CountryCodes.Hungary)
			{
				var regNumber = Transaction?.BranchAddress?.RegistrationNumberCollection?
									.FirstOrDefault(x => (x.Type?.Code ?? ZString.Empty) == Country.GetConsumptionTaxRegistrationOrgCusCode(countryCode)
													&& (x.CountryOfIssue?.Code ?? ZString.Empty) == countryCode);
				result = regNumber?.Value;
			}
			return result;
		}

		public ZString? GetNonHungaryCustomerTaxID(string countryCode)
		{
			ZString? result = null;
			if (countryCode != CountryCodes.Hungary)
			{
				var regNumber = Transaction?.OrganizationAddress?.RegistrationNumberCollection?
					.FirstOrDefault(x =>
						(x.Type?.Code ?? ZString.Empty) == Country.GetConsumptionTaxRegistrationOrgCusCode(countryCode)
						&& (x.CountryOfIssue?.Code ?? ZString.Empty) == countryCode);
				result = regNumber?.Value;
			}
			return result;
		}

		#region Implementation

		ZString? CleanRegistrationNumber(ZString? regNumber)
		{
			if (regNumber.HasValue && !regNumber.Value.IsEmpty)
			{
				var result = regNumber.Value;
				if (result.StartsWith("HU", StringComparison.OrdinalIgnoreCase))
				{
					result = result.Substring(2);
				}
				result = result.SubstringSafe(0, 8);
				return result;
			}
			return null;
		}
		IReadOnlyCollection<PreviousInvoice> GetPreviousInvoices(BusinessObjectFactory factory, GlbBranch branch)
		{
			var previousInvoices = GetPreviousInvoicesThatWereSentToHUGovt(factory, branch);
			var originalInvoiceSubmittedToHUGovt = previousInvoices?.Any() ?? false;
			if ((!originalInvoiceSubmittedToHUGovt || !previousInvoices.First().WasSubmittedSuccessfully) && (Transaction?.OriginalReference?.OriginalTransactionNumber.HasValue ?? false))
			{
				previousInvoices = new[] { BuildPreviousInvoiceFromOriginalTransaction(branch) };
			}
			return previousInvoices;
		}

		IReadOnlyCollection<PreviousInvoice> GetPreviousInvoicesThatWereSentToHUGovt(BusinessObjectFactory factory, GlbBranch branch)
		{
			var query = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, Transaction.Ledger);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, Transaction.TransactionType.ToString());
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, Transaction.Number);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCount, (byte)1);
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, branch.GB_GC);
			var thisTransHeader = factory.LoadTop1<InvoicingBase>(query);

			if (thisTransHeader == null || thisTransHeader.AH_TransactionBelongsToGroup.IsEmpty)
			{
				return Array.Empty<PreviousInvoice>();
			}

			var originalTransHeader = factory.Load<AccTransactionHeader>(thisTransHeader.AH_TransactionBelongsToGroup);
			if (originalTransHeader == null)
			{
				return Array.Empty<PreviousInvoice>();
			}

			var selectQuery = $@"
SELECT AH_TransactionNum, AH_TransactionType, AIP_Status, 2 AS OriginalTransactionSort, AIP_LastResponseReceivedUtc, COUNT(AL_Sequence) AS AL_Sequence, AH_GB
FROM dbo.AccTransactionHeader
LEFT OUTER JOIN dbo.AccEInvoicingTransactionPivot
	ON AIP_ParentID = AH_PK
	AND AIP_ParentTableCode = 'AH'
LEFT OUTER JOIN dbo.AccTransactionLines
	ON AL_AH = AH_PK
WHERE AH_TransactionBelongsToGroup = @OriginalTransactionPK
	AND (AIP_ActionType = '{EInvoicingPivotActionType.Submit}' OR AIP_ActionType IS NULL)
	AND AH_PK <> @ThisTransactionPK
	AND AL_LineAmount <> 0
		
GROUP BY AH_GB, AH_TransactionNum, AH_TransactionType, AIP_Status, AIP_LastResponseReceivedUtc

UNION ALL

SELECT AH_TransactionNum, AH_TransactionType, AIP_Status, 1 AS OriginalTransactionSort, AIP_LastResponseReceivedUtc, COUNT(AL_Sequence) AS AL_Sequence, AH_GB
FROM dbo.AccTransactionHeader
LEFT OUTER JOIN dbo.AccEInvoicingTransactionPivot
	ON AIP_ParentID = AH_PK
	AND AIP_ParentTableCode = 'AH'
LEFT OUTER JOIN dbo.AccTransactionLines
	ON AL_AH = AH_PK
WHERE AH_PK = @OriginalTransactionPK
	AND (AIP_ActionType = '{EInvoicingPivotActionType.Submit}' OR AIP_ActionType IS NULL)
	AND AL_LineAmount <> 0
GROUP BY AH_GB, AH_TransactionNum, AH_TransactionType, AIP_Status, AIP_LastResponseReceivedUtc

ORDER BY OriginalTransactionSort, AIP_LastResponseReceivedUtc
";
			var sqlParameters = new[]
			{
				ZSqlParameter.New("@OriginalTransactionPK", originalTransHeader.PK, AccTransactionHeaderSchema.AH_TransactionBelongsToGroup),
				ZSqlParameter.New("@ThisTransactionPK", thisTransHeader.PK, AccTransactionHeaderSchema.PK),
			};
			var previousInvoicesCollection = new DynamicBusinessObjectCollection(factory);
			previousInvoicesCollection.Load(selectQuery, sqlParameters);
			var previousInvoices = new List<PreviousInvoice>(previousInvoicesCollection.Count);
			foreach (DynamicBusinessObject bizo in previousInvoicesCollection)
			{
				previousInvoices.Add(new PreviousInvoice(bizo));
			}
			var originalTransactionWasEventuallySuccessful = previousInvoices.Any(inv => inv.WasSubmittedSuccessfully
																						&& inv.AH_TransactionNum == originalTransHeader.AH_TransactionNum
																						&& inv.AH_TransactionType == originalTransHeader.AH_TransactionType);
			if (originalTransactionWasEventuallySuccessful)
			{
				previousInvoices[0].WasSubmittedSuccessfully = true;
			}

			return previousInvoices;
		}

		PreviousInvoice BuildPreviousInvoiceFromOriginalTransaction(GlbBranch branch)
		{
			return new PreviousInvoice()
			{
				AH_TransactionNum = Transaction.OriginalReference.OriginalTransactionNumber,
				WasSubmittedSuccessfully = false,
				AH_GB = branch.PK,
				LastResponseReceivedTime = TransactionExtraInfo.OriginalTransactionPostDate ?? ZDateTime.Empty
			};
		}

		bool IsEUCountry(ZString? countryCode)
		{
			var provider = ObjectFactory.Get<IEuropeanUnionCustomsMembersProvider>();
			return provider.IsMemberOfEU(countryCode);
		}
	#endregion
}
}
