using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentScanning.Module;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.PlugIn
{
	[TestClass]
	public class eDocsPlugInControllerForTesting : ZController
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
			get { return DummyControllerIDs.eDocsPlugInForTesting; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return null; }
		}

		public override ResourceStringData PluginTabPageCaption { get { return NoResourceStringData.GetData("eDocs", "The eDocs tab."); } }

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			IEDocsPluginHostDecider pluginDecider = businessEntity as IEDocsPluginHostDecider;
			return new eDocsPlugInForTesting(pluginDecider != null ? pluginDecider.HostBusinessEntity : businessEntity);
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
}
