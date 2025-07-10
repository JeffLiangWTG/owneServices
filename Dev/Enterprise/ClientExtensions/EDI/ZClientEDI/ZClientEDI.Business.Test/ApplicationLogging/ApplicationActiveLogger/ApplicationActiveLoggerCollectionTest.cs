using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.ApplicationLogging.Business.Test
{
	[TestedType(typeof(ApplicationActiveLoggerCollection))]
	public class ApplicationActiveLoggerCollectionTest : ActiveBusinessObjectCollectionTestCase<ApplicationActiveLoggerCollection>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(ApplicationActiveLoggerCollection);
		}
	}
}
