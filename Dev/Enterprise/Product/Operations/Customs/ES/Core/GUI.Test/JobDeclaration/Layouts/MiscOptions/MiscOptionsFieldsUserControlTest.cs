using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing;

sealed class MiscOptionsFieldsUserControlTest : TestCase
{
	public void TestCaptionRenderingEnabled()
	{
		AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
	}

	public void TestOtherEmailAddrTextBox()
	{
		AssertType<ZTextBox>(control.OtherEmailAddrTextBox);
	}

	public void TestDeclEmailAddrTextBox()
	{
		AssertType<ZTextBox>(control.DeclEmailAddrTextBox);
	}

	public void TestAuthPerDeclarationCheckBox()
	{
		AssertType<ZCheckBox>(control.AuthPerDeclarationCheckBox);
	}

	public void TestCertificateDropEdit()
	{
		AssertType<ZDropEdit>(control.CertificateDropEdit);
	}

	public void TestSupportingInformationUserControl()
	{
		AssertType<SupportingInformationControl>(control.SupportingInformationUserControl);
	}

	public void TestDontSendImporterIdCheckBox()
	{
		var dontSendImporterIdCheckBox = control.DontSendImporterIdCheckBox;
		CombineAssertions(() =>
		{
			AssertType<ZCheckBox>(dontSendImporterIdCheckBox);

			var captionResourceString = dontSendImporterIdCheckBox.CaptionResourceString;
			AssertEquals("Caption", "Do not send Importer ID Number", captionResourceString.Caption);
			AssertEquals("MediumCaption", "Do not send Imp. ID Num", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "No Send Imp. ID", captionResourceString.ShortCaption);
			AssertEquals("FullDescription", "If ticked the Importer Identification Number will not be sent.", captionResourceString.FullDescription);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new MiscOptionsFieldsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
	MiscOptionsFieldsUserControl control;
}
