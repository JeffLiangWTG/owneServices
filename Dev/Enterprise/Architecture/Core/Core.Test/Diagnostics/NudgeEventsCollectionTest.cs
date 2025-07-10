using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Diagnostics.Testing
{
	[TestedType(typeof(NudgeEventsCollection))]
	sealed class NudgeEventsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NudgeEventsCollection>
	{
		protected override NudgeEventsCollection GetCollectionToTest()
		{
			return new NudgeEventsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new NudgeEventDetails("Failed", "Some error", ZDateTime.Now, "task", 10);
		}
	}
}
