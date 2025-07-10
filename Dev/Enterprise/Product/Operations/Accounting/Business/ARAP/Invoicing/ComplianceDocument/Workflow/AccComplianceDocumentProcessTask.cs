using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business
{
	public class AccComplianceDocumentProcessTask : ProcessTasks
	{
		public AccComplianceDocumentProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		#region Parent

		public override ControllerID ParentControllerID
		{
			get
			{
				ControllerID controllerId = null;

				var header = Parent as AccComplianceDocumentHeader;
				if (header != null)
				{
					if (header.ADH_Ledger == LedgerTypes.AccountsReceivable)
					{
						controllerId = ControllerIDs.ARComplianceDocument;
					}
					else if (header.ADH_Ledger == LedgerTypes.AccountsPayable)
					{
						controllerId = ControllerIDs.APComplianceDocument;
					}
				}

				return controllerId;
			}
		}

		protected override Type ParentType
		{
			get
			{
				return typeof(AccComplianceDocumentHeader);
			}
		}

		#endregion

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
