using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.DataExportBatch;
using Enterprise.Accounting.GUI.DataExportBatch;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class DataExportBatchPluginController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.DataExportBatchPlugin; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("this is a plugin"); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("this is a plugin");
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override ResourceStringData PluginTabPageCaption
		{
			get { return Res.GetData("10840294-66c9-4e14-9dbc-bd2d8bc25976", "Data Export Batch Numbers"); }
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			var source = businessEntity as IDataExportBatchSource;
			if (source != null)
			{
				return new DataExportBatchPlugin(source);
			}

			throw new ArgumentException("businessEntity has to implement IDataExportBatchSource to use this plugin");
		}

		#region Security

		// If the user has the security right to open this form in whichever display mode
		// they can use this plugin that displays read-only information.
		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}
