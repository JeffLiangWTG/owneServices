using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.CDS.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	class CDSDataTransferHelperTest : TestCaseWithFactory
	{
		public void TestGetLocationAtClearanceInfoForWritingUXML()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_Calc_LocationOtherInformationCountry = ZString.Empty;
			declaration.JE_Calc_LocationOtherInformationType = ZString.Empty;
			declaration.JE_LocationQualifier = ZString.Empty;
			declaration.JE_GoodsLocation = "TESTPLACE";
			declaration.JE_LocationOfGoods = "NOTUSEDFORCDS";

			CombineAssertions(() =>
			{
				AssertEquals("JE_GoodsLocation", "      TESTPLACE", helper.GetLocationAtClearanceInfoForWritingUXML(declaration));
				declaration.JE_Calc_LocationOtherInformationCountry = "GB";
				AssertEquals("JE_Calc_LocationOtherInformationCountry", "GB    TESTPLACE", helper.GetLocationAtClearanceInfoForWritingUXML(declaration));
				declaration.JE_Calc_LocationOtherInformationType = "BU";
				AssertEquals("JE_Calc_LocationOtherInformationType", "GBBU  TESTPLACE", helper.GetLocationAtClearanceInfoForWritingUXML(declaration));
				declaration.JE_LocationQualifier = "CW";
				AssertEquals("JE_LocationQualifier", "GBBUCWTESTPLACE", helper.GetLocationAtClearanceInfoForWritingUXML(declaration));
			});
		}

		public void TestGetLocationOtherInformationFromLocationAtClearanceForReadingUXML()
		{
			AssertEquals("AaBbCcDdddddddddddddd", helper.GetLocationOtherInformationFromLocationAtClearanceForReadingUXML(locationAtClearance));
		}

		public void TestGetLocationQualifierFromLocationAtClearanceForReadingUXML()
		{
			AssertEquals("Cc", helper.GetLocationQualifierFromLocationAtClearanceForReadingUXML(locationAtClearance));
		}

		public void TestGetLocationOfGoodsFromLocationAtClearanceForReadingUXML()
		{
			AssertNull(helper.GetLocationOfGoodsFromLocationAtClearanceForReadingUXML(locationAtClearance));
		}

		protected override void SetUp()
		{
			base.SetUp();

			helper = new CDSDataTransferHelper();
			locationAtClearance = new CodeDescriptionPair35Char();
			locationAtClearance.Code = "A".PadRight(JobDeclaration.Schema.JE_Calc_LocationOtherInformationCountryMaxLength, 'a') +
									   "B".PadRight(JobDeclaration.Schema.JE_Calc_LocationOtherInformationTypeMaxLength, 'b') +
									   "C".PadRight(JobDeclaration.Schema.JE_LocationQualifierMaxLength, 'c') +
									   "D".PadRight(JobDeclaration.Schema.JE_GoodsLocationMaxLength, 'd') +
									   "SomeExtraRubbish";
		}

		CodeDescriptionPair35Char locationAtClearance;
		CDSDataTransferHelper helper;
	}
}
