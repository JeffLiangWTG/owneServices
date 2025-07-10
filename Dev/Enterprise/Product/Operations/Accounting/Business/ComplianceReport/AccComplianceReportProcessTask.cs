using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public class AccComplianceReportProcessTask : ProcessTask
	{
		public AccComplianceReportProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void CancelIfParentIsCancelled()
		{
		}

		protected override Type ParentType
		{
			get { return typeof(AccComplianceReport); }
		}

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.AccComplianceReport; }
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
