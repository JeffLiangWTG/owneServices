using System;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.BR.GUI.Testing
{
	abstract class BaseInvoiceLineDetailsLayoutTest<T> : LayoutsAbstractTest where T : IPanelLayoutProvider
	{
		public void TestNullJobDeclaration()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertNull(invoiceLine.Declaration);

			AssertNoExceptionThrown("NullReferenceException not expected", () =>
			{
				using (var panel = new DynamicLayoutPanel())
				{
					panel.Width = 1500;
					panel.SetDataBinding(invoiceLine, null);
					panel.UpdateLayout(Activator.CreateInstance<T>());
				}
			});
		}
	}
}
