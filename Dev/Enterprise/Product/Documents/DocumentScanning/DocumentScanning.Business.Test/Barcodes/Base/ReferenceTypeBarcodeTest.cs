using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(DocTypeBarcode))]
	public class ReferenceTypeBarcodeTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			// because it is an abstract class, return a concrete example
			return new DocTypeBarcode(new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()), "ABC");
		}
	}
}
