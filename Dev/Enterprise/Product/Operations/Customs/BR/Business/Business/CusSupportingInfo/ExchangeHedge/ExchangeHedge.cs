using System.Collections.Generic;
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
	public class ExchangeHedge : SingleCusSupportingInfo
	{
		public ExchangeHedge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool SupportsNotes => false;

		public override bool ReadOnly => base.ReadOnly || (Parent is JobComInvoiceHeader parent && parent.IsImportExcludingLicense && parent.AnyLineHasLinkedInvoiceLine);

		bool IsGeneratedFromImportSiscomex => (Parent is JobComInvoiceHeader parent && parent.AnyLineIsImportLicenseGeneratedFromImportSiscomexLine);

		[MaxLength(1)]
		[ReadOnlyMember(nameof(IsGeneratedFromImportSiscomex))]
		[List(nameof(Lookups) + "." + nameof(ExchangeHedgeLookups.ExchangeHedgeList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.ExchangeHedge|CSI_Code", Caption = "Exchange Hedge Type")]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = CSI_Code;
				base.CSI_Code = value;
				if (!IsCopying && oldValue != CSI_Code)
				{
					if (CSI_SubType_ReadOnly)
					{
						CSI_SubType = ZString.Empty;
					}
					if (CSI_Quantity_ReadOnly)
					{
						CSI_Quantity = ZDecimal.Zero;
					}
					if (CSI_AdditionalDescription_ReadOnly)
					{
						CSI_AdditionalDescription = ZString.Empty;
					}
					if (CSI_IssuerType_ReadOnly)
					{
						CSI_IssuerType = ZString.Empty;
					}
					if (CSI_ReferenceNumber_ReadOnly)
					{
						CSI_ReferenceNumber = ZString.Empty;
					}
					if (CSI_Value_ReadOnly)
					{
						CSI_Value = ZDecimal.Zero;
					}
				}
			}
		}

		[MaxLength(2)]
		[ReadOnlyMember(nameof(CSI_SubType_ReadOnly))]
		[List(nameof(Lookups) + "." + nameof(ExchangeHedgeLookups.PaymentMethodList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.ExchangeHedge|CSI_SubType", Caption = "Exchange Hedge Payment Method")]
		public override ZString CSI_SubType
		{
			get => base.CSI_SubType;
			set => base.CSI_SubType = value;
		}

		public bool CSI_SubType_ReadOnly
		{
			get
			{
				if (Parent?.IsImportLicense ?? false)
				{
					return CSI_Code != ExchangeHedgeList.Codes._1 && CSI_Code != ExchangeHedgeList.Codes._2;
				}
				return true;
			}
		}

		[MaxLength(3)]
		[ReadOnlyMember(nameof(CSI_Quantity_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.ExchangeHedge|CSI_Quantity", Caption = "Exchange Hedge Payment Deadline")]
		public override ZDecimal CSI_Quantity
		{
			get => base.CSI_Quantity;
			set => base.CSI_Quantity = value;
		}

		public bool CSI_Quantity_ReadOnly
		{
			get
			{
				if (Parent?.IsImportLicense ?? false)
				{
					return CSI_Code != ExchangeHedgeList.Codes._1;
				}
				return true;
			}
		}

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(ExchangeHedgeLookups.ReasonTypeList))]
		[ReadOnlyMember(nameof(CSI_AdditionalDescription_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.ExchangeHedge|CSI_AdditionalDescription", Caption = "Exchange Hedge Reason")]
		public override ZString CSI_AdditionalDescription
		{
			get => base.CSI_AdditionalDescription;
			set => base.CSI_AdditionalDescription = value;
		}

		public bool CSI_AdditionalDescription_ReadOnly => CSI_Code != ExchangeHedgeList.Codes._4 || IsGeneratedFromImportSiscomex;

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(ExchangeHedgeLookups.FinancialInstitutionList))]
		[ReadOnlyMember(nameof(CSI_IssuerType_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.ExchangeHedge|CSI_IssuerType", Caption = "Exchange Hedge Financial Institution")]
		public override ZString CSI_IssuerType
		{
			get => base.CSI_IssuerType;
			set => base.CSI_IssuerType = value;
		}

		public bool CSI_IssuerType_ReadOnly => CSI_Code != ExchangeHedgeList.Codes._3 || IsGeneratedFromImportSiscomex;

		[MaxLength(8)]
		[ReadOnlyMember(nameof(CSI_ReferenceNumber_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.ExchangeHedge|CSI_ReferenceNumber", Caption = "Exchange Hedge ROF/BACEN Number")]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}

		public bool CSI_ReferenceNumber_ReadOnly => CSI_Code != ExchangeHedgeList.Codes._3 && CSI_Code != ExchangeHedgeList.Codes._4 || IsGeneratedFromImportSiscomex;

		[DecimalPlaces(2)]
		[ReadOnlyMember(nameof(CSI_Value_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.BR.Business.ExchangeHedge|CSI_Value", Caption = "Exchange Hedge Value")]
		public override ZDecimal CSI_Value
		{
			get => base.CSI_Value;
			set => base.CSI_Value = value;
		}

		public bool CSI_Value_ReadOnly => CSI_Code != ExchangeHedgeList.Codes._3 || IsGeneratedFromImportSiscomex;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.ExchangeHedge;
			CSI_ParentTableCode = JobComInvoiceHeaderSchema.Constants.Prefix;
		}

		protected override CusSupportingInfoValidation GetNewValidation() => new ExchangeHedgeValidation(this);

		public new ExchangeHedgeLookups Lookups => (ExchangeHedgeLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new ExchangeHedgeLookups(this);

		public override IEnumerable<ZPropertyInfo> GetUsedFieldsInfos()
		{
			yield return CSI_CodeInfo;
			yield return CSI_SubTypeInfo;
			yield return CSI_QuantityInfo;
			yield return CSI_AdditionalDescriptionInfo;
			yield return CSI_IssuerTypeInfo;
			yield return CSI_ReferenceNumberInfo;
			yield return CSI_ValueInfo;
		}

		public new JobComInvoiceHeader Parent => base.Parent as JobComInvoiceHeader;
	}
}
