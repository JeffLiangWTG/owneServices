using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZStmALogModule : ZFilterGridModule
	{
		public ZStmALogModule()
		{
			ShouldPerformSearchAsync = true;
		}

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;
		public override bool AllowNew => false;
		public override bool AllowEdit => false;
		public override bool AllowView => false;
		public override bool AllowDelete => false;
		public override bool AllowDefaultActivateDeactivate => false;

		public void InitData(IStmALogParent master, GetStmALogFilterStripBusinessObject getStmALogFilterStripBusinessObject = null)
		{
			this.master = master;
			this.getStmALogFilterStripBusinessObject = getStmALogFilterStripBusinessObject;
		}

		protected IStmALogParent master;

		protected GetStmALogFilterStripBusinessObject getStmALogFilterStripBusinessObject;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(Modules.ControllerIDs.StmALog);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return getStmALogFilterStripBusinessObject == null ? new ZStmALogFilterBusinessObject(master) : getStmALogFilterStripBusinessObject(master);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ZStmALogFilterControl(master, GridCollection, FilterBusinessObject, this);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			if (master != null)
			{
				return new StmALogCollectionWithMaster(master);
			}
			return new StmALogCollection(Factory);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.StmALog; }
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override Security.SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}

		protected override void OnBeforePerformSearchCore()
		{
			if (master != null)
			{
				changedLogs = ((IBusinessObjectFactoryInternals)master.Factory).RowFactory.GetTable(StmALog.Schema.TableName, true).GetChanges();
			}
		}
		DataTable changedLogs;

		protected override PerformSearchResult PerformSearchCore(Type type, ZQuery query)
		{
			var factory = SearchManager.GetNewFactory();
			if (changedLogs != null)
			{
				((IBusinessObjectFactoryInternals)factory).RowFactory.GetTable(StmALog.Schema.TableName, true).Merge(changedLogs);
			}

			return LoadCollection(factory, type, query);
		}

		protected override void OnAfterPerformSearchCore()
		{
			changedLogs = null;
			if (EmbeddedControl == null || master == null)
			{
				return;
			}
			foreach (StmALog log in GridCollection)
			{
				var wholeCode = ((ZStmALogFilterBusinessObject)FilterBusinessObject).BizObjNameToShowEventsFor_List.GetCodeFromDescription(log.SL_Parent.ToStringKey());
				if (!string.IsNullOrEmpty(wholeCode) && wholeCode.Contains("["))
				{
					var suffix = wholeCode.Substring(wholeCode.IndexOf("["));
					log.SuffixForTableFriendlyName = suffix;
				}
			}
		}

		protected override void DisposeFilterBusinessObject()
		{
			if (FilterBusinessObject != null && FilterBusinessObject.AreModuleFiltersLoaded)
			{
				try
				{
					FilterBusinessObject.RunPreSaveValidation();
					if (!FilterBusinessObject.HasErrors)
					{
						FilterBusinessObject.SaveLastUsedLayout();
					}
				}
				catch (Exception e) when (!(e is ZSaveConcurrencyException) && !e.IsCriticalException())
				{
					ErrorReporter.ReportOnce("DisposeFilterBusinessObject threw", e);
				}
			}
		}
	}
	public delegate FilterStripBusinessObject GetStmALogFilterStripBusinessObject(IStmALogParent master);
}
