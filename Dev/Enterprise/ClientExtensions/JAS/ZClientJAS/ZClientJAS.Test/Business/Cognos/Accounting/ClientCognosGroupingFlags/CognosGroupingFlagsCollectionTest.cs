using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	[TestedType(typeof(CognosGroupingFlagsCollection))]
	class CognosGroupingFlagsCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CognosGroupingFlagsCollection(Factory);
		}
	}
}
