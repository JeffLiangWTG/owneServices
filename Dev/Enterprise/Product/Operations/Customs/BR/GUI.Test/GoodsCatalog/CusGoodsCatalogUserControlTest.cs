using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	sealed class CusGoodsCatalogUserControlTest : TestCaseWithFactory
	{
		public void TestComponents()
		{
			using (var control = new CusGoodsCatalogUserControl())
			{
				var complementaryDescriptionTextBox = control.ComplementaryDescriptionTextBox;
				AssertType<Customs.GUI.LongTextControl>("complementaryDescriptionTextBox should be a LongTextControl", complementaryDescriptionTextBox);
				AssertEquals("ComplementaryDescription", control.BindingSource.GetBindingMember(complementaryDescriptionTextBox));

				var messageStatusDropEdit = control.MessageStatusDropEdit;
				AssertType<ZDropEdit>("messageStatusDropEdit should be a ZDropEdit", messageStatusDropEdit);
				Assert("messageStatusDropEdit should be visible", messageStatusDropEdit.Visible);

				var statusDropEdit = control.StatusDropEdit;
				AssertType<ZDropEdit>("statusTextBox should be a ZDropEdit", statusDropEdit);
				Assert("statusDropEdit should be visible", statusDropEdit.Visible);

				var authorityInfoGroupBox = control.AuthorityInfoGroupBox;
				AssertType<ZGroupBox>("authorityInfoGroupBox should be a ZGroupBox", authorityInfoGroupBox);
			}
		}

		public void TestLocalPartNumbers()
		{
			using (var control = new CusGoodsCatalogUserControl())
			{
				var localPartNumberGroupBox = control.LocalPartNumbersGroupBox;
				AssertType<ZGroupBox>("localPartNumberGroupBox should be a ZGroupBox", localPartNumberGroupBox);

				var localPartNumberGrid = control.LocalPartNumbersGrid;
				AssertType<ZGrid>("localPartNumberGrid should be a ZGrid", localPartNumberGrid);

				var localPartNumbersColumn = control.LocalPartNumbersGrid.GetColumnStyle("CGI_Reference");
				Assert("CGI_Reference column is visible", localPartNumbersColumn.IsVisible);

				AssertEquals("CharacterCasing should be UPPER ", System.Windows.Forms.CharacterCasing.Upper, localPartNumbersColumn.CharacterCasing);
			}
		}

		public void TestAttributesUserControl()
		{
			using (var control = new CusGoodsCatalogUserControl())
			{
				var attributesUserControl = control.AttributesUserControl;
				AssertType<AttributesUserControl>("AttributesUserControl should be a AttributesUserControl", attributesUserControl);
			}
		}
	}
}

