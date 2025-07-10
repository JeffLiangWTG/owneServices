using System;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class CountryListControl : ZUserControl
	{
		public CountryListControl(BusinessObjectFactory factory)
		{
			this.factory = factory;
			InitializeComponent();
		}

		public Guid[] CountryPKs
		{
			get { return (collection == null) ? Array.Empty<Guid>() : collection.CountryPKs; }
			set
			{
				if (collection == null)
				{
					collection = new CountryListCollection(factory);
					SetDataBinding(collection, "");
				}
				else
				{
					collection.RemoveAll();
				}

				collection.Load(value);
			}
		}

		public bool ReadOnly
		{
			get { return CountryListGrid.ReadOnly; }
			set { CountryListGrid.ReadOnly = value; }
		}

		internal readonly BusinessObjectFactory factory;
		internal CountryListCollection collection;
	}
}
