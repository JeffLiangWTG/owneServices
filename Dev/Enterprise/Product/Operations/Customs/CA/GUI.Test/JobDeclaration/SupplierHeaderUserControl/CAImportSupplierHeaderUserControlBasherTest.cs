using System;
using NUnit.Framework;

#if !WINZOR
namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(CAImportSupplierHeaderUserControl))]
	sealed class CAImportSupplierHeaderUserControlBasherTest : ImportCustomsUserControlBasherTest
	{
		protected override Type UserControlToBashType => typeof(CAImportSupplierHeaderUserControl);

		protected override Customs.Business.BaseJobDeclaration GetPopulatedDeclarationForFormBashing()
		{
			var declaration = base.GetPopulatedDeclarationForFormBashing();
			declaration.Invoices.AddNew();
			return declaration;
		}
	}
}
#endif
