using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(EUMemberStateCommunication))]
	sealed class EUMemberStateCommunicationClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var manifestHeader = (AsycudaManifestHeader)NewParentObject();
			var euMemberStateCommunication = Factory.NewWithValidTestData<EUMemberStateCommunication>();
			euMemberStateCommunication.EUS_Type = "T";
			euMemberStateCommunication.EUS_Identifier = "I";
			euMemberStateCommunication.EUS_ParentTableCode = manifestHeader.TablePrefix;
			euMemberStateCommunication.EUS_ParentId = manifestHeader.PK;
			return euMemberStateCommunication;
		}

		protected override EnterpriseBusinessObject NewParentObject() => Factory.NewWithValidTestData<AsycudaManifestHeader>();

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;
	}
}
