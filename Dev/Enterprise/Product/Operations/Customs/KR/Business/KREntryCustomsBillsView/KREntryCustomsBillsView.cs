using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeProperty(KREntryCustomsBillsView.Schema.KEB_CustomsDisbursementBillNumber), DescriptionProperty(KREntryCustomsBillsView.Schema.KEB_CustomsDisbursementBillNumber)]
	public partial class KREntryCustomsBillsView : AutoKREntryCustomsBillsView
	{
		public KREntryCustomsBillsView(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[List(nameof(Lookups) + "." + nameof(KREntryCustomsBillsViewLookups.ConsigneeList))]
		[ResourceStringData("DF6A6FE0-C276-4ABC-B3AD-FE15C9B6AE5E", Caption = "Payer")]
		public override ZGuid KEB_OH_Importer { get => base.KEB_OH_Importer; set => base.KEB_OH_Importer = value; }

		[ResourceStringData("F8B1B6E7-BF4B-4290-B73A-DD26B0D00817", Caption = "Entry Number")]
		public override ZString KEB_ImportEntryNum { get => base.KEB_ImportEntryNum; set => base.KEB_ImportEntryNum = value; }

		[ResourceStringData("9118FA31-A34D-482B-8D74-4C73E2BDC7D7", Caption = "Accepted Date")]
		public override ZDateTime KEB_ImportIssueDate { get => base.KEB_ImportIssueDate; set => base.KEB_ImportIssueDate = value; }

		[ResourceStringData("09A48E5E-0A22-4864-BC8F-87B7E4909950", Caption = "Payment Date")]
		public override ZDateTime KEB_PaymentAuthorizationDate { get => base.KEB_PaymentAuthorizationDate; set => base.KEB_PaymentAuthorizationDate = value; }

		[ResourceStringData("5918EF81-45D7-42E9-A549-A2E67178D754", Caption = "Branch")]
		public override ZGuid KEB_BranchPK { get => base.KEB_BranchPK; set => base.KEB_BranchPK = value; }

		[ResourceStringData("99E7242C-F0A1-4D8C-B187-C33B9872F78D", Caption = "Cleared Date")]
		public override ZDateTime KEB_EntryReleaseDate { get => base.KEB_EntryReleaseDate; set => base.KEB_EntryReleaseDate = value; }

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("4DB28968-2B09-4EFE-B980-907BD8029DBB", Caption = "Total Customs Fees")]
		public override ZDecimal KEB_CustomsFeesTotal { get => base.KEB_CustomsFeesTotal; set => base.KEB_CustomsFeesTotal = value; }

		[ResourceStringData("FA422B51-BCF2-4922-9183-C8E0C0C5056B", Caption = "Print Date")]
		public override ZDateTime KEB_PrintDate { get => base.KEB_PrintDate; set => base.KEB_PrintDate = value; }

		[ResourceStringData("5FB2C580-813C-43E5-942D-5E444670B30B", Caption = "Process Date")]
		public override ZDateTime KEB_ProcessDate { get => base.KEB_ProcessDate; set => base.KEB_ProcessDate = value; }

		[ResourceStringData("A4548318-EB88-4641-96D4-656DB052F551", Caption = "Due Date")]
		public override ZDateTime KEB_DueDate { get => base.KEB_DueDate; set => base.KEB_DueDate = value; }

		[ResourceStringData("B3566647-AE4F-44A9-9E2E-3645B94DC47F", Caption = "Customs Disbursement Bill #")]
		public override ZString KEB_CustomsDisbursementBillNumber { get => base.KEB_CustomsDisbursementBillNumber; set => base.KEB_CustomsDisbursementBillNumber = value; }

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("C48AA73E-D0DB-4D9F-AE1B-C837D8D67E69", Caption = "Duty Amount")]
		public override ZDecimal KEB_Duty { get => base.KEB_Duty; set => base.KEB_Duty = value; }

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("CEC46479-8EC8-4D39-9BFB-31C31A70AA88", Caption = "VAT")]
		public override ZDecimal KEB_ValueAddedTax { get => base.KEB_ValueAddedTax; set => base.KEB_ValueAddedTax = value; }

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("8DD4D87D-BD54-475F-8E65-71928FC12FA5", Caption = "Liquor Tax")]
		public override ZDecimal KEB_LiquorTax { get => base.KEB_LiquorTax; set => base.KEB_LiquorTax = value; }

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("315C3469-E638-49DB-AAD0-1AB457CAFF78", Caption = "Agriculture Tax")]
		public override ZDecimal KEB_AgricultureTax { get => base.KEB_AgricultureTax; set => base.KEB_AgricultureTax = value; }

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("768D870F-B2EF-41F0-AF19-FF2248CC3B2B", Caption = "Special Consumption Tax")]
		public override ZDecimal KEB_SpecialConsumptionTax { get => base.KEB_SpecialConsumptionTax; set => base.KEB_SpecialConsumptionTax = value; }

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("0B02CDD5-4F96-4238-9660-F3B7C4BD0991", Caption = "Transportation Tax")]
		public override ZDecimal KEB_TransportationTax { get => base.KEB_TransportationTax; set => base.KEB_TransportationTax = value; }

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("3231B772-EF80-41C9-8870-3CF7A4D3FC76", Caption = "Education Tax")]
		public override ZDecimal KEB_EducationTax { get => base.KEB_EducationTax; set => base.KEB_EducationTax = value; }

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("CAC1D7DE-F5B2-4C2E-84F3-EDE8F749F82F", Caption = "Penalty And Interest")]
		public override ZDecimal KEB_PenaltyAndInterest { get => base.KEB_PenaltyAndInterest; set => base.KEB_PenaltyAndInterest = value; }

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("035F59F7-4404-412A-A4A1-4548C5D23719", Caption = "Late Penalty")]
		public override ZDecimal KEB_LatePenalty { get => base.KEB_LatePenalty; set => base.KEB_LatePenalty = value; }

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("E8D8869B-165E-47D5-A23D-43EE7477B541", Caption = "Value For VAT")]
		public override ZDecimal KEB_ValueForVAT { get => base.KEB_ValueForVAT; set => base.KEB_ValueForVAT = value; }

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new KREntryCustomsBillsViewFetchHintStrategy(this);

		// TODO: It will be removed when the relevant 5WN View is created.
		public EDIMessage[] Messages5WN { get; set; } = [];

		public ImportEntryOrEntryLineSerializable GetImportEntryOrEntryLineSerializable()
		{
			var importEntryOrEntryLineSerializable = new ImportEntryOrEntryLineSerializable();
			var charges = new ChargesSerializable();
			if (KEB_PaymentAuthorizationDate.IsValid)
			{
				importEntryOrEntryLineSerializable.PaidDate = KEB_PaymentAuthorizationDate.ToDateTime();
			}
			charges.DutyAmount = KEB_Duty;
			charges.LiquorTaxAmount = KEB_LiquorTax;
			charges.SpecialConsumptionTaxAmount = KEB_SpecialConsumptionTax;
			charges.TransportTaxAmount = KEB_TransportationTax;
			charges.EducationTaxAmount = KEB_EducationTax;
			charges.AgricultureTaxAmount = KEB_AgricultureTax;
			charges.VATAmount = KEB_ValueAddedTax;
			charges.ValueForVAT = KEB_ValueForVAT;
			charges.TotalPenalty = KEB_PenaltyAndInterest;
			charges.LatePaymentPenalty = KEB_LatePenalty;
			charges.TotalPaid = KEB_CustomsFeesTotal;
			importEntryOrEntryLineSerializable.PaidAmounts = charges;

			// TODO: It will add the function to find a view and return EDIMessage array. 
			if (Messages5WN != null && Messages5WN.Length > 0)
			{
				importEntryOrEntryLineSerializable.RefundAmounts = new System.Collections.ObjectModel.Collection<ChargesIn5WNSerializable>();
				foreach (var message in Messages5WN)
				{
					var chargesIn5WNSerializable = new ChargesIn5WNSerializable();
					GOVCBR5WNMessageData messageData5WN;
					using (var textReader = message.GetEM_MessageTextReader())
					{
						messageData5WN = new GOVCBR5WNDataProvider().GetMessageData(Factory, textReader);
					}
					chargesIn5WNSerializable.RefundAmounts = new ChargesSerializable();
					chargesIn5WNSerializable.VersionNumber = ZShort.ParseSafe(message.EM_MessageOwner, 0);
					chargesIn5WNSerializable.VersionDescription = messageData5WN.AmendmentDeclarationDate.ToString(Constants.DateFormatType.DateKorean) + "(" + messageData5WN.AmendmentVersionNo.ToString() + ")";
					chargesIn5WNSerializable.RefundAmounts.DutyAmount = messageData5WN.DutyTax[EntryTaxTypeList.Codes.CUD];
					chargesIn5WNSerializable.RefundAmounts.LiquorTaxAmount = messageData5WN.DutyTax[EntryTaxTypeList.Codes.ACT];
					chargesIn5WNSerializable.RefundAmounts.SpecialConsumptionTaxAmount = messageData5WN.DutyTax[EntryTaxTypeList.Codes.IND];
					chargesIn5WNSerializable.RefundAmounts.TransportTaxAmount = messageData5WN.DutyTax[EntryTaxTypeList.Codes.ENV];
					chargesIn5WNSerializable.RefundAmounts.EducationTaxAmount = messageData5WN.DutyTax[EntryTaxTypeList.Codes._5AB];
					chargesIn5WNSerializable.RefundAmounts.AgricultureTaxAmount = messageData5WN.DutyTax[EntryTaxTypeList.Codes.CAP];
					chargesIn5WNSerializable.RefundAmounts.VATAmount = messageData5WN.DutyTax[EntryTaxTypeList.Codes.VAT];
					chargesIn5WNSerializable.RefundAmounts.LateDeclarationPenalty = messageData5WN.DutyTax[EntryTaxTypeList.Codes._5AC];
					chargesIn5WNSerializable.RefundAmounts.MissedDeclarationPenalty = messageData5WN.DutyTax[EntryTaxTypeList.Codes._5AY];

					importEntryOrEntryLineSerializable.RefundAmounts.Add(chargesIn5WNSerializable);
				}
			}
			return importEntryOrEntryLineSerializable;
		}

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public ZQuery GetCustomsDisbursementBills(ZString entryNumber)
			{
				var result = new ZQuery(KREntryCustomsBillsViewSchema.KEB_StatementType, SQLComparisonOperator.Contains, StatementHeaderTypeList.EntryDisbursementBillTypes);
				result.AddToFilter(KREntryCustomsBillsViewSchema.KEB_PaymentStatus, SQLComparisonOperator.Equal, StatementHeaderPaymentStatusList.Codes.PYC);
				result.AddToFilter(KREntryCustomsBillsViewSchema.KEB_ImportEntryNum, SQLComparisonOperator.Equal, entryNumber);
				return result;
			}

			public ZQuery GetQueryFor5UL() => new ZQuery(KREntryCustomsBillsViewSchema.KEB_PaymentStatus, SQLComparisonOperator.Equal, StatementHeaderPaymentStatusList.Codes.PYC);

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(KREntryCustomsBillsView);
		}
	}
}
