using System.ComponentModel;
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
	public class CertificateOfOrigin : CusSupportingInfo
	{
		public CertificateOfOrigin(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool SupportsNotes => false;

		[MaxLength(5)]
		[List(nameof(Lookups) + "." + nameof(CertificateOfOriginLookups.CertificateTypeList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.CertificateOfOrigin|CSI_SubType", Caption = "Type")]
		public override ZString CSI_SubType
		{
			get => base.CSI_SubType;
			set => base.CSI_SubType = value;
		}

		[MaxLength(25)]
		[ResourceStringData("Enterprise.Customs.BR.Business.CertificateOfOrigin|CSI_ReferenceNumber", Caption = "Code")]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.BR.Business.CertificateOfOrigin|TariffCode", Caption = "Tariff Code")]
		public ZString TariffCode
		{
			get => Parent?.JI_Tariff ?? ZString.Empty;
		}

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.BR.Business.CertificateOfOrigin|UQ", Caption = "UQ")]
		public ZString UQ
		{
			get => Parent?.JI_CustomsUnitQty ?? ZString.Empty;
		}

		[DecimalPlaces(5)]
		[ResourceStringData("Enterprise.Customs.BR.Business.CertificateOfOrigin|CSI_Quantity", Caption = "Customs Qty")]
		public override ZDecimal CSI_Quantity
		{
			get => base.CSI_Quantity;
			set => base.CSI_Quantity = value;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.CertificateOfOrigin;
			CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
		}

		public new CertificateOfOriginLookups Lookups => (CertificateOfOriginLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new CertificateOfOriginLookups(this);

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			CusSupportingInfoValidation result;
			if (Parent?.IsExport ?? ZBool.False)
			{
				result = new CertificateOfOriginValidation(this);
			}
			else
			{
				result = base.GetNewValidation();
			}
			return result;
		}

		public new JobComInvoiceLine Parent => base.Parent as JobComInvoiceLine;
	}
}
