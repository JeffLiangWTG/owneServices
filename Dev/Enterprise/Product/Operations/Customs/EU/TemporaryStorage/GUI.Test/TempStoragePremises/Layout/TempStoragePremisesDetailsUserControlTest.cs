using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	sealed class TempStoragePremisesDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestCodeTextBox()
		{
			var codeTextBox = control.CodeTextBox;
			AssertNotNull("CodeTextBox", codeTextBox);
			AssertEquals("CodeTextBox BindTo", "SRP_Code", codeTextBox.BindTo);
		}

		public void TestDescriptionTextBox()
		{
			var descriptionTextBox = control.DescriptionTextBox;
			AssertNotNull("DescriptionTextBox", descriptionTextBox);
			AssertEquals("DescriptionTextBox BindTo", "SRP_Description", descriptionTextBox.BindTo);
		}

		public void TestTypeDropEdit()
		{
			var typeDropEdit = control.TypeDropEdit;
			AssertNotNull("TypeDropEdit", typeDropEdit);
			AssertEquals("TypeDropEdit BindTo", "SRP_Type", typeDropEdit.BindTo);
		}

		public void TestAuthorizationNumberCodeFindBox()
		{
			var authorizationNumberCodeFindBox = control.AuthorizationNumberCodeFindBox;
			AssertNotNull("AuthorizationNumberCodeFindBox", authorizationNumberCodeFindBox);
			AssertEquals("AuthorizationNumberCodeFindBox BindTo", "AuthorizationNumber", authorizationNumberCodeFindBox.BindTo);
		}

		public void TestAuthorizationOwnerGuidFindBox()
		{
			var authorizationOwnerGuidFindBox = control.AuthorizationOwnerGuidFindBox;
			AssertNotNull("AuthorizationOwnerGuidFindBox", authorizationOwnerGuidFindBox);
			AssertEquals("AuthorizationOwnerGuidFindBox BindTo", "AuthorizationOwner", authorizationOwnerGuidFindBox.BindTo);
		}

		public void TestPremisesAddressAddressControl()
		{
			var premisesAddressAddressControl = control.PremisesAddressAddressControl;
			AssertNotNull("PremisesAddressAddressControl", premisesAddressAddressControl);
			AssertEquals("PremisesAddressAddressControl BindTo", "SRP_OA_PremisesAddress", premisesAddressAddressControl.BindTo);
		}

		public void TestCustomsLocationCodeFindBox()
		{
			var customsLocationCodeFindBox = control.CustomsLocationCodeFindBox;
			AssertNotNull("CustomsLocationCodeFindBox", customsLocationCodeFindBox);
			AssertEquals("CustomsLocationCodeFindBox BindTo", "SRP_CustomsLocation", customsLocationCodeFindBox.BindTo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TempStoragePremisesDetailsUserControl();
		}
		TempStoragePremisesDetailsUserControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
