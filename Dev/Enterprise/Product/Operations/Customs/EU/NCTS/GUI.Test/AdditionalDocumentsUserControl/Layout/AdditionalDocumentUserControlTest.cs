using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class AdditionalDocumentUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsAdditionalInfo), control.BindingSource.DataSourceType);
		}

		public void TestStatusLabel()
		{
			var statusLabel = control.StatusLabel;
			AssertType<ZLabel>("Type", statusLabel);
		}

		public void TestLineNoCalcEdit()
		{
			var lineNumberCalcEdit = control.LineNoCalcEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcEdit>("Type", lineNumberCalcEdit);
				AssertEquals("BindTo", nameof(NctsAdditionalInfo.CSI_LineNo), lineNumberCalcEdit.BindTo);
			});
		}

		public void TestKindDropEdit()
		{
			var kindDropEdit = control.KindDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", kindDropEdit);
				AssertEquals("BindTo", nameof(NctsAdditionalInfo.CSI_SubType), kindDropEdit.BindTo);
			});
		}

		public void TestTypeFindBox()
		{
			var typeCodeFindBox = control.TypeCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Type", typeCodeFindBox);
				AssertEquals("BindTo", nameof(NctsAdditionalInfo.CSI_Code), typeCodeFindBox.BindTo);
			});
		}

		public void TestReferenceNumberTextBox()
		{
			var referenceNumberTextBox = control.ReferenceNumberTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", referenceNumberTextBox);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, referenceNumberTextBox.CharacterCasing);
				AssertEquals("BindTo", nameof(NctsAdditionalInfo.CSI_ReferenceNumber), referenceNumberTextBox.BindTo);
			});
		}

		public void TestDescriptionTextBox()
		{
			var descriptionTextBox = control.DescriptionTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", descriptionTextBox);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, descriptionTextBox.CharacterCasing);
				AssertEquals("BindTo", nameof(NctsAdditionalInfo.CSI_Description), descriptionTextBox.BindTo);
			});
		}

		public void TestDescriptionMultiLineTextBox()
		{
			var descriptionMultilineTextBox = control.DescriptionMultilineTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", descriptionMultilineTextBox);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, descriptionMultilineTextBox.CharacterCasing);
				AssertEquals("BindTo", nameof(NctsAdditionalInfo.CSI_Description), descriptionMultilineTextBox.BindTo);
				AssertEquals("Multiline", true, descriptionMultilineTextBox.Multiline);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new AdditionalDocumentUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		AdditionalDocumentUserControl control;
	}
}
