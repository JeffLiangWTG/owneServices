using System;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core.Test;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Integration.ServiceTasks.CW.Test
{
	[TestedType(typeof(HostedServiceAttribute))]
	public class HostedServiceAttributeTest : AssemblyMetaDataAttributeTestCase<HostedServiceAttribute>
	{
		public void TestIsForClient_WithNone_MatchesClient()
		{
			// Arrange
			var attribute = new HostedServiceAttribute
			{
				ClientSpecificCode = Clients.None
			};

			// Act
			var result = attribute.IsForClient();

			// Assert
			Assert(result);
		}

		public void TestIsForClient_WithClientSpecificCode_MatchesClient()
		{
			// Arrange
			var attribute = new HostedServiceAttribute
			{
				ClientSpecificCode = ClientHookLoader.Instance.Client
			};

			// Act
			var result = attribute.IsForClient();

			// Assert
			Assert(result);
		}

		public void TestIsForClient_WithClientSpecificCode_DoesNotMatchClient()
		{
			// Arrange
			var attribute = new HostedServiceAttribute
			{
				ClientSpecificCode =
					ClientHookLoader.Instance.Client == Clients.ICS
						? Clients.TLU
						: Clients.ICS
			};

			// Act
			var result = attribute.IsForClient();

			// Assert
			Assert(!result);
		}

		public void TestEquals_AllPropertiesInEquals()
		{
			var attribute1 = GetAssemblyMetaDataAttributeForTesting();
			var attribute2 = GetAssemblyMetaDataAttributeForTesting();
			Assert(attribute1.Equals(attribute2));

			attribute1.Code = "Code";
			Assert(!attribute1.Equals(attribute2));

			attribute2.Code = "Code";
			Assert(attribute1.Equals(attribute2));

			attribute1.Description = "Description";
			Assert(!attribute1.Equals(attribute2));

			attribute2.Description = "Description";
			Assert(attribute1.Equals(attribute2));

			attribute1.Category = "Category";
			Assert(!attribute1.Equals(attribute2));

			attribute2.Category = "Category";
			Assert(attribute1.Equals(attribute2));

			attribute1.ProcessFileName = "ProcessFileName";
			Assert(!attribute1.Equals(attribute2));

			attribute2.ProcessFileName = "ProcessFileName";
			Assert(attribute1.Equals(attribute2));

			attribute1.ProcessArguments = "ProcessArguments";
			Assert(!attribute1.Equals(attribute2));

			attribute2.ProcessArguments = "ProcessArguments";
			Assert(attribute1.Equals(attribute2));

			attribute1.IsMandatory = true;
			Assert(!attribute1.Equals(attribute2));

			attribute2.IsMandatory = true;
			Assert(attribute1.Equals(attribute2));

			attribute1.AllowsMultipleInstances = true;
			Assert(!attribute1.Equals(attribute2));

			attribute2.AllowsMultipleInstances = true;
			Assert(attribute1.Equals(attribute2));

			attribute1.MinimumPeriod = "MinimumPeriod";
			Assert(!attribute1.Equals(attribute2));

			attribute2.MinimumPeriod = "MinimumPeriod";
			Assert(attribute1.Equals(attribute2));

			attribute1.MaximumPeriod = "MaximumPeriod";
			Assert(!attribute1.Equals(attribute2));

			attribute2.MaximumPeriod = "MaximumPeriod";
			Assert(attribute1.Equals(attribute2));

			attribute1.IsScheduleReadOnly = true;
			Assert(!attribute1.Equals(attribute2));

			attribute2.IsScheduleReadOnly = true;
			Assert(attribute1.Equals(attribute2));

			attribute1.IsReadOnlyForWiseCloudClient = true;
			Assert(!attribute1.Equals(attribute2));

			attribute2.IsReadOnlyForWiseCloudClient = true;
			Assert(attribute1.Equals(attribute2));

			attribute1.RequiresCompanyInCountry = "RequiresCompanyInCountry";
			Assert(!attribute1.Equals(attribute2));

			attribute2.RequiresCompanyInCountry = "RequiresCompanyInCountry";
			Assert(attribute1.Equals(attribute2));

			attribute1.CanRunInAnyBranch = true;
			Assert(!attribute1.Equals(attribute2));

			attribute2.CanRunInAnyBranch = true;
			Assert(attribute1.Equals(attribute2));

			attribute1.ConfigControlTypeName = "ConfigControlTypeName";
			Assert(!attribute1.Equals(attribute2));

			attribute2.ConfigControlTypeName = "ConfigControlTypeName";
			Assert(attribute1.Equals(attribute2));

			attribute1.ConfigControlTypeAssemblyName = "ConfigControlTypeAssemblyName";
			Assert(!attribute1.Equals(attribute2));

			attribute2.ConfigControlTypeAssemblyName = "ConfigControlTypeAssemblyName";
			Assert(attribute1.Equals(attribute2));

			attribute1.MutuallyExclusiveTaskGroup = MutuallyExclusiveServiceTaskGroups.BiAudit;
			Assert(!attribute1.Equals(attribute2));

			attribute2.MutuallyExclusiveTaskGroup = MutuallyExclusiveServiceTaskGroups.BiAudit;
			Assert(attribute1.Equals(attribute2));

			attribute1.AlwaysRunAtStartup = true;
			Assert(!attribute1.Equals(attribute2));

			attribute2.AlwaysRunAtStartup = true;
			Assert(attribute1.Equals(attribute2));

			attribute1.ActiveByDefault = true;
			Assert(!attribute1.Equals(attribute2));

			attribute2.ActiveByDefault = true;
			Assert(attribute1.Equals(attribute2));

			attribute1.DefaultScheduleRunEvery = "DefaultScheduleRunEvery";
			Assert(!attribute1.Equals(attribute2));

			attribute2.DefaultScheduleRunEvery = "DefaultScheduleRunEvery";
			Assert(attribute1.Equals(attribute2));

			attribute1.DefaultScheduleDaysOfWeek = [DayOfWeek.Friday];
			Assert(!attribute1.Equals(attribute2));

			attribute2.DefaultScheduleDaysOfWeek = [DayOfWeek.Friday];
			Assert(attribute1.Equals(attribute2));

			attribute1.DefaultScheduleDayOfMonth = 2;
			Assert(!attribute1.Equals(attribute2));

			attribute2.DefaultScheduleDayOfMonth = 2;
			Assert(attribute1.Equals(attribute2));

			attribute1.DefaultScheduleStartAtLocal = "DefaultScheduleStartAtLocal";
			Assert(!attribute1.Equals(attribute2));

			attribute2.DefaultScheduleStartAtLocal = "DefaultScheduleStartAtLocal";
			Assert(attribute1.Equals(attribute2));

			attribute1.DefaultScheduleStartAtUtc = "DefaultScheduleStartAtUtc";
			Assert(!attribute1.Equals(attribute2));

			attribute2.DefaultScheduleStartAtUtc = "DefaultScheduleStartAtUtc";
			Assert(attribute1.Equals(attribute2));

			attribute1.DefaultScheduleRandomStartOffset = "DefaultScheduleRandomStartOffset";
			Assert(!attribute1.Equals(attribute2));

			attribute2.DefaultScheduleRandomStartOffset = "DefaultScheduleRandomStartOffset";
			Assert(attribute1.Equals(attribute2));

			attribute1.DefaultScheduleEndAtLocal = "DefaultScheduleEndAtLocal";
			Assert(!attribute1.Equals(attribute2));

			attribute2.DefaultScheduleEndAtLocal = "DefaultScheduleEndAtLocal";
			Assert(attribute1.Equals(attribute2));

			attribute1.DefaultScheduleEndAtUtc = "DefaultScheduleEndAtUtc";
			Assert(!attribute1.Equals(attribute2));

			attribute2.DefaultScheduleEndAtUtc = "DefaultScheduleEndAtUtc";
			Assert(attribute1.Equals(attribute2));

			attribute1.DefaultScheduleDoNotRunTillNextDueTimeIfOverdue = "DefaultScheduleDoNotRunTillNextDueTimeIfOverdue";
			Assert(!attribute1.Equals(attribute2));

			attribute2.DefaultScheduleDoNotRunTillNextDueTimeIfOverdue = "DefaultScheduleDoNotRunTillNextDueTimeIfOverdue";
			Assert(attribute1.Equals(attribute2));

			attribute1.TaskSpecificValidationTypeName = "TaskSpecificValidationTypeName";
			Assert(!attribute1.Equals(attribute2));

			attribute2.TaskSpecificValidationTypeName = "TaskSpecificValidationTypeName";
			Assert(attribute1.Equals(attribute2));

			attribute1.TaskSpecificValidationTypeAssemblyName = "TaskSpecificValidationTypeAssemblyName";
			Assert(!attribute1.Equals(attribute2));

			attribute2.TaskSpecificValidationTypeAssemblyName = "TaskSpecificValidationTypeAssemblyName";
			Assert(attribute1.Equals(attribute2));
		}
	}
}
