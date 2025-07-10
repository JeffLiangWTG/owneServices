using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public class ConsolCusCAeMHController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Security

		protected override Security.SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ConsolCAeManifestHouseBill; }
		}

		protected override Security.SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ConsolCAeManifestHouseBill; }
		}

		protected override Security.SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ConsolCAeManifestHouseBill; }
		}

		protected override Security.SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ConsolCAeManifestHouseBill; }
		}

		#endregion

		protected override ZArchitecture.GUI.IZForm GetForm(IBusiness businessEntity)
		{
			var result = new ConsolForm(businessEntity as ForwardingConsol);
			result.PlugInIDToSelectOnLoaded = ControllerIDs.Customs.CA.CAConsoleManifest;
			return result;
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.CA.CAConsoleManifest; }
		}

		public override System.Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ForwardingConsol); }
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new CusCAeMHConsolPlugIn((ForwardingConsol)businessEntity);
		}

		public override ResourceStringData PluginTabPageCaption
		{
			get { return Res.GetData("PlugInTabPage|ConsolCusCAeMHController", "eManifest"); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.CA.CAHouseBilleManifest; }
		}

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			return factory.Load<ForwardingConsol>(sourceEntityPK) ?? factory.Load<CusCAeMHMaster>(sourceEntityPK)?.Consol;
		}
	}
}
