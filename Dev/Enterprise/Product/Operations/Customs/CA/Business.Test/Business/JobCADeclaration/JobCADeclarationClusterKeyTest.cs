using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(JobCADeclaration))]
	sealed class JobCADeclarationClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var declaration = (JobDeclaration)NewParentObject();
			return declaration.CADeclaration;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			return Factory.New<JobDeclaration>();
		}
	}
}
