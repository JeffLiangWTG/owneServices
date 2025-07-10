using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	[TestedType(typeof(CustomMapPair))]
	sealed class CustomMapPairTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CustomMapPair();
		}

		#endregion
	}
}
