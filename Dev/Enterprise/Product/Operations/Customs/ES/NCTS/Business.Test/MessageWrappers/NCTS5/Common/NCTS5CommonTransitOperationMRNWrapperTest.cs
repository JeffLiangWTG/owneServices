using System;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NCTS5CommonTransitOperationMRNWrapperTest : WrapperHelperTest<NCTS5CommonTransitOperationMRNWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if header is null", typeof(ArgumentNullException),
			ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "header"), () => new NCTS5CommonTransitOperationMRNWrapper(null));
		}

		public void TestMRN()
		{
			CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, nctsHeader.CountryCode).CE_EntryNum = "23ES00999912345678";
			AssertEquals("Expected filled MRN with MovementReferenceNumber", "23ES00999912345678", wrapper.MRN);
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			wrapper = new NCTS5CommonTransitOperationMRNWrapper(nctsHeader);
		}

		NctsHeader nctsHeader;
		NCTS5CommonTransitOperationMRNWrapper wrapper;

		protected override NCTS5CommonTransitOperationMRNWrapper GetProvider() => wrapper;
	}
}
