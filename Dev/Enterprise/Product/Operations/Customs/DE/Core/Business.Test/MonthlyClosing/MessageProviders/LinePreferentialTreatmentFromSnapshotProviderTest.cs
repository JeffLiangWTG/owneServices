using System;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	class LinePreferentialTreatmentFromSnapshotProviderTest : Customs.Business.Testing.DataProviderTestCase<LinePreferentialTreatmentFromSnapshotProvider>
	{
		public void TestNew()
		{
			AssertNull(LinePreferentialTreatmentFromSnapshotProvider.NewOrNull(null));
		}

		public void TestRequestedPreferentialTreatment()
		{
			AssertEquals("200", dataProvider.RequestedPreferentialTreatment);
		}

		public void TestContingentNumber()
		{
			AssertContainsExactElementsInExactOrder(new string[] { "ONE", "TWO" }, dataProvider.ContingentNumber);
		}

		public void TestContingentNumber_Null()
		{
			preferentialTreatment.Declaration.Contingent = null;
			AssertEquals(0, dataProvider.ContingentNumber.Count);
		}

		public void TestQuantity()
		{
			CombineAssertions(() =>
			{
				AssertEquals(12345.78m, dataProvider.Quantity.Quantity);
				AssertEquals("KGM", dataProvider.Quantity.MeasurementUnit);
				AssertEquals(String.Empty, dataProvider.Quantity.Qualifier);
			});
		}

		public void TestQuantity_Null()
		{
			preferentialTreatment.Declaration.PreferentialTreatmentQuantity = null;
			AssertNull(dataProvider.Quantity);
		}

		protected override void SetUp()
		{
			base.SetUp();

			preferentialTreatment = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatment()
			{
				RequestedPreferentialTreatment = "200",
				Declaration = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclaration()
				{
					PreferentialTreatmentQuantity = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationPreferentialTreatmentQuantity { Quantity = 12345.78m, MeasurementUnit = "KGM" },
					Contingent = new string[] { "ONE", "TWO" }.Select(s => new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingent() { ContingentNumber = s }).ToArray()
				},
			};
			dataProvider = LinePreferentialTreatmentFromSnapshotProvider.NewOrNull(preferentialTreatment);
		}

		ILinePreferentialTreatment dataProvider;
		DEMonthlyClosingEntryLineSnapshotPreferentialTreatment preferentialTreatment;

		protected override LinePreferentialTreatmentFromSnapshotProvider GetProvider() => (LinePreferentialTreatmentFromSnapshotProvider)dataProvider;
	}
}
