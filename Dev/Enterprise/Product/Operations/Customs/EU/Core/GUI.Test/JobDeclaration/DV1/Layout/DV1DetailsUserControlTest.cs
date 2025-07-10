using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class DV1DetailsUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new DV1DetailsUserControl())
			{
				CombineAssertions(() =>
				{
					AssertNotNull("Buyer / Seller Relationship", control.FindSingle<ZDropEdit>("RelationshipDropEdit"));
					AssertNotNull("Price influenced?", control.FindSingle<ZDropEdit>("PriceInfluenceDropEdit"));
					AssertNotNull("Price influenced Details", control.FindSingle<ZTextBox>("RelationDetailsTextBox"));
					AssertNotNull("Restrictions", control.FindSingle<ZDropEdit>("RestrictionsDropEdit"));
					AssertNotNull("Restrictions Details", control.FindSingle<ZTextBox>("RestrictionsConsiderationTextBox"));
					AssertNotNull("Condition", control.FindSingle<ZDropEdit>("ConsiderationDropEdit"));
					AssertNotNull("Licence Fees", control.FindSingle<ZDropEdit>("RoyalitiesLicenceDropEdit"));
					AssertNotNull("Licence Fees Details", control.FindSingle<ZTextBox>("RoyalitiesLicenceDetailsTextBox"));
					AssertNotNull("Resale", control.FindSingle<ZDropEdit>("ResaleDropEdit"));
					AssertNotNull("Resale Details", control.FindSingle<ZTextBox>("ResaleDetailsTextBox"));
					AssertNotNull("Former Decisions", control.FindSingle<ZTextBox>("CustomsDecisionNumberTextBox"));
					AssertNotNull("Contract Number", control.FindSingle<ZTextBox>("ContractNumberTextBox"));
					AssertNotNull("Contract Date ", control.FindSingle<ZDateEdit>("ContractDateDateEdit"));
				});
			}
		}

		public void TestPriceInfluenceDropEdit()
		{
			AssertEquals(CharacterCasing.Upper, control.PriceInfluenceDropEdit.CharacterCasing);
		}

		public void TestRelationshipDropEdit()
		{
			AssertEquals(CharacterCasing.Upper, control.RelationshipDropEdit.CharacterCasing);
		}

		public void TestRelationDetailsTextBox()
		{
			AssertEquals(CharacterCasing.Normal, control.RelationDetailsTextBox.CharacterCasing);
		}

		public void TestConsiderationDropEdit()
		{
			AssertEquals(CharacterCasing.Upper, control.ConsiderationDropEdit.CharacterCasing);
		}
		public void TestRestrictionsDropEdit()
		{
			AssertEquals(CharacterCasing.Upper, control.RestrictionsDropEdit.CharacterCasing);
		}

		public void TestRestrictionsConsiderationTextBox()
		{
			AssertEquals(CharacterCasing.Normal, control.RestrictionsConsiderationTextBox.CharacterCasing);
		}

		public void TestRoyalitiesLicenceDropEdit()
		{
			AssertEquals(CharacterCasing.Upper, control.RoyalitiesLicenceDropEdit.CharacterCasing);
		}

		public void TestRoyalitiesLicenceDetailsTextBox()
		{
			AssertEquals(CharacterCasing.Normal, control.RoyalitiesLicenceDetailsTextBox.CharacterCasing);
		}

		public void TestResaleDropEdit()
		{
			AssertEquals(CharacterCasing.Upper, control.ResaleDropEdit.CharacterCasing);
		}

		public void TestResaleDetailsTextBox()
		{
			AssertEquals(CharacterCasing.Normal, control.ResaleDetailsTextBox.CharacterCasing);
		}

		public void TestCustomsDecisionNumberTextBox()
		{
			AssertEquals(CharacterCasing.Normal, control.CustomsDecisionNumberTextBox.CharacterCasing);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new DV1DetailsUserControl();
		}
		DV1DetailsUserControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control?.Dispose();
		}
	}
}
