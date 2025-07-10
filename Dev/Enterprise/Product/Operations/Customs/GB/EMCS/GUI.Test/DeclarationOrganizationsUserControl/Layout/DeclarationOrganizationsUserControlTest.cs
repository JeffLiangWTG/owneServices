using System.Windows.Forms;
using Enterprise.Customs.GB.EMCS.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.GUI.Testing
{
	sealed class DeclarationOrganizationsUserControlTest : TestCase
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(EMCSJobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestCertificateIdentifierDropEdit()
		{
			CombineAssertions(() =>
			{
				var certificateIdentifierDropEdit = control.CustomsProfileDropEdit;
				AssertEquals("CharacterCasing", CharacterCasing.Normal, certificateIdentifierDropEdit.CharacterCasing);
				AssertEquals("ShowDescriptionBox", false, certificateIdentifierDropEdit.ShowDescriptionBox);
				AssertEquals("ShowInDropDown", ZDropEdit.ShowInDropDownList.ShowCodeAndDescription, certificateIdentifierDropEdit.ShowInDropDown);
				AssertType<ZDropEdit>("Control", certificateIdentifierDropEdit);
			});
		}

		public void TestCertificateIdentifierGroupBox()
		{
			CombineAssertions(() =>
			{
				var certificateIdentifierGroupBox = control.CertificateIdentifierGroupBox;
				Assert("GroupBox contains CertificateIdentifierDropEdit", certificateIdentifierGroupBox.Controls.Contains(control.CustomsProfileDropEdit));
				AssertType<ZGroupBox>("Control", certificateIdentifierGroupBox);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new DeclarationOrganizationUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		DeclarationOrganizationUserControl control;
	}
}
