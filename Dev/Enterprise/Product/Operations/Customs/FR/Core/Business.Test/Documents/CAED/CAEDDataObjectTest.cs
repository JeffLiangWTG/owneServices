using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Documents.Testing
{
	[TestedType(typeof(CAEDDataObject))]
	sealed class CAEDDataObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var caedDataObject = new CAEDDataObject();
			return caedDataObject;
		}
	}
}
