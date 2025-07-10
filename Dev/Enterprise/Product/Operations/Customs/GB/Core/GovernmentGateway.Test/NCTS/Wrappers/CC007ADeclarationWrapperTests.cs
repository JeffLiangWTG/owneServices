using Enterprise.Customs.GB.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging.Testing
{
	[TestedType(typeof(CC007ADeclarationWrapper))]
	class CC007ADeclarationWrapperTests : EU.NCTS.Business.Testing.DeclarationWrapperAbstractTest<CC007ADeclarationWrapper>
	{
		protected override CC007ADeclarationWrapper GetProvider()
		{
			header.DestinationTrader.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			return new CC007ADeclarationWrapper(header);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		}

		NctsHeader header;
	}
}
