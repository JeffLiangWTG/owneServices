using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Client.EDI.UserManagement.GUI
{
	public class EdiAgreementAssignmentPlugin : ZPlugIn
	{
		public EdiAgreementAssignmentPlugin(IBusiness hostBusinessEntity) : base(hostBusinessEntity)
		{
			this.hostBusinessEntity = hostBusinessEntity;
		}

		readonly IBusiness hostBusinessEntity;

		public override string Name => "Agreements";

		protected override ZBool HasUserControl => true;

		protected override LicenceCheckpoint LicenceCheckPoint => null;

		protected override Control GetNewUserControl()
		{
			return new EdiAgreementAssignmentUserControl();
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			var businessObject = (BusinessObject)hostBusinessEntity;
			var agreementAssignments = new EdiUserAgreementAssignmentCollection(businessObject);
			businessObject.RegisterEditableChildObject(agreementAssignments);
			return agreementAssignments;
		}
	}

	public class EdiAgreementAssignmentPluginController : ZController
	{
		public override ControllerID ID => Modules.ClientControllerRegistration.EdiUserAgreementAssignment;

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");

		public override Type TypeOfTopLevelBusinessObject => null;

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new NotImplementedException();
		}

		protected override ZPlugIn GetPlugIn(IBusiness host)
		{
			return new EdiAgreementAssignmentPlugin(host);
		}
	}
}
