using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.AUS.Modules.Testing
{
	[TestedType(typeof(ProductImportAndExportController))]
	public class ProductImportAndExportControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ClientControllerRegistration.ProductImportAndExport;
		}
	}
}
