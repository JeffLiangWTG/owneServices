using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.PBN.Business
{
	public abstract class PBNReferenceItem : CusSupportingInfo
	{
		protected PBNReferenceItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public const string PBNReferenceType = "PBN";

		public new static readonly TypeDecider TypeDecider = new PBNCusSupportingInfoTypeDecider();

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = PBNReferenceType;
			CSI_Status = PBNDeclarationReferenceStatusList.Codes.TBA;
		}

		[ResourceStringData("Enterprise.Customs.IE.PBN.PBNItemReference.CSI_Code", Caption = "Type")]
		public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

		[ResourceStringData("Enterprise.Customs.IE.PBN.PBNItemReference.CSI_DateOfIssue", Caption = "Date of Issue")]
		public override ZDateTime CSI_DateOfIssue { get => base.CSI_DateOfIssue; set => base.CSI_DateOfIssue = value; }

		[ResourceStringData("Enterprise.Customs.IE.PBN.PBNItemReference.CSI_RN_NKCountryCode", Caption = "Country Code")]
		public override ZString CSI_RN_NKCountryCode { get => base.CSI_RN_NKCountryCode; set => base.CSI_RN_NKCountryCode = value; }

		[ResourceStringData("Enterprise.Customs.IE.PBN.PBNItemReference.CSI_ReferenceNumber", Caption = "Reference Number")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }
	}
}
