using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class SupportingDocumentUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(SupportingDocument), control.BindingSource.DataSourceType);
		}

		public void TestStatusLabel()
		{
			var statusLabel = control.StatusLabel;
			CombineAssertions(() =>
			{
				AssertType<ZLabel>("Type", statusLabel);
				AssertEquals("Caption", "New Value", statusLabel.CaptionResourceString.Caption);
			});
		}

		public void TestLineNoCalcEdit()
		{
			var lineNumberCalcEdit = control.LineNoCalcEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcEdit>("Type", lineNumberCalcEdit);
				AssertEquals("BindTo", nameof(PreviousDocument.CSI_LineNo), lineNumberCalcEdit.BindTo);
			});
		}

		public void TestTypeFindBox()
		{
			var typeCodeFindBox = control.TypeCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Type", typeCodeFindBox);
				AssertEquals("BindTo", nameof(SupportingDocument.CSI_Code), typeCodeFindBox.BindTo);
			});
		}

		public void TestReferenceNumberTextBox()
		{
			var referenceNumberTextBox = control.ReferenceNumberTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", referenceNumberTextBox);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, referenceNumberTextBox.CharacterCasing);
				AssertEquals("BindTo", nameof(SupportingDocument.CSI_ReferenceNumber), referenceNumberTextBox.BindTo);
			});
		}

		[RequiresSTA]
		public void TestItemNumberCalcEdit()
		{
			var itemNumberCalcEdit = control.ItemNumberCalcEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcEdit>("Type", itemNumberCalcEdit);
				AssertEquals("BindTo", nameof(SupportingDocument.CSI_ItemNumber), itemNumberCalcEdit.BindTo);
				AssertEquals("DecimalPlaces", 0, itemNumberCalcEdit.DecimalPlaces);
				AssertEquals("Decimals", 0, itemNumberCalcEdit.Decimals);
			});
		}

		public void TestComplementTextBox()
		{
			var complementTextBox = control.ComplementTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", complementTextBox);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, complementTextBox.CharacterCasing);
				AssertEquals("BindTo", nameof(SupportingDocument.CSI_ReferenceNumber2), complementTextBox.BindTo);
			});
		}

		[RequiresSTA]
		public void TestCountryCodeFindBox()
		{
			var countryCodeFindBox = control.CountryCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Type", countryCodeFindBox);
				AssertEquals("BindTo", nameof(SupportingDocument.CSI_RN_NKCountryCode), countryCodeFindBox.BindTo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new SupportingDocumentUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		SupportingDocumentUserControl control;
	}
}
