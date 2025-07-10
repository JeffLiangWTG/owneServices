using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class AllCusUnderbondsCollection : CusUnderbondCollection<CusUnderbond>
	{
		public AllCusUnderbondsCollection(CusHAWB parent)
			: base(parent)
		{
		}

		public override bool ReadOnly
		{
			get { return true; }
		}

		protected override BusinessObject AddNewCore(Type bizOType)
		{
			throw new NotSupportedException();
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			return new ZQuery();
		}
	}
}
