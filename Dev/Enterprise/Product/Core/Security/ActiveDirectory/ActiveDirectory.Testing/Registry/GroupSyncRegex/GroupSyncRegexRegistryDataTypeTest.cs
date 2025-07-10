using System;
using System.Text;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test.Registry
{
	[TestedType(typeof(GroupSyncRegexRegistryDataType))]
	class GroupSyncRegexRegistryDataTypeTest : RegistryDataTypeTestCase<GroupSyncRegexRegistryDataType>
	{
		public void TestResetLastSuccessfulSyncUTCWhenChangingGroupSyncRegex()
		{
			var registryItem = ActiveDirectoryRegistry.Instance.GroupSyncRegex;

			ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(-1));
			DataType.ValidateBeforeRegistryFormSave(registryItem, "D^", Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("Reset LastSuccessfulSyncUTC when setting GroupSyncRegex", ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.DefaultValue, ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value);

			ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(-1));
			DataType.ValidateBeforeRegistryFormSave(registryItem, "D^|S$", Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("Reset LastSuccessfulSyncUTC when update GroupSyncRegex", ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.DefaultValue, ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value);
		}

		protected override GroupSyncRegexRegistryDataType GetNewDataType()
		{
			return new GroupSyncRegexRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB("D^", Encoding.Unicode.GetBytes("D^")),
				new ValidSampleAndBinaryValueInDB("AD^|Release Group$", Encoding.Unicode.GetBytes("AD^|Release Group$"))
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { @"blah\", @"*^core" };
		}

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValue => true;
	}
}
