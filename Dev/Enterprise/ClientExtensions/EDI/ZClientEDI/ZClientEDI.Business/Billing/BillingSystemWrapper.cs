using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class BillingSystemWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public BillingSystemWrapper(BillingSystem billingSystem) : base()
		{
			BillingSystemObject = billingSystem;
		}

		readonly BillingSystem BillingSystemObject;

		public ZBool IsEnabled
		{
			get => BillingSystemObject.IsEnabled;
			set
			{
				BillingSystemObject.IsEnabled = value;
				IsEnabledInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsEnabledInfo => GetZPropertyInfo(nameof(IsEnabled));
		public ZString SystemCode => BillingSystemObject.SystemCode;
		public ZString SystemDescription => BillingSystemObject.SystemDescription;
	}

	public class BillingSystemWrapperCollection : NonPersistentBusinessObjectCollection<BillingSystemWrapper>
	{
		public BillingSystemWrapperCollection() : base()
		{
		}

		public BillingSystemWrapperCollection(BillingSystemList systems) : this()
		{
			AddRange(systems.Select(x => new BillingSystemWrapper(x)));
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BillingSystemWrapper(null);
		}

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
	}
}
