using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing.Periodic_Invoicing;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class TestUseDefaultingLogicCollection : PeriodicInvoiceSelectableJobCollection, IIncludeInThePeriodicInvoiceChangedObserver
	{
		public TestUseDefaultingLogicCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
		protected override void OnAdded(BusinessObject bizOAdded)
		{
			PeriodicInvoiceSelectableJob jobAdded = (PeriodicInvoiceSelectableJob)bizOAdded;
			jobAdded.ValidateIncludeInThePeriodicInvoiceEventHandler += OnValidateIncludeInThePeriodicInvoice;
			base.OnAdded(bizOAdded);
		}

		public int IncludeInThePeriodicInvoiceValidations;
		void OnValidateIncludeInThePeriodicInvoice(object sender, EventArgs e)
		{
			++IncludeInThePeriodicInvoiceValidations;
		}

		bool trigger;
		public int TriggerCount { get; private set; }

		public void SetTrigger(bool value)
		{
			TriggerCount = 0;
			trigger = value;
		}

		bool currentInclude;
		public void Notify(object sender)
		{
			if (trigger)
			{
				bool include = this.Cast<PeriodicInvoiceSelectableJob>().Any(j => j.IncludeInThePeriodicInvoice);
				if (include != currentInclude)
				{
					++TriggerCount;
					currentInclude = include;
					OnParentIncludeInThePeriodicInvoiceChanged(include);
				}
			}
		}
	}
}
