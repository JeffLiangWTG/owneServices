using System;
using Enterprise.Customs.Common.GUI;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Common.Module
{
	public static class CusEntryNumberFilterStripHelper
	{
		public static void AddFilters(ModuleFilterCollection filters, Type businessObjectType)
		{
			AddReferenceNumberFilter(filters, businessObjectType);
			AddReferenceNumberDateFilter(filters, businessObjectType);
		}

		public static void AddReferenceNumberFilter(ModuleFilterCollection filters, Type businessObjectType)
		{
			filters.AddCustomFilter(
				new CusEntryNumTextFilter(CusEntryNumberFilterStripDescriptions.AdditionalReferenceNumber, businessObjectType)
				{
					MultilingualDescription = ResString.GetMultilingualString("Customs|CusEntryNumberFilterStripHelper|AdditionalReferenceNumber", "Additional Reference Number")
				});
		}

		public static void AddReferenceNumberDateFilter(ModuleFilterCollection filters, Type businessObjectType)
		{
			filters.AddCustomFilter(
				new CusEntryNumDateFilter(
					CusEntryNumberFilterStripDescriptions.AdditionalReferenceNumberIssueDate, businessObjectType)
				{
					MultilingualDescription = ResString.GetMultilingualString("Customs|CusEntryNumberFilterStripHelper|AdditionalReferenceNumberIssueDate", "Additional Ref No Issue Date")
				});
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
	public static class CusEntryNumberFilterStripDescriptions
	{
		public const string AdditionalReferenceNumber = "Additional Reference Number";
		public const string AdditionalReferenceNumberIssueDate = "Additional Ref Num Issue Date";
	}
}
