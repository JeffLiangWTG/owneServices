using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public class APPaymentWorkflowDescriptor : ReceiptPaymentBaseWorkflowDescriptor
	{
		public override Type WorkflowProviderType
		{
			get { return typeof(Payment); }
		}

		public override string Code
		{
			get { return WorkflowDescriptors.APPaymentWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("0BC3AE3C-C1C6-4BBE-9081-9CFCCD0ECD8B", "AP Payment"); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.ZAPPayment; }
		}

		public override BusinessContext[] DocumentBusinessContext => new BusinessContext[] { BusinessContext.APTransaction };

#if DEBUG

		public override BusinessObject GetBizOForTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData(typeof(APPayment));
		}

#endif
	}
}
