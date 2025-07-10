using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.Module
{
	public class DocumentUDFPlugInController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public DocumentUDFPlugInController()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		protected override Enterprise.ZArchitecture.GUI.IZForm GetForm(IBusiness businessEntity)
		{
			return null;
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.DocumentUDFPlugIn; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new NotSupportedException("TypeOfTopLevelBusinessObject property is not supported by DocumentUDFPlugInController"); }
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.DocumentEngine.Module.Res.GetData("PlugInTabPage|DocumentUDFPlugIn", "User Defined Data", "The User Defined Data tab."); } }

		protected override Enterprise.ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			DocumentUDFPlugIn plugIn = new DocumentUDFPlugIn(businessEntity);
			return plugIn;
		}

		public override IZForm ShowNewForm()
		{
			throw new ModuleGuiNotSupportedException("Document UDF plugin does not support ShowNewForm");
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			throw new ModuleGuiNotSupportedException("Document UDF plugin does not support ShowEditForm");
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			throw new ModuleGuiNotSupportedException("Document UDF plugin does not support ShowDeleteForm");
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			throw new ModuleGuiNotSupportedException("Document UDF plugin does not support ShowViewForm");
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }//TODO: I am not sure about this security level. must confirm with Henry. Ali
		}
		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }//TODO: I am not sure about this security level. must confirm with Henry. Ali
		}
		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }//TODO: I am not sure about this security level. must confirm with Henry. Ali
		}
		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }//TODO: I am not sure about this security level. must confirm with Henry. Ali
		}
	}
}
