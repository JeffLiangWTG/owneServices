using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Registry
{
	[TestedType(typeof(RenameIdPUserManagementTimeoutInSecondsRegistryDataTransformation))]
	public class RenameIdPUserManagementTimeoutInSecondsRegistryDataTransformationTest : RegistryDataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals(0, Helper.GetStmDataRowCount("IdPUserManagementTimeoutInSeconds"));
			AssertEquals(1, Helper.GetStmDataRowCount("IdentityProviderTimeoutInSeconds"));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RenameIdPUserManagementTimeoutInSecondsRegistryDataTransformation();
		}

		protected override void PrepareTestData()
		{
			Helper.InsertStmDataRow("IdPUserManagementTimeoutInSeconds", Guid.Empty, Guid.Empty, true);
		}
	}
}
