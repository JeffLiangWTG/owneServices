using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding.Testing
{
	[TestedType(typeof(DeleteRoleOrderManagerCustomerFromSystemDefinedGroups))]
	class DeleteRoleOrderManagerCustomerFromSystemDefinedGroupsTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertGlbGroupRoleExistence("Should delete", ExpectedToDelete, shouldExist: false);
			AssertGlbGroupRoleExistence("Should keep", ExpectedToKeep, shouldExist: true);
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new DeleteRoleOrderManagerCustomerFromSystemDefinedGroups();
		}

		protected override void PrepareTestData()
		{
			var testDataCreator = new TransformationTestDataCreator();

			var systemDefinedGroup1Pk = testDataCreator.GetOrCreateGroupByCode("NEOROLES", "ORG", "", isSystemDefined: true);
			var systemDefinedGroup2Pk = testDataCreator.GetOrCreateGroupByCode("SYSTEM2", "ORG", "", isSystemDefined: true);
			var nonSystemDefinedGroup1Pk = testDataCreator.GetOrCreateGroupByCode("NON_SYSTEM1", "ORG", "", isSystemDefined: false);
			var nonSystemDefinedGroup2Pk = testDataCreator.GetOrCreateGroupByCode("NON_SYSTEM2", "ORG", "", isSystemDefined: false);

			ExpectedToDelete = new[] {
				testDataCreator.GetOrCreateGlbGroupRole(systemDefinedGroup1Pk, OrderManagerCustomerRoleName),
				testDataCreator.GetOrCreateGlbGroupRole(systemDefinedGroup2Pk, OrderManagerCustomerRoleName),
			};

			ExpectedToKeep = new[] {
				testDataCreator.GetOrCreateGlbGroupRole(systemDefinedGroup1Pk, NonConcernedRoleName),
				testDataCreator.GetOrCreateGlbGroupRole(systemDefinedGroup2Pk, NonConcernedRoleName),
				testDataCreator.GetOrCreateGlbGroupRole(nonSystemDefinedGroup1Pk, OrderManagerCustomerRoleName),
				testDataCreator.GetOrCreateGlbGroupRole(nonSystemDefinedGroup2Pk, OrderManagerCustomerRoleName),
				testDataCreator.GetOrCreateGlbGroupRole(nonSystemDefinedGroup1Pk, NonConcernedRoleName),
				testDataCreator.GetOrCreateGlbGroupRole(nonSystemDefinedGroup2Pk, NonConcernedRoleName),
			};

			var testedPkList = ExpectedToDelete.Concat(ExpectedToKeep);
			AssertGlbGroupRoleExistence("Prerequisite: tested records are in database", testedPkList, shouldExist: true);
		}

		void AssertGlbGroupRoleExistence(string message, IEnumerable<Guid> queriedList, bool shouldExist)
		{
			var actualPkList = new List<Guid>();
			TestConnection.ExecuteReader(
				$"SELECT GGR_PK FROM dbo.GlbGroupRole WHERE GGR_PK IN ({(string.Join(",", queriedList.Select(x => $"'{x}'")))})",
				record => {
					actualPkList.Add((Guid)record["GGR_PK"]);
				});
			var expectedPkList = shouldExist ? queriedList : Array.Empty<Guid>();
			AssertContainsExactElementsInAnyOrder(message, expectedPkList, actualPkList);
		}

		Guid[] ExpectedToKeep;
		Guid[] ExpectedToDelete;
		readonly string OrderManagerCustomerRoleName = DeleteRoleOrderManagerCustomerFromSystemDefinedGroups.OrderManagerCustomerRoleName;
		readonly string NonConcernedRoleName = "TransitWarehouseCustomer";
	}
}
