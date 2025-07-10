using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class Ucc6ExportInvLineAddInfoUserControlWithGridTest : TestCaseWithFactory
{
	public void TestColumnOrder()
	{
		using (var form = new ZForm(declaration))
		using (var control = CreateControl())
		{
			form.Controls.Add(control);
			form.Show();

			var grid = control.FindSingle<ZGrid>("AdditionalInfosGrid");
			AssertNotNull("AdditionalInfosGrid", grid);

			var columns = grid.DefaultColumns.Select(c => c.ColumnName).ToArray();
			AssertSequencesEqual("Columns", gridColumns, columns);
		}
	}

	public void TestAdditionalInfosGroupBoxCaption()
	{
		using (var form = new ZForm(declaration))
		using (var control = CreateControl())
		{
			form.Controls.Add(control);
			form.Show();

			var groupBox = control.FindSingle<ZGroupBox>("AdditionalInfosGroupBox");
			AssertEquals("AdditionalInfosGroupBox.Caption", "Additional Documents", groupBox.CaptionResourceString.Caption);
		}
	}

	public void TestCreateNewInvoiceHeaderAdditionalInformationDetailsLayout()
	{
		using (var control = CreateControl())
		{
			AssertType<Ucc6ExportInvLineAddInfoDetailsLayoutProvider>(control.CreateNewInvoiceHeaderAdditionalInformationDetailsLayoutExposed());
		}
	}

	public void TestCreateNewInvoiceLineAdditionalInformationDetailsLayout()
	{
		using (var control = CreateControl())
		{
			AssertType<Ucc6ExportInvLineAddInfoDetailsLayoutProvider>(control.CreateNewInvoiceLineAdditionalInformationDetailsLayoutExposed());
		}
	}

	public void TestCreateNewExitSummaryAdditionalInformationDetailsLayout()
	{
		using (var control = CreateControl())
		{
			AssertType<Ucc6ExportInvLineAddInfoDetailsLayoutProvider>(control.CreateNewExitSummaryAdditionalInformationDetailsLayoutExposed());
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}

	JobDeclaration declaration;

	Ucc6ExportInvLineAddInfoUserControlWithGridForTest CreateControl() => new Ucc6ExportInvLineAddInfoUserControlWithGridForTest();

	string[] gridColumns => new[]
	{
		CusSupportingInfo.Schema.CSI_SubType,
		CusSupportingInfo.Schema.CSI_Code,
		CusSupportingInfo.Schema.CSI_ReferenceNumber,
		CusSupportingInfo.Schema.CSI_Description,
	};
}

sealed class Ucc6ExportInvLineAddInfoUserControlWithGridForTest : Ucc6ExportInvLineAddInfoUserControlWithGrid
{
	public IPanelLayoutProvider CreateNewInvoiceHeaderAdditionalInformationDetailsLayoutExposed() => base.CreateNewInvoiceHeaderAdditionalInformationDetailsLayout();
	public IPanelLayoutProvider CreateNewInvoiceLineAdditionalInformationDetailsLayoutExposed() => base.CreateNewInvoiceLineAdditionalInformationDetailsLayout();
	public IPanelLayoutProvider CreateNewExitSummaryAdditionalInformationDetailsLayoutExposed() => base.CreateNewExitSummaryAdditionalInformationDetailsLayout();
}
