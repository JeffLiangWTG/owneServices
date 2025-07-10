using System;
using CargoWise.Customs.BE.NCTS.Business;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CusAuthorizationProvider))]
	sealed class CusAuthorizationProviderTest : Customs.Business.Testing.DataProviderTestCase<CusAuthorizationProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CusAuthorizationProvider(null, ZInt.Zero));
		}

		public void TestSequenceNumber()
		{
			AssertEquals("1", Provider.SequenceNumber);
		}

		public void TestType()
		{
			cusAuthorisation.CPH_Type = "ACE";
			AssertEquals(Constants.AuthorisationTypes.C522, Provider.Type);
		}

		public void TestReferenceNumber()
		{
			cusAuthorisation.CPH_Number = "000001";
			AssertEquals("000001", Provider.ReferenceNumber);
		}

		public void TestHolderOfAuthorisation()
		{
			AssertNull(Provider.HolderOfAuthorisation);
		}

		protected override CusAuthorizationProvider GetProvider() => provider;
		protected override void SetUp()
		{
			base.SetUp();

			cusAuthorisation = Factory.New<CusAuthorisationHeader>();
			provider = new CusAuthorizationProvider(cusAuthorisation, 1);
		}

		CusAuthorizationProvider provider;
		CusAuthorisationHeader cusAuthorisation;
	}
}
