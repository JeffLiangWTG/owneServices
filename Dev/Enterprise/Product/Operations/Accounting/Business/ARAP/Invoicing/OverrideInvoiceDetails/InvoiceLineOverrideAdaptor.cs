using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public abstract class InvoiceLineOverrideAdaptor<T> : WrapperCollectionAdaptor<T> where T : InvoiceLineOverride
	{
		public InvoiceLineOverrideAdaptor(BusinessObjectFactory factory, params ZGuid[] linePks) : base(factory, linePks) { }

		protected override BusinessObject[] LoadInnerObjects(ZGuid[] pks)
		{
			var innerObjects = Factory.Load<TransactionLine>(new ZQuery(AccTransactionLinesSchema.PK, pks));
			if (innerObjects.Any())
			{
				innerObjectType = innerObjects[0].GetType();
			}
			return innerObjects;
		}

		public override Type InnerObjectType
		{
			get
			{
				return innerObjectType;
			}
		}
		Type innerObjectType;
	}
}
