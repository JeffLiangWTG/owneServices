using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Module
{
	public class ProcessHeaderModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public ProcessHeaderModule()
			: this(shouldAddOperationalActions: true)
		{
		}

		protected ProcessHeaderModule(bool shouldAddOperationalActions)
		{
			if (shouldAddOperationalActions)
			{
				Plugins.Add(ControllerIDs.OperationalActions);
			}
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ProcessHeader; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ProcessHeader);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ProcessHeaderFilterControl(GridCollection, (ProcessHeaderFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ProcessHeaderCollection(Factory, new ZQuery());
		}

		protected override PerformSearchResult LoadCollection(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			var result = base.LoadCollection(factory, type, query);
			WorkflowTransferOrderSorter.SortAndSetReleaseSequence(factory, result.LoadedRows.Cast<ProcessHeader>());
			return result;
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ProcessHeaderFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.BufferManagement; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.WorkflowHeaders; }
		}

		public override bool AllowUniversalCopy
		{
			get { return false; }
		}

		public override bool SupportsWorkflow
		{
			get { return false; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowDelete
		{
			get { return true; }
		}

		protected override bool AllowMultiDeleteWithoutListing
		{
			get { return true; }
		}

		protected override void HandleDeleteClickCore(object sender, EventArgs e)
		{
			if (HasActions && AllowDelete)
			{
				if (SelectedBusinessObjects.Length == 0)
				{
					ShowNoSelectedMessage();
				}
				else
				{
					DeleteMultipleWithoutListing(SelectedBusinessObjects);
				}
			}
		}

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter
		{
			get { return new ProcessHeaderOperationalActionSupporter(); }
		}
	}
}
