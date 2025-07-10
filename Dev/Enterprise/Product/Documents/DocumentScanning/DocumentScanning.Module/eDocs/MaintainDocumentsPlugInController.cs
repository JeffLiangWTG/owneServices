using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentScanning.PlugIn;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.DocumentScanning.Module
{
	public class eDocsPlugInController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.eDocs; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.eDocsModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.eDocsModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.eDocsModify; }
		}

		#endregion

		public override ControllerID ID
		{
			get { return ControllerIDs.eDocsPlugIn; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return null; }
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.DocumentScanning.Module.Res.GetData("PlugInTabPage|eDocsPlugIn", "eDocs", "The eDocs tab."); } }

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			IEDocsPluginHostDecider pluginDecider = businessEntity as IEDocsPluginHostDecider;
			return new eDocsPlugIn(pluginDecider != null ? pluginDecider.HostBusinessEntity : businessEntity);
		}

		#region GUI

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new PlugInControllerException("eDocs does not have a form, it is only available as a plugin.");
		}

		public override IZForm ShowNewForm()
		{
			throw new PlugInControllerException("eDocs does not have a form, it is only available as a plugin.");
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			throw new PlugInControllerException("eDocs does not have a form, it is only available as a plugin.");
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			throw new PlugInControllerException("eDocs does not have a form, it is only available as a plugin.");
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			throw new PlugInControllerException("eDocs does not have a form, it is only available as a plugin.");
		}

		#endregion
	}

	[Serializable]
	public class PlugInControllerException : ModuleFeatureNotSupportedException
	{
		public PlugInControllerException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected PlugInControllerException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}
