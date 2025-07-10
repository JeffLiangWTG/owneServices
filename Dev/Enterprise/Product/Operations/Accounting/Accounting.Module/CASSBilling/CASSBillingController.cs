using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class CASSBillingController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.CASSCostFileImport; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CASSBilling); }
		}

		#region Implementation

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GUI.CASSExportBillingForm((CASSBilling)businessEntity);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new CASSBilling(Factory);
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.PayablesTransactions; }
		}

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.NewSaved;
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		#endregion
	}
}
