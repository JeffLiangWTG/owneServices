using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class CusUnderbondOutturnReportHeaderInformationAbstractTest : TestCaseWithFactory
	{
		public void TestResponsiblePartyID()
		{
			const string localBusinessNumber = "21003980130";
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = localBusinessNumber;
			AssertEquals(localBusinessNumber, HeaderInfo.ResponsiblePartyID);

			const string localBusinessNumberOverride = "67094168242";
			Underbond.C4_DestinationPremiseID = "AA33N";
			var codeDescriptionPairList = new CodeDescriptionPairList();
			codeDescriptionPairList.AddPair("AA33N", "67094168242");
			FreightDataRegistry.Instance.OuturnResponsiblePartyIDOverride.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, codeDescriptionPairList);

			AssertEquals(localBusinessNumberOverride, HeaderInfo.ResponsiblePartyID);

			Underbond.AU_OutturnResponsiblePartyID = "1234567890";
			AssertEquals("1234567890", HeaderInfo.ResponsiblePartyID);
		}

		public void TestEstablishmentID()
		{
			Underbond.C4_DestinationPremiseID = "12345";
			AssertEquals("EstablishmentID", "12345", HeaderInfo.EstablishmentID);
		}

		protected abstract CusUnderbondOutturnReportHeaderInformation GetHeaderInfo();

		CusUnderbond underbond;
		protected CusUnderbond Underbond => underbond ?? (underbond = Factory.New<CusUnderbond>());

		CusUnderbondOutturnReportHeaderInformation HeaderInfo => GetHeaderInfo();
	}
}
