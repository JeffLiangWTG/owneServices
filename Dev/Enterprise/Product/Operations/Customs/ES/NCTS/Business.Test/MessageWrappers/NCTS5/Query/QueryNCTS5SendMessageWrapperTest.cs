using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class QueryNCTS5SendMessageWrapperTest : WrapperHelperTest<QueryNCTS5SendMessageWrapper>
	{
		public void TestTransitOperation()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var wrapper = GetWrapper(nctsHeader);
			var transitOperation = wrapper.TransitOperation;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled TransitOperation", transitOperation);
				AssertSame("Cached TransitOperation", wrapper.TransitOperation, transitOperation);
			});
		}

		QueryNCTS5SendMessageWrapper GetWrapper(NctsHeader header) => new QueryNCTS5SendMessageWrapper(header, Certificate);

		protected override QueryNCTS5SendMessageWrapper GetProvider() => new QueryNCTS5SendMessageWrapper(Factory.New<NctsHeader>(), Certificate);
	}
}
