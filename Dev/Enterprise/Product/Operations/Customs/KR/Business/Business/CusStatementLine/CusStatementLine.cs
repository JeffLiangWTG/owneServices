using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	[DependentBusinessObject(typeof(CusStatementHeader), "StatementLines")]
	public class CusStatementLine : BaseCusStatementLine
	{
		public CusStatementLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region ReadOnly

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return true;
		}

		#endregion

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public static ZQuery GetQuery(ZString entryNumber, ZString entryType, ZGuid companyPK, string[] statementTypes)
			{
				var result = new ZDBOnlyQuery(typeof(CusStatementLine));
				result.AddToFilter(CusStatementLineSchema.B3_EntryNum, entryNumber);
				result.AddToFilter(CusStatementLineSchema.B3_EntryType, entryType);

				var headerQuery = new ZDBOnlySubQuery(typeof(CusStatementHeader), CusStatementHeaderSchema.PK);
				headerQuery.AddToFilter(CusStatementHeaderSchema.B2_StatementType, statementTypes);
				headerQuery.AddToFilter(CusStatementHeaderSchema.B2_GC, companyPK);

				result.AddSubQuery(CusStatementLineSchema.B3_B2, CusStatementHeaderSchema.PK, headerQuery, JoinCondition.And);
				return result;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CusStatementLine);
			}
		}
		#endregion

		public ZString IndividualCustomsDisbursementBillNo => StatementHeader.B2_StatementType == StatementHeaderTypeList.Codes.Invoice ? B3_AssociatedEntry : StatementHeader.B2_StatementNumber;
		public ZString FormattedCustomsDisbursementBill => MessageFunctions.GetFormattedNumber(IndividualCustomsDisbursementBillNo, new int[] { 0, 4, 7, 9, 11, 12, 18 });
		public new CusStatementHeader StatementHeader => (CusStatementHeader)base.StatementHeader;

		[ChildEditable(true)]
		public CusStatementLineChargeCollection Charges
		{
			get
			{
				if (charges == null)
				{
					charges = new CusStatementLineChargeCollection(this);
					RegisterEditableChildObject(charges);
				}
				return charges;
			}
		}
		CusStatementLineChargeCollection charges;

		public override void Delete()
		{
			Charges.DeleteAll();
			base.Delete();
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new CusStatementLineFetchStrategy(this);

		[ResourceStringData("A3111F7F-E410-43D1-A38A-8AFF55DE5E97", Caption = "Seq #")]
		public override ZShort B3_SequenceNumber
		{
			get { return base.B3_SequenceNumber; }
			set { base.B3_SequenceNumber = value; }
		}

		[ResourceStringData("7688EE42-B6D6-48C9-B464-B7D19645B724", Caption = "Entry Type")]
		public override ZString B3_EntryType { get => base.B3_EntryType; set => base.B3_EntryType = value; }

		[ResourceStringData("6BFD01A9-3547-4401-8DB6-6B5020F062E3", Caption = "Individual Payment Date")]
		public override ZDate B3_EntryDate { get => base.B3_EntryDate; set => base.B3_EntryDate = value; }

		[DecimalPlaces(0)]
		[ResourceStringData("9B4F9ACC-6826-4906-A1BD-2497791C94A0", Caption = "Total Fee")]
		[ResourceStringData("352055A8-ADA0-4E56-B701-B5A23CB48D05", Caption = "Total Tax", MultipleKey = StatementHeaderTypeList.Codes.Invoice)]
		[ResourceStringData("C0AB4D11-AA25-4E23-A8FB-B63F83891691", Caption = "Tax (VAT)", MultipleKey = StatementHeaderTypeList.Codes.MonthlyReceipt)]
		[ResourceStringData("D276B9E0-B5A8-431B-B85A-12E2B1714C74", Caption = "Tax (VAT)", MultipleKey = StatementHeaderTypeList.Codes.IndividualCollectionReceipt)]
		public override ZDecimal B3_CustomsFeesTotal { get => base.B3_CustomsFeesTotal; set => base.B3_CustomsFeesTotal = value; }

		[ResourceStringData("608693C9-731F-43BF-9ADE-DCF115D5044C", Caption = "Entry Number")]
		public ZString FormattedNumber => MessageFunctions.DeclarationNumberFormat(B3_EntryNum);
		public ZPropertyInfo FormattedNumberInfo => GetZPropertyInfo(nameof(FormattedNumber));

		[ResourceStringData("22804952-48A7-463C-B67F-F79B957FC3E0", Caption = "Individual Payment No.")]
		public ZString FormattedLinePaymentNumber => GetFormattedNumber(StatementHeader.B2_StatementType, B3_AssociatedEntry);

		static ZString GetFormattedNumber(string numberType, ZString unformattedStatementNumber)
		{
			var result = ZString.Empty;
			switch (numberType)
			{
				case StatementHeaderTypeList.Codes.Invoice:
					result = MessageFunctions.GetFormattedNumber(unformattedStatementNumber, new int[] { 0, 4, 7, 9, 11, 12, 18 });
					break;
				default:
					result = MessageFunctions.GetFormattedNumber(unformattedStatementNumber, new int[] { 0, 3, 5, 7, 8, 14 });
					break;
			}
			return result;
		}

		public ZString StatementNumber5WNDescription => StatementHeader.B2_PaymentAuthorizationDate.IsValid ?
			Res.GetString("F49E3019-5804-47BF-A3AE-BD59956FF68B", "Due Date: {0}, Paid on {1}", StatementHeader.B2_DueDate.ToString(Constants.DateFormatType.DateKorean), StatementHeader.B2_PaymentAuthorizationDate.ToString(Constants.DateFormatType.DateKorean))
			: Res.GetString("A8669AFE-C74D-4586-847C-57CE6CF2158F", "Due Date: {0}, Unpaid", StatementHeader.B2_DueDate.ToString(Constants.DateFormatType.DateKorean));
		public CusEntryHeader Entry => B3_EntryType == SharedJobMessageTypeList.Codes.Import ? MessageLinkedObjectManager.GetLinkedObject(Factory, StatementHeader.Company, B3_EntryNum, B3_EntryType) : null;

		[DecimalPlaces(0)]
		[ResourceStringData("752F9E8E-4A8D-46A6-B27B-75011F9BF275", Caption = "Customs Value")]
		public ZDecimal BaseAmount => baseAmount ?? (baseAmount = Charges.GetChargeAmount(ChargeTypeList.Codes.ValueForVAT)).Value;
		ZDecimal? baseAmount;

		[DecimalPlaces(0)]
		[ResourceStringData("67E57981-38D5-457D-9B63-C0275E6E0C2E", Caption = "Duty Amount")]
		public ZDecimal DutyAmount => dutyAmount ?? (dutyAmount = Charges.GetChargeAmount(ChargeTypeList.Codes.Duty)).Value;
		ZDecimal? dutyAmount;

		[DecimalPlaces(0)]
		[ResourceStringData("D1DD84B6-A1BD-4180-8293-E4409424AF2E", Caption = "VAT")]
		public ZDecimal VAT => vat ?? (vat = Charges.GetChargeAmount(ChargeTypeList.Codes.VAT)).Value;
		ZDecimal? vat;

		[DecimalPlaces(0)]
		[ResourceStringData("1A8EE16A-4C38-46D3-A976-6C5BF3D02888", Caption = "Liquor Tax")]
		public ZDecimal LiquorTax => liquorTax ?? (liquorTax = Charges.GetChargeAmount(ChargeTypeList.Codes.LiquorTax)).Value;
		ZDecimal? liquorTax;

		[DecimalPlaces(0)]
		[ResourceStringData("E26CDC97-A9DF-4CDE-B980-41BF9FD86CB2", Caption = "Agriculture Tax")]
		public ZDecimal AgricultureTax => agricultureTax ?? (agricultureTax = Charges.GetChargeAmount(ChargeTypeList.Codes.AgricultureTax)).Value;
		ZDecimal? agricultureTax;

		[DecimalPlaces(0)]
		[ResourceStringData("83940DC2-F8E9-4363-8E58-315BDA788681", Caption = "Transportation Tax")]
		public ZDecimal TransportationTax => transportationTax ?? (transportationTax = Charges.GetChargeAmount(ChargeTypeList.Codes.TransportationTax)).Value;
		ZDecimal? transportationTax;

		[DecimalPlaces(0)]
		[ResourceStringData("611F87D3-6224-4DD7-BE29-FCE743EC69A4", Caption = "Education Tax")]
		public ZDecimal EducationTax => educationTax ?? (educationTax = Charges.GetChargeAmount(ChargeTypeList.Codes.EducationTax)).Value;
		ZDecimal? educationTax;

		[DecimalPlaces(0)]
		[ResourceStringData("E459A477-45AB-4D32-BF20-F96E567AB423", Caption = "Interest And Penalty")]
		public ZDecimal PenaltyAndInterest => penaltyAndInterest ?? (penaltyAndInterest = Charges.GetChargeAmount(ChargeTypeList.Codes.PenaltyAndInterest)).Value;
		ZDecimal? penaltyAndInterest;

		[DecimalPlaces(0)]
		[ResourceStringData("AFDF4882-12CF-402E-AD46-00129DF44F9D", Caption = "Special Consumption Tax")]
		public ZDecimal SpecialConsumptionTax => specialConsumptionTax ?? (specialConsumptionTax = Charges.GetChargeAmount(ChargeTypeList.Codes.SpecialConsumptionTax)).Value;
		ZDecimal? specialConsumptionTax;

		[DecimalPlaces(0)]
		[ResourceStringData("8E2D96AF-B7DB-4D81-9F3E-B9EE1DB49516", Caption = "Penalty for Late Payment")]
		public ZDecimal PenaltyForLatePayment => penaltyForLatePayment ?? (penaltyForLatePayment = Charges.GetChargeAmount(ChargeTypeList.Codes.PenaltyForLateOrMissedDeclaration)).Value;
		ZDecimal? penaltyForLatePayment;
	}
}
