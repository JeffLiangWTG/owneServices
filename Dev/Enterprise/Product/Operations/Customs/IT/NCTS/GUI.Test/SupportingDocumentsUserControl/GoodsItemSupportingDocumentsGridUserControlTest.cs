using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.IT.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class GoodsItemSupportingDocumentsGridUserControlTest : TestCase
{
	public void TestCaptionRenderingEnabled()
	{
		using (var userControl = new GoodsItemSupportingDocumentsGridUserControl())
		{
			AssertEquals("CaptionRenderingEnabled", true, userControl.CaptionRenderingEnabled);
		}
	}

	public void TestBindingSourceDataSourceType()
	{
		using (var userControl = new GoodsItemSupportingDocumentsGridUserControl())
		{
			AssertEquals("BindingSource DataSourceType", typeof(EU.NCTS.Business.NctsSupportingDocumentCollection<NctsSupportingDocument>), userControl.BindingSource.DataSourceType);
		}
	}

	public void TestSupportingDocumentsGridBindingMember()
	{
		using (var userControl = new GoodsItemSupportingDocumentsGridUserControl())
		{
			AssertEquals("SupportingDocumentsGrid BindingMember", ".", userControl.SupportingDocumentsGrid.GetBindingMember());
		}
	}

	public void TestSupportingDocumentsGridAvailableColumns()
	{
		using (var userControl = new GoodsItemSupportingDocumentsGridUserControl())
		{
			var availableColumnNames = userControl.SupportingDocumentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName);
			AssertSequencesEqual("Columns", new[] { "CSI_Code", "CSI_ReferenceNumber", "CSI_YearOfIssue", "CSI_RN_NKCountryCode", "CSI_ItemNumber", "CSI_ReferenceNumber2" }, availableColumnNames);
		}
	}
}
