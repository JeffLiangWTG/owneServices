using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class BaseCusOutturnValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateGoodsDescription()
		{
			string messageError = "Goods description is mandatory for Surplus Consignment or Surplus Packages.";
			Outturn.C5_OutturnResultType = CMROutturnResultType.Codes.SurplusConsignment;
			Outturn.C5_GoodsDescription = "";
			Assert("Should contain error for SurplusConsignment type", Outturn.C5_GoodsDescriptionInfo.GetErrors().Contains(messageError));
			Outturn.C5_GoodsDescription = "Cuckoo Squeakers";
			Assert("Should NOT contain error", !Outturn.C5_GoodsDescriptionInfo.GetErrors().Contains(messageError));
			Outturn.C5_OutturnResultType = CMROutturnResultType.Codes.SurplusPackages;
			Outturn.C5_GoodsDescription = "";
			Assert("Should contain error for SurplusPackages type", Outturn.C5_GoodsDescriptionInfo.GetErrors().Contains(messageError));
			Outturn.C5_GoodsDescription = "Cuckoo Squeakers";
			Assert("Should NOT contain error", !Outturn.C5_GoodsDescriptionInfo.GetErrors().Contains(messageError));
			Outturn.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;
			Outturn.C5_GoodsDescription = "";
			Assert("Should NOT contain error", !Outturn.C5_GoodsDescriptionInfo.GetErrors().Contains(messageError));
			Outturn.C5_OutturnResultType = CMROutturnResultType.Codes.ShortLanded;
			Outturn.C5_GoodsDescription = "";
			Assert("Should NOT contain an error for ShortLanded type when goods description is blank", !Outturn.C5_GoodsDescriptionInfo.GetErrors().Contains(messageError));
		}

		CusOutturnHeader header;
		protected CusOutturnHeader Header => header ?? (header = CusOutturnHeader.New(Factory));

		protected virtual CusOutturn GetNewOutturn()
		{
			var outturn = Header.Outturns.AddNew();
			return outturn;
		}

		CusOutturn outturn;
		protected CusOutturn Outturn => outturn ?? (outturn = GetNewOutturn());
	}
}
