using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	[TestedType(typeof(ProperCaseExcludeWordCollection))]
	sealed class ProperCaseExcludeWordCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ProperCaseExcludeWordCollection>
	{
		#region Implementation

		protected override ProperCaseExcludeWordCollection GetCollectionToTest()
		{
			return new ProperCaseExcludeWordCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ProperCaseExcludeWord(Factory);
		}

		#endregion
	}
}
