using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.BE.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.NCTS.GUI.Testing;

sealed class PreviousDocumentN785ReferenceUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		AssertEquals(typeof(CommonPreviousDocument), control.BindingSource.DataSourceType);
	}

	public void TestPart1TextBox()
	{
		CombineAssertions(() =>
		{
			AssertType<ZTextBox>("Control Type", control.Part1TextBox);
			AssertEquals("BindTo", nameof(CommonPreviousDocument.CSI_ReferenceNumberN785Pos1), control.Part1TextBox.BindTo);
		});
	}

	public void TestPart2TextBox()
	{
		CombineAssertions(() =>
		{
			AssertType<ZTextBox>("Control Type", control.Part2TextBox);
			AssertEquals("BindTo", nameof(CommonPreviousDocument.CSI_ReferenceNumberN785Pos2To7), control.Part2TextBox.BindTo);
		});
	}

	public void TestPart3TextBox()
	{
		CombineAssertions(() =>
		{
			AssertType<ZTextBox>("Control Type", control.Part3TextBox);
			AssertEquals("BindTo", nameof(CommonPreviousDocument.CSI_ReferenceNumberN785Pos8), control.Part3TextBox.BindTo);
		});
	}

	public void TestPart4TextBox()
	{
		CombineAssertions(() =>
		{
			AssertType<ZTextBox>("Control Type", control.Part4TextBox);
			AssertEquals("BindTo", nameof(CommonPreviousDocument.CSI_ReferenceNumberN785Pos9To15), control.Part4TextBox.BindTo);
		});
	}

	public void TestPart5TextBox()
	{
		CombineAssertions(() =>
		{
			AssertType<ZTextBox>("Control Type", control.Part5TextBox);
			AssertEquals("BindTo", nameof(CommonPreviousDocument.CSI_ReferenceNumberN785Pos16), control.Part5TextBox.BindTo);
		});
	}

	public void TestPart6TextBox()
	{
		CombineAssertions(() =>
		{
			AssertType<ZTextBox>("Control Type", control.Part6TextBox);
			AssertEquals("BindTo", nameof(CommonPreviousDocument.CSI_ReferenceNumberN785Pos17To20), control.Part6TextBox.BindTo);
		});
	}

	public void TestIExtendedControl()
	{
		CombineAssertions(() =>
		{
			var extendedControlSupporter = (IExtendedControl)control;
			AssertSame("Host", control, extendedControlSupporter.Host);
			AssertType<DefaultControlExtensionCollection>("Extensions", extendedControlSupporter.Extensions);
		});
	}

	public void TestIResourceStringBindingMember()
	{
		var resourceStringBindingMemberSupporter = (IResourceStringBindingMember)control;
		AssertEquals("ResourceStringBindingMember", "CSI_ReferenceNumber", resourceStringBindingMemberSupporter.ResourceStringBindingMember);
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new PreviousDocumentN785ReferenceUserControl();
	}

	PreviousDocumentN785ReferenceUserControl control;

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
}
