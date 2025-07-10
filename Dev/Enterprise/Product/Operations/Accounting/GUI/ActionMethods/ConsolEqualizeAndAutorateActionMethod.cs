using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Accounting.GUI
{
	public sealed class ConsolEqualizeAndAutorateActionMethod : OperationalActionMethod
	{
		public ConsolEqualizeAndAutorateActionMethod(JobInvoicingSecurityHelper securityHelper)
			: base(new ZGuid("7CA4A47B-0C4E-4EFB-B3A3-3DAB0B14A28E"))
		{
			this.securityHelper = securityHelper;
		}

		public override string Name
		{
			get { return Res.GetString("9bf99543-1c09-4da0-8cd4-249a8ea44f02", "Auto-Cost With Volume Discount"); }
		}

		public override string Description
		{
			get
			{
				return Res.GetString("66f10617-202f-4de1-8eb5-de5b29f529ed", "Auto-Cost selected Consols with Volume Discount.");
			}
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new ConsolEqualizeAndAutorateActionMethodApplicator(factory);
		}

		public override SecurityCheckpoint[] GetRequiredSecurityCheckpoints()
		{
			return new SecurityCheckpoint[]
			{
				this.securityHelper.GetInvSecurity(SecurityCore.AutoRateCost)
			};
		}

		readonly JobInvoicingSecurityHelper securityHelper;
	}
}
