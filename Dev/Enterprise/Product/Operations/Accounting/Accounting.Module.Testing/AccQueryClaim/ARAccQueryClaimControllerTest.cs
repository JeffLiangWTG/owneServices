using System;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARAccQueryClaimController))]
	public class ARAccQueryClaimControllerTest : AccQueryClaimControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ARAccQueryClaim;
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(ARAccQueryClaim);
		}
	}
}
