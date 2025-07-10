using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Matching;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public partial class ILineMatchingCollection : InvoicingLineBaseCollection
	{
		public ILineMatchingCollection(InvoicingBase invoice, MatchingBase matching)
			: base(invoice)
		{
			this.Matching = matching;
		}

		readonly MatchingBase Matching;

		public new ILineMatching this[int index]
		{
			get { return (ILineMatching)Elements[index]; }
		}

		public void Add(ILineMatching line)
		{
			Add((BusinessObject)line);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			SetPropertiesReadOnly(bizOAdded);
		}

		#region Implementation

		void SetPropertiesReadOnly(BusinessObject line)
		{
			line.ReadOnly = false;
			if (((ILineMatching)line).Charge != null)
			{
				WritableProperties.Add("PaidAmountInChargeCurrency");
			}
			if (Matching.IsOverPaymentAllowedInThisMatchingSession)
			{
				WritableProperties.Add("OVPAmount");
			}
			var type = line.GetType();
			var method = type.GetMethod("AddWritableProperties");
			if (method != null)
			{
				method.Invoke(line, new object[] { WritableProperties.ToArray() });
			}
		}

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			// This collection is only for view we dont want to update InvoicingBase when the collection is changed
		}

		List<string> WritableProperties
		{
			get
			{
				if (writableProperties == null)
				{
					writableProperties = new List<string>();
				}
				return writableProperties;
			}
		}
		List<string> writableProperties;

		#endregion
	}
}
