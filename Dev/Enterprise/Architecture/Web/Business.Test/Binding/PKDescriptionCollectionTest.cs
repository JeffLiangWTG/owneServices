using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	[TestedType(typeof(PKDescriptionCollection))]
	sealed class PKDescriptionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PKDescriptionCollection>
	{
		protected override PKDescriptionCollection GetCollectionToTest()
		{
			return new PKDescriptionCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PKDescription();
		}
	}
}
