using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Module;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Module
{
	public class LicenseKeyModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public LicenseKeyModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID => Modules.ClientModuleRegistration.LicenseKey;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(Modules.ClientControllerRegistration.LicenseKey);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new LicenceKeyFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection() => new LicenceHeaderCollection(Factory);
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new LicenceKeyFilterBusinessObject();

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;
		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.Organisation;
		public override bool AllowDelete => false;
		public override bool AllowNew => false;
		public override bool SupportsWorkflow => true;

		virtual protected LicenceAction LicenceAction => licenceAction ?? (licenceAction = new LicenceAction(LocateMainForm(), this, null, null, true));
		LicenceAction licenceAction;

		public OperationalActionSupporter OperationalActionSupporter => new LicenceKeyOpAccSupporter();

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewActionMenuItems());
			result.Add(new ZMenuItem("Save && Email Licenses", new EventHandler(LicenceAction.SendEmailLicence)));
			result.Add(new ZMenuItem("Update Licenses Remotely", new EventHandler(LicenceAction.UpdateLicenceRemotely)));
			result.Add(new ZMenuItem("Set Licence Module Fee Basis", new EventHandler(LicenceAction.SetLicenceModuleFeeBasis)));
			return result.ToArray();
		}
	}

	internal sealed class LicenceKeyOpAccSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext => BusinessContext.LicenceHeader;
		public override SecurityCheckpoint CustomizationSecurityCheckpoint => EDISecurityCheckpoints.OrgLicenceModify;
		public override Type RootType => typeof(LicenceHeader);
		public override SecurityCheckpoint RunSecurityCheckpoint => EDISecurityCheckpoints.OrgLicenceModify;
	}
}
