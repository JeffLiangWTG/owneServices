using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing;

sealed class AdditionalInformationDetailsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		using (var control = new AdditionalInformationDetailsUserControl())
		{
			AssertEquals(typeof(AdditionalInfo), control.BindingSource.DataSourceType);
		}
	}

	public void TestNKCountryCodeFindBox()
	{
		AssertType<ZCodeFindBox>(control.NKCountryCodeFindBox);
	}

	public void TestFieldsProperties()
	{
		AssertEquals("NKCountryCodeFindBox: CharacterCasing", CharacterCasing.Upper, control.FindSingleOrDefault<ZCodeFindBox>("NKCountryCodeFindBox").CodeBox.CharacterCasing);
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new AdditionalInformationDetailsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	AdditionalInformationDetailsUserControl control;
}
