using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface IGuaranteeAccessCodesSendingObjectCollection<out TSendingObject> : IBusinessObjectCollection<TSendingObject> where TSendingObject : GuaranteeAccessCodesSendingObject
	{
		new TSendingObject this[int index] { get; }
		new TSendingObject AddNew();
	}

	public class GuaranteeAccessCodesSendingObjectCollection<TSendingObject> : NonPersistentBusinessObjectCollection<TSendingObject>, IGuaranteeAccessCodesSendingObjectCollection<TSendingObject> where TSendingObject : GuaranteeAccessCodesSendingObject
	{
		public GuaranteeAccessCodesSendingObjectCollection(GuaranteeAccessCodesSendingObjectParent sendingParent)
			: this(Argument.NotNull(sendingParent, nameof(sendingParent)), (TSendingObject)Activator.CreateInstance(typeof(TSendingObject), sendingParent?.CusGuaranteeHeader))
		{
		}

		GuaranteeAccessCodesSendingObjectCollection(GuaranteeAccessCodesSendingObjectParent sendingParent, TSendingObject sendingObject) : base(sendingParent?.Factory)
		{
			Parent = Argument.NotNull(sendingParent, nameof(sendingParent));
			Add(Argument.NotNull(sendingObject, nameof(sendingObject)));
		}

		public static explicit operator GuaranteeAccessCodesSendingObjectCollection<GuaranteeAccessCodesSendingObject>(GuaranteeAccessCodesSendingObjectCollection<TSendingObject> sendingObjectSubs)
			=> new GuaranteeAccessCodesSendingObjectCollection<GuaranteeAccessCodesSendingObject>(sendingObjectSubs.Parent, sendingObjectSubs[0]);

		public static explicit operator GuaranteeAccessCodesSendingObjectCollection<TSendingObject>(GuaranteeAccessCodesSendingObjectCollection<GuaranteeAccessCodesSendingObject> sendingObjectBase)
			=> new GuaranteeAccessCodesSendingObjectCollection<TSendingObject>(sendingObjectBase.Parent, (TSendingObject)sendingObjectBase[0]);

		protected GuaranteeAccessCodesSendingObjectParent Parent { get; }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new InvalidOperationException("Guarantee Access Codes Message Sending Action Collection should not support adding.");
		}

		public IEnumerator<TSendingObject> GetEnumerator() => Elements.Cast<TSendingObject>().GetEnumerator();

		protected sealed override bool AllowNewCore => false;

		protected sealed override bool AllowRemoveCore => false;
	}
}
