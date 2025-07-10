using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	class DummyAutoLogged : DummyLogged
	{
		DummyChildBusinessObjectCollection collection;

		public DummyAutoLogged(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public DummyChildBusinessObjectCollection Collection
		{
			get
			{
				if (collection == null)
				{
					collection = new DummyChildBusinessObjectCollection(Factory);
				}
				return collection;
			}
			set { collection = value; }
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;
	}
}
