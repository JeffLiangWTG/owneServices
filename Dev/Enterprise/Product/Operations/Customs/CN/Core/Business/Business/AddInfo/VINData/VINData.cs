using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.CN.Business
{
	public class VINData : AutoCNVINData
	{
		public VINData(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

		public override bool SupportsNotes => false;

		[ResourceStringData("Enterprise.Customs.CN.Business.VINData|XC_QGP", Caption = "Quality Guarantee Period", ShortCaption = "QGP")]
		public override ZString XC_QGP
		{
			get => base.XC_QGP;
			set => base.XC_QGP = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.VINData|XC_EngineNo", Caption = "Engine Number", ShortCaption = "Engine No")]
		public override ZString XC_EngineNo
		{
			get => base.XC_EngineNo;
			set => base.XC_EngineNo = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.VINData|XC_VIN", Caption = "Vehicle Identification Number", ShortCaption = "VIN")]
		public override ZString XC_VIN
		{
			get => base.XC_VIN;
			set => base.XC_VIN = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.VINData|XC_ChassisNo", Caption = "Chassis No")]
		public override ZString XC_ChassisNo
		{
			get => base.XC_ChassisNo;
			set => base.XC_ChassisNo = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.VINData|XC_ProductNameCN", ShortCaption = "Product Name (CN)")]
		public override ZString XC_ProductNameCN
		{
			get => base.XC_ProductNameCN;
			set => base.XC_ProductNameCN = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.VINData|XC_ProductNameEN", ShortCaption = "Product Name (EN)")]
		public override ZString XC_ProductNameEN
		{
			get => base.XC_ProductNameEN;
			set => base.XC_ProductNameEN = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.VINData|XC_ModelEN", Caption = "Model (EN)")]
		public override ZString XC_ModelEN
		{
			get => base.XC_ModelEN;
			set => base.XC_ModelEN = value;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("D67DE3EF-A012-4F07-8ECC-1DEC315605C3", "VIN Data");

		protected override CusAddInfoValidation GetNewValidation()
		{
			return new VINDataValidation(this);
		}

		public ZString GetDefaultCNProductName()
		{
			if (Parent != null)
			{
				return !Parent.JI_NameOfGoods.IsEmpty ? Parent.JI_NameOfGoods : Parent.JI_NameOfGoods2;
			}
			return ZString.Empty;
		}

		public ZString GetDefaultENProductName()
		{
			return Parent?.JI_Description.Left(AutoCNVINDataAddInfo.Schema.XC_ProductNameENMaxLength) ?? ZString.Empty;
		}
	}
}
