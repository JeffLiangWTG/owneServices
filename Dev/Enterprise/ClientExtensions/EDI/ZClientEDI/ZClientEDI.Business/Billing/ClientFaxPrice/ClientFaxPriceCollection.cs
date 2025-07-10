using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientFaxPriceCollection : ActiveBusinessObjectCollection<ClientFaxPrice>
	{
		public ClientFaxPriceCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ClientFaxPriceCollection(BusinessObjectFactory factory, BulkClientFaxPriceUpdater updater)
			: base(factory)
		{
			Updater = updater;
		}

		public BulkClientFaxPriceUpdater Updater
		{
			get { return updater; }
			private set { updater = value; }
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		BulkClientFaxPriceUpdater updater;

		protected override void SetDefaultsForNewElementCore(ClientFaxPrice newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			if (Updater != null)
			{
				newElement.CFP_Month = (ZByte)Updater.MonthAndYearPeriod.Month;
				newElement.CFP_Year = (ZShort)Updater.MonthAndYearPeriod.Year;
			}
		}

		public bool HasChanges
		{
			get
			{
				foreach (ClientFaxPrice price in this)
				{
					if (price.HasChanges || !price.IsInDatabase)
					{
						return true;
					}
				}

				if (((IBusinessObjectCollection)this).HasChanges)
				{
					return true;
				}

				return false;
			}
		}
	}
}
