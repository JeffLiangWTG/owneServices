using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Diagnostics.Testing
{
	[TestedType(typeof(DocumentSections))]
	public class DocumentSectionsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocumentSections(new BusinessObjectFactory());
		}
	}
}
