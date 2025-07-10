using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class AdditionalInfoSendingObjectCollection : NonPersistentBusinessObjectCollection<AdditionalInfoSendingObject>
	{
		public AdditionalInfoSendingObjectCollection()
		{
			MaxCountValidationEnable(99);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new AdditionalInfoSendingObject(string.Empty);
	}
}
