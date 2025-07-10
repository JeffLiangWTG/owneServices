using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class ArrivalCustomsEffectiveDestinationOfficeWrapperTest : Customs.Business.Testing.DataProviderTestCase<ArrivalCustomsEffectiveDestinationOfficeWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("Throw exception if nctsHeader is null", () => new ArrivalCustomsEffectiveDestinationOfficeWrapper(null));
		}

		public void TestCustomsDestinationOfficeCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty CustomsDestinationOfficeCode because there are no CustomsOffices in the declaration", ZString.Empty, officeWrapper.CustomsDestinationOfficeCode);

				var office = nctsHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival);
				office.CY_Data = "123";
				AssertEquals("Expected filled CustomsDestinationOfficeCode with 3 characters", "123", officeWrapper.CustomsDestinationOfficeCode);

				office.CY_Data = "98761234";
				AssertEquals("Expected filled CustomsDestinationOfficeCode with last 4 characters", "1234", officeWrapper.CustomsDestinationOfficeCode);

				office.CY_Data = "21";
				AssertEquals("Expected filled CustomsDestinationOfficeCode with 2 characters", "21", officeWrapper.CustomsDestinationOfficeCode);
			});
		}

		public void TestCustomsDestinationLocationCode()
		{
			CombineAssertions(() =>
			{
				nctsHeader.ArrivalMovementHeader.BM_LocationOfGoodsCode = "7654321";
				AssertEquals("Expected filled CustomsDestinationLocationCode with last 6 characters", "654321", officeWrapper.CustomsDestinationLocationCode);

				nctsHeader.ArrivalMovementHeader.BM_LocationOfGoodsCode = "21";
				AssertEquals("Expected filled CustomsDestinationLocationCode with 2 characters", "21", officeWrapper.CustomsDestinationLocationCode);

				nctsHeader.ArrivalMovementHeader.BM_LocationOfGoodsCode = ZString.Empty;
				AssertEquals("Expected empty CustomsDestinationLocationCode", ZString.Empty, officeWrapper.CustomsDestinationLocationCode);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();
			wrapper = new ArrivalSendMessageWrapper(nctsHeader, Certificate, DeclarationMessageTypeList.Codes.NctsArrivalNotification);
			officeWrapper = (ArrivalCustomsEffectiveDestinationOfficeWrapper)wrapper.CustomsOfficesOfDestination;
		}

		NctsHeader nctsHeader;
		ArrivalSendMessageWrapper wrapper;
		ArrivalCustomsEffectiveDestinationOfficeWrapper officeWrapper;

		protected override ArrivalCustomsEffectiveDestinationOfficeWrapper GetProvider() => officeWrapper;

		CertificateProviderTestClass Certificate
		{
			get
			{
				return certificate ?? (certificate = new StaffWithCertificateTestHelper(Factory).Certificate);
			}
		}
		CertificateProviderTestClass certificate;
	}
}
