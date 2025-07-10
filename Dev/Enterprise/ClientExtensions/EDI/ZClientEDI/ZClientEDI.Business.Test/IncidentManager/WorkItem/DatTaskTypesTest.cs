using System.Linq;
using System.Reflection;
using CargoWise.PAVE.Common.Implementation;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class DatTaskTypesTest : TestCase
	{
		public void TestGetDatTaskTypes()
		{
			var checkinTaskTypes = ReleaseRings.List().Select(r => r.CheckinTask).ToArray();
			var availableTaskTypes = typeof(EDITaskTypes)
				.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
				.Where(field => field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
				.Select(field => (string)field.GetRawConstantValue())
				.Union(checkinTaskTypes)
				.Distinct()
				.ToArray();

			var types = DatTaskType.GetAllTypes();

			AssertEquals(availableTaskTypes.Length, types.Length);

			foreach (var type in types)
			{
				var allowToSubmit = type.TypeCode == EDITaskTypes.TaskCheckin ||
					type.TypeCode == EDITaskTypes.TaskShelfTest ||
					type.TypeCode == EDITaskTypes.TaskUATBuild ||
					type.TypeCode == EDITaskTypes.TaskAspectOnlyBuild ||
					type.TypeCode == EDITaskTypes.TaskExperimentalPullRequest;
				var isMerge = checkinTaskTypes.Contains(type.TypeCode);
				var runTests = isMerge ||
					type.TypeCode == EDITaskTypes.TaskCheckin ||
					type.TypeCode == EDITaskTypes.TaskShelfTest ||
					type.TypeCode == EDITaskTypes.TaskActiveShelfTest ||
					type.TypeCode == EDITaskTypes.TaskExperimentalPullRequest ||
					type.TypeCode == EDITaskTypes.TaskActiveExperimentalPullRequest;
				var canChangeRunTests = type.TypeCode == EDITaskTypes.TaskShelfTest ||
					type.TypeCode == EDITaskTypes.TaskUATBuild;

				CombineAssertions("Type: " + type.TypeCode, () =>
				{
					AssertEquals(allowToSubmit, type.AllowToSubmit);
					AssertEquals(isMerge, type.IsMerge);
					AssertEquals(runTests, type.RunTests);
					AssertEquals(canChangeRunTests, type.CanChangeRunTests);
				});
			}
		}
	}
}
