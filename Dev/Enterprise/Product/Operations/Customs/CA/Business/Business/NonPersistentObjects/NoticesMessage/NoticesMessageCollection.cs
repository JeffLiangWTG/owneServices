using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public sealed class NoticesMessageCollection : NonPersistentBusinessObjectCollection<NoticesMessage>
	{
		public NoticesMessageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new NoticesMessage(Factory.New<EDIMessage>(), ZString.Empty);
		}
	}
}
