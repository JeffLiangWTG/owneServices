using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(JobCAComInvoiceHeader))]
	sealed class JobCAComInvoiceHeaderClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			return header.CAInvoiceHeader;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			return declaration.Invoices.AddNew();
		}
	}
}
