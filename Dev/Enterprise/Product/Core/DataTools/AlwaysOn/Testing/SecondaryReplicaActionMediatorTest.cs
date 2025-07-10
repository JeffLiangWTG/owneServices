using System;
using Enterprise.AlwaysOn.Setup;
using Enterprise.AlwaysOn.Setup.GUI;
using NUnit.Framework;

namespace Enterprise.AlwaysOn.Testing
{
	class SecondaryReplicaActionMediatorTest : TestCase
	{
		public void Test_EachPreJoinLevelValueShouldHaveAUniqueColor()
		{
			var number = Enum.GetNames(typeof(PreJoinLevel)).Length;

			AssertEquals(SecondaryReplicaActionMediator.DbListColour.Count, number);
		}
	}
}
