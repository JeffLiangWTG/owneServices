using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class UNDGDataItemCollection : MasterFiles.Business.UNDGDataItemCollection
	{
		public UNDGDataItemCollection(IUNDGDataItemProvider master)
			: base(master)
		{
		}

		public new UNDGDataItem this[int index]
		{
			get { return (UNDGDataItem)base[index]; }
		}

		public new UNDGDataItem AddNew()
		{
			return (UNDGDataItem)base.AddNew();
		}

		protected override MasterFiles.Business.UNDGDataItemCollection.UNDGDataItemStandAloneCollection GetNewStandaloneCollection()
		{
			return new UNDGDataItemStandAloneCollection(Factory, typeof(UNDGDataItem));
		}

		public new class UNDGDataItemStandAloneCollection : MasterFiles.Business.UNDGDataItemCollection.UNDGDataItemStandAloneCollection
		{
			public UNDGDataItemStandAloneCollection(BusinessObjectFactory factory, Type type)
				: base(factory, type)
			{
			}

			public new UNDGDataItem this[int index] => (UNDGDataItem)base[index];

			public new UNDGDataItem AddNew() => (UNDGDataItem)base.AddNew();
		}
	}
}
