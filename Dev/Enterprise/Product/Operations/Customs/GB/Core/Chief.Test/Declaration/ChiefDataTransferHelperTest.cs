using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.GB.Chief.Declaration.Testing
{
	class ChiefDataTransferHelperTest : TestCaseWithFactory
	{
		public void TestGetLocationAtClearanceInfoForWritingUXML()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;

			declaration.JE_GoodsLocation = "TESTPLACE";
			declaration.JE_LocationOfGoods = "NOTUSEDFORCDS";

			AssertEquals("NOTUSEDFORCDS", helper.GetLocationAtClearanceInfoForWritingUXML(declaration));
		}

		public void TestGetLocationOtherInformationFromLocationAtClearanceForReadingUXML()
		{
			AssertNull(helper.GetLocationOtherInformationFromLocationAtClearanceForReadingUXML(locationAtClearance));
		}

		public void TestGetLocationQualifierFromLocationAtClearanceForReadingUXML()
		{
			AssertNull(helper.GetLocationQualifierFromLocationAtClearanceForReadingUXML(locationAtClearance));
		}

		public void TestGetLocationOfGoodsFromLocationAtClearanceForReadingUXML()
		{
			AssertEquals("Aaa", helper.GetLocationOfGoodsFromLocationAtClearanceForReadingUXML(locationAtClearance));
		}

		//public void TestGetGoodsLocationFromLocationAtClearanceForReadingUXML()
		//{
		//	AssertNull(helper.GetGoodsLocationFromLocationAtClearanceForReadingUXML(locationAtClearance));
		//}

		protected override void SetUp()
		{
			base.SetUp();

			helper = new ChiefDataTransferHelper();
			locationAtClearance = new CodeDescriptionPair35Char();
			locationAtClearance.Code = "A".PadRight(JobDeclaration.Schema.JE_CHIEF_GoodsLocationMaxLength, 'a') + "SomeExtraRubbish";
		}

		CodeDescriptionPair35Char locationAtClearance;
		ChiefDataTransferHelper helper;
	}
}
