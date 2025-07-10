using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Diagnostics.Testing
{
	[TestedType(typeof(Document))]
	public class DocumentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new Document();
		}
	}
}
