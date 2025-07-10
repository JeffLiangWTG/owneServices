using System;
using CargoWise.BuildTools;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class BuildXmlInstanceTestListener : BaseTestListener
	{
		public static readonly BuildXmlInstanceTestListener Instance = new BuildXmlInstanceTestListener();

		public override void EndTest(TestCase test, DateTime endTime)
		{
			base.EndTest(test, endTime);

			if (BuildXml.HasInstanceForTest)
			{
				BuildXml.RemoveTestingInstance();
			}
		}
	}
}
