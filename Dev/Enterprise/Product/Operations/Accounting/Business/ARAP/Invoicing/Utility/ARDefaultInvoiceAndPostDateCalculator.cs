using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public static class ARDefaultInvoiceAndPostDateCalculator
	{
		public static bool ShouldUseDefaultDate()
		{
			return (AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.Value == AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code);
		}

		public static ZDateTime GetDefaultDate(ZDateTime defaultDate, GlbCompany company = null)
		{
			ZDateTime invoiceDate = defaultDate;

			var invDate = GetDefaultDate(company);
			if (invDate.HasValue)
			{
				invoiceDate = invDate.Value;
			}

			return invoiceDate;
		}

		public static MultilingualString GetDefaultDateMenuCaption()
		{
			var invoiceDate = GetDefaultDate();
			return invoiceDate.HasValue ? ResString.GetMultilingualString("dc9b58f1-973d-47e9-809a-0c36d0b0ff1f", "Current Invoice Date : {0}", invoiceDate.Value.ToShortDateString()) : null;
		}

		public static string CanClosePeriod(Period period)
		{
			if (AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.Value == AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code)
			{
				var invoiceDate = GetDefaultDate();
				if (invoiceDate.HasValue)
				{
					var invoicdDateOnly = invoiceDate.Value.Date;
					if (invoicdDateOnly >= period.AM_StartDate.Date && invoicdDateOnly <= period.AM_EndDate.Date)
					{
						return Res.GetString("ccbc5036-a801-46b5-aed5-6fefadced689", @"You cannot close the period as current invoice date is within the period.
Please review or reinstate the invoice date before closing the period.");
					}
				}
			}

			return string.Empty;
		}

		public static bool CheckSuspensionNeedsToBeLifted(ZDateTime dateTime, GlbCompany company)
		{
			var currentInvoiceDate = AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);

			return currentInvoiceDate.Year != dateTime.Year || currentInvoiceDate.Month != dateTime.Month;
		}

		public static string DefaultInvoiceDateReadOnlyWarningText
		{
			get
			{
				return Res.GetString("402960c8-ba00-42b0-b7f5-a01d8c325a76", @"Invoice Date is non-editable as the Registry 'Invoice and Post Dates Defaulting Behavior' is set to 'MTH - Month End Suspension'.");
			}
		}

		public static string DefaultPostDateReadOnlyWarningText
		{
			get
			{
				return Res.GetString("351533ca-95ce-4190-b9e4-98ae068f2b71", @"Post Date is non-editable as the Registry 'Invoice and Post Dates Defaulting Behavior' is set to 'MTH - Month End Suspension'.");
			}
		}

		#region implement
		static ZDateTime? GetDefaultDate(GlbCompany company = null)
		{
			var companyPK = (company ?? GlbCompany.CurrentCompany).PK.ToGuid();
			ZDateTime? invoiceDate = null;

			var invAndPstDateValue = AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
			if (invAndPstDateValue == AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code)
			{
				var currentInvoiceDate = AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);

				if (currentInvoiceDate != DateTime.MinValue)
				{
					var todayDate = ZDateTime.Now;

					if (todayDate.Year == currentInvoiceDate.Year && todayDate.Month == currentInvoiceDate.Month)
					{
						invoiceDate = todayDate;
					}
					else
					{
						invoiceDate = new ZDateTime(currentInvoiceDate.Year, currentInvoiceDate.Month, DateTime.DaysInMonth(currentInvoiceDate.Year, currentInvoiceDate.Month));
					}
				}
			}

			return invoiceDate;
		}
		#endregion
	}
}
