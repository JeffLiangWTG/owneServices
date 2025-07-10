using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Messaging.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Messaging.Module
{
	public class EDICommunicationsModeModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public EDICommunicationsModeModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Organisation; }
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Messaging.EDICommunicationsMode; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Messaging.EDICommunicationsMode);

		protected override IFilterControl GetNewFilterControl()
		{
			return new EDICommunicationsModeFilterControl((EDICommunicationsModeCollection)GridCollection, (EDICommunicationsModeFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new EDICommunicationsModeCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new EDICommunicationsModeFilterBusinessObject();
		}

		public override bool BypassParentSecurityCheckpointVerification
		{
			get { return true; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowEdit
		{
			get { return true; }
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());

			return result.ToArray();
		}

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		#region IOperationalActionSupportable

		OperationalActionSupporter operationalActionSupporter;
		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter => operationalActionSupporter ??= new EDICommunicationsModeOperationalActionSupporter();
		#endregion
	}
}
