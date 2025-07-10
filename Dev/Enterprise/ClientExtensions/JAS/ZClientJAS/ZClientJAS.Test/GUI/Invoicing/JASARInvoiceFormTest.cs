using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.Testing;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.JAS.GUI.Testing
{
	[TestedType(typeof(JASARInvoiceForm))]
	internal class JASARInvoiceFormTest : ARInvoiceFormTest
	{
		public void TestConstructor()
		{
			var invoice = GetJASInvoiceWithValidTestData();
			invoice.FillWithValidTestData();
			Factory.Save();
			using (var form = new JASARInvoiceFormForTest(invoice))
			{
				AssertNotNull("JXC Export menu item should exist", form.ActionsMenuItem.MenuItems.FindByText("Export JXC Financial Message"));
			}
		}

		#region IJXCExportForm
		public void TestValidateAll()
		{
			var invoice = GetJASInvoiceWithValidTestData();
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			using (invoice.SuspendValidationTesting())
			using (line.SuspendValidationTesting())
			using (var form = new JASARInvoiceFormForTest(invoice))
			{
				line.AL_Desc = "";
				invoice.AH_GB = ZGuid.Empty;
				invoice.ClearAllNotifications();
				line.ClearAllNotifications();
				Assert("Pre-condition", !invoice.HasNotifications());
				Assert("Pre-condition", !line.HasNotifications());
				form.Show();
				((IJXCExportForm)form).ValidateAll();
				Assert("Should have notifications now", invoice.HasNotifications());
				Assert("Should have notifications now", line.HasNotifications());
			}
		}

		public void TestBusinessEntity()
		{
			var invoice = GetJASInvoiceWithValidTestData();
			using (var form = new JASARInvoiceFormForTest(invoice))
			{
				var formAsInterface = (IJXCExportForm)form;
				AssertEquals("Should be the ZWinForm.BusinessEntity", invoice, formAsInterface.BusinessEntity);
			}
		}

		#endregion
		#region Implementation
		protected override BaseInvoicingForm GetFormByInvoice(InvoicingBase invoice)
		{
			return new JASARInvoiceForm((JASARInvoice)invoice)
			{ ControllerID = Enterprise.ZArchitecture.Modules.ControllerIDs.ARInvoice };
		}

		protected override InvoicingBase GetInvoiceWithValidTestData(bool fillTestData = true, BusinessObjectFactory factory = null) => GetJASInvoiceWithValidTestData(fillTestData: fillTestData, factory: factory);
		JASARInvoice GetJASInvoiceWithValidTestData(bool fillTestData = true, BusinessObjectFactory factory = null)
		{
			var invoice = factory != null ? factory.New<JASARInvoice>() : Factory.New<JASARInvoice>();
			if (fillTestData)
			{
				invoice.FillWithValidTestData();
			}

			return invoice;
		}

		#region class JASARInvoiceFormForTest
		class JASARInvoiceFormForTest : JASARInvoiceForm
		{
			public JASARInvoiceFormForTest(JASARInvoice businessEntity) : base(businessEntity)
			{
			}

			public new MenuItem ActionsMenuItem
			{
				get
				{
					return base.ActionsMenuItem;
				}
			}
		}
		#endregion
		#endregion
	}
}
