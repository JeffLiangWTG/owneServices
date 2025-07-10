using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineExDocHeader))]
	sealed class QuarantineExDocHeaderClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
		{
			var compartment = ((QuarantineExDocHeader)ClusterKeyEntityToTest).Compartments.AddNew();
			return new IClusterKeyWorker[] { compartment };
		}

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var invoice = (JobComInvoiceHeader)NewParentObject();
			return invoice.QuarantineExDocHeader;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = AUJobMessageTypeList.Codes.Quarantine;
			return invoice;
		}
	}
}
