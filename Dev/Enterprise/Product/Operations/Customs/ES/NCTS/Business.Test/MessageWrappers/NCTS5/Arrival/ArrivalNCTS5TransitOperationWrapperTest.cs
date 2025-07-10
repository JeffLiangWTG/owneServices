using System;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class ArrivalNCTS5TransitOperationWrapperTest : WrapperHelperTest<ArrivalNCTS5TransitOperationWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if ArrivalMovementHeader is null", typeof(ArgumentNullException),
			ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "ArrivalMovementHeader"), () => GetWrapper(Factory.New<NctsHeader>()));
		}

		public void TestSimplifiedProcedure()
		{
			CombineAssertions(() =>
			{
				nctsHeader.ArrivalMovementHeader.IsSimplifiedNctsProcedure = true;
				AssertEquals("Expected filled SimplifiedProcedure true", true, wrapper.SimplifiedProcedure);
				nctsHeader.ArrivalMovementHeader.IsSimplifiedNctsProcedure = false;
				AssertEquals("Expected filled SimplifiedProcedure false", false, wrapper.SimplifiedProcedure);
			});
		}

		public void TestIncidentFlag()
		{
			CombineAssertions(() =>
			{
				nctsHeader.BH_ExportFlag = "Y";
				AssertEquals("Expected filled BH_ExportFlag = Y", true, wrapper.IncidentFlag);
				nctsHeader.BH_ExportFlag = "N";
				AssertEquals("Expected filled BH_ExportFlag = N", false, wrapper.IncidentFlag);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			wrapper = GetWrapper(nctsHeader);
		}

		ArrivalNCTS5TransitOperationWrapper wrapper;
		NctsHeader nctsHeader;

		ArrivalNCTS5TransitOperationWrapper GetWrapper(NctsHeader nctsHeader) => new ArrivalNCTS5TransitOperationWrapper(nctsHeader);

		protected override ArrivalNCTS5TransitOperationWrapper GetProvider() => wrapper;
	}
}
