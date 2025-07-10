using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineExDocLine))]
	sealed class QuarantineExDocLineClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
		{
			var quarantineProcess = ((QuarantineExDocLine)ClusterKeyEntityToTest).Processes.AddNew();
			return new IClusterKeyWorker[] { quarantineProcess };
		}

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var invLine = (JobComInvoiceLine)NewParentObject();
			return invLine.QuarantineExDocLine;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = AUJobMessageTypeList.Codes.Quarantine;
			var invLine = Factory.New<JobComInvoiceLine>();
			invLine.JI_JZ = invoice.PK;
			return invLine;
		}
	}
}
