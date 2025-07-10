using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Module
{
	public class RefDocTypeModule : ZFilterGridModule
	{
		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.RefDocType; }
		}

		#endregion

		public override BusinessContext[] BusinessContexts
		{
			get { return new BusinessContext[] { BusinessContext.RefDocType }; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.RefDocType);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new RefDocTypeFilterControl(GridCollection, (RefDocTypeFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new RefDocTypeCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new RefDocTypeFilterBusinessObject();
		}

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.DocumentTypes; }
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.DocManager; }
		}

		#endregion

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());
			menuItems.Remove(DeleteMenuItem);
			return menuItems.ToArray();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewActionMenuItems());
			MenuItem printAllMenuItem = new ZMenuItem(ResString.GetMultilingualString("91419ad1-e1ff-47de-8d07-524c732334ee", "Print All Cover Sheets"), new EventHandler(PrintCoverSheetSet));
			menuItems.Add(printAllMenuItem);
			return menuItems.ToArray();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String used in SQL query")]
		void PrintCoverSheetSet(object sender, EventArgs e)
		{
			var task = new PrintTask();
			var addedDocTypes = new ArrayList();

			foreach (RefDocType docType in Grid.List)
			{
				if (!addedDocTypes.Contains(docType.RT_DocType))
				{
					addedDocTypes.Add(docType.RT_DocType);
					var coverSheetQuery = new ZQuery(StmMenuItemSchema.SU_MenuName, "Doc Type Cover Sheet");
					var documentCommands = new DocumentCommandCollection(docType);
					documentCommands.Load(coverSheetQuery);

					if (documentCommands.Count > 0)
					{
						var pack = new DocumentPack(documentCommands[0], docType, null, null);
						task.Add(pack);
					}
				}
			}
			task.Run(Env.Security.None);
		}
	}
}
