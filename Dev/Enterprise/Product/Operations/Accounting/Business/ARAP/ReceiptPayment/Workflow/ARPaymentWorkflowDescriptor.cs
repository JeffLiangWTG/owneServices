using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public class ARPaymentWorkflowDescriptor : ReceiptPaymentBaseWorkflowDescriptor
	{
		public override Type WorkflowProviderType
		{
			get { return typeof(Payment); }
		}

		public override string Code
		{
			get { return WorkflowDescriptors.ARPaymentWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("2122CFCA-AD20-4740-BAF3-E2A9CF92D9C2", "AR Payment"); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.ZARPayment; }
		}

		public override BusinessContext[] DocumentBusinessContext => new BusinessContext[] { BusinessContext.APTransaction };

#if DEBUG

		public override BusinessObject GetBizOForTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData(typeof(ARPayment));
		}

#endif
	}
}
