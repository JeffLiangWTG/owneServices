using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.Customs.EU.Intrastat.GUI.Transactions;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.Intrastat.Module
{
	public class IntrastatTransactionsController : ZController
	{
		protected override IZForm GetForm(IBusiness businessEntity) => new IntrastatTransactionForm((CusIntrastatHeader)businessEntity);

		public override ControllerID ID => ControllerIDs.Customs.EU.IntrastatTransactionsController;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.EU.IntrastatTransactions;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusIntrastatHeader);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.EuIntrastatTransactions;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.EuIntrastatTransactions;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.EuIntrastatTransactions;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.EuIntrastatTransactions;
	}
}
