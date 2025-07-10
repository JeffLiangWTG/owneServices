using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CSARSFAssessment))]
	sealed class CSARSFAssessmentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestInstance()
		{
			var charge = line.CustomsAssessmentCharge;
			line.B3_EntryProcessPort = "PORT";
			line.B3_BrokerReference = "BROK";
			charge.B4_ChargeAmount = 23m;
			charge.B4_ChargeType = "CAR";
			var assessment = GetNewBusinessObject() as CSARSFAssessment;
			AssertEquals("PORT", assessment.PortCode);
			AssertEquals("BROK", assessment.ReferenceNumber);
			AssertEquals("CAR", assessment.Type);
			AssertEquals(23m, assessment.Amount);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CSARSFAssessment(line);
		}

		protected override void SetUp()
		{
			base.SetUp();

			rSF = Factory.New<CusStatementHeader>();
			line = rSF.StatementLines.AddNew();
			line.B3_EntryType = CSARSFAssessmentTypes.Codes.CustomsAssessment;
			csaRSFAssessment = new CSARSFAssessment(line);
		}

		#region TEST ICSARSFItem

		public void TestICSARSFItemProperties()
		{
			csaRSFAssessment.Type = CustomsAssessmentsCodes.Codes.B2Dash1;
			csaRSFAssessment.PortCode = "PORT";
			csaRSFAssessment.ReferenceNumber = "REFE";
			csaRSFAssessment.Amount = 12m;

			var item = csaRSFAssessment as ICSARSFItem;

			AssertEquals("REFE", item.LineItemNumber);
			AssertEquals("PORT", item.PortCode);
			AssertEquals("B21", item.Type);
			AssertEquals(12m, item.MonetaryAmount);
		}

		#endregion

		CSARSFAssessment csaRSFAssessment;
		CusStatementHeader rSF;
		CusStatementLine line;
	}
}
