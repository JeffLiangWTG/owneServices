using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.BE.GUI.PlugIn;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI.Testing;

class InvoiceLineAdditionalInfosUserControlWithGridTest : TestCaseWithFactory
{
	public void TestGridColumnNames_InvoiceLine()
	{
		using (var form = new ZForm(Factory.New<JobDeclaration>()))
		using (var control = new InvoiceLineAdditionalInfosUserControlWithGridForTest())
		{
			form.Controls.Add(control);
			form.Show();
			var grid = control.FindSingle<ZGrid>("AdditionalInfosGrid");

			AssertArrayEqualsByElements(new[]
			{
				CusSupportingInfo.Schema.CSI_SubType,
				CusSupportingInfo.Schema.CSI_Code,
				CusSupportingInfo.Schema.CSI_ReferenceNumber,
				CusSupportingInfo.Schema.CSI_Description
			}, grid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName).ToArray());
		}
	}

	public void TestCreateNewInvoiceLineAdditionalInformationDetailsLayout()
	{
		using (var control = new InvoiceLineAdditionalInfosUserControlWithGridForTest())
		{
			AssertType<InvoiceLineAdditionalInformationDetailsLayout>(control.CreateNewInvoiceLineAdditionalInformationDetailsLayoutExposed());
		}
	}

	class InvoiceLineAdditionalInfosUserControlWithGridForTest : InvoiceLineAdditionalInfosUserControlWithGrid
	{
		public IPanelLayoutProvider CreateNewInvoiceLineAdditionalInformationDetailsLayoutExposed() => base.CreateNewInvoiceLineAdditionalInformationDetailsLayout();
	}
}
