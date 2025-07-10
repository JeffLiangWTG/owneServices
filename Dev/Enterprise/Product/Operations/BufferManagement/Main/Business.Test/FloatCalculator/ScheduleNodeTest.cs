using System;
using System.Reflection;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class ScheduleNodeTest : BMSTestCaseWithFactory
	{
		public void TestAllRelevantPropertiesAreIncludedInMethod_SchedulingInfoIsEqual()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var network = new ScheduleGraph();
			var node1 = new ScheduleNode(network, jobHeader);
			var node2 = new ScheduleNode(network, jobHeader);
			var node3 = new ScheduleNode(network, null);

			AssertEquals("Nodes differing by entity only should not be equal", false, node1.SchedulingInfoIsEqual(node3));
			AssertEquals("Nodes with the same everything, including entity should be equal", true, node1.SchedulingInfoIsEqual(node2));

			foreach (var property in typeof(ScheduleNode).GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (property.SetMethod != null && property.SetMethod.IsPublic && DoWeCareAboutThisPropertyForMethod_SchedulingInfoIsEqual(property))
				{
					var defaultValue = property.GetValue(node1);
					var nonDefaultValue = GetNonDefaultValue(property);

					CombineAssertions($"Testing property {property.Name} with default value [{defaultValue}] and non-default value [{nonDefaultValue}].", () =>
					{
						property.SetValue(node2, nonDefaultValue);
						AssertEquals("When node2 has the non-default value", false, node1.SchedulingInfoIsEqual(node2));

						property.SetValue(node1, nonDefaultValue);
						AssertEquals("When both nodes have the non-default value", true, node1.SchedulingInfoIsEqual(node2));

						property.SetValue(node1, defaultValue);
						property.SetValue(node2, defaultValue);
						AssertEquals("When both nodes have the default value", true, node1.SchedulingInfoIsEqual(node2));
					});
				}
			}
		}

		static bool DoWeCareAboutThisPropertyForMethod_SchedulingInfoIsEqual(PropertyInfo property)
		{
			switch (property.Name)
			{
				case "Parent":
				case "Pin":
					return false;
			}

			return true;
		}

		static object GetNonDefaultValue(PropertyInfo property)
		{
			var propertyType = property.PropertyType;

			if (propertyType == typeof(DeliveryDateThreat))
			{
				return new DeliveryDateThreat(null, 0m);
			}
			else if (propertyType == typeof(decimal))
			{
				return 1m;
			}
			else if (propertyType == typeof(ZDateTime))
			{
				return new ZDateTime(1985, 8, 30, 12, 42, 0);
			}
			else if (propertyType == typeof(bool))
			{
				return true;
			}
			else if (propertyType == typeof(int))
			{
				return 1;
			}

			throw new InvalidOperationException($"{nameof(propertyType)} {propertyType.Name} is not supported. Please add it to this statement so we can test property {property.Name}.");
		}
	}
}
