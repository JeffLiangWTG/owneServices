using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.ApplicationLogging.Business.Test
{
	[TestedType(typeof(ApplicationLoggerCollection))]
	public class ApplicationLoggerCollectionTest : ActiveBusinessObjectCollectionTestCase<ApplicationLoggerCollection>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(ApplicationLoggerCollection);
		}
	}
}
