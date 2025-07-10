using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.CommissionManagement.Business.Test
{
	[TestedType(typeof(EdiCommissionHeader))]
	public class EdiCommissionHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetOrCreateAdditionalInfo()
		{
			var commissionHeader = Factory.New<EdiCommissionHeader>();

			var additionalInfo = commissionHeader.GetOrCreateAdditionalInfo();
			AssertEquals(additionalInfo.ECH_CH0, commissionHeader.PK);

			AssertEquals(additionalInfo, commissionHeader.GetOrCreateAdditionalInfo());

			additionalInfo.Delete();

			var additionalInfo2 = commissionHeader.GetOrCreateAdditionalInfo();
			AssertNotEquals(additionalInfo, additionalInfo2);

			var query = new ZQuery(EdiCommissionHeaderAdditionalInfoSchema.ECH_CH0, commissionHeader.PK);
			AssertContainsExactElementsInAnyOrder(
				new[] { additionalInfo2 },
				Factory.Load<EdiCommissionHeaderAdditionalInfo>(query));
		}

		public void TestDelete()
		{
			var commissionHeader = Factory.New<EdiCommissionHeader>();
			var additionalInfo = commissionHeader.GetOrCreateAdditionalInfo();

			commissionHeader.Delete();

			AssertEquals(true, additionalInfo.IsDeleted);
		}
	}
}
