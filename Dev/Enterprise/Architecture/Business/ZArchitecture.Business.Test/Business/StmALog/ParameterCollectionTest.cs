using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ParameterCollection))]
	sealed class ParameterCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ParameterCollection>
	{
		protected override ParameterCollection GetCollectionToTest()
		{
			return new ParameterCollection(Events.CustomisableEvent00Code);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new Parameter(Events.CustomisableEvent00Code);
		}
	}
}
