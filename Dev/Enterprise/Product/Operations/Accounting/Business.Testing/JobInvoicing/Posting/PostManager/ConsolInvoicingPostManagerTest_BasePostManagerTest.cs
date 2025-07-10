using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(ConsolInvoicingPostManager))]
	public class ConsolInvoicingPostManagerTest_BasePostManagerTest : BasePostManagerTest
	{
		protected override BasePostManager GetPostManager(IEnumerable<Job> jobs, GlbBranch taxBranch)
		{
			ZString consolNumber = "C001";
			var consol = Factory.LoadFromUniqueKey<ForwardingConsol>(JobConsolSchema.JK_UniqueConsignRef, consolNumber) ?? TestObjectCreator.CreateConsol("AUSYD", "USLAX", consolNumber);
			var apportionments = new ApportionmentListing(Factory, consol);
			return new ConsolInvoicingPostManager(Factory, jobs, consol, apportionments);
		}

		protected override JobInvoicingPostingOption GetRevenuePostingOption()
		{
			return JobInvoicingPostingOption.All;
		}

		protected override void SetSendingForwarderAddressForConsol(ZGuid addressPK)
		{
			var consol = Factory.Load<ForwardingConsol>(new ZQuery()).FirstOrDefault();
			if (consol != null)
			{
				consol.JK_OA_SendingForwarderAddress = addressPK;
			}
		}
	}
}
