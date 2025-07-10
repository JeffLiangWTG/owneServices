using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMComponentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBMComponentTypes()
		{
			var component = Factory.New<BMComponent>();
			AssertNull("Prerequisite: system is null", component.System);

			var expectedComponentTypesWithoutSystem = new[] { BMComponentTypeList.Codes.ComponentRelationship };
			AssertContainsExactElementsInAnyOrder(
				"WHEN system is null THEN should only contain ComponentView",
				expectedComponentTypesWithoutSystem,
				component.Lookups.Types.ToArray().Select(x => x.Code));

			component.FC_FS_System = Factory.NewWithValidTestData<BMSystem>().PK;
			AssertNotNull("Prerequisite: system is set", component.System);

			var expectedComponentTypesWithSystem = new[]
			{
				BMComponentTypeList.Codes.Buffer,
				BMComponentTypeList.Codes.Bucket,
				BMComponentTypeList.Codes.Decouple,
				BMComponentTypeList.Codes.Constraint,
			};
			AssertContainsExactElementsInAnyOrder(
				"WHEN system is set THEN should not contain ComponentView",
				expectedComponentTypesWithSystem,
				component.Lookups.Types.ToArray().Select(x => x.Code));

			component.FC_FS_System = ZGuid.Empty;
			AssertNull("Prerequisite: system is set to null again", component.System);

			AssertContainsExactElementsInAnyOrder(
				"WHEN system is null again THEN should only contain ComponentView",
				expectedComponentTypesWithoutSystem,
				component.Lookups.Types.ToArray().Select(x => x.Code));
		}
	}
}
