using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	public class ComplianceRiskActionTest : TestCase
	{
		public void TestNoExceptionThrowWhenActionIsNull()
		{
			var complianceAction = new ComplianceRiskAction(null, null, null, "CLR");
			AssertNoExceptionThrown(() => complianceAction.ShowMessageIfNeeded());
			AssertNoExceptionThrown(() => complianceAction.SynchronizeAndSaveIfNeeded());
			AssertNoExceptionThrown(() => complianceAction.UpdateVisibilityIfNeeded());
		}

		public void TestActionOnlyTriggerOneTime()
		{
			var numberA = 0;
			var numberB = 0;
			var numberC = 0;
			Action actionA = () => { numberA++; };
			Action<List<ZString>> actionB = (List<ZString> originalJobComplianceStatus) => { numberB++; };
			Action actionC = () => { numberC++; };

			var complianceAction = new ComplianceRiskAction(actionA, actionB, actionC, "CLR");
			complianceAction.SynchronizeAndSaveIfNeeded();
			AssertEquals(1, numberA);
			AssertEquals(0, numberB);
			AssertEquals(0, numberC);

			complianceAction.ShowMessageIfNeeded();
			AssertEquals(1, numberA);
			AssertEquals(1, numberB);
			AssertEquals(0, numberC);

			complianceAction.SynchronizeAndSaveIfNeeded();
			complianceAction.ShowMessageIfNeeded();
			complianceAction.UpdateVisibilityIfNeeded();
			AssertEquals("Only Call One Time", 1, numberA);
			AssertEquals("Only Call One Time", 1, numberB);
			AssertEquals("Only Call One Time", 1, numberC);
		}
	}
}
