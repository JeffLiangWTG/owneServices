using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	sealed class AsycudaPackValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAPA_GoodsDescription()
		{
			var pack = Factory.New<AsycudaPack>();

			pack.APA_GoodsDescription = "Innovation is the engine of progress, driven by creativity, resilience, and the desire to improve the world. It involves seeing challenges as opportunities to solve problems and make life better. Through collaboration and curiosity, groundbreaking ideas emerge, leading to advances in technology, science, and culture. By embracing change and thinking beyond boundaries, humanity shapes its future, transforming dreams into reality, one step at a time, fueled by determination and a shared vision for a better tomorrow.";
			pack.Validation.ValidateAPA_GoodsDescription();
			AssertHasMessageError(pack.APA_GoodsDescriptionInfo, "Goods Description has exceeded the max length of 512.");

			pack.APA_GoodsDescription = "";
			pack.Validation.ValidateAPA_GoodsDescription();
			AssertNoErrors(pack.APA_GoodsDescriptionInfo);

			pack.APA_GoodsDescription = "Innovation is the engine of progress, driven by creativity, resilience, and the desire to improve the world. It involves seeing challenges as opportunities to solve problems and make life better. Through collaboration and curiosity, groundbreaking ideas emerge, leading to advances in technology, science, and culture. By embracing change and thinking beyond boundaries, humanity shapes its future, transforming dreams into reality, one step at a time, fueled by determination and a shared vision for a better tomorrow";
			pack.Validation.ValidateAPA_GoodsDescription();
			AssertNoErrors(pack.APA_GoodsDescriptionInfo);
		}

		public void TestCheckAPA_PackUQ()
		{
			SetupRegionName();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfDischarge = "GBBEL";
			var bill1 = header.Bills.AddNew();
			var pack1 = bill1.Packs.AddNew();
			pack1.APA_PackUQ = ZString.Empty;
			AssertNoMessageErrorContaining("No enter check errors if PackUQ is empty and PortOfLoading is in GB and admin region is in NORTHERN IRELAND", pack1.APA_PackUQInfo, MandatoryValidation.YouHaveNotEntered);

			pack1.APA_PackUQ = "ABC";
			AssertNoMessageErrorContaining("No enter check errors if PackUQ is not empty", pack1.APA_PackUQInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_RL_NKPortOfDischarge = "GBLON";
			var bill2 = header.Bills.AddNew();
			var pack2 = bill2.Packs.AddNew();
			pack2.APA_PackUQ = ZString.Empty;
			AssertHasMessageErrorContaining("Has enter check errors if PackUQ is empty and PortOfLoading is in GB and admin region is not in NORTHERN IRELAND", pack2.APA_PackUQInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckAPA_MarksAndNumbers()
		{
			SetupRegionName();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfDischarge = "GBBEL";
			var bill1 = header.Bills.AddNew();
			var pack1 = bill1.Packs.AddNew();
			pack1.APA_MarksAndNumbers = ZString.Empty;
			AssertNoMessageErrorContaining("No enter check errors if MarksAndNumbers is empty and PortOfLoading is in GB and admin region is in NORTHERN IRELAND", pack1.APA_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);

			pack1.APA_MarksAndNumbers = "999";
			AssertNoMessageErrorContaining("No enter check errors if MarksAndNumbers is not empty", pack1.APA_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_RL_NKPortOfDischarge = "GBLON";
			var bill2 = header.Bills.AddNew();
			var pack2 = bill2.Packs.AddNew();
			pack2.APA_MarksAndNumbers = ZString.Empty;
			AssertHasMessageErrorContaining("Has enter check errors if MarksAndNumbers is empty and PortOfLoading is in GB and admin region is not in NORTHERN IRELAND", pack2.APA_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);
		}

		void SetupRegionName()
		{
			var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
			if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
			{
				var refCountryStates = Factory.New<RefCountryStates>();
				refCountryStates.RW_RN_NKCountryCode = "UK";
				refCountryStates.RW_Code = "XXX";
				refCountryStates.RW_RegionName = "NORTHERN IRELAND";
				belfast.RL_RW = refCountryStates.PK;
			}

			var london = new RefUNLOCO.Loader(Factory).Load("GBLON");
			if (london.CountryStates == null || string.Compare(london.CountryStates.RW_RegionName, "ENGLAND", true) != 0)
			{
				var refCountryStates = Factory.New<RefCountryStates>();
				refCountryStates.RW_RN_NKCountryCode = "UK";
				refCountryStates.RW_Code = "YYY";
				refCountryStates.RW_RegionName = "ENGLAND";
				london.RL_RW = refCountryStates.PK;
			}
		}
	}
}
