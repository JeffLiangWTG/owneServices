using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business
{
	public class PortCollection : NonPersistentBusinessObjectCollection<Port>
	{
		public PortCollection(BusinessObjectFactory factory, ZString dataGroupingCode, ZString transportMode) : base(factory)
		{
			Load(dataGroupingCode, transportMode);
		}

		void Load(ZString dataGroupingCode, ZString transportMode)
		{
			AddRange(Factory.Load<ZZRefCusCodeListCombined>(Port.GetFilter(Factory, dataGroupingCode, transportMode)).Select(x => new Port(x)));
		}

		#region Implementation

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new Port(Factory.New<ZZRefCusCodeListCombined>());
		}

		#endregion
	}
}
