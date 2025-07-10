using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class SupportingDocumentUserControlTest : TestCase
{
	public void TestCaptionRenderingEnabled()
	{
		using (var userControl = new SupportingDocumentUserControl())
		{
			AssertEquals("CaptionRenderingEnabled", true, userControl.CaptionRenderingEnabled);
		}
	}

	public void TestBindingSourceDataSourceType()
	{
		using (var userControl = new SupportingDocumentUserControl())
		{
			AssertEquals("BindingSource DataSourceType", typeof(NctsSupportingDocument), userControl.BindingSource.DataSourceType);
		}
	}

	public void TestComplementTextBox()
	{
		using (var userControl = new SupportingDocumentUserControl())
		{
			var yearOfIssueTextBox = userControl.YearOfIssueTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", yearOfIssueTextBox);
				AssertEquals("BindTo", "CSI_YearOfIssue", yearOfIssueTextBox.BindTo);
			});
		}
	}
}
