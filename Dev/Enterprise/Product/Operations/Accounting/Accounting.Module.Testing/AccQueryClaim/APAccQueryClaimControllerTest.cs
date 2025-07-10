using System;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APAccQueryClaimController))]
	public class APAccQueryClaimControllerTest : AccQueryClaimControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.APAccQueryClaim;
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(APAccQueryClaim);
		}
	}
}
