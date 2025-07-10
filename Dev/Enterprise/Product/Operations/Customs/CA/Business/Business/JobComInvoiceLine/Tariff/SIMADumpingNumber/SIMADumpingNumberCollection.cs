using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class SIMADumpingNumberCollection : NonPersistentBusinessObjectCollection<SIMADumpingNumber>
	{
		public SIMADumpingNumberCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SIMADumpingNumber(Factory);
		}

		public SIMADumpingNumber AddNew(ZString number, ZString description)
		{
			var dumpingNumber = AddNew();
			dumpingNumber.CA_DumpingNumber = number;
			dumpingNumber.CA_DumpingDescription = description;
			return dumpingNumber;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}
	}
}
