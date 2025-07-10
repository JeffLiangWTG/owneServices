using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(EUImportSupplierHeaderUserControl))]
	public class EUImportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<EUImportSupplierHeaderUserControl, JobDeclaration>
	{
		protected override IEnumerable<string> ExpectedControlList => DefaultControlList;

		public void TestGridLayoutContext()
		{
			using (var control = new EUImportSupplierHeaderUserControl())
			{
				AssertEquals("context is set", nameof(Customs.GUI.DeclarationType.Import), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestHeaderDescriptionsTabPageVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var frm = new ZForm(declaration))
			{
				var control = new EUImportSupplierHeaderUserControl();

				frm.Controls.Add(control);
				frm.Show();

				Application.DoEvents();

				var tabPage = (ZTabPage)control.Controls.Find("HeaderDescriptionsTabPage", true).FirstOrDefault();
				AssertNull("Should is null as its tab visible is false.", tabPage);
			}
		}

		public void TestGridId()
		{
			using (var control = new EUImportSupplierHeaderUserControl())
			{
				AssertEquals("GridLayoutRh5OdMrTXWw1W+7iTHilOQ==", control.JobComInvoiceHeadersBoundGrid.InnerGrid.GridId);
			}
		}

		public void TestImportInvoiceHeaderDescriptionsUserControl()
		{
			using (var control = new EUImportSupplierHeaderUserControlForTest())
			using (var importInvoiceHeaderDescriptionsUserControl = control.ImportInvoiceHeaderDescriptionsUserControl)
			{
				AssertType(typeof(ImportInvoiceHeaderDescriptionsUserControl), importInvoiceHeaderDescriptionsUserControl);
			}
		}

		public void TestAdditionalInfoTabPageCaptionAndUserControlType()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<EUImportSupplierHeaderUserControl>(declaration, "AdditionalInfoTabPage", "additionalInfosUserControl1", "Additional Documents", typeof(AdditionalInfosUserControlWithGrid));
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
			{
				EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<EUImportSupplierHeaderUserControl>(declaration, "AdditionalInfoTabPage", "additionalInfosUserControl1", "[44] Additional Info", typeof(AdditionalInfosUserControl));
			}
		}

		class EUImportSupplierHeaderUserControlForTest : EUImportSupplierHeaderUserControl
		{
			public ImportInvoiceHeaderDescriptionsUserControl ImportInvoiceHeaderDescriptionsUserControl => base.GetImportInvoiceHeaderDescriptionsUserControl();
		}
	}
}
