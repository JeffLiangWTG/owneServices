using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.KR.Module
{
	public class KROrganisationController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public KROrganisationController()
		{
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("No form");
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.KR.OrganisationDetailsPlugIn; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("No form"); }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.OrgDetailsViewCountryDefaults; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.OrgConfigModifyCountryDefaults; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.OrgConfigModifyCountryDefaults; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.OrgConfigModifyCountryDefaults; }
		}

		public override ResourceStringData PluginTabPageCaption { get { return Res.GetData("19FC37A5-05E4-40AB-900F-85EF65C02807", "Korea"); } }

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new OrganisationPlugIn((OrgHeader)businessEntity);
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}
	}
}
