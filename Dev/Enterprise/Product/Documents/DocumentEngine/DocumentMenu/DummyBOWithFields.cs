#if DEBUG
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.Testing
{
	public class DummyBOWithFields : DummyBusinessObject
	{
		public DummyBOWithFields(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new DummyBOWithFieldsCollection Collection
		{
			get { return collection ?? (collection = new DummyBOWithFieldsCollection(Factory)); }
		}
		DummyBOWithFieldsCollection collection;

		#region Properties
		public ZDecimal Decimal1
		{
			get { return 1m; }
		}

		public ZDecimal Decimal2
		{
			get { return 2m; }
		}

		public ZDecimal Decimal3
		{
			get { return 3m; }
		}

		public ZDecimal Decimal4
		{
			get { return 4m; }
		}

		public ZString Text1
		{
			get { return "Text1"; }
		}

		public ZString Text2
		{
			get { return "Text2"; }
		}

		public ZString Text3
		{
			get { return "Text3"; }
		}

		public ZString Text4
		{
			get { return "Text4"; }
		}
		#endregion

		public class DummyBOWithFieldsCollection : BusinessObjectCollection<DummyBOWithFields>
		{
			public DummyBOWithFieldsCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}
	}
}
#endif
