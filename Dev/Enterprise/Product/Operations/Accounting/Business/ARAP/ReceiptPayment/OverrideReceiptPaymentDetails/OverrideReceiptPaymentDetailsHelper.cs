using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public abstract class OverrideReceiptPaymentDetailsHelper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public OverrideReceiptPaymentDetailsHelper(BusinessObjectFactory factory, params ZGuid[] receiptPaymentPKs)
			: base(factory)
		{
			this.CanBizOBeSaved = true;
			this.receiptPaymentPKs = receiptPaymentPKs;
		}

		readonly ZGuid[] receiptPaymentPKs;
		public bool CanBizOBeSaved { get; set; }
		protected BusinessObjectFactory ParentFactory { get; set; }

		public ReceiptPaymentBaseCollection WrappedObjects
		{
			get
			{
				if (wrappedObjects == null)
				{
					ZQuery filter = new ZQuery(AccTransactionHeaderSchema.PK, receiptPaymentPKs);
					wrappedObjects = new ReceiptPaymentBaseCollection(Factory, filter);
					wrappedObjects.Load();
					foreach (ReceiptPaymentBase receiptPayment in wrappedObjects)
					{
						SetBusinessContext(receiptPayment);
						receiptPayment.AddWritableProperties(ColumnsToOverride());
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
		protected abstract void SetBusinessContext(ReceiptPaymentBase invoice);

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

		ReceiptPaymentBaseCollection wrappedObjects;
	}
}
