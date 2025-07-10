using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Diagnostics;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	public partial class JobConsolCost
	{
		#region AP Invoice Consol Cost Calcalation Strategy (AP Invoice Consol App. Screen)

		public class InvoicingBaseConsolCostCalculationStrategy : ConsolCostCalculationStrategyWithCalculations
		{
			public InvoicingBaseConsolCostCalculationStrategy(JobConsolCost parent)
				: base(parent)
			{
				Tracer = ObjectFactory.Get<ITracer>();
			}

			protected override void DefaultExchangeRate()
			{
				var shouldUseHeaderRate = Cost.ParentAPInvoice != null && !Cost.ParentAPInvoice.AH_PostedToEFT && Cost.IsForeignCurrencyParentAPInvoice;
				if (!shouldUseHeaderRate)
				{
					base.DefaultExchangeRate();
				}
			}

			protected override ZDecimal GetExchangeRateFromRateFinder(RefCurrency currency, OrgHeader creditor = null)
			{
				var exchangeRate = base.GetExchangeRateFromRateFinder(currency);
				if (Cost.ParentAPInvoice != null)
				{
					var isLocalCurrency = Cost.ParentAPInvoice.IsLocalCurrencyTransaction;
					if (ExchangeRateCalculator.IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AP, isLocalCurrency, Cost.E6_GC))
					{
						if (Cost.ParentAPInvoice.AH_PostedToEFT)
						{
							// Invoice Posting Exchange Rate Option is not DEF and Use Job Exchange Rate flag ticked
							exchangeRate = AccExchangeRateConfigurationRateFinder.GetExchangeRateBasedOnInvoicePostingOption(Cost.ExchangeRateConfigurationRateConsumer, currency, isLocalCurrency,
								Cost.E6_GC, Cost.Creditor, ExchangeRateValidLedgerEnum.AP, Cost.ParentAPInvoice.AH_InvoiceDate, Cost.ParentAPInvoice.AH_PostDate, Cost.ParentAPInvoice.InvoiceTaxDate);
						}
						else if (isLocalCurrency)
						{
							// Invoice Posting Exchange Rate Option is not DEF and Invoice header currency is local
							exchangeRate = AccExchangeRateConfigurationRateFinder.GetBuyExchangeRateBasedOnInvoicePostingOption(Cost.ExchangeRateConfigurationRateConsumer, currency, isLocalCurrency,
								Cost.E6_GC, ExchangeRateValidLedgerEnum.AP, Cost.ParentAPInvoice.AH_InvoiceDate, Cost.ParentAPInvoice.AH_PostDate, Cost.ParentAPInvoice.InvoiceTaxDate);
						}
					}
					else if (!Cost.ParentAPInvoice.AH_PostedToEFT)
					{
						// Invoice Posting Exchange Rate Option is DEF and Use Job Exchange Rate flag not ticked
						exchangeRate = AccExchangeRateConfigurationRateFinder.GetBuyExchangeRateForDate(Cost.ExchangeRateConfigurationRateConsumer, currency, ZDateTime.Today);
					}
				}
				return exchangeRate;
			}

			public override void OnE6_ParentIDConsolSet()
			{
				base.OnE6_ParentIDConsolSet();
				Cost.UpdateCostTaxDateBasedOnRegistry();
				if (Cost.E6_ParentID.IsValid)
				{
					var costAmount = Cost.E6_OSCostAmount;
					if (!Cost.E6_OSCostAmount.IsEmpty)
					{
						Cost.E6_OSCostAmount = ZDecimal.Zero;
					}
					Cost.SetGSTRateAndTaxMessage();
					Cost.UpdateGovtChargeCode();
					Cost.UpdateApportionmentMethod();
					UpdateApportionmentChargesListing(); // can reset consol to empty when there is a mutex conflict
					if (!costAmount.IsEmpty)
					{
						Cost.E6_OSCostAmount = costAmount;
					}

					if (Cost.E6_ParentID.IsValid)
					{
						if (Cost.ParentAPInvoice != null && !Cost.ParentAPInvoice.IsConsolCostImportPopupSuspended)
						{
							InvoicingBaseConsolCostImporter costImporter = GetCostImporter();
							if (costImporter.CostsCollection.Count > 0)
							{
								if (Cost.E6_AC_ChargeCode.IsValid && Cost.E6_ParentID.IsValid)
								{
									var costInDB = GetTheOnlyCostWithMatchingOrEmptyCreditor(costImporter.CostsCollection.Cast<InvoicingBaseConsolCostForImporting>());

									if (costInDB != null)
									{
										costImporter.ImportCostsIntoCosting(new BusinessObject[1] { costInDB });
									}
									else
									{
										Cost.RaiseOnConsolChanged(costImporter);
									}
								}
								else
								{
									Cost.RaiseOnConsolChanged(costImporter);
								}
							}
						}
					}
				}
			}

			InvoicingBaseConsolCostImporter GetCostImporter()
			{
				return new InvoicingBaseConsolCostImporter(ImporterFactory, Cost, Cost.ParentAPInvoice);
			}

			BusinessObjectFactory ImporterFactory
			{
				get { return importerFactory ?? (importerFactory = new BusinessObjectFactory()); }
			}
			BusinessObjectFactory importerFactory;

			public override void UpdateApportionmentChargesListing()
			{
				if (Cost.Consol != null)
				{
					foreach (IJobInvoicingPlugIn shipment in Cost.ShipmentsToApportion)
					{
						var localClientJobHandler = shipment as ILocalClientJobHandler;
						var jobHandler = new LocalClientJobHandler(localClientJobHandler);
						var job = jobHandler.JobLoader.Load() as Job;

						if (job == null)
						{
							if (Cost.IsConsolChangedSuspended)
							{
								continue;
							}

							jobHandler.Initialize();
							job = localClientJobHandler.JobHeader as Job;

							if (job == null)
							{
								if (jobHandler.IsClientJobHandlerEligibleToCreateJob)
								{
									ReleaseMutexes();
									RaiseJobCreationError(jobHandler.JobLoader.GetJobCreationError());
									using (Cost.ReportSettingParentSuspender.GetSuspender())
									{
										Cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(ZGuid.Empty, ZString.Empty);
									}
									break;
								}

								if (!jobHandler.HasJobCreationTriggered && !string.IsNullOrEmpty(jobHandler.InitializationMessage))
								{
									((CommonShipment)shipment).AddRowWarning(jobHandler.InitializationMessage);
									continue;
								}
							}
						}

						if (job != null && !Cost.JobsWithMutexes.Contains(job) && (!job.IsInDatabase || job.IsJobActivating))
						{
							Cost.JobsWithMutexes.Add(job);
						}

						IDisposable jobValidationSuspender = Cost.IsValidationSuspended ? job.GetValidationSuspender() : null;
						IDisposable setJobDefaultsSuspender = Cost.IsConsolChangedSuspended ? job.GetSetJobDefaultsSuspender() : null;
						try
						{
							if (job.PlugInData != shipment)
							{
								job.PlugInData = shipment;
							}

							if (job.JH_GE.IsEmpty)
							{
								job.JH_GE = GlbDepartment.CurrentDepartment.PK;
							}

							if (job.JH_GB.IsEmpty)
							{
								job.JH_GB = GlbBranch.CurrentBranch.PK;
							}
						}
						finally
						{
							if (jobValidationSuspender != null)
							{
								jobValidationSuspender.Dispose();
							}

							if (setJobDefaultsSuspender != null)
							{
								setJobDefaultsSuspender.Dispose();
							}
						}
					}

					base.UpdateApportionmentChargesListing();

					Tracer.TraceInformation(AccountingTraceSourceCodes.ConsolCost, () => Cost.BuildTraceMessageForApportionedCharges(FormattableString.Invariant($"Traced @{GetType().FullName}.{nameof(UpdateApportionmentChargesListing)}- after calling base.{nameof(UpdateApportionmentChargesListing)}")));

					if (!Cost.IsPosted)
					{
						Cost.SplitApportionAmount();
						Cost.ApportionGSTCharges();
						Tracer.TraceInformation(AccountingTraceSourceCodes.ConsolCost, () => Cost.BuildTraceMessageForApportionedCharges(FormattableString.Invariant($"Traced @{GetType().FullName}.{nameof(UpdateApportionmentChargesListing)}- after calling Cost.{nameof(Cost.SplitApportionAmount)}")));
						Cost.SetIsUsedForApportionment();
						Tracer.TraceInformation(AccountingTraceSourceCodes.ConsolCost, () => Cost.BuildTraceMessageForApportionedCharges(FormattableString.Invariant($"Traced @{GetType().FullName}.{nameof(UpdateApportionmentChargesListing)}- after calling Cost.{nameof(Cost.SetIsUsedForApportionment)}")));
					}
				}
			}

			InvoicingBaseConsolCostForImporting GetTheOnlyCostWithMatchingOrEmptyCreditor(IEnumerable<InvoicingBaseConsolCostForImporting> costsCollection)
			{
				InvoicingBaseConsolCostForImporting result = null;

				var getPriority = new Func<JobConsolCost, int>((x) => x.E6_OH_Creditor.IsEmpty ? 2 : x.E6_OH_Creditor == Cost.E6_OH_Creditor ? 1 : 3);

				var costsWithPriority = costsCollection.Select(x => new { cost = x, priority = getPriority(x) })
														.Where(x => x.priority == 1 || x.priority == 2)
														.OrderBy(x => x.priority).ToList();

				var topCostPriority = costsWithPriority.FirstOrDefault();
				if (topCostPriority != null &&
					!costsWithPriority.Any(x => x.priority == topCostPriority.priority && x != topCostPriority))
				{
					result = topCostPriority.cost;
				}

				return result;
			}

			void RaiseJobCreationError(string errorMessage)
			{
				if (Cost.APInvoiceConsolCostingJobCreationError != null)
				{
					Cost.APInvoiceConsolCostingJobCreationError(this, new APInvoiceCostingJobCreationErrorEventArgs(errorMessage));
				}
				else
				{
					throw new JobCreationException(errorMessage);
				}
			}

			public override void HandleDelete()
			{
				base.HandleDelete();

				foreach (var job in Cost.JobsWithMutexes)
				{
					if (!job.IsDeleted && !job.IsInDatabase && !job.HasTransactions)
					{
						job.Delete();
					}
					else if (job.IsJobActivating && !job.HasTransactions)
					{
						job.MarkAsInactive();
					}
				}
			}

			readonly ITracer Tracer;
		}

		#endregion
	}
}
