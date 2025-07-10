using System;
using NUnit.Framework;

#if !WINZOR
namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(GroupInvoiceUserControl))]
	sealed class GroupInvoiceUserControlBasherTest : ImportCustomsUserControlBasherTest
	{
		protected override Type UserControlToBashType => typeof(GroupInvoiceUserControl);

		protected override Customs.Business.BaseJobDeclaration GetPopulatedDeclarationForFormBashing()
		{
			var declaration = base.GetPopulatedDeclarationForFormBashing();
			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			return declaration;
		}
	}
}
#endif
