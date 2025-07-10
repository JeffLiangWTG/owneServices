using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.SystemDescriptors
{
	public abstract class BaseArchiveSystemTest : TestCase
	{
		protected abstract IArchiveSystemDescriptor SystemDescriptor { get; }

		protected abstract string ExpectedCode { get; }

		protected abstract string ExpectedName { get; }

		protected abstract string ExpectedNoun { get; }

		protected abstract string ExpectedPresentTenseVerb { get; }

		protected abstract string ExpectedPastTenseVerb { get; }

		protected abstract IReportGenerator ExpectedReportGeneratorType { get; }

		protected abstract List<IArchiveStageDescriptor> ExpectedArchiveStageDescriptors { get; }

		protected abstract List<IArchiveStageDescriptor> ExpectedArchiveStageDescriptorsWithCustoms { get; }

		protected abstract List<IArchiveStage> ExpectedArchiveStages { get; }

		public void TestCode()
			=> AssertEquals(ExpectedCode, SystemDescriptor.Code);

		public void TestName()
			=> AssertEquals(ExpectedName, SystemDescriptor.Name);

		public void TestNoun()
			=> AssertEquals(ExpectedNoun, SystemDescriptor.Noun);

		public void TestPresentTenseVerb()
			=> AssertEquals(ExpectedPresentTenseVerb, SystemDescriptor.PresentTenseVerb);

		public void TestPastTenseVerb()
			=> AssertEquals(ExpectedPastTenseVerb, SystemDescriptor.PastTenseVerb);

		public void TestReportGenerator()
			=> AssertType(ExpectedReportGeneratorType.GetType(), SystemDescriptor.ReportGenerator);

		public virtual void TestAllowShouldArchiveDeclaration()
			=> Assert(SystemDescriptor.AllowShouldArchiveDeclaration);

		public void TestGetArchiveStageDescriptors()
		{
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, false);
			var archiveStageDescriptors = SystemDescriptor.GetArchiveStageDescriptors(config).ToList();
			var archiveStageDescriptorListsEqual = ExpectedArchiveStageDescriptors.Select(s => s.GetType()).SequenceEqual(archiveStageDescriptors.Select(s => s.GetType()));

			Assert($"Following archive stage descriptors were expected:\n{string.Join("\n", ExpectedArchiveStageDescriptors.Select(t => t.Name))}", archiveStageDescriptorListsEqual);
		}

		public void TestGetArchiveStageDescriptors_WhenShouldIncludeDeclarations()
		{
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, shouldIncludeDeclarations: true);
			var archiveStageDescriptors = SystemDescriptor.GetArchiveStageDescriptors(config).ToList();
			var archiveStageDescriptorListsEqual = ExpectedArchiveStageDescriptorsWithCustoms.Select(s => s.GetType()).SequenceEqual(archiveStageDescriptors.Select(s => s.GetType()));

			Assert($"Following archive stage descriptors were expected:\n{string.Join("\n", ExpectedArchiveStageDescriptorsWithCustoms.Select(t => t.Name))}", archiveStageDescriptorListsEqual);
		}

		public void TestGetArchiveStages()
		{
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, false);
			var archiveStages = SystemDescriptor.GetArchiveStages(config).ToList();
			var archiveStageListsEqual = ExpectedArchiveStages.Select(s => s.GetType()).SequenceEqual(archiveStages.Select(s => s.GetType()));

			Assert($"Following archive stages were expected:\n{string.Join("\n", ExpectedArchiveStages.Select(t => t.Name))}", archiveStageListsEqual);
		}

		public virtual void TestGetRegistryLogs()
		{
			var registryLogs = SystemDescriptor.GetRegistryLogs().ToList();

			AssertEquals(2, registryLogs.Count);
			CombineAssertions("Registry logs", () =>
			{
				AssertEquals("On or Before Minimum: 7", registryLogs[0]);
				AssertEquals("Set Batch Size for Archiving and Purging Operational Jobs: 50", registryLogs[1]);
			});
		}
	}
}
