using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class PostDateConfigurationHelper
	{
		public PostDateConfigurationHelper(IJobInvoicingPlugIn plugIn, bool reversing = false)
		{
			this.plugIn = plugIn;
			this.Reversing = reversing;
		}

		readonly IJobInvoicingPlugIn plugIn;
		readonly bool Reversing;

		BusinessObjectFactory Factory
		{
			get { return plugIn != null ? plugIn.Factory : (factory ?? (factory = new BusinessObjectFactory())); }
		}
		BusinessObjectFactory factory;

		string JobType
		{
			get
			{
				string consumerTypeCode = PostDateConfigurationLookups.JobTypeAdditionalCodes.All;

				BusinessObject jobParentBizO = plugIn as BusinessObject;
				bool isPluginDataDeleted = jobParentBizO != null && jobParentBizO.IsDeleted;

				if (plugIn != null && !isPluginDataDeleted)
				{
					if (plugIn.InvoicingSupporter.ConsumerType != null)
					{
						consumerTypeCode = plugIn.InvoicingSupporter.ConsumerType.Code;
					}
				}

				return consumerTypeCode;
			}
		}

		string Direction
		{
			get
			{
				string directionCode = Constants.FreightShipmentDirection.Code.All;

				BusinessObject jobParentBizO = plugIn as BusinessObject;
				bool isPluginDataDeleted = jobParentBizO != null && jobParentBizO.IsDeleted;

				if (plugIn != null && !isPluginDataDeleted)
				{
					if (plugIn.InvoicingSupporter.IsDomestic)
					{
						directionCode = Constants.FreightShipmentDirection.Code.Domestic;
					}
					else if (plugIn.InvoicingSupporter.IsImport)
					{
						directionCode = Constants.FreightShipmentDirection.Code.Import;
					}
					else if (plugIn.InvoicingSupporter.IsExport)
					{
						directionCode = Constants.FreightShipmentDirection.Code.Export;
					}
					else
					{
						directionCode = Constants.FreightShipmentDirection.Code.Other;
					}
				}

				return directionCode;
			}
		}

		string Mode
		{
			get
			{
				string transportMode = PostDateConfigurationLookups.ModeAdditionalCodes.All;

				BusinessObject jobParentBizO = plugIn as BusinessObject;
				bool isPluginDataDeleted = jobParentBizO != null && jobParentBizO.IsDeleted;

				if (plugIn != null && !isPluginDataDeleted)
				{
					if (!plugIn.InvoicingSupporter.TransportMode.IsEmpty)
					{
						transportMode = (string)plugIn.InvoicingSupporter.TransportMode;
					}
				}

				return transportMode;
			}
		}

		string Broker
		{
			get
			{
				string broker = PostDateConfigurationLookups.BrokerCodes.All;

				BusinessObject jobParentBizO = plugIn as BusinessObject;
				bool isPluginDataDeleted = jobParentBizO != null && jobParentBizO.IsDeleted;

				if (plugIn != null && !isPluginDataDeleted)
				{
					if (plugIn.InvoicingSupporter.Broker != null)
					{
						broker = plugIn.InvoicingSupporter.Broker.IsProxyOrg(GlbCompany.CurrentCompany) ?
							PostDateConfigurationLookups.BrokerCodes.Internal :
							PostDateConfigurationLookups.BrokerCodes.External;
					}
				}

				return broker;
			}
		}

		public PostDateConfiguration FindPostDateConfiguration()
		{
			List<PostDateConfiguration> result = new List<PostDateConfiguration>();
			foreach (PostDateConfiguration configuration in AccountingConfigurationRegistry.Instance.BackDateAPInvoicesConfiguration.Value.PostDateConfigurationCollection)
			{
				if ((JobType == null ||
					 string.Equals(configuration.JobType, JobType, StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(configuration.JobType, PostDateConfigurationLookups.JobTypeAdditionalCodes.All, StringComparison.OrdinalIgnoreCase))
					&&
					(Direction == null ||
					 string.Equals(configuration.DirectionCode, Constants.FreightShipmentDirection.Code.All, StringComparison.OrdinalIgnoreCase) ||
					 configuration.DirectionCode == "" ||
					 string.Equals(configuration.DirectionCode, Direction, StringComparison.OrdinalIgnoreCase))
					&&
					(Mode == null ||
					 string.Equals(configuration.Mode, Mode, StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(configuration.Mode, PostDateConfigurationLookups.ModeAdditionalCodes.All, StringComparison.OrdinalIgnoreCase) ||
					 configuration.Mode == "")
					 &&
					(Broker == null ||
					 string.Equals(configuration.BrokerCode, Broker, StringComparison.OrdinalIgnoreCase) ||
					 string.Equals(configuration.BrokerCode, PostDateConfigurationLookups.BrokerCodes.All, StringComparison.OrdinalIgnoreCase) ||
					 configuration.BrokerCode == ""))
				{
					result.Add(configuration);
				}
			}

			return result.Count > 0 ? result[0] : null;
		}

		public ZDateTime GetPostDate(ZDateTime defaultDate, ZDateTime invoiceDate, ZDateTime? originalPostDate = null)
		{
			ZDateTime postDate = defaultDate;
			PostDateConfiguration configuration = FindPostDateConfiguration();
			if (configuration != null)
			{
				postDate = GetPostDate(configuration, defaultDate, invoiceDate, originalPostDate);
			}
			return postDate;
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

		ZDateTime GetPostDate(ZString significantDatePeriodCode, ZDateTime defaultDate, ZDateTime significantDate, ZDateTime invoiceDate)
		{
			ZDateTime postDate = defaultDate;

			switch (significantDatePeriodCode)
			{
				case PostDateConfigurationLookups.SignificantDatePeriodCodes.InvoiceAddDate:
					postDate = defaultDate;
					break;
				case PostDateConfigurationLookups.SignificantDatePeriodCodes.SignificantDate:
					postDate = significantDate;
					break;
				case PostDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorMonth:
					postDate = (new ZDateTime(ZDateTime.Today.Year, ZDateTime.Today.Month, 1)).AddDays(-1);
					break;
				case PostDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorPeriod:
					postDate = GetEndOfPriorPeriod();
					break;
				case PostDateConfigurationLookups.SignificantDatePeriodCodes.InvoiceDate:
					postDate = invoiceDate;
					break;
				case PostDateConfigurationLookups.SignificantDatePeriodCodes.FirstDayOfFirstOpenPeriodAfterSignificantDate:
					postDate = GetFirstDayOfFirstOpenPeriodAfterDate(significantDate);
					break;
			}

			return postDate;
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

		ZDateTime GetPostDate(PostDateConfiguration configuration, ZDateTime significantDate, ZDateTime defaultDate, ZDateTime invoiceDate)
		{
			ZDateTime postDate = defaultDate;

			PeriodType periodType = GetPeriodType(significantDate);

			switch (periodType)
			{
				case PeriodType.PriorClosedPeriod:
					postDate = GetPostDate(configuration.PriorClosedPeriod, defaultDate, significantDate, invoiceDate);
					break;
				case PeriodType.PriorOpenPeriod:
					postDate = GetPostDate(configuration.PriorOpenPeriod, defaultDate, significantDate, invoiceDate);
					break;
				case PeriodType.CurrentPeriod:
					postDate = GetPostDate(configuration.CurrentPeriod, defaultDate, significantDate, invoiceDate);
					break;
				case PeriodType.FuturePeriod:
					postDate = GetPostDate(configuration.FuturePeriod, defaultDate, significantDate, invoiceDate);
					break;
			}

			return postDate;
		}

		ZDateTime GetPostDate(PostDateConfiguration configuration, ZDateTime defaultDate, ZDateTime invoiceDate, ZDateTime? originalPostDate)
		{
			ZDateTime postDate = defaultDate;
			AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(Factory);
			ZInt period = 0;

			if (Reversing && configuration.ReversalRule != PostDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules && originalPostDate.HasValue)
			{
				//originalPostDate is Original Transaction Post Date

				postDate = originalPostDate.Value;
				period = periodCalculator.GetPeriodFromDate(postDate);

				if (configuration.ReversalRule == PostDateConfigurationLookups.ReversalRuleCodes.DefaultFromOriginalTransactionPostDateOrCurrentDate)
				{
					if (period == 0 || periodCalculator.IsPeriodSubLedgerClosed(period))
					{
						postDate = ZDateTime.Today;
					}
				}
				else if (configuration.ReversalRule == PostDateConfigurationLookups.ReversalRuleCodes.DefaultFromOriginalTransactionPostDateOrFirstDayOfFirstOpenPeriod)
				{
					if (period == 0 || periodCalculator.IsPeriodSubLedgerClosed(period))
					{
						AccPeriodManagement periodManagement = periodCalculator.GetFirstOpenPeriod(GlbCompany.CurrentCompany.PK);
						if (periodManagement != null)
						{
							postDate = periodManagement.AM_StartDate;
						}
						else
						{
							postDate = ZDateTime.Today;
						}
					}
				}
			}
			else if (configuration.SignificantDateCode == PostDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate)
			{
				postDate = GetPostDate(configuration.CurrentPeriod, defaultDate, defaultDate, invoiceDate);
			}
			else
			{
				ZDateTime significantDate = ZDateTime.Empty;

				if (configuration.SignificantDateCode == PostDateConfigurationLookups.SignificantDateCodes.JobOpenDate && plugIn != null)
				{
					Job.Loader loader = new Job.Loader(plugIn);
					Job job = loader.Load();
					if (job != null)
					{
						significantDate = job.JH_A_JOP;
					}
				}
				else if (configuration.SignificantDateCode == PostDateConfigurationLookups.SignificantDateCodes.InvoiceDate)
				{
					significantDate = invoiceDate;
				}
				else if (plugIn != null)
				{
					significantDate = plugIn.InvoicingSupporter.GetOperationsSignificantDate(configuration.SignificantDateCode);
				}

				if (significantDate == ZDateTime.Empty)
				{
					significantDate = defaultDate;
				}

				postDate = GetPostDate(configuration, significantDate, defaultDate, invoiceDate);
			}

			if (Reversing
				&& configuration.ReversalRule == PostDateConfigurationLookups.ReversalRuleCodes.StandardPostingRules
				&& originalPostDate.HasValue
				&& postDate < originalPostDate.Value)
			{
				postDate = originalPostDate.Value;
			}

			period = periodCalculator.GetPeriodFromDate(postDate);

			if (periodCalculator.IsPeriodSubLedgerClosed(period) || postDate.Date > ZDateTime.Today || period == 0)
			{
				postDate = defaultDate;
			}

			return postDate;
		}
	}
}
