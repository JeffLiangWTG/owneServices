using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class AdditionalInfoSendingObjectCollection : NonPersistentBusinessObjectCollection<AdditionalInfoSendingObject>
	{
		public AdditionalInfoSendingObjectCollection(NctsHeader header)
		{
			nctsHeader = Argument.NotNull(header, nameof(header));
			MaxCountValidationEnable(99);
		}
		NctsHeader nctsHeader { get; }

		protected override BusinessObject CreateNonPersistentBusinessObject() => new AdditionalInfoSendingObject(nctsHeader.Factory, nctsHeader.DefaultDataGroupingCode);
	}
}
