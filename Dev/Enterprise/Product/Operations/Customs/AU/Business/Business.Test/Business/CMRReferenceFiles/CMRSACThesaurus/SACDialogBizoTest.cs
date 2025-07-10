using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(SACDialogBizo))]
	sealed class SACDialogBizoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSACQuestionForHAWB()
		{
			string expectedResult = @"Checking this box means that the person sending the cargo report is also making a self assessed clearance declaration for the purposes of section 71 of the Customs Act. The self assessed clearance declaration made by checking this indicator declares that:" + System.Environment.NewLine + System.Environment.NewLine +
				"* the value of the goods does not exceed $250 (or other prescribed amount); and" + System.Environment.NewLine + System.Environment.NewLine +
				"* the description of the goods does not include any word, term or description specified in the Thesaurus provided by Customs." + System.Environment.NewLine + System.Environment.NewLine +
				"Do not check this if you cannot declare the above with certainty. A separately lodged self assessed clearance declaration can be made if you wish to provide more information in relation to the goods for Customs or Quarantine consideration.";

			SACDialogBizo bizo = new SACDialogBizo(hAWB);
			AssertEquals("SAC Questions", expectedResult, bizo.SACQuestion);
		}

		public void TestValueForHAWB()
		{
			hAWB.CS_GoodsValue = 249;
			SACDialogBizo bizo = new SACDialogBizo(hAWB);
			string expectedResult = "Under the screen value of $250";
			AssertEquals("Value", expectedResult, bizo.Value);

			hAWB.CS_GoodsValue = 300;
			bizo = new SACDialogBizo(hAWB);
			expectedResult = "Over the screen value of $250";
			AssertEquals("Value", expectedResult, bizo.Value);
		}

		public void TestThesaurusForHAWB()
		{
			CMRReferenceFilesTestHelper.InsertThesaurusData(Factory, "Cigar");
			hAWB.CS_GoodsDescription = "Description";
			SACDialogBizo bizo = new SACDialogBizo(hAWB);
			string expectedResult = "No words found.";
			AssertEquals("Thesaurus", expectedResult, bizo.Thesaurus);

			hAWB.CS_GoodsDescription = "Word at the end is CIGAR";
			bizo = new SACDialogBizo(hAWB);
			expectedResult = "Cigar";
			AssertEquals("Thesaurus", expectedResult, bizo.Thesaurus);
		}

		public void TestSetSACFlagForHAWB()
		{
			SACDialogBizo bizo = new SACDialogBizo(hAWB);
			bizo.SetSACFlag(true);
			AssertEquals("Is SAC", true, hAWB.CS_IsSelfAssessedClearance);

			bizo.SetSACFlag(false);
			AssertEquals("Is SAC", false, hAWB.CS_IsSelfAssessedClearance);
		}

		public void TestValueForSCAPivot()
		{
			SACDialogBizo bizo = new SACDialogBizo(sCAPivot);
			string expectedResult = "Under the screen value of $250";
			AssertEquals("Value", expectedResult, bizo.Value);
		}

		public void TestThesaurusForSCAPivot()
		{
			CMRReferenceFilesTestHelper.InsertThesaurusData(Factory, "Cigar");
			sCAPivot.CV_GoodsDescription = "Description";
			SACDialogBizo bizo = new SACDialogBizo(sCAPivot);
			string expectedResult = "No words found.";
			AssertEquals("Thesaurus", expectedResult, bizo.Thesaurus);

			sCAPivot.CV_GoodsDescription = "Word at the end is CIGAR";
			bizo = new SACDialogBizo(sCAPivot);
			expectedResult = "Cigar";
			AssertEquals("Thesaurus", expectedResult, bizo.Thesaurus);
		}

		public void TestSetSACFlagForSCAPivot()
		{
			SACDialogBizo bizo = new SACDialogBizo(sCAPivot);
			bizo.SetSACFlag(true);
			AssertEquals("Is SAC", true, sCAPivot.CV_IsSAC);

			bizo.SetSACFlag(false);
			AssertEquals("Is SAC", false, sCAPivot.CV_IsSAC);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			hAWB.CS_GoodsValue = 350M;
			hAWB.CS_GoodsDescription = "Description";
			return new SACDialogBizo(hAWB);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TaxOrFeeTestHelper.SetUp();
			TaxOrFeeTestHelper.SetDeminimus(Factory, 250m);
			hAWB = Factory.New<CusHAWB>();

			var oceanBill = Factory.New<CusSCAOceanBill>();
			var container = oceanBill.Containers.AddNew();
			sCAPivot = container.Pivots.AddNew();
		}

		CusHAWB hAWB;
		CusSCAPivot sCAPivot;
	}
}
