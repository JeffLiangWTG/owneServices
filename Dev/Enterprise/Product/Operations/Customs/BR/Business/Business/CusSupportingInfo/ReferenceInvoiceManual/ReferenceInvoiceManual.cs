using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ReferenceInvoiceManual : CusSupportingInfo
	{
		const string charSeparator = "|";

		public new class Schema : CusSupportingInfo.Schema
		{
			public const string CSI_State = "CSI_State";
			public const string CSI_Serie = "CSI_Serie";
			public const string CSI_NumberRefenceInvoiceManual = "CSI_NumberRefenceInvoiceManual";
		}

		public ReferenceInvoiceManual(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool SupportsNotes => false;

		[MaxLength(14)]
		[ResourceStringData("Enterprise.Customs.BR.Business.ReferenceInvoiceManual|CSI_ReferenceNumber", Caption = "CNPJ/ CPF of the Issuer")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.ReferenceInvoiceManual|CSI_LineNo", Caption = "Line Item")]
		public override ZInt CSI_LineNo { get => base.CSI_LineNo; set => base.CSI_LineNo = value; }

		[ResourceStringData("Enterprise.Customs.BR.Business.ReferenceInvoiceManual|CSI_Quantity", Caption = "Quantity")]
		public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(ReferenceInvoiceManualLookups.ModelOfNotaFiscalList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.ReferenceInvoiceManual|CSI_Code", Caption = "Model")]
		public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

		[MaxLength(7)]
		[ResourceStringData("Enterprise.Customs.BR.Business.ReferenceInvoiceManual|CSI_AdditionalDescription", Caption = "Year/Month")]
		public override ZString CSI_AdditionalDescription { get => base.CSI_AdditionalDescription; set => base.CSI_AdditionalDescription = value; }

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(ReferenceInvoiceManualLookups.BRStateList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.ReferenceInvoiceManual|CSI_State", Caption = "State")]
		public ZString CSI_State
		{
			get { return getPartOfReferenceNumber(0); }
			set
			{
				var oldValue = CSI_State;
				if (!IsCopying && oldValue != value)
				{
					CheckMaximumLength(CSI_StateInfo, value);
					CSI_StateInfo.RefreshBinding();
					CSI_ReferenceNumber2 = value + charSeparator + CSI_Serie + charSeparator + CSI_NumberRefenceInvoiceManual;
				}
				if (!IsValidationSuspended)
				{
					(Validation as ReferenceInvoiceManualValidation)?.ValidateCSI_State();
				}
			}
		}

		public ZPropertyInfo CSI_StateInfo
		{
			get { return GetZPropertyInfo(Schema.CSI_State); }
		}

		[MaxLength(3)]
		[ResourceStringData("Enterprise.Customs.BR.Business.ReferenceInvoiceManual|CSI_Serie", Caption = "Series")]
		public ZString CSI_Serie
		{
			get { return getPartOfReferenceNumber(1); }
			set
			{
				var oldValue = CSI_Serie;
				if (!IsCopying && oldValue != value)
				{
					CSI_ReferenceNumber2 = CSI_State + charSeparator + value + charSeparator + CSI_NumberRefenceInvoiceManual;
				}
			}
		}

		[MaxLength(9)]
		[ResourceStringData("Enterprise.Customs.BR.Business.ReferenceInvoiceManual|CSI_NumberRefenceInvoiceManual", Caption = "Number")]
		public ZString CSI_NumberRefenceInvoiceManual
		{
			get { return getPartOfReferenceNumber(2); }
			set
			{
				var oldValue = CSI_NumberRefenceInvoiceManual;
				if (!IsCopying && oldValue != value)
				{
					CSI_ReferenceNumber2 = CSI_State + charSeparator + CSI_Serie + charSeparator + value;
				}
			}
		}

		ZString getPartOfReferenceNumber(int position)
		{
			var referenceNumber = CSI_ReferenceNumber2.Split(charSeparator);
			return (referenceNumber.Length == 3) ? referenceNumber[position] : ZString.Empty;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.ReferenceInvoiceManual;
			CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
		}

		public new ReferenceInvoiceManualLookups Lookups => (ReferenceInvoiceManualLookups)base.Lookups;

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			CusSupportingInfoValidation result;
			if (Parent?.IsExport ?? ZBool.False)
			{
				result = new ReferenceInvoiceManualValidation(this);
			}
			else
			{
				result = base.GetNewValidation();
			}
			return result;
		}

		protected override CusSupportingInfoLookups GetNewLookups() => new ReferenceInvoiceManualLookups(this);

		public new JobComInvoiceLine Parent => base.Parent as JobComInvoiceLine;
	}
}
