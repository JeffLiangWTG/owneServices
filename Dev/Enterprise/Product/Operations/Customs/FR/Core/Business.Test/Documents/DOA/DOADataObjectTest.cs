using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Documents.Testing
{
	[TestedType(typeof(DOADataObject))]
	sealed class DOADataObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var doaDataObject = new DOADataObject();
			return doaDataObject;
		}
	}
}
