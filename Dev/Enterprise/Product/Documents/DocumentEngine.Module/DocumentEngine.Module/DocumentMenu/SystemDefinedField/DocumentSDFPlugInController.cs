using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.GUI.SDF;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.Module.SDF
{
	public class DocumentSDFPlugInController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public DocumentSDFPlugInController()
		{
		}

		protected override Enterprise.ZArchitecture.GUI.IZForm GetForm(IBusiness businessEntity)
		{
			return null;
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.DocumentSDFPlugIn; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new NotSupportedException("TypeOfTopLevelBusinessObject property is not supported by DocumentUDFPlugInController"); }
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.DocumentEngine.Module.Res.GetData("PlugInTabPage|DocumentSDFPlugIn", "System Defined Data", "The System Defined Data."); } }

		protected override Enterprise.ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			DocumentSDFPlugIn plugIn = new DocumentSDFPlugIn(businessEntity);
			return plugIn;
		}

		#region FormOverrides

		public override IZForm ShowNewForm()
		{
			throw new ModuleGuiNotSupportedException("Document SDF plugin does not support ShowNewForm");
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			throw new ModuleGuiNotSupportedException("Document SDF plugin does not support ShowEditForm");
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			throw new ModuleGuiNotSupportedException("Document SDF plugin does not support ShowDeleteForm");
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			throw new ModuleGuiNotSupportedException("Document SDF plugin does not support ShowViewForm");
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForView
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
		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		#endregion

	}
}
