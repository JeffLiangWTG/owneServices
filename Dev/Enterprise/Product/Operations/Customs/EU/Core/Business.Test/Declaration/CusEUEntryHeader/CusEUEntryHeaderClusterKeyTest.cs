using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CusEUEntryHeader))]
	class CusEUEntryHeaderClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IClusterKeyEntity NewClusterKeyEntity() => GetNewEntryHeader().AddInfoChild;

		protected override EnterpriseBusinessObject NewParentObject() => GetNewEntryHeader();

		CusEntryHeader GetNewEntryHeader() => Factory.NewWithValidTestData<JobDeclaration>().CustomsEntryHeaders.AddNew();
	}
}
