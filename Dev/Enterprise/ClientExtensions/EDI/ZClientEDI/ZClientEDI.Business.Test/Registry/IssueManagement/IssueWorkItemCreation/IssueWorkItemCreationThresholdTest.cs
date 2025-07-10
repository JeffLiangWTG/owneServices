using System;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(IssueWorkItemCreationThreshold))]
	class IssueWorkItemCreationThresholdTest : RegistryBusinessObjectTemplateTestCase<IssueWorkItemCreationThreshold>
	{
		protected override IssueWorkItemCreationThreshold GetBusinessObjectToClone()
		{
			return new IssueWorkItemCreationThreshold(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override IssueWorkItemCreationThreshold GetBusinessObjectToSerialise()
		{
			return new IssueWorkItemCreationThreshold { ThresholdTimespan = 12, IssueOccurrenceThreshold = 15 };
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}
	}
}
