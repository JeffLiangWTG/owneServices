using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ElectronicProcessingChargeConfiguration))]
	public class ElectronicProcessingChargeConfigurationTest : RegistryBusinessObjectTemplateTestCase<ElectronicProcessingChargeConfiguration>
	{
		public void TestValidateJobType()
		{
			BizObj.JobType = ZString.Empty;
			AssertHasError(BizObj.JobTypeInfo, "Please enter a Job Type.");

			BizObj.JobType = "xxx";
			AssertHasError(BizObj.JobTypeInfo, "Enter a valid Job Type.");

			BizObj.JobType = "BRK";
			AssertNoErrors(BizObj.JobTypeInfo);

			BizObj.StartDate = ZDate.Today.AddDays(-1);
			BizObj.EndDate = ZDate.Today.AddDays(1);
			AssertValidateDubplicated(BizObj.JobTypeInfo);
		}

		public void TestValidateStartDate()
		{
			BizObj.StartDate = ZDate.Empty;
			AssertHasError(BizObj.StartDateInfo, "Please enter a Start Date.");

			BizObj.StartDate = ZDate.Invalid;
			AssertHasError(BizObj.StartDateInfo, "Enter a valid selection.");

			BizObj.StartDate = new ZDate(2021, 10, 10);
			AssertNoErrors(BizObj.StartDateInfo);
		}

		public void TestValidateEndDate()
		{
			BizObj.StartDate = new ZDate(2021, 10, 10);

			BizObj.EndDate = ZDate.Empty;
			AssertNoErrors(BizObj.EndDateInfo);

			BizObj.EndDate = ZDate.Invalid;
			AssertHasError(BizObj.EndDateInfo, "Enter a valid selection.");

			BizObj.EndDate = new ZDate(2021, 10, 9);
			BizObj.RunPreSaveValidation();
			AssertHasError(BizObj.EndDateInfo, "The 'End Date' must be after the 'Start Date'.");

			BizObj.EndDate = new ZDate(2021, 10, 10);
			BizObj.RunPreSaveValidation();
			AssertNoErrors(BizObj.EndDateInfo);

			BizObj.EndDate = new ZDate(2021, 10, 11);
			BizObj.RunPreSaveValidation();
			AssertNoErrors(BizObj.EndDateInfo);
		}

		void AssertValidateDubplicated(ZPropertyInfo propInfo)
		{
			var currentItem = (ElectronicProcessingChargeConfiguration)propInfo.BizObj;
			var duplicatedItem = currentItem.ParentCollection.AddNew();
			duplicatedItem.JobType = currentItem.JobType;
			duplicatedItem.StartDate = currentItem.StartDate;
			duplicatedItem.EndDate = currentItem.EndDate;

			currentItem.ClearRowNotifications();
			var prop = typeof(ElectronicProcessingChargeConfiguration).GetProperty(propInfo.Name);
			prop.SetValue(currentItem, propInfo.Value);

			AssertEquals(1, currentItem.RowErrors.Count());
			AssertHasRowError(currentItem, "This row has been duplicated and must be unique.");
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => true;

		protected override ElectronicProcessingChargeConfiguration GetBusinessObjectToClone()
		{
			return (ElectronicProcessingChargeConfiguration)GetNewBusinessObject();
		}

		protected override ElectronicProcessingChargeConfiguration GetBusinessObjectToSerialise()
		{
			return (ElectronicProcessingChargeConfiguration)GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return RegistryValue.AddNew();
		}

		#endregion

		ElectronicProcessingChargeConfigurationCollection RegistryValue
		{
			get
			{
				var result = AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeConfiguration.Value;
				result.CurrentFallbackLevel = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
				return result;
			}
		}
	}
}
