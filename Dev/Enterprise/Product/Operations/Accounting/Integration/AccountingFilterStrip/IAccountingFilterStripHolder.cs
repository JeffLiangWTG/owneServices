using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Integration
{
	public interface IAccountingFilterStripHolder
	{
		ZQuery TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery);
		ZBool IsFilterStripForParentTable { get; }
		BusinessObjectFactory Factory { get; }
		MultilingualString AmountFiltersCategoryNameOveride { get; }
		MultilingualString BillingFiltersCategoryNameOveride { get; }
		MultilingualString FilterNameSuffixInOtherCategories { get; }
		Dictionary<string, object> AccountingFilterStripConfiguration { get; }
	}

	[Flags]
	public enum InvoicedChargesFilterOptions
	{
		None = 0,
		Default = 1 << 0,
		NotInvoicedOnly = 1 << 1,
		NotInvoicedImmediate = 1 << 2,
		NotInvoicedDeferred = 1 << 3,
		CostsNotPostedOnly = 1 << 4,
		NoChargesOnly = 1 << 5,
		NoCostsOnly = 1 << 6,
		LocalBillingNotPaid = 1 << 7
	}

	public static class AccountingFilterStripConfigurationKeys
	{
		public const string BusinessObjectType = "BusinessObjectType";
		public const string InvoicedChargesFilterOptionsSelected = "InvoicedChargesFilterOptionsSelected";
		public const string JobHeaderBusinessObjectType = "JobHeaderBusinessObjectType";
		public const string JobHeaderAdditionalKeyColumn = "JobHeaderAdditionalKeyColumn";
		public const string InvoicingJobStatusFilterNameOverride = "InvoicingJobStatusFilterNameOverride";
		public const string InvoicingJobStatusFilterMakeCustomFilter = "InvoicingJobStatusFilterMakeCustomFilter";
		public const string InvoicedChargesFilterNameOverride = "InvoicedChargesFilterNameOverride";
	}
}
