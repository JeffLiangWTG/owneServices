using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class JobComInvoiceHeader : AutoJobComInvoiceHeader, Integration.Customs.GB.IJobComInvoiceHeader, Customs.Business.ICommonInvoice
	{
		public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new JobComInvoiceLineViewCollection JobComInvoiceLines => (JobComInvoiceLineViewCollection)base.JobComInvoiceLines;

		public new JobComInvoiceLineViewCollection InvoiceLines => JobComInvoiceLines;

		protected override BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
		{
			var dec = JobDeclaration;
			return dec != null ? new JobComInvoiceLineViewCollection(this, dec.InvoiceLines) : null;
		}

		public new JobDeclaration JobDeclaration => base.JobDeclaration as JobDeclaration;

		public new JobComInvoiceHeaderLookups Lookups => (JobComInvoiceHeaderLookups)base.Lookups;

		protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups() => new JobComInvoiceHeaderLookups(this);

		protected override ZString LocalCurrencyCodeCore => JobDeclaration.LocalCurrencyConstantCode;

		protected override BaseJobComInvoiceLineViewCollection CreateNewInvoiceLineCollectionWhenDeclarationIsNull()
		{
			var collection = new InvoiceLineDependentCollection(this);
			collection.Load();
			return new JobComInvoiceLineViewCollection(this, collection);
		}

		public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

		protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection()
		{
			return new SupportingDocumentCollection(this);
		}

		public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

		protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection()
		{
			return new AdditionalInfoCollection(this);
		}

		public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

		protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection()
		{
			return new PreviousDocumentCollection(this);
		}

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[Customs.Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			result[Customs.Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
			result[Customs.Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			return result;
		}

		[ReadOnly(true)]
		public override ZString ZG_HouseSplitReference
		{
			get => base.ZG_HouseSplitReference;
			set
			{
				base.ZG_HouseSplitReference = value;
				if (!value.IsEmpty && JobDeclaration != null)
				{
					JobDeclaration.ZG_HouseSplitReference = ZString.Empty;
				}
			}
		}

		const string CommercialInvoiceCode = "380";

		public override ZString JZ_InvoiceNumber
		{
			get { return base.JZ_InvoiceNumber; }
			set
			{
				bool hasChanged = JZ_InvoiceNumber != value;
				base.JZ_InvoiceNumber = value;
				if (hasChanged)
				{
					PreviousDocument invoiceDocument = null;
					foreach (PreviousDocument addInfo in PreviousDocuments)
					{
						if (addInfo.CSI_SubType == PreviousDocumentClassList.Codes.PreviousDocument && addInfo.CSI_Code == "380") //CommercialInvoice
						{
							invoiceDocument = addInfo;
						}
					}
					if (invoiceDocument == null)
					{
						invoiceDocument = PreviousDocuments.AddNew();
						invoiceDocument.CSI_SubType = PreviousDocumentClassList.Codes.PreviousDocument;
						invoiceDocument.CSI_Code = CommercialInvoiceCode;
					}
					invoiceDocument.CSI_ReferenceNumber = JZ_InvoiceNumber.Left(invoiceDocument.CSI_ReferenceNumberInfo.MaxLength);
				}
			}
		}

		protected override void UpdateWhenAnInvoiceIsLinkedToADeclaration(BaseJobComInvoiceLine invoiceLine, LinkedToDeclarationData linkedToDeclarationData, bool updatePartSyncManagerAndRefresh)
		{
			InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line =>
			{
				line.ResetCustomsUnitDefaultingStrategy();
				line.ExecuteCustomsUnitDefaultingStrategy();
			});
			base.UpdateWhenAnInvoiceIsLinkedToADeclaration(invoiceLine, linkedToDeclarationData, updatePartSyncManagerAndRefresh);
		}

		protected override IJobComInvApportionedChargeCollection<BaseApportionedCharge> CreateNewApportionedChargeCollection()
		{
			return new JobComInvApportionedChargeCollection<InvoiceApportionCharge>(this);
		}

		protected override IJobComInvChargeCollection<BaseInvoiceCharge> CreateNewJobComInvHeaderCharges()
		{
			return new InvoiceChargeCollection<InvoiceCharge>(this);
		}

		public new JobComInvApportionedChargeCollection<InvoiceApportionCharge> GroupCharges => (JobComInvApportionedChargeCollection<InvoiceApportionCharge>)base.GroupCharges;

		public new InvoiceChargeCollection<InvoiceCharge> Charges => (InvoiceChargeCollection<InvoiceCharge>)base.Charges;

		public override ZBool AddingSupportingDocumentAutomaticallyEnabled => Registry.GBCustomsDataRegistry.Instance.N935_AddSupportingDocToInvoiceHeader.Value && IsApplicationCodeAllowedForDefaultingSupportingDocumentCore;

		protected override bool IsApplicationCodeAllowedForDefaultingSupportingDocumentCore => JobDeclaration != null;

		protected override ZString DefaultInvoiceDocument => SupportingDocumentTypes.N935;

		protected override HashSet<ZString> ApplicableForUpdateDefaultSupportingDocumentsCodes { get; } =
			new()
			{
				SupportingDocumentTypes.N935,
			};

		protected override void AddDefaultSupportingDocumentIfNecessary()
		{
			if (IsImport
				&& (JobDeclaration?.Configuration.InvoiceHeaderConfiguration.MultipleSupportingDocumentsForAllInvoiceNumbersSupport(JobDeclaration) ?? false))
			{
				JobDeclaration.Invoices?.Cast<JobComInvoiceHeader>().ForEach(header =>
				{
					MeasuresToTaxAndDocsHelper.FindExistingSupportingDocument(DefaultInvoiceDocument, header, header.JZ_InvoiceNumber, out SupportingDocument sd);
					if (sd == null)
					{
						base.AddDefaultSupportingDocumentIfNecessary();
					}
				});
			}
		}

		[ResourceStringData("95A87140-91B0-4959-B12D-0BD6C36F5437", Caption = "Exchange Rate", MediumCaption = "Exch. Rate", ShortCaption = "Ex. Rate", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("CEBB4948-E946-4600-8AE7-430D0CD7FB9B", Caption = "[4/15] Exchange Rate", MediumCaption = "[4/15] Exch. Rate", ShortCaption = "Exch. Rate")]
		public override ZDecimal JZ_InvoiceCurrExRate
		{
			get => base.JZ_InvoiceCurrExRate;
			set => base.JZ_InvoiceCurrExRate = value;
		}

		[ResourceStringData("BDFCCFCD-C66B-441A-862B-AEE2CAA2B81C", Caption = "[S29] Transport charges MoP", MediumCaption = "Transp. Charges MoP", ShortCaption = "MoP", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("3FF17D31-3EBC-4DA9-8EEB-D8D8DD34A77D", Caption = "[UCC 4/2] Transport charges MoP", MediumCaption = "[4/2] Transp. Charges MoP", ShortCaption = "MoP")]
		public override ZString ZG_TransportChargesMethodOfPayment
		{
			get => base.ZG_TransportChargesMethodOfPayment;
			set => base.ZG_TransportChargesMethodOfPayment = value;
		}

		[ResourceStringData("96C9EABF-E617-4140-96D2-68F5F124C45D", Caption = "[22] Inv. Amount", FullDescription = "The total amount of the invoice and its currency.", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("6D0E2454-E43A-4C4A-A293-FFB268EC6E20", Caption = "[UCC 4/11] Inv. Amount", MediumCaption = "[4/11] Inv. Amount", ShortCaption = "Inv. Amount")]
		public override ZDecimal JZ_InvoiceAmount
		{
			get => base.JZ_InvoiceAmount;
			set => base.JZ_InvoiceAmount = value;
		}

		[ResourceStringData("28B3591D-BBE7-4EA5-B6B0-BCC9E63B5CC6", Caption = "[24] Tran. Nature", FullDescription = "The nature of the transaction.", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("0FD97B48-1E36-4FE4-A2DC-A45643E74CFF", Caption = "[UCC 8/5] Tran. Nature", MediumCaption = "[8/5] Tran. Nature", ShortCaption = "Tran. Nature")]
		public override ZString JZ_ValuationCode
		{
			get => base.JZ_ValuationCode;
			set => base.JZ_ValuationCode = value;
		}

		[ResourceStringData("7D08EDA6-6B8C-45A6-A009-9EC8821EC944", Caption = "Incoterm", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("74748778-9407-4D21-ADEC-02F9C4039562", Caption = "[UCC 4/1] Incoterm")]
		public override ZString JZ_IncoTerm
		{
			get => base.JZ_IncoTerm;
			set => base.JZ_IncoTerm = value;
		}

		[ResourceStringData("DA8F69ED-B4BB-4707-8417-71635CD2DF36", Caption = "Agreed Place")]
		public override ZString JZ_IncoTermPlace
		{
			get => base.JZ_IncoTermPlace;
			set => base.JZ_IncoTermPlace = value;
		}

		[ResourceStringData("6555CF8E-1E4F-490A-8843-FD80CEEA0886", Caption = "Supplier", MultipleKey = JobDeclaration.MultipleKeyChief)]
		[ResourceStringData("43E8DD0C-C3A2-42DA-B652-DA91C68984D6", Caption = "[UCC 3/1] Exporter")]
		public override ZGuid JZ_OH_Supplier
		{
			get => base.JZ_OH_Supplier;
			set => base.JZ_OH_Supplier = value;
		}

		public override ZString JZ_MessageType
		{
			get => base.JZ_MessageType;
			set
			{
				if (!base.JZ_MessageType.Equals(value))
				{
					base.JZ_MessageType = value;
					if (JobDeclaration != null)
					{
						if (!JobDeclaration.IsPersistent)
						{
							JobDeclaration.DefaultValueForFakeDeclaration();
						}
					}
				}
			}
		}

		public override ZString JZ_RX_NKInvoice_Currency
		{
			get
			{
				return base.JZ_RX_NKInvoice_Currency;
			}
			set
			{
				base.JZ_RX_NKInvoice_Currency = value;
				if (!JobMessageTypeList.Codes.Export.Equals(JobDeclaration?.JE_MessageType)
					&& !JZ_RX_NKInvoice_Currency.IsEmpty
					&& IsApplicationCodeAllowedForDefaultingSupportingDocumentCore
					&& JZ_RX_NKInvoice_Currency != Core.Constants.CurrencyCodes.UnitedKingdom)
				{
					var add9WKS = JobDeclaration?.Invoices.Select(c => c.JZ_RX_NKInvoice_Currency).Where(x => !x.IsEmpty).Distinct().Count() > 1;
					if (add9WKS)
					{
						JobDeclaration?.Invoices.Cast<JobComInvoiceHeader>().ForEach(header =>
						{
							MeasuresToTaxAndDocsHelper.FindExistingSupportingDocument("9WKS", header, header.JZ_InvoiceNumber, out SupportingDocument sd);
							if (sd == null)
							{
								sd = header.SupportingDocuments.AddNew();
								sd.CSI_Code = "9WKS";
							}
							sd.CSI_ReferenceNumber = header.JZ_InvoiceNumber;
							sd.CSI_DateOfIssue = header.JZ_InvoiceDate;
							sd.CSI_RN_NKCountryCode = header.Supplier_Effective?.MainAddress?.OA_RN_NKCountryCode ?? ZString.Empty;
							sd.CSI_Description = string.Format("SEE ATTACHED WORKSHEET {0}", header.JobDeclaration.JE_DeclarationReference);
						});
					}
				}
			}
		}

		CodeDescriptionPairList Customs.Business.ICommonInvoice.ChargeTypeList
		{
			get { return GetCustomsChargeTypeList(ChargeParentTypes.GroupInvoice); }
		}

		protected override string GetStandaloneIncoTermAndChargeFactoryCountryContext()
		{
			if (JobDeclaration != null)
			{
				return JobDeclaration.JE_ApplicationCode == Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services ?
					JobDeclaration.GetCountryCodeForSupplementaryCodeProvider() :
					JobDeclaration.CountryCode;
			}
			else
			{
				return base.GetStandaloneIncoTermAndChargeFactoryCountryContext();
			}
		}

		protected override ZDateTime EffectiveValuationDateCore
		{
			get
			{
				var originalEntryInstruction = InvoiceLines.Cast<JobComInvoiceLine>().Select(ji => ji.EntryInstruction)
					.Distinct()
					.Where(cei => cei != null && cei.CEI_DateForDuty.IsValid)
					.OrderBy(cei => cei.CEI_SubStyle)
					.ThenBy(cei => cei.CEI_Description)
					.FirstOrDefault();
				return originalEntryInstruction?.CEI_DateForDuty ?? base.EffectiveValuationDateCore;
			}
		}
	}
}
