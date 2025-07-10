using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.GUI.CDSCashPayments;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Module
{
	public class CDSCashPaymentsController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.GB.CDSCashPaymentsController;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.EU.GB.CDSCashPayments;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusEntryPayInfo);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.GBCDSCashPayments;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.GBCDSCashPayments;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.GBCDSCashPayments;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.GBCDSCashPayments;

		protected override IZForm GetForm(IBusiness businessEntity) => new CDSCashPaymentsForm((CusEntryPayInfo)businessEntity);
	}
}
