using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public abstract class OverrideInvoiceDetailsHelper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public OverrideInvoiceDetailsHelper(BusinessObjectFactory factory, params ZGuid[] invoicePKs)
			: this(factory, null, true, invoicePKs)
		{
		}

		public OverrideInvoiceDetailsHelper(BusinessObjectFactory formFactory, BusinessObjectFactory parentFactory, bool canBizOBeSaved, params ZGuid[] invoicePKs)
			: base(formFactory)
		{
			this.ParentFactory = parentFactory;
			this.CanBizOBeSaved = canBizOBeSaved;
			this.invoicePKs = invoicePKs;
		}

		readonly ZGuid[] invoicePKs;
		public bool CanBizOBeSaved { get; set; }
		protected BusinessObjectFactory ParentFactory { get; set; }

		public TransactionHeaderCollection WrappedObjects
		{
			get
			{
				if (wrappedObjects == null)
				{
					ZQuery filter = new ZQuery(AccTransactionHeaderSchema.PK, invoicePKs);
					wrappedObjects = new TransactionHeaderCollection(Factory, filter);
					wrappedObjects.Load();
					foreach (TransactionHeader transaction in wrappedObjects)
					{
						SetBusinessContext(transaction);
						transaction.AddWritableProperties(ColumnsToOverride());
					}

					if (CanBizOBeSaved)
					{
						RegisterEditableChildObject(wrappedObjects);
					}
				}

				return wrappedObjects;
			}
		}

		protected abstract string[] ColumnsToOverride();
		protected abstract void SetBusinessContext(BusinessObject bizObj);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1084:DoNotUseVirtualNew", Justification = "Baseline")]
		new public virtual void SetDefaultValues()
		{
		}

		public virtual void UpdateRelatedParentBizO()
		{
			if (ParentFactory == null)
			{
				throw new ArgumentException("Incorrect constructor used to instantiate BizO.");
			}
		}

		TransactionHeaderCollection wrappedObjects;
	}
}
