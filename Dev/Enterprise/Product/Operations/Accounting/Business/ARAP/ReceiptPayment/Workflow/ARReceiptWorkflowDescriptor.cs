using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public class ARReceiptWorkflowDescriptor : ReceiptPaymentBaseWorkflowDescriptor
	{
		public override Type WorkflowProviderType
		{
			get { return typeof(Receipt); }
		}

		public override string Code
		{
			get { return WorkflowDescriptors.ARReceiptWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("7F6ECB9C-931D-4E2C-B9F7-A77A72F9C2E6", "AR Receipt"); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.ZARReceipt; }
		}

		public override BusinessContext[] DocumentBusinessContext => new BusinessContext[] { BusinessContext.ARTransaction };

#if DEBUG

		public override BusinessObject GetBizOForTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData(typeof(ARReceipt));
		}

#endif
	}
}
