using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsInvoicePostingAccountingIntegrator
	{
		public NctsInvoicePostingAccountingIntegrator()
		{
		}

		public NctsInvoicePostingAccountingIntegrator(LoggingInformation logger)
		{
			this.logger = logger;
		}

		readonly LoggingInformation logger;

		public bool HasExceptionDuringIntegration { get; private set; }

		public IAutoBillingResult IntegrateIfNecessary(IAccIntegrationDataProvider integrationDataProvider)
		{
			var result = new AutoBillingResult();

			if (integrationDataProvider.SupportIntegration)
			{
				try
				{
					if (integrationDataProvider.Company.PK != GlbCompany.CurrentCompany.PK)
					{
						result.Message = Enterprise.Customs.Business.InvoicePostingAccountingIntegrator.GetAutoBillingCompanyMismatchText(integrationDataProvider);
						logger?.Log(result.Message);
					}
					else
					{
						if (integrationDataProvider.Action > 0)
						{
							// NCTS doesn't have any charges for fees but if one day you want to add them you can detail them here.
						}
						result.WasSuccessful = true;
					}
				}
				catch (ZSaveConcurrencyException)
				{
					result.WasSuccessful = false;
					result.Message = Customs.Business.IAccIntegrationDataProviderExtensionMethods.ConcurrencyExceptionUserExplanation;
					logger?.Log(result.Message);
					HasExceptionDuringIntegration = true;
				}
				catch (CustomsInvoiceRaiseException invoicingException)
				{
					result.WasSuccessful = false;
					result.Message = invoicingException.Message;
					logger?.Log(result.Message);
					HasExceptionDuringIntegration = true;
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					result.WasSuccessful = false;
					result.Message = Enterprise.Customs.Business.InvoicePostingAccountingIntegrator.GetBillingFailureMessage(e);
					logger?.Log(result.Message);
					ErrorReporter.ReportOnce("Invoicing broken for " + integrationDataProvider.JobType, e);
					HasExceptionDuringIntegration = true;
				}

				if (HasExceptionDuringIntegration)
				{
					Customs.Business.IAccIntegrationDataProviderExtensionMethods.SendEmailOnException(integrationDataProvider, result.Message);
				}
			}

			return result;
		}
	}
}
