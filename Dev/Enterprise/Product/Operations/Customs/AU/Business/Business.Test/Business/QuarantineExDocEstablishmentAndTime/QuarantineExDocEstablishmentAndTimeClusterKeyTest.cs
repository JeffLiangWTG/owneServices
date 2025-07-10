using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineExDocEstablishmentAndTime))]
	sealed class QuarantineExDocEstablishmentAndTimeClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var quarantineLine = (QuarantineExDocLine)NewParentObject();
			var quarantineProcess = Factory.New<QuarantineExDocEstablishmentAndTime>();
			quarantineProcess.EE_QL = quarantineLine.PK;
			return quarantineProcess;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = AUJobMessageTypeList.Codes.Quarantine;
			var invLine = Factory.New<JobComInvoiceLine>();
			invLine.JI_JZ = invoice.PK;
			return invLine.QuarantineExDocLine;
		}
	}
}
