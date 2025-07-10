using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Messaging.Module
{
	public class EDIInterchangeModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Messaging.EDIInterchange; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowEdit
		{
			get { return false; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Messaging.EDIInterchange);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new EDIInterchangeFilterControl((EDIInterchangeCollection)GridCollection, (EDIInterchangeFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new EDIInterchangeCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new EDIInterchangeFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.EDIInterchange; }
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			if (GlbStaff.CurrentUser.IsSupportUser)
			{
				result.Add(new ZMenuItem(EDIMessageModule.ResetStatusToQueuedMenuName, new EventHandler(ResetToQueued_Click)));
			}
			return result.ToArray();
		}

		protected void ResetToQueued_Click(object sender, EventArgs e)
		{
			EDIMessageModule.ResetToQueuedClickHandler(Grid, Factory);
		}
	}
}
