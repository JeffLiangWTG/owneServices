using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(EUMemberStateCommunication))]
	public abstract class EUMemberStateCommunicationTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var euMemberStateCommunication = Factory.NewWithValidTestData<EUMemberStateCommunication>();
			euMemberStateCommunication.EUS_Type = "T";
			euMemberStateCommunication.EUS_Identifier = "I";
			euMemberStateCommunication.EUS_ParentTableCode = manifestHeader.TablePrefix;
			euMemberStateCommunication.EUS_ParentId = manifestHeader.PK;
			return euMemberStateCommunication;
		}
	}

	[TestedType(typeof(EUMemberStateCommunication))]
	sealed class EUMemberStateCommunicationBaseOnlyTest : EUMemberStateCommunicationTest
	{
		public void TestParent()
		{
			var parent = Factory.New<AsycudaManifestHeader>();
			var euMemberStateCommunication = GetEUMemberStateCommunicationObject();
			euMemberStateCommunication.EUS_ParentTableCode = parent.TablePrefix;
			euMemberStateCommunication.EUS_ParentId = parent.PK;
			AssertSame(parent, euMemberStateCommunication.Parent);
		}

		public void TestTypeDecider() => AssertType<EUMemberStateCommunicationTypeDecider>(EUMemberStateCommunication.TypeDecider);

		EUMemberStateCommunication GetEUMemberStateCommunicationObject() => Factory.New<EUMemberStateCommunication>();
	}
}
