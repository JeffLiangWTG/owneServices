using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ArchiveManager.Business;

namespace Enterprise.ArchiveManager.Test
{
	public class SystemDescriptorStringsTest : TestCaseWithFactory
	{
		public void TestGetNoun()
		{
			CombineAssertions(() =>
			{
				Assert("There should only be 3 items in SystemDescriptorType.", Enum.GetNames(typeof(SystemDescriptorType)).Length == 3);
				AssertEquals("Archive", SystemDescriptorStrings.GetNoun(SystemDescriptorType.Archive));
				AssertEquals("Purge", SystemDescriptorStrings.GetNoun(SystemDescriptorType.Purge));
				AssertEquals("Offline Archive", SystemDescriptorStrings.GetNoun(SystemDescriptorType.Offline));
			});
		}

		public void TestGetPresentTenseVerb()
		{
			CombineAssertions(() =>
			{
				Assert("There should only be 3 items in SystemDescriptorType.", Enum.GetNames(typeof(SystemDescriptorType)).Length == 3);
				AssertEquals("Archiving", SystemDescriptorStrings.GetPresentTenseVerb(SystemDescriptorType.Archive));
				AssertEquals("Purging", SystemDescriptorStrings.GetPresentTenseVerb(SystemDescriptorType.Purge));
				AssertEquals("Offline Archiving", SystemDescriptorStrings.GetPresentTenseVerb(SystemDescriptorType.Offline));
			});
		}

		public void TestGetPastTenseVerb()
		{
			CombineAssertions(() =>
			{
				Assert("There should only be 3 items in SystemDescriptorType.", Enum.GetNames(typeof(SystemDescriptorType)).Length == 3);
				AssertEquals("Archived", SystemDescriptorStrings.GetPastTenseVerb(SystemDescriptorType.Archive));
				AssertEquals("Purged", SystemDescriptorStrings.GetPastTenseVerb(SystemDescriptorType.Purge));
				AssertEquals("Archived Offline", SystemDescriptorStrings.GetPastTenseVerb(SystemDescriptorType.Offline));
			});
		}
	}
}
