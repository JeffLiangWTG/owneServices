using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CO.Manifest.Module.Testing
{
	[TestedType(typeof(DocumentIDsCollection))]
	class DocumentIDsCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType() => typeof(DocumentIDsCollection);

		protected override BusinessObjectCollection GetCollectionToTest() => new DocumentIDsCollection(Factory, new ZQuery());
	}
}
