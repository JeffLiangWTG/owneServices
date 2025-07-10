using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class ShipmentTypeUserControlTest : TestCase
{
	public void TestCaptionRenderingEnabled()
	{
		AssertEquals("CaptionRenderingEnabled", true, control.CaptionRenderingEnabled);
	}

	public void TestBindingSourceDataSourceType()
	{
		AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
	}

	public void TestAuthorisationNumberDropEdit()
	{
		AssertType<ZDropEdit>(control.AuthorisationNumberDropEdit);
		AssertEquals("BindTo", "ZG_AuthorisationNumber", control.AuthorisationNumberDropEdit.BindTo);
		AssertEquals("Caption", "Authorization", control.AuthorisationNumberDropEdit.CaptionResourceString.Caption);
	}

	public void TestMessageVersionDropEdit()
	{
		AssertType<ZDropEdit>(control.MessageVersionDropEdit);
		AssertEquals("BindTo", "MessageVersion", control.MessageVersionDropEdit.BindTo);
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new ShipmentTypeUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	ShipmentTypeUserControl control;
}
