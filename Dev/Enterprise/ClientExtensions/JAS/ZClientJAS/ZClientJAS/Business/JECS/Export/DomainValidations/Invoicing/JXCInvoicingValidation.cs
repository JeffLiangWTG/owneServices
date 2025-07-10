using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations
{
	public class JXCInvoicingValidation : AutoAccTransactionHeaderValidation
	{
		public JXCInvoicingValidation(IJASInvoicingBase parent)
			: base(parent as AccTransactionHeader)
		{
		}

		protected override void CheckAH_OH()
		{
			base.CheckAH_OH();

			ValidationHelper.ValidateJXCDebtor(Parent.AH_OHInfo, (JASOrgHeader)Parent.Header);
		}

		protected override void CheckAH_GB()
		{
			base.CheckAH_GB();

			if (Parent.Branch == null || Parent.Branch.OrgProxy == null)
			{
				ValidationHelper.AddJXCWarning(Parent.AH_GBInfo, "Cannot export JXC Financial Message for this branch as there is no Proxy Organisation setup");
			}
			else
			{
				ValidationHelper.ValidateJXCBranchProxy(Parent.AH_GBInfo, (JASOrgHeader)Parent.Branch.OrgProxy);
			}
		}

		ValidationHelper ValidationHelper
		{
			get
			{
				if (fValidationHelper == null)
				{
					fValidationHelper = new ValidationHelper();
				}
				return fValidationHelper;
			}
		}

		ValidationHelper fValidationHelper;
	}
}
