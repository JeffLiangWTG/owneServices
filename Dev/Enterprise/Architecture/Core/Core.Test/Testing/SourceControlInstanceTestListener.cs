using System;
using CargoWise.BuildTools;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class SourceControlInstanceTestListener : BaseTestListener
	{
		public static readonly SourceControlInstanceTestListener Instance = new SourceControlInstanceTestListener();

		public override void EndTest(TestCase test, DateTime endTime)
		{
			base.EndTest(test, endTime);
			SourceControl.RemoveEnterpriseInstanceForTesting();
		}
	}
}
