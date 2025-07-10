using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Accounting.GUI.Riba;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class AccCollectionOrderController : ZController
	{
		public AccCollectionOrderController()
		{ }

		public override ModuleIdentifier ModuleID => ModuleIDs.AccCollectionOrder;

		public override ControllerID ID => ControllerIDs.AccCollectionOrder;

		public override Type TypeOfTopLevelBusinessObject => typeof(AccCollectionOrder);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override IZForm ShowNewForm()
		{
			Globals.Message.Show(Res.GetString("5642DCEC-C386-414B-937E-E9E50F290819", "Collection order can only be created from the receivable module"));
			return null;
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var order = (AccCollectionOrder)businessEntity;
			order.IncludeInBatch = true;
			return new AccCollectionOrderForm(order);
		}

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.CollectionOrderReject;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CollectionOrderEdit;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CollectionOrderView;

#if DEBUG
		public IZForm GetForm_ForTestOnly(IBusiness businessEntity)
		{
			return GetForm(businessEntity);
		}
#endif
	}
}