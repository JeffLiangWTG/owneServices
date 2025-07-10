using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.Module.DocumentMenu.DocDataPlugIn
{
	public class DocDataPlugInController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public DocDataPlugInController()
		{
		}

		protected override Enterprise.ZArchitecture.GUI.IZForm GetForm(IBusiness businessEntity)
		{
			return null;
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.DocumentEngine.Module.Res.GetData("PlugInTabPage|DocDataPlugIn", "Doc Data", "The Doc Data tab."); } }

		protected override Enterprise.ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new Enterprise.DocumentEngine.GUI.DocumentMenu.DocDataPlugIn.DocDataPlugIn(businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.DocDataPlugIn; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new NotImplementedException(); }
		}

		#region FormOverrides

		public override IZForm ShowNewForm()
		{
			throw new ModuleGuiNotSupportedException("DocData plugin does not support ShowNewForm");
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			throw new ModuleGuiNotSupportedException("DocData plugin does not support ShowEditForm");
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			throw new ModuleGuiNotSupportedException("DocData plugin does not support ShowDeleteForm");
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			throw new ModuleGuiNotSupportedException("DocData plugin does not support ShowViewForm");
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}
