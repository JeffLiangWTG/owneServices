using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class InvoicingBaseCollection : AccTransactionHeaderCollection
	{
		public InvoicingBaseCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
			SetAllowNew(base.AllowNewCore);
		}

		public InvoicingBaseCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			SetAllowNew(base.AllowNewCore);
		}

		public new InvoicingBase this[int index]
		{
			get { return (InvoicingBase)Elements[index]; }
		}

		/// <summary>
		/// Do not call this method. It will throw a NoConcreteTypeException because InvoicingBase is abstract.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new BusinessObject AddNew()
		{
			return base.AddNew();
		}

		protected override bool AllowNewCore
		{
			get { return AllowNew_internal; }
		}
		bool AllowNew_internal;

		public void SetAllowNew(bool allowNewValue)
		{
			AllowNew_internal = allowNewValue;
		}
	}
}
