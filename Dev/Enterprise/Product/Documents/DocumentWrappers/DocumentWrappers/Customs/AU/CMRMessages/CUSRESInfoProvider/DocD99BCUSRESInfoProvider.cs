using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public abstract class DocD99BCUSRESInfoProvider : DocBaseWrapper
	{
		protected DocD99BCUSRESInfoProvider(D99BCUSRESInfoProvider d99BCUSRESInfoProvider, BusinessObjectFactory factory)
			: base(d99BCUSRESInfoProvider, factory)
		{
		}

		D99BCUSRESInfoProvider D99BCUSRESInfoProvider
		{
			get { return (D99BCUSRESInfoProvider)WrappedObject; }
		}

		public ZString DocumentName
		{
			get { return D99BCUSRESInfoProvider.DocumentName; }
		}

		public ZString VersionNumber
		{
			get { return D99BCUSRESInfoProvider.VersionNumber; }
		}

		public ZString EntryNumber
		{
			get { return D99BCUSRESInfoProvider.EntryNumber; }
		}

		public ZDateTime PaymentFinalisedDate
		{
			get { return D99BCUSRESInfoProvider.PaymentFinalisedDate; }
		}

		public abstract DocCMRMessageChargeItemCollection Charges { get; }
		public abstract ZDecimal TotalPayable { get; }
	}
}
