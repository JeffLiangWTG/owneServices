using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class AlertOrRejectReasonCollection : NonPersistentBusinessObjectCollection<AlertOrRejectReason>
	{
		public AlertOrRejectReasonCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			MaxCountValidationEnable(9);
		}
		protected override BusinessObject CreateNonPersistentBusinessObject() => new AlertOrRejectReason(Factory);
	}
}
