using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	public abstract class EUInvoiceLineUserControlTest<T> : BaseInvoiceLineUserControlForVirtualPropertiesTest<T>
		where T : EUInvoiceLineUserControl, new()
	{
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();
		}
		JobDeclaration declaration;

		[RequiresSTA]
		public void TestAddCopyDocumentsOfInvoiceLineMenuToGrid()
		{
			using var form = new ZForm(declaration);
			using var control = new T();
			form.Controls.Add(control);
			form.Show();
			control.CustomsInvoiceLinesBoundGrid.ListManager.Position = 1;
			var item = control.CustomsInvoiceLinesBoundGrid.ContextMenu.MenuItems.FindByText("Copy Documents of Invoice Line");
			AssertNotNull(item);

			item.PerformClick();

			CombineAssertions(() =>
			{
				AssertType<InvoiceLineCopyDocumentsForm>(ZFormModaliser.LastFormShownDialogForTest);
				AssertSame(declaration.InvoiceLines[0], ((CopyDocumentsSelectionHeader)ZFormModaliser.LastIBusinessShownOnDialogForTest).InvoiceLine);
			});
		}
	}
}
