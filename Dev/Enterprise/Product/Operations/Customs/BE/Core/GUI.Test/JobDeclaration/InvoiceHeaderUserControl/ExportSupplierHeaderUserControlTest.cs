using System.Collections.Generic;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.BE.GUI.PlugIn;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Testing;

[TestedType(typeof(ExportSupplierHeaderUserControl))]
sealed class ExportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<ExportSupplierHeaderUserControl, JobDeclaration>
{
	public void TestPreviousDocumentsTabPageCaptionAndUserControlType()
	{
		EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<ExportSupplierHeaderUserControl>(Factory.New<JobDeclaration>(), "PreviousDocumentsTabPage", "previousDocumentsUserControl1", "[40] Previous Docs", typeof(SupplierHeaderPreviousDocumentsUserControl));
	}

	public void TestAdditionalInfoTabPageCaptionAndUserControlType()
	{
		EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<ExportSupplierHeaderUserControl>(Factory.New<JobDeclaration>(), "AdditionalInfoTabPage", "additionalInfosUserControl1", "[44] Additional Documents", typeof(AdditionalInfosUserControlWithGrid));
	}

	public void TestSupportingDocumentsTabPageCaptionAndUserControlType()
	{
		EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<ExportSupplierHeaderUserControl>(Factory.New<JobDeclaration>(), "SupportingDocumentsTabPage", "SupportingDocumentsUserControl", "Supporting Documents", typeof(InvoiceHeaderSupportingDocumentsUserControl));
	}

	public void TestColumnsAdded()
	{
		using (var control = new ExportSupplierHeaderUserControl())
		{
			control.InitializeGridLayout();
			control.Show();
			var columns = control.JobComInvoiceHeadersBoundGrid.ColumnStyles.OfType<ZGridColumnInfo>().Select(x => x.ColumnName);
			AssertCollectionContains("JZ_UCR", columns);
		}
	}

	public void TestFieldsAdded()
	{
		var declaration = GetDeclaration();
		using (var form = new ZForm(declaration))
		using (var userControl = new ExportSupplierHeaderUserControl())
		{
			form.Controls.Add(userControl);
			form.Show();

			userControl.FindSingle<ZTabPage>("PreviousDocumentsTabPage").Show();
			var previousDocumentsUserControl1 = userControl.FindSingle<ZDynamicControlCreationUserControl>("previousDocumentsUserControl1");
			CombineAssertions(() =>
			{
				AssertEquals("previousDocumentsUserControl1.UserControlType", typeof(SupplierHeaderPreviousDocumentsUserControl), previousDocumentsUserControl1.UserControlType);
			});
		}
	}

	public void TestAgreedPlaceControlsVisibility()
	{
		var declaration = GetDeclaration();
		declaration.Invoices.AddNew();
		using (var form = new ZForm(declaration))
		{
			using (var userControl = new ExportSupplierHeaderUserControl())
			{
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				var codeFindBox = userControl.FindSingle<ZCodeFindBox>("AgreedPlaceCodeFindBox");
				AssertEquals("AgreedPlaceCodeFindBox visibility", true, codeFindBox.Visible);
			}
		}
	}

	protected override IEnumerable<string> ExpectedControlList => DefaultControlList.Except(new[] { "GroupInvoiceDropEdit" }).Union(new[] { "AgreedPlaceCodeFindBox", "TransportChargesMethodOfPaymentDropEdit" });

	JobDeclaration GetDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.Invoices.AddNew();
		return declaration;
	}
}
