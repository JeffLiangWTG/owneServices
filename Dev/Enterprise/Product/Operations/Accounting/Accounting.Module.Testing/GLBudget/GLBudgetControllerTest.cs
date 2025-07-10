using System;
using Enterprise.Accounting.Business.GeneralLedger.GLBudget;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(GLBudgetController))]
	class GLBudgetControllerTest : ZControllerBasherTest
	{
		protected override Type GetBusinessObjectType()
		{
			return typeof(GLBudget);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GLBudget;
		}
	}
}
