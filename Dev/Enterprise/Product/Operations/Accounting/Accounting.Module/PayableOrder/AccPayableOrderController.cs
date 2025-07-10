using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.PayableOrder;
using Enterprise.Accounting.GUI.PayableOrder;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class AccPayableOrderController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.AccPayableOrder; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.AccPayableOrder; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AccPayableOrderHeader); }
		}

		public IZForm ShowSplitForm(AccPayableOrderHeader orderToSplit, AccPayableOrderHeader.CreateOrderType splitType)
		{
			IZForm result = null;

			var newFactory = new BusinessObjectFactory();
			if (orderToSplit.IsInDatabase)
			{
				var orderToSplitInOtherFactory = newFactory.Load<AccPayableOrderHeader>(orderToSplit.PK);
				var newSplitOrder = orderToSplitInOtherFactory.SplitOrder(splitType);
				result = ShowFormForNewEntity(newSplitOrder);
				newSplitOrder.HasChanges = true;
			}

			return result;
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AccPayableOrderForm((AccPayableOrderHeader)businessEntity);
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get
			{
				return Env.Security.PayableOrderTracking;
			}
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.PayableOrderTrackingNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.PayableOrderLineTrackingEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.PayableOrderTrackingDelete; }
		}

		#endregion
	}
}
