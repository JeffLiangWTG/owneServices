using System;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.IL;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class RequestContentHeaderWrapperTest : Customs.Business.Testing.DataProviderTestCase<IRequestContentHeader>
	{
		public void TestConvertor() => AssertNull("Convertor", Provider.Convertor);

		public void TestRecieverId()
		{
			AssertNotNull("RecieverId", Provider.RecieverId);
			AssertEquals(1, Provider.RecieverId.Count);
			AssertEquals(0, Provider.RecieverId[0]);
		}

		public void TestSenderId()
		{
			AssertEquals(0, Provider.SenderId);
			var currentCompany = GlbCompany.CurrentCompany;
			var companyWrapper = (IILGlbCompanyWrapper)GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(currentCompany);
			var externalPassword = companyWrapper.GetGlbExternalPasswordOrCreateNew();
			externalPassword.GP_MailBoxID = "560038416";
			currentCompany.Factory.Save();

			var wrapper = GetProvider();
			AssertEquals(560038416, wrapper.SenderId);
		}

		[TestDate(2024, 09, 09, 8, 7, 9)]
		public void TestTransmitionDateTime() => AssertEquals(new DateTime(2024, 09, 09, 8, 7, 9), Provider.TransmitionDateTime);

		protected override IRequestContentHeader GetProvider() => RequestContentHeaderWrapper.New();
	}
}
