using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Documents.Testing
{
	[TestedType(typeof(FRDocDataObject))]
	public class FRDocDataObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var doaDataObject = new FRDocDataObject();
			return doaDataObject;
		}
	}
}
