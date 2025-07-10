#if DEBUG

using System;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class SetGlobalsIsWebAttribute : TestSetupAttribute
	{
		public override void SetUp(TestCase testCase)
		{
			Globals.IsWeb = true;
		}

		public override void TearDown(TestCase testCase)
		{
			Globals.IsWeb = false;
		}
	}
}

#endif
