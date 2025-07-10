using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.SWL.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Client.SWL
{
	public class ShipnetSetupPlugInController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new PlugInControllerException("Shipnet Setup does not have a form, it is only available as a plugin.");
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.Organisation; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.OrganisationDelete; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.OrganisationNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.OrganisationModify; }
		}

		#endregion

		public override ControllerID ID
		{
			get { return ControllerIDs.ShipnetSetupPlugIn; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not a GUI"); }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return null; }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new ShipnetSetupPlugIn(businessEntity);
		}

		public override IZForm ShowNewForm()
		{
			throw new PlugInControllerException("Shipnet Setup does not have a form, it is only available as a plugin.");
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			throw new PlugInControllerException("Shipnet Setup does not have a form, it is only available as a plugin.");
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			throw new PlugInControllerException("Shipnet Setup does not have a form, it is only available as a plugin.");
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			throw new PlugInControllerException("Shipnet Setup does not have a form, it is only available as a plugin.");
		}

		public override ResourceStringData PluginTabPageCaption
		{
			get { return Res.GetData("PlugInTabPage|ShipnetSetupPlugIn", "Setup Shipnet Data"); }
		}
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
