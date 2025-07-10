using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public interface IInvoicingBaseLineImporter
	{
		public void ImportLinesFromChargeCollection(InvoicingBase invoice, IEnumerable<Charge> charges);
		public void ImportLinesFromConsolCostCollection(IInvoicingBaseImporterTarget invoice, IEnumerable<JobConsolCost> consolCosts);
	}

	public interface IInvoicingBaseImporterTarget
	{
		public BusinessObjectFactory Factory { get; }
		public APInvoiceConsolCosting ConsolCosting { get; }
		public void ImportAllApportionmentsFromCosting_SuspendListChanged();
	}

	public class InvoicingBaseLineImporter : IInvoicingBaseLineImporter
	{
		void IInvoicingBaseLineImporter.ImportLinesFromChargeCollection(InvoicingBase invoice, IEnumerable<Charge> chargesToImport)
		{
			var factory = invoice.Factory;

			using (new DisposableAction(() => SuspendValidations(), () => ResumeValidations()))
			using (ServiceContainerSuspenderHelper.FunctionalitySuspender<FunctionalitySuspender>.GetSuspender(factory))
			using (invoice.Lines.SuspendListChanged())
			{
				AddFetchHints();
				foreach (Charge charge in chargesToImport)
				{
					InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
					using (line.GetValidationSuspender())
					{
						invoice.ImportJobChargesIntoInvoice(new[] { charge }, line, false);
					}
				}
			}

			void AddFetchHints()
			{
				var jobPKs = chargesToImport.Select(x => x.JR_JH)
					.Where(x => x.IsValid)
					.Distinct()
					.ToArray();
				factory.AddFetchHint(JobHeaderSchema.Instance, new ZQuery(JobHeaderSchema.PK, jobPKs));
				factory.AddFetchHint(JobHeaderSchema.Instance, new ZQuery(JobHeaderSchema.JH_JH_ParentJob, jobPKs));

				factory.AddFetchHint(JobChargeSchema.Instance, new ZQuery(JobChargeSchema.JR_JH, jobPKs));
				factory.AddFetchHint(JobExRateSchema.Instance, new ZQuery(JobExRateSchema.JF_JH, jobPKs));

				var chargeCodePKs = chargesToImport.Select(x => x.JR_AC)
					.Where(x => x.IsValid)
					.Distinct()
					.ToArray();
				factory.AddFetchHint(ViewGenericChargeSchema.Instance, new ZQuery(ViewGenericChargeSchema.PK, chargeCodePKs));
				factory.AddFetchHint(AccChargeGLPostingOverrideSchema.Instance, new ZQuery(AccChargeGLPostingOverrideSchema.Y1_AC, chargeCodePKs));
				factory.AddFetchHint(AccChargeRevRecOverrideSchema.Instance, new ZQuery(AccChargeRevRecOverrideSchema.AE_AC, chargeCodePKs));

				var currencyCodes = chargesToImport.Select(x => x.JR_OSCostCurrencyCode)
					.Where(x => !x.IsEmpty)
					.Distinct()
					.ToArray();
				factory.AddFetchHint(RefCurrencySchema.Instance, new ZQuery(RefCurrencySchema.RX_Code, currencyCodes));
			}

			void SuspendValidations()
			{
				invoice.Factory.SuspendValidation();
			}

			void ResumeValidations()
			{
				invoice.Factory.ResumeValidation();

				using (invoice.ValidateEmptyComplianceSequenceSuspender.GetSuspender())
				{
					invoice.Validation.ValidateAll();
				}
			}
		}

		void IInvoicingBaseLineImporter.ImportLinesFromConsolCostCollection(IInvoicingBaseImporterTarget invoice, IEnumerable<JobConsolCost> consolCosts)
		{
			using (invoice.Factory.SetTempContext(BusinessContext.APInvoiceApportionToConsol))
			{
				invoice.ConsolCosting.ConsolCosts.RemoveAll();
				using(invoice.ConsolCosting.ConsolCosts.GetSuspenderForSettingDefaultConsol())
				{
					ObjectFactory.Get<IConsolCostImporter>()
						.ImportCostsToCollection(invoice.ConsolCosting.ConsolCosts, consolCosts, syncParentInfo: true);
				}
				invoice.ImportAllApportionmentsFromCosting_SuspendListChanged();
			}
		}

		internal class FunctionalitySuspender : ServiceContainerSuspenderHelper.FunctionalitySuspenderService
		{
		}
	}
}