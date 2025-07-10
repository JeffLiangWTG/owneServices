using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Enterprise.Core.Testing
{
	sealed class ContainerModesTest : TestCase
	{
		public void TestIsContainerised()
		{
			AssertEquals(true, Core.Constants.ContainerModes.IsContainerised(Core.Constants.ContainerModes.BuyersConsol));
			AssertEquals(true, Core.Constants.ContainerModes.IsContainerised(Core.Constants.ContainerModes.Combination));
			AssertEquals(true, Core.Constants.ContainerModes.IsContainerised(Core.Constants.ContainerModes.Containerised));
			AssertEquals(true, Core.Constants.ContainerModes.IsContainerised(Core.Constants.ContainerModes.FCL));
			AssertEquals(true, Core.Constants.ContainerModes.IsContainerised(Core.Constants.ContainerModes.FCLMixedShipper));
			AssertEquals(true, Core.Constants.ContainerModes.IsContainerised(Core.Constants.ContainerModes.FreightAllKind));
			AssertEquals(true, Core.Constants.ContainerModes.IsContainerised(Core.Constants.ContainerModes.Groupage));
			AssertEquals(true, Core.Constants.ContainerModes.IsContainerised(Core.Constants.ContainerModes.LCL));

			AssertEquals(false, Core.Constants.ContainerModes.IsContainerised(Core.Constants.ContainerModes.AgentConsol));
			AssertEquals(false, Core.Constants.ContainerModes.IsContainerised(Core.Constants.ContainerModes.AIR));
			AssertEquals(false, Core.Constants.ContainerModes.IsContainerised(Core.Constants.ContainerModes.BreakBulk));
			AssertEquals(false, Core.Constants.ContainerModes.IsContainerised(Core.Constants.ContainerModes.Bulk));
			AssertEquals(false, Core.Constants.ContainerModes.IsContainerised(Core.Constants.ContainerModes.Empty));
			AssertEquals(false, Core.Constants.ContainerModes.IsContainerised(Core.Constants.ContainerModes.FTL));
			AssertEquals(false, Core.Constants.ContainerModes.IsContainerised(Core.Constants.ContainerModes.Liquid));
			AssertEquals(false, Core.Constants.ContainerModes.IsContainerised(Core.Constants.ContainerModes.Loose));
			AssertEquals(false, Core.Constants.ContainerModes.IsContainerised(Core.Constants.ContainerModes.LTL));
			AssertEquals(false, Core.Constants.ContainerModes.IsContainerised(Core.Constants.ContainerModes.Mail));
			AssertEquals(false, Core.Constants.ContainerModes.IsContainerised(Core.Constants.ContainerModes.NonContainerised));
			AssertEquals(false, Core.Constants.ContainerModes.IsContainerised(Core.Constants.ContainerModes.OnBoardCourier));
			AssertEquals(false, Core.Constants.ContainerModes.IsContainerised(Core.Constants.ContainerModes.Other));
			AssertEquals(false, Core.Constants.ContainerModes.IsContainerised(Core.Constants.ContainerModes.RollOnRollOff));
			AssertEquals(false, Core.Constants.ContainerModes.IsContainerised(Core.Constants.ContainerModes.ULD));
			AssertEquals(false, Core.Constants.ContainerModes.IsContainerised(Core.Constants.ContainerModes.Unaccompanied));
		}

		public void TestIsLCLType()
		{
			AssertEquals(true, Core.Constants.ContainerModes.IsLCLType(Core.Constants.ContainerModes.AIR));
			AssertEquals(true, Core.Constants.ContainerModes.IsLCLType(Core.Constants.ContainerModes.BuyersConsol));
			AssertEquals(true, Core.Constants.ContainerModes.IsLCLType(Core.Constants.ContainerModes.FreightAllKind));
			AssertEquals(true, Core.Constants.ContainerModes.IsLCLType(Core.Constants.ContainerModes.Groupage));
			AssertEquals(true, Core.Constants.ContainerModes.IsLCLType(Core.Constants.ContainerModes.LCL));
			AssertEquals(true, Core.Constants.ContainerModes.IsLCLType(Core.Constants.ContainerModes.Loose));
			AssertEquals(true, Core.Constants.ContainerModes.IsLCLType(Core.Constants.ContainerModes.LTL));
			AssertEquals(true, Core.Constants.ContainerModes.IsLCLType(Core.Constants.ContainerModes.ULD));

			AssertEquals(false, Core.Constants.ContainerModes.IsLCLType(Core.Constants.ContainerModes.AgentConsol));
			AssertEquals(false, Core.Constants.ContainerModes.IsLCLType(Core.Constants.ContainerModes.BreakBulk));
			AssertEquals(false, Core.Constants.ContainerModes.IsLCLType(Core.Constants.ContainerModes.Bulk));
			AssertEquals(false, Core.Constants.ContainerModes.IsLCLType(Core.Constants.ContainerModes.Combination));
			AssertEquals(false, Core.Constants.ContainerModes.IsLCLType(Core.Constants.ContainerModes.Containerised));
			AssertEquals(false, Core.Constants.ContainerModes.IsLCLType(Core.Constants.ContainerModes.Empty));
			AssertEquals(false, Core.Constants.ContainerModes.IsLCLType(Core.Constants.ContainerModes.FCL));
			AssertEquals(false, Core.Constants.ContainerModes.IsLCLType(Core.Constants.ContainerModes.FTL));
			AssertEquals(false, Core.Constants.ContainerModes.IsLCLType(Core.Constants.ContainerModes.Liquid));
			AssertEquals(false, Core.Constants.ContainerModes.IsLCLType(Core.Constants.ContainerModes.Mail));
			AssertEquals(false, Core.Constants.ContainerModes.IsLCLType(Core.Constants.ContainerModes.NonContainerised));
			AssertEquals(false, Core.Constants.ContainerModes.IsLCLType(Core.Constants.ContainerModes.OnBoardCourier));
			AssertEquals(false, Core.Constants.ContainerModes.IsLCLType(Core.Constants.ContainerModes.Other));
			AssertEquals(false, Core.Constants.ContainerModes.IsLCLType(Core.Constants.ContainerModes.RollOnRollOff));
			AssertEquals(false, Core.Constants.ContainerModes.IsLCLType(Core.Constants.ContainerModes.Unaccompanied));
			AssertEquals(false, Core.Constants.ContainerModes.IsLCLType(Core.Constants.ContainerModes.FCLMixedShipper));
		}

		public void TestDescriptionsCoverage()
		{
			var containerModeCodes = typeof(Constants.ContainerModes).GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
				.Select(fieldInfo => (string)fieldInfo.GetValue(null));

			var codesWithoutDescription = new List<string>();

			foreach (var code in containerModeCodes)
			{
				if (string.IsNullOrWhiteSpace(Constants.ContainerModeDescriptions.GetDescription(code)))
				{
					codesWithoutDescription.Add(code);
				}
			}

			AssertEquals(string.Format("missing container mode descriptions: {0}{1}", System.Environment.NewLine, string.Join(System.Environment.NewLine, codesWithoutDescription)),
				0, codesWithoutDescription.Count);
		}
	}
}
