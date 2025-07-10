using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	[TestedType(typeof(NudgeEventDetails))]
	sealed class NudgeEventDetailsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new NudgeEventDetails("Failed", "Some error", ZDateTime.Now, "task", 5);
		}
	}
}
