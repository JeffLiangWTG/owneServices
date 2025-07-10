using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineExDocShipsCompartment))]
	sealed class QuarantineExDocShipsCompartmentClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var quarantineExDocHeader = (QuarantineExDocHeader)NewParentObject();
			return quarantineExDocHeader.Compartments.AddNew();
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = AUJobMessageTypeList.Codes.Quarantine;
			return invoice.QuarantineExDocHeader;
		}
	}
}
