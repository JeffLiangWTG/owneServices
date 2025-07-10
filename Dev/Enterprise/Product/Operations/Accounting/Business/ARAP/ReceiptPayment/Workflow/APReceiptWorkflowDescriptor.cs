using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public class APReceiptWorkflowDescriptor : ReceiptPaymentBaseWorkflowDescriptor
	{
		public override Type WorkflowProviderType
		{
			get { return typeof(Receipt); }
		}

		public override string Code
		{
			get { return WorkflowDescriptors.APReceiptWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("50829752-BB57-4383-8C9D-5D0B14B11EC5", "AP Receipt"); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.ZAPReceipt; }
		}

		public override BusinessContext[] DocumentBusinessContext => new BusinessContext[] { BusinessContext.ARTransaction };

#if DEBUG

		public override BusinessObject GetBizOForTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData(typeof(APReceipt));
		}

#endif
	}
}
