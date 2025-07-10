using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(OnlineOrderCollection))]
	sealed class OnlineOrderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			return new OnlineOrderCollection(entryInstruction);
		}
	}
}
