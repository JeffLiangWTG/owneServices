using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.GUI.XmlExport;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class TransactionsExportController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.TransactionsExport; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(XmlExportGUIWrapper); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new XmlExportForm(new XmlExportGUIWrapper(Factory));
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new XmlExportGUIWrapper(Factory);
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ExportAccountingTransactions; }
		}

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.NewSaved;
		}
	}
}
