using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class EntryInstructionDV1DetailsUserControlTest : TestCaseWithFactory
	{
		public void TestDetailsFieldsAreLinedCorrectly()
		{
			using (var control = new EntryInstructionDV1DetailsUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("relationDetailsTextBox muss be in same Y Location as priceInfluenceTextBox", control.FindSingle<ZTextBox>("priceInfluenceTextBox").Location.Y, control.FindSingle<ZTextBox>("relationDetailsTextBox").Location.Y);
					AssertEquals("restrictionsConsiderationTextBox muss be in same Y Location as considerationTextBox", control.FindSingle<ZTextBox>("considerationTextBox").Location.Y, control.FindSingle<ZTextBox>("restrictionsConsiderationTextBox").Location.Y);
					AssertEquals("royalitiesLicenceDetailsTextBox muss be in same Y Location as royalitiesLicenceTextBox", control.FindSingle<ZTextBox>("royalitiesTextBox").Location.Y, control.FindSingle<ZTextBox>("royalitiesLicenceDetailsTextBox").Location.Y);
					AssertEquals("resaleDetailsTextBox muss be in same Y Location as resaleTextBox", control.FindSingle<ZTextBox>("resaleTextBox").Location.Y, control.FindSingle<ZTextBox>("resaleDetailsTextBox").Location.Y);
				});
			}
		}

		public void TestDV1DetailsGridColumnSize()
		{
			using (var control = new EntryInstructionDV1DetailsUserControl())
			{
				var detailsGrid = control.FindSingleOrDefault<ZGrid>("DetailsGrid");

				CombineAssertions(() =>
				{
					AssertEquals("IsForEntryInstruction", 120, detailsGrid.GetColumnWidth(AutoNonPersistentCusDV1DetailPivot.Schema.IsForEntryInstruction));
					AssertEquals("Relatonship", 80, detailsGrid.GetColumnWidth(AutoNonPersistentCusDV1DetailPivot.Schema.Relationship));
					AssertEquals("RelationDetails", 100, detailsGrid.GetColumnWidth(AutoNonPersistentCusDV1DetailPivot.Schema.RelationDetails));
					AssertEquals("PriceInfluence", 100, detailsGrid.GetColumnWidth(AutoNonPersistentCusDV1DetailPivot.Schema.PriceInfluence));
					AssertEquals("Restrictions", 80, detailsGrid.GetColumnWidth(AutoNonPersistentCusDV1DetailPivot.Schema.Restrictions));
					AssertEquals("Consideration", 80, detailsGrid.GetColumnWidth(AutoNonPersistentCusDV1DetailPivot.Schema.Consideration));
					AssertEquals("RestrictionConsiderationDetails", 100, detailsGrid.GetColumnWidth(AutoNonPersistentCusDV1DetailPivot.Schema.RestrictionConsiderationDetails));
					AssertEquals("RoyaltiesLicence", 80, detailsGrid.GetColumnWidth(AutoNonPersistentCusDV1DetailPivot.Schema.RoyaltiesLicence));
					AssertEquals("RoyaltiesLicenceDetails", 100, detailsGrid.GetColumnWidth(AutoNonPersistentCusDV1DetailPivot.Schema.RoyaltiesLicenceDetails));
					AssertEquals("Resale", 80, detailsGrid.GetColumnWidth(AutoNonPersistentCusDV1DetailPivot.Schema.Resale));
					AssertEquals("ResaleDetails", 100, detailsGrid.GetColumnWidth(AutoNonPersistentCusDV1DetailPivot.Schema.ResaleDetails));
					AssertEquals("CustomsDecisionNumber", 120, detailsGrid.GetColumnWidth(AutoNonPersistentCusDV1DetailPivot.Schema.CustomsDecisionNumber));
				});
			}
		}
	}
}
