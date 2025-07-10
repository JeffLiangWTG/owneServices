using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class ExitControlMessageSendingObjectCollection : EU.ExitControl.Business.ExitControlMessageSendingObjectCollection
	{
		public ExitControlMessageSendingObjectCollection(IEnumerable<EU.ExitControl.Business.CusExitReport> messagingObjects, BusinessObjectFactory factory) : base(messagingObjects, factory)
		{
		}

		public new ExitControlMessageSendingObject this[int i] => (ExitControlMessageSendingObject)base[i];

		public new ExitControlMessageSendingObject AddNew() => (ExitControlMessageSendingObject)base.AddNew();

		protected override EU.ExitControl.Business.ExitControlMessageSendingObject GetSendingAction(EU.ExitControl.Business.CusExitReport messagingObject) => new ExitControlMessageSendingObject((CusExitReport)messagingObject);
	}
}
