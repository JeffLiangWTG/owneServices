using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class InvoiceDateConfigurationHelper
	{
		public InvoiceDateConfigurationHelper(OperationsJobConfigurationCodes codes, IJobInvoicingPlugIn jobPlugin, bool reversing = false)
		{
			JobType = codes.ConsumerTypeCode;
			Direction = codes.DirectionCode;
			Mode = codes.TransportMode;
			Broker = codes.Broker;
			JobPlugin = jobPlugin;
			Reversing = reversing;
		}

		readonly string JobType;
		readonly string Direction;
		readonly string Mode;
		readonly string Broker;
		readonly IJobInvoicingPlugIn JobPlugin;
		readonly bool Reversing;

		public InvoiceDateConfiguration FindInvoiceDateConfiguration()
		{
			List<InvoiceDateConfiguration> result = new List<InvoiceDateConfiguration>();
			foreach (InvoiceDateConfiguration configuration in AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.Value.InvoiceDateConfigurationCollection)
			{
				if ((JobType == null ||
					 string.Equals(configuration.JobType, JobType, StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(configuration.JobType, InvoiceDateConfigurationLookups.JobTypeAdditionalCodes.All, StringComparison.OrdinalIgnoreCase))
					&&
					(Direction == null ||
					 string.Equals(configuration.DirectionCode, Constants.FreightShipmentDirection.Code.All, StringComparison.OrdinalIgnoreCase) ||
					 configuration.DirectionCode == "" ||
					 string.Equals(configuration.DirectionCode, Direction, StringComparison.OrdinalIgnoreCase))
					&&
					(Mode == null ||
					 string.Equals(configuration.Mode, Mode, StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(configuration.Mode, InvoiceDateConfigurationLookups.ModeAdditionalCodes.All, StringComparison.OrdinalIgnoreCase) ||
					 configuration.Mode == "")
					 &&
					(Broker == null ||
					 string.Equals(configuration.BrokerCode, Broker, StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(configuration.BrokerCode, InvoiceDateConfigurationLookups.BrokerCodes.All, StringComparison.OrdinalIgnoreCase) ||
					 configuration.BrokerCode == ""))
				{
					result.Add(configuration);
				}
			}

			return result.Count > 0 ? result[0] : null;
		}

		public bool CanBackPost
		{
			get
			{
				bool result = false;

				InvoiceDateConfiguration configuration = FindInvoiceDateConfiguration();
				if (configuration != null)
				{
					if (configuration.SignificantDateCode == InvoiceDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate
						&& (configuration.CurrentPeriod == InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorMonth
							|| configuration.CurrentPeriod == InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorPeriod))
					{
						result = true;
					}
					else if  (configuration.SignificantDateCode != InvoiceDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate
						&& (configuration.CurrentPeriod == InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorMonth
							|| configuration.CurrentPeriod == InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorPeriod
							|| configuration.CurrentPeriod == InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate
							|| configuration.PriorClosedPeriod == InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorMonth
							|| configuration.PriorClosedPeriod == InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorPeriod
							|| configuration.PriorClosedPeriod == InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate
							|| configuration.PriorClosedPeriod == InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.FirstDayOfFirstOpenPeriodAfterSignificantDate
							|| configuration.PriorOpenPeriod == InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorMonth
							|| configuration.PriorOpenPeriod == InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorPeriod
							|| configuration.PriorOpenPeriod == InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate
							|| configuration.FuturePeriod == InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate))
					{
						result = true;
					}
				}

				return result;
			}
		}

		public ZDateTime GetInvoiceDate(ZDateTime invoiceCreateDate)
		{
			ZDateTime invoiceDate = invoiceCreateDate;
			InvoiceDateConfiguration configuration = FindInvoiceDateConfiguration();
			if (configuration != null)
			{
				invoiceDate = GetInvoiceDate(configuration, invoiceCreateDate);
			}
			return invoiceDate;
		}

		enum PeriodType { PriorClosedPeriod, PriorOpenPeriod, CurrentPeriod, FuturePeriod, Unknown }

		PeriodType GetPeriodType(ZDateTime significantDate)
		{
			PeriodType result = PeriodType.Unknown;
			AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(Factory);
			ZInt period = periodCalculator.GetPeriodFromDate(significantDate);
			if (periodCalculator.IsCurrentPeriod(period))
			{
				result = PeriodType.CurrentPeriod;
			}
			else if (periodCalculator.IsPreviousPeriod(period) && periodCalculator.IsPeriodSubLedgerClosed(period))
			{
				result = PeriodType.PriorClosedPeriod;
			}
			else if (periodCalculator.IsPreviousPeriod(period) && !periodCalculator.IsPeriodSubLedgerClosed(period))
			{
				result = PeriodType.PriorOpenPeriod;
			}
			else if (periodCalculator.IsFuturePeriod(period))
			{
				result = PeriodType.FuturePeriod;
			}
			return result;
		}

		ZDateTime GetInvoiceDate(ZString significantDatePeriodCode, ZDateTime invoiceCreateDate, ZDateTime significantDate)
		{
			ZDateTime invoiceDate = invoiceCreateDate;

			switch (significantDatePeriodCode)
			{
				case InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.InvoiceAddDate:
					invoiceDate = invoiceCreateDate;
					break;
				case InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate:
					invoiceDate = significantDate;
					break;
				case InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorMonth:
					invoiceDate = (new ZDateTime(ZDateTime.Today.Year, ZDateTime.Today.Month, 1)).AddDays(-1);
					break;
				case InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorPeriod:
					invoiceDate = GetEndOfPriorPeriod();
					break;
				case InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.FirstDayOfFirstOpenPeriodAfterSignificantDate:
					invoiceDate = GetFirstDayOfFirstOpenPeriodAfterDate(significantDate);
					break;
			}

			return invoiceDate;
		}

		ZDateTime GetFirstDayOfFirstOpenPeriodAfterDate(ZDateTime date)
		{
			AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(Factory);
			AccPeriodManagement period = periodCalculator.GetNextSubLedgerOpenPeriodManagementFromDate(date, GlbCompany.CurrentCompany.PK);
			return period != null ? period.AM_StartDate : ZDateTime.Today;
		}

		ZDateTime GetEndOfPriorPeriod()
		{
			AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(Factory);
			return periodCalculator.GetLastDayForPeriod(periodCalculator.GetPreviousPeriod(periodCalculator.GetPeriodFromDate(ZDateTime.Today)));
		}

		ZDateTime GetInvoiceDate(InvoiceDateConfiguration configuration, ZDateTime significantDate, ZDateTime invoiceCreateDate)
		{
			ZDateTime invoiceDate = invoiceCreateDate;

			PeriodType periodType = GetPeriodType(significantDate);

			switch (periodType)
			{
				case PeriodType.PriorClosedPeriod:
					if (Reversing && configuration.PriorClosedPeriod == InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.InvoiceAddDate)
					{
						invoiceCreateDate = ZDateTime.Today;
					}
					invoiceDate = GetInvoiceDate(configuration.PriorClosedPeriod, invoiceCreateDate, significantDate);
					break;
				case PeriodType.PriorOpenPeriod:
					if (Reversing && configuration.PriorOpenPeriod == InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.InvoiceAddDate)
					{
						invoiceCreateDate = ZDateTime.Today;
					}
					invoiceDate = GetInvoiceDate(configuration.PriorOpenPeriod, invoiceCreateDate, significantDate);
					break;
				case PeriodType.CurrentPeriod:
					if (Reversing && configuration.CurrentPeriod == InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.InvoiceAddDate)
					{
						invoiceCreateDate = ZDateTime.Today;
					}
					invoiceDate = GetInvoiceDate(configuration.CurrentPeriod, invoiceCreateDate, significantDate);
					break;
				case PeriodType.FuturePeriod:
					if (Reversing && configuration.FuturePeriod == InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.InvoiceAddDate)
					{
						invoiceCreateDate = ZDateTime.Today;
					}
					invoiceDate = GetInvoiceDate(configuration.FuturePeriod, invoiceCreateDate, significantDate);
					break;
			}

			return invoiceDate;
		}

		ZDateTime GetInvoiceDate(InvoiceDateConfiguration configuration, ZDateTime invoiceCreateDate)
		{
			ZDateTime invoiceDate = invoiceCreateDate;

			if (Reversing && configuration.ReversalRule != InvoiceDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules)
			{
				//assume invoiceCreateDate is Original Transaction Invoice Date

				AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(Factory);
				ZInt period = periodCalculator.GetPeriodFromDate(invoiceDate);

				if (configuration.ReversalRule == InvoiceDateConfigurationLookups.ReversalRuleCodes.DefaultFromOriginalTransactionInvoiceDateOrCurrentDate)
				{
					if (period == 0 || periodCalculator.IsPeriodSubLedgerClosed(period))
					{
						invoiceDate = ZDateTime.Today;
					}
				}
				else if (configuration.ReversalRule == InvoiceDateConfigurationLookups.ReversalRuleCodes.DefaultFromOriginalTransactionInvoiceDateOrFirstDayOfFirstOpenPeriod)
				{
					if (periodCalculator.IsPeriodSubLedgerClosed(period))
					{
						AccPeriodManagement periodManagement = periodCalculator.GetFirstOpenPeriod(GlbCompany.CurrentCompany.PK);
						if (periodManagement != null)
						{
							invoiceDate = periodManagement.AM_StartDate;
						}
						else
						{
							invoiceDate = ZDateTime.Today;
						}
					}
				}
			}
			else if (configuration.SignificantDateCode == InvoiceDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate)
			{
				invoiceDate = GetInvoiceDate(configuration.CurrentPeriod, invoiceCreateDate, invoiceCreateDate);
			}
			else
			{
				ZDateTime significantDate = ZDateTime.Empty;

				if (configuration.SignificantDateCode == InvoiceDateConfigurationLookups.SignificantDateCodes.JobOpenDate)
				{
					Job.Loader loader = new Job.Loader(JobPlugin);
					Job job = loader.Load();
					if (job != null)
					{
						significantDate = job.JH_A_JOP;
					}
				}
				else
				{
					significantDate = JobPlugin.InvoicingSupporter.GetOperationsSignificantDate(configuration.SignificantDateCode);
				}

				if (significantDate == ZDateTime.Empty)
				{
					significantDate = invoiceCreateDate;
				}

				invoiceDate = GetInvoiceDate(configuration, significantDate, invoiceCreateDate);
			}

			return invoiceDate;
		}

		BusinessObjectFactory Factory
		{
			get { return JobPlugin != null ? JobPlugin.Factory : (factory ?? (factory = new BusinessObjectFactory())); }
		}
		BusinessObjectFactory factory;
	}
}