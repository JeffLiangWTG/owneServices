using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CSARSFCustomsAssessmentCollection))]
	sealed class CSARSFCustomsAssessmentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CSARSFCustomsAssessmentCollection>
	{
		public void TestLoadCollection()
		{
			var collection = rSF.CustomsAssessments;
			AssertEquals(1, collection.Count);

			collection[0].Type = "CUS";
			var line2 = rSF.StatementLines.AddNew();
			line2.B3_EntryType = "TST";
			AssertNull(line2.CustomsAssessmentCharge);
			var line3 = rSF.StatementLines.AddNew();
			line3.B3_EntryType = CSARSFAssessmentTypes.Codes.CustomsAssessment;
			line3.CustomsAssessmentCharge.B4_ChargeType = "CUS";

			collection.Load();
			AssertEquals(2, collection.Count);
			foreach (CSARSFAssessment assessment in collection)
			{
				AssertEquals("CUS", assessment.Type);
			}
		}

		public void TestAddNewElement()
		{
			var collection = rSF.CustomsAssessments;
			AssertEquals(1, collection.Count);
			AssertEquals(1, rSF.StatementLines.Count);

			collection.AddNew();
			AssertEquals(2, collection.Count);

			var lines = rSF.StatementLines.Cast<CusStatementLine>().Where(x => x.B3_EntryType == CSARSFAssessmentTypes.Codes.CustomsAssessment);
			AssertEquals(2, lines.Count());
			foreach (CusStatementLine line in lines)
			{
				AssertEquals(CSARSFAssessmentTypes.Codes.CustomsAssessment, line.B3_EntryType);
			}
		}

		protected override CSARSFCustomsAssessmentCollection GetCollectionToTest()
		{
			return new CSARSFCustomsAssessmentCollection(rSF);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CSARSFAssessment(line);
		}

		protected override void SetUp()
		{
			base.SetUp();
			rSF = Factory.New<CusStatementHeader>();
			rSF.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			line = rSF.StatementLines.AddNew();
			line.B3_EntryType = CSARSFAssessmentTypes.Codes.CustomsAssessment;
		}

		CusStatementHeader rSF;
		CusStatementLine line;
	}
}
