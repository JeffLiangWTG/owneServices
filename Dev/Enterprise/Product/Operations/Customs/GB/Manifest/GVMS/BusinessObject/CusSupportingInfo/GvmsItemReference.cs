using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.GVMS
{
	public abstract class GvmsItemReference : CusSupportingInfo
	{
		public GvmsItemReference(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new readonly static GvmsCusSupportingInfoTypeDecider TypeDecider = new GvmsCusSupportingInfoTypeDecider();

		protected override ZString HumanReadableNameCore => "Customs Reference";

		public const string SystemStatus = "SYS";

		public static string GvmsItemReferenceType => GVMSManifestType.Codes.GoodsVehicleMovementSystemGvms;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = GvmsItemReferenceType;
		}

		public new GvmsItemReferenceLookups Lookups => (GvmsItemReferenceLookups)base.Lookups;

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return new GvmsItemReferenceValidation(this);
		}

		[ResourceStringData("Enterprise.Customs.GB.GVMS.GvmsItemReference.CSI_Code", Caption = "Customs Ref")]
		[List(nameof(Lookups) + "." + nameof(GvmsItemReferenceLookups.CodeList))]
		public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }
		[ResourceStringData("Enterprise.Customs.GB.GVMS.GvmsItemReference.CSI_DateOfIssue", Caption = "Date of Issue")]
		public override ZDateTime CSI_DateOfIssue { get => base.CSI_DateOfIssue; set => base.CSI_DateOfIssue = value; }
		[ResourceStringData("Enterprise.Customs.GB.GVMS.GvmsItemReference.CSI_RN_NKCountryCode", Caption = "Country Code")]
		public override ZString CSI_RN_NKCountryCode { get => base.CSI_RN_NKCountryCode; set => base.CSI_RN_NKCountryCode = value; }
		[ResourceStringData("Enterprise.Customs.GB.GVMS.GvmsItemReference.CSI_ReferenceNumber", Caption = "Reference No.")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }
		[ResourceStringData("Enterprise.Customs.GB.GVMS.GvmsItemReference.CSI_ReferenceNumber2", Caption = "Pallet Ref")]
		public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }
		[ResourceStringData("Enterprise.Customs.GB.GVMS.GvmsItemReference.CSI_Status", Caption = "TSAD")]
		public override ZString CSI_Status { get => base.CSI_Status; set => base.CSI_Status = value; }

		public override bool ReadOnly
		{
			get
			{
				return base.ReadOnly || CSI_IssuerType == SystemStatus;
			}
			set
			{
				base.ReadOnly = value;
				RefreshBinding();
			}
		}
	}
}
