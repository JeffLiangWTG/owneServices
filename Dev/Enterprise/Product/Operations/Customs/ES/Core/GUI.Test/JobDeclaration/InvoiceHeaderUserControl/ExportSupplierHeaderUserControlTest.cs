using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(ExportSupplierHeaderUserControl))]
	public class ExportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<ExportSupplierHeaderUserControl, JobDeclaration>
	{
		public void TestSupplierOrgPKColumn()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				var supplierHeaderUserControl = (ExportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				var supplierColumnStyle = supplierHeaderUserControl.JobComInvoiceHeadersBoundGrid.ColumnStyles.Cast<Core.Forms.ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "SupplierOrgPK");
				AssertNull(supplierColumnStyle);
			}
		}

		public void TestBuyerColumn() => CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			using var form = new JobDeclarationForm(declaration);

			form.Show();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
			var supplierHeaderUserControl = (ExportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
			var columns = supplierHeaderUserControl.JobComInvoiceHeadersBoundGrid.ColumnStyles.Cast<Core.Forms.ZGridColumnInfo>();

			var oldBuyerColumnStyle = columns.FirstOrDefault(x => x.ColumnName == "BuyerOrgPK");
			var newBuyerColumnStyle = columns.FirstOrDefault(x => x.ColumnName == "JZ_OH_Buyer");
			var buyerAddressColumnStyle = columns.FirstOrDefault(x => x.ColumnName == "JZ_OA_BuyerAddress");
			AssertNull(oldBuyerColumnStyle);
			AssertNotNull(newBuyerColumnStyle);
			AssertNotNull(buyerAddressColumnStyle);
			AssertEquals("Same buyer group", newBuyerColumnStyle.GroupName, buyerAddressColumnStyle.GroupName);
		});

		public void TestAdditionalInfosTabPage()
		{
			using (var frm = new ZForm(declaration))
			using (var userControl = new ExportSupplierHeaderUserControl())
			{
				frm.Controls.Add(userControl);
				userControl.JobDeclaration = declaration;
				frm.Show();

				var tabPage = userControl.FindSingle<ZTabPage>("AdditionalInfoTabPage");
				tabPage.Show();
				CombineAssertions(() =>
				{
					AssertEquals("AdditionalInfosTabPage visible", true, tabPage.TabVisible);
					AssertEquals("Caption", "Additional Documents", tabPage.CaptionResourceString.Caption);
					var foundUserControl = userControl.FindSingle<ZDynamicControlCreationUserControl>("additionalInfosUserControl1");
					AssertEquals("UserControl type", typeof(EU.GUI.PlugIn.AdditionalInfosUserControlWithGrid), foundUserControl.UserControlType);
				});
			}
		}

		public void TestPreviousDocumentsUserControlType()
		{
			using (var control = new ExportSupplierHeaderUserControlForTest())
			{
				AssertEquals(typeof(PreviousDocumentsUserControl), control.GetPreviousDocumentsUserControlTypeExposed());
			}
		}

		class ExportSupplierHeaderUserControlForTest : ExportSupplierHeaderUserControl
		{
			public Type GetPreviousDocumentsUserControlTypeExposed() => base.GetPreviousDocumentsUserControlType();
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
		}
		JobDeclaration declaration;

		protected override IEnumerable<string> ExpectedControlList => DefaultControlList;
	}
}
