using System;
using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class StlSummaryDocumentSupporter : DocumentSupporter
	{
		public StlSummaryDocumentSupporter(StlMonthlyUsage bizo)
			: base(bizo)
		{
			//This is needed because OrgUsage is Nonpersistent. See DocEngine or ask Ben for more details.
			this.HasChanges = false;
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CargoWiseBilling; }
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Core.Constants.DataContext[] { Enterprise.Core.Constants.DataContext.CargoWiseBilling };
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Core.Constants.DataContext.CargoWiseBilling && commandBeingRun.SU_MenuName.StartsWith((NoResString)"STL Billing Summary", StringComparison.OrdinalIgnoreCase))
			{
				return new DocumentWrapper[] { DocStlSummary.New(BusinessObject, Factory) };
			}
			return Array.Empty<DocumentWrapper>();
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		public new StlMonthlyUsage BusinessObject
		{
			get { return (StlMonthlyUsage)base.BusinessObject; }
		}
	}
}

