using System;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Utility.Testing
{
	public static class MenuItemTestHelper
	{
		public static void ValidateDefaultARInvoiceDateMenuText(Func<string> menuTextGetter)
		{
			Func<string> menuText = () =>
			{
				var text = menuTextGetter();

				if (text.StartsWith("Current Invoice Date"))
				{
					return text;
				}

				return null;
			};

			var today = ZDateTime.Now;

			//registry not configed
			var menu = menuText();
			Assertion.AssertEquals("should be no menu", menu, null);

			//default registry
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.Default.Code);
			menu = menuText();
			Assertion.AssertEquals("should be no menu", menu, null);

			//MonthEndSuspension configed but hidden registry CurrentInvoiceDate not configed
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code);
			Assertion.AssertEquals(AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.Value, DateTime.MinValue);
			menu = menuText();
			Assertion.AssertEquals("should be no menu", menu, null);

			//MonthEndSuspension configed, hidden registry CurrentInvoiceDate configed as current year/month
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, today.AddDays(2).ToDateTime());
			menu = menuText();
			Assertion.AssertEquals("should be today", "Current Invoice Date : " + today.ToShortDateString(), menu);

			//MonthEndSuspension configed, hidden registry CurrentInvoiceDate configed as not current year/month
			var configDateTime = today.AddMonths(1).ToDateTime();
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, configDateTime);
			menu = menuText();
			Assertion.AssertEquals("should be the last day of the configed year/month",
				"Current Invoice Date : " + (new ZDateTime(configDateTime.Year, configDateTime.Month, DateTime.DaysInMonth(configDateTime.Year, configDateTime.Month))).ToShortDateString()
				, menu);
		}
	}
}
