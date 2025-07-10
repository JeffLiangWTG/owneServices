using System.Data;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class CachedPropertyForTest : DummyBusinessObject
	{
		public CachedPropertyForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public CachedProperty<ZLong> RelatedZ0_Int => new CachedProperty<ZLong>(Factory, () => { return Z0_Long + 3; });

		public CachedProperty<ZLong> RelatedZ0_Int_WithDeletedMonitor
		{
			get => relatedZ0_Int_WithDeletedMonitor ?? (relatedZ0_Int_WithDeletedMonitor =
				new CachedProperty<ZLong>(Factory, () => Z0_Long + 5, this, 0));
			set => relatedZ0_Int_WithDeletedMonitor = value;
		}

		CachedProperty<ZLong> relatedZ0_Int_WithDeletedMonitor;
	}
}
