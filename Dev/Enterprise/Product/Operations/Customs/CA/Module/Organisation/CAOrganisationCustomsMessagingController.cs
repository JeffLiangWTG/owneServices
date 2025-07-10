using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public class CAOrganisationCustomsMessagingController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public CAOrganisationCustomsMessagingController()
		{
		}

		public override ControllerID ID => ControllerIDs.Customs.CA.OrganisationCustomsMessaging;

		public override ResourceStringData PluginTabPageCaption
			=> Res.GetData("PlugInTabPage|CAOrganisationCustomsMessaging", "Customs Messaging", "The Customs Messaging tab.");

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");

		public override Type TypeOfTopLevelBusinessObject => throw new ModuleGuiNotSupportedException("No form");

#if DEBUG
		internal ZPlugIn GetPlugInInternal(IBusiness businessEntity) => GetPlugIn(businessEntity);
#endif

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
			=> new GUI.OrganisationCustomsMessagingPlugIn(new CustomsMessagingPlugInSupportOrgHeaderWrapper((OrgHeader)businessEntity));

		protected override SecurityCheckpoint CheckPointForView => Env.Security.QueryMessagesView;

		protected override IZForm GetForm(IBusiness businessEntity) => throw new ModuleGuiNotSupportedException("No form");

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.QueryMessages;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.QueryMessages;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.QueryMessages;
	}
}
