using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	class AuthorisationProviderTest : DataProviderTestCase<AuthorisationProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new AuthorisationProvider(null, ZInt.Zero));
		}

		public void TestSequenceNumber()
		{
			AssertEquals(provider.SequenceNumber, 123);
		}

		public void TestType()
		{
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_Code = Constants.CusPermitHeaderTypes.ACR;
				AssertEquals(GetMessage(), Constants.AuthorisationTypes.C521, provider.Type);

				cusAuthorizationUsage.AGC_Code = Constants.CusPermitHeaderTypes.SSE;
				AssertEquals(GetMessage(), Constants.AuthorisationTypes.C523, provider.Type);

				cusAuthorizationUsage.AGC_Code = Constants.CusPermitHeaderTypes.TransitOperation;
				AssertEquals(GetMessage(), Constants.AuthorisationTypes.C524, provider.Type);

				cusAuthorizationUsage.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
				AssertEquals(GetMessage(), Constants.AuthorisationTypes.C522, provider.Type);

				cusAuthorizationUsage.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				AssertEquals(GetMessage(), Constants.AuthorisationTypes.C520, provider.Type);

				cusAuthorizationUsage.AGC_Code = "123";
				AssertEquals(GetMessage(), string.Empty, provider.Type);
			});

			string GetMessage() => $"{nameof(cusAuthorizationUsage.AGC_Code)} = {cusAuthorizationUsage.AGC_Code}";
		}

		public void TestReferenceNumber()
		{
			var agcNumber = "345";
			cusAuthorizationUsage.AGC_Number = agcNumber;

			AssertEquals(agcNumber, provider.ReferenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<NctsHeader>();
			cusAuthorizationUsage = header.CusAuthorizationUsages.AddNew();
			provider = new AuthorisationProvider(cusAuthorizationUsage, 123);
		}

		AuthorisationProvider provider;
		CusAuthorizationUsage cusAuthorizationUsage;

		protected override AuthorisationProvider GetProvider() => provider;
	}
}
