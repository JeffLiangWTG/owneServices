using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	[TestedType(typeof(UncollectedType))]
	sealed class UncollectedTypeTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new UncollectedType();
		}
	}
}
