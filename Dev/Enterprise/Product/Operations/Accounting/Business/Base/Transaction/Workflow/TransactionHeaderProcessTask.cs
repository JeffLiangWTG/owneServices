using System;
using System.ComponentModel;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class TransactionHeaderProcessTask : ProcessTask
	{
		public TransactionHeaderProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void CancelIfParentIsCancelled()
		{
		}

		protected override Type ParentType
		{
			get { return typeof(TransactionHeader); }
		}

		public override ControllerID ParentControllerID
		{
			get
			{
				var controllerIDProvider = ObjectFactory.Get<IAccountingControllerIdDecider>();
				if (Parent == null)
				{
					return null;
				}
				else
				{
					var transactionHeader = (TransactionHeader)Parent;
					return controllerIDProvider.GetControllerID(transactionHeader.AH_TransactionType, transactionHeader.AH_Ledger, null);
				}
			}
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if ((kind & TestBusinessObjectKind.PopulateDependentCollections) != 0)
			{
			}
			else
			{
				base.FillWithValidTestDataCore(kind, propertyPath);
			}
		}
#endif
	}
}
