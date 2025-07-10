using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public abstract class JobBillingDefaultingManager
	{
		public JobBillingDefaultingManager(IFactLoaderProvider factLoaderProvider)
		{
			FactLoaderProvider = CargoWise.Common.Argument.NotNull(factLoaderProvider, nameof(factLoaderProvider));
		}

		public void SetDefaultValue(IJobInvoicingPlugIn jobPlugin, ICompany loginCompany, IBranch loginBranch, IDepartment loginDepartment)
		{
			if (jobPlugin?.InvoicingSupporter?.ConsumerType?.Code != null)
			{
				var consumerType = jobPlugin.InvoicingSupporter.ConsumerType.Code;
				var loader = FactLoaderProvider.GetFactLoader(consumerType, ConetxtType);
				if (loader != null)
				{
					var contextSubType = consumerType.GetEnumValue<RulesContextSubType>() ?? RulesContextSubType.None;
					SetDefaultValue(jobPlugin, loginCompany, loginBranch, loginDepartment, loader, contextSubType);
				}
			}
		}

		public void SetDefaultValue(IJobInvoicingPlugIn jobPlugin, ICompany loginCompany, IBranch loginBranch, IDepartment loginDepartment, IFactLoader loader, RulesContextSubType contextSubType)
		{
			if (jobPlugin?.InvoicingSupporter?.Job != null)
			{
				var factsArray = loader.GetFacts(jobPlugin, loginCompany, loginBranch, loginDepartment);
				if (factsArray != null && factsArray.Any())
				{
					var invoicingSupporter = jobPlugin.InvoicingSupporter;
					var filter = ProductionRuleSetFilter.WithCompany(invoicingSupporter.Job.JH_GC.ToGuid());

					var rulesEngineService = ObjectFactory.Get<IProductionRulesEnginePushService>(nameof(IProductionRulesEnginePushService), new[] { invoicingSupporter.Job.Factory });
					var result = rulesEngineService.RunRulesEngine(ConetxtType, contextSubType, filter, factsArray, CancellationToken.None);

					if (result.Status == ResultStatus.Success)
					{
						SetDefaultValue(result);
					}
				}
			}
		}

		public IZType DefaultValue { get; protected set; }

		abstract protected void SetDefaultValue(ProductionRulesEngineResult result);

		abstract protected RulesContextType ConetxtType { get; }

		IFactLoaderProvider FactLoaderProvider { get; }
	}
}
