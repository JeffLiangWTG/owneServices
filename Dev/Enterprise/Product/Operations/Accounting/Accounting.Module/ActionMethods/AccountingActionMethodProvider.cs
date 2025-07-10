using System.Collections.Generic;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Accounting.Module
{
	public class AccountingActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			List<OperationalActionMethod> result = new List<OperationalActionMethod>();

			if (typeof(IJobInvoicingPlugIn).IsAssignableFrom(actionSupporter.RootType))
			{
				AddAutoRatingMethods(actionSupporter, result);
				AddExchangeRateMethods(actionSupporter, result);
				AddProfitShareMethods(actionSupporter, result);
				AddUpdateJobMethods(actionSupporter, result);
			}

			return result.Count > 0 ? result.ToArray() : null;
		}

		static void AddProfitShareMethods(OperationalActionSupporter actionSupporter, List<OperationalActionMethod> methods)
		{
			methods.Add(new ProfitShareActionMethod(actionSupporter.GetType()));
		}

		static void AddUpdateJobMethods(OperationalActionSupporter actionSupporter, List<OperationalActionMethod> methods)
		{
			if (typeof(IJobInvoicingPlugIn).IsAssignableFrom(actionSupporter.RootType))
			{
				methods.Add(new UpdateJobStatusActionMethod());
				methods.Add(new UpdateJobDeptActionMethod());
				methods.Add(new UpdateJobLocalClientActionMethod());
				methods.Add(new UpdateJobOperatorActionMethod());
				methods.Add(new UpdateJobOverseasAgentActionMethod());
				methods.Add(new UpdateJobProfitLossReasonActionMethod());
			}
		}

		static void AddAutoRatingMethods(OperationalActionSupporter actionSupporter, List<OperationalActionMethod> methods)
		{
			var securityProvider = actionSupporter as IInvoicingSecurityCheckpointProvider;
			SecurityCheckpoint checkpoint;

			if (securityProvider != null
			&& (checkpoint = securityProvider.InvoicingCheckpoint) != null
			&& (typeof(IRatingSupporter).IsAssignableFrom(actionSupporter.RootType)))
			{
				var helper = new JobInvoicingSecurityHelper(checkpoint);
				methods.Add(new AutoRateCostsRevenueActionMethod(helper));
				methods.Add(new AutoRateRevenueActionMethod(helper));
				methods.Add(new AutoRateCostsActionMethod(helper));
				methods.Add(new ConsolEqualizeAndAutorateActionMethod(helper));
			}
		}

		static void AddExchangeRateMethods(OperationalActionSupporter actionSupporter, List<OperationalActionMethod> methods)
		{
			ExRateSourceType[] rateSources = null;

			if (typeof(IJobInvoicingExRateSourceProvider).IsAssignableFrom(actionSupporter.RootType))
			{
				rateSources = SupportExRateSourceAttribute.SupportedExRateSources(actionSupporter.RootType);
			}

			methods.Add(new ExchangeRatesActionMethod(rateSources));
		}
	}
}
