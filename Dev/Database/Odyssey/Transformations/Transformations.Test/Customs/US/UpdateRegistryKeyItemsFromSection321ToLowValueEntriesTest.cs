using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.US;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.US
{
	[TestedType(typeof(UpdateRegistryKeyItemsFromSection321ToLowValueEntries))]
	public class UpdateRegistryKeyItemsFromSection321ToLowValueEntriesTest : RegistryDataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateRegistryKeyItemsFromSection321ToLowValueEntries();

		protected override void AssertTransformationResults()
		{
			AssertEquals("EnableFDAforSection321Entries", 0, Helper.GetStmDataRowCount("EnableFDAforSection321Entries"));
			AssertEquals("Section321ReleaseMessages", 0, Helper.GetStmDataRowCount("Section321ReleaseMessages"));
			AssertEquals("RemoveNonWesternEuropeanCharactersUSSection321", 0, Helper.GetStmDataRowCount("RemoveNonWesternEuropeanCharactersUSSection321"));

			AssertEquals("EnableFDAForLowValueEntries", 1, Helper.GetStmDataRowCount("EnableFDAForLowValueEntries"));
			AssertEquals("LowValueEntriesReleaseMessages", 1, Helper.GetStmDataRowCount("LowValueEntriesReleaseMessages"));
			AssertEquals("RemoveNonWesternEuropeanCharactersUSLowValueEntries", 1, Helper.GetStmDataRowCount("RemoveNonWesternEuropeanCharactersUSLowValueEntries"));
		}

		protected override void PrepareTestData()
		{
			Helper.InsertStmDataRow("EnableFDAforSection321Entries", Guid.Empty, Guid.Empty);
			Helper.InsertStmDataRow("Section321ReleaseMessages", Guid.Empty, Guid.Empty);
			Helper.InsertStmDataRow("RemoveNonWesternEuropeanCharactersUSSection321", Guid.Empty, Guid.Empty);
		}
	}
}
