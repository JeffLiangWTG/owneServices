using System;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Billing.Business
{
	internal class BilledFeature : BaseUsageFeature
	{
		readonly RefStlFieldMapping fieldMapping;
		readonly string systemId;

		[ThreadSafe]
		internal static readonly BillingManager BillingManager = new BillingManager();

		public BilledFeature(UsageFeatureProperties featureProperties, RefStlFieldMapping fieldMapping) : base(featureProperties)
		{
			this.fieldMapping = fieldMapping;
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			systemId = registrationKey.SystemId;
		}

		public override void Report(JObject allProperties, GlbBranch currentBranch, BusinessObjectFactory factory)
		{
			var transaction = new BillingTransactionWrapper(new BillingTransaction(), SubmissionPriority.MandatoryForMilestone);
			var mappedCategory = fieldMapping.SFM_Category.ToString();
			transaction.Transaction.Category = string.IsNullOrEmpty(mappedCategory) ? "STL" : mappedCategory;
			transaction.Transaction.ClientID = currentBranch.Company.LicenceKeyIdentifier;
			transaction.Transaction.ClientNumber = $"{systemId}.{currentBranch.Company.GC_Code}";
			transaction.Transaction.PriceItemCode = GetBilledPropertyValue(allProperties, fieldMapping.SFM_PriceItemCode, FeatureProperties.Code);
			transaction.Transaction.ServiceOccuredUTC = GetBilledPropertyValueAsUniversalDateTime(allProperties, fieldMapping.SFM_ServiceOccuredUTC, ZDateTime.UtcNow.ToDateTime());
			transaction.Transaction.ClientStaffCode = GetBilledPropertyValue<string>(allProperties, fieldMapping.SFM_ClientStaffCode, GlbStaff.CurrentUser.GS_Code);
			transaction.Transaction.Reference1 = GetBilledPropertyValue<string>(allProperties, fieldMapping.SFM_Reference1, null);
			transaction.Transaction.Reference2 = GetBilledPropertyValue<string>(allProperties, fieldMapping.SFM_Reference2, null);
			transaction.Transaction.Reference3 = GetBilledPropertyValue<string>(allProperties, fieldMapping.SFM_Reference3, null);
			transaction.Transaction.Reference4 = GetBilledPropertyValue<string>(allProperties, fieldMapping.SFM_Reference4, null);
			transaction.Transaction.Reference5 = GetBilledPropertyValue<string>(allProperties, fieldMapping.SFM_Reference5, null);
			transaction.Transaction.BillableCount = GetBilledPropertyValue(allProperties, fieldMapping.SFM_BillableCount, 1);
			transaction.Transaction.ReportingSource = BillingManager.ReportingSource;
			transaction.Transaction.Branch = currentBranch.GB_Code;
			transaction.Transaction.AdditionalRefs = JsonConvert.SerializeObject(allProperties, Formatting.Indented);
			BillingManager.AddTransactions(new[] { transaction }, factory);
		}

		T GetBilledPropertyValue<T>(JObject allProperties, string reportedFieldName, T defaultValue)
		{
			var billedPropertyValue = GetBilledPropertyValue(allProperties, reportedFieldName);
			return (billedPropertyValue != null) ? billedPropertyValue.ToObject<T>() : defaultValue;
		}

		DateTime GetBilledPropertyValueAsUniversalDateTime(JObject allProperties, string reportedFieldName, DateTime defaultValue)
		{
			var billedPropertyValue = GetBilledPropertyValue(allProperties, reportedFieldName);
			return (billedPropertyValue != null) ? (DateTime.TryParse(billedPropertyValue.ToObject<string>(), CultureInfo.CurrentCulture, DateTimeStyles.AdjustToUniversal, out var result) ? result : defaultValue) : defaultValue;
		}

		JToken GetBilledPropertyValue(JObject allProperties, string reportedFieldName)
		{
			if (string.IsNullOrEmpty(reportedFieldName))
			{
				return null;
			}

			return allProperties.Properties().FirstOrDefault(kp => kp.Name.Equals(reportedFieldName, StringComparison.InvariantCultureIgnoreCase))?.Value;
		}
	}
}
