using System.Linq;
using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CHGTSTCusTempStorageDecProvider))]
	class CHGTSTCusTempStorageDecProviderTest : CusTempStorageDecProviderAbstractTest<CHGTSTCusTempStorageDecProvider>
	{
		public void TestNewCustodianBranch()
		{
			TempStorageDec.NewCustodianBranch = "0123";
			AssertEquals("0123", TempStorageDecWrapped.NewCustodianBranch);
		}

		public void TestNewCustodianEoriNumber()
		{
			var companyOrgProxy = EU.Business.Testing.ExtensionsTest.SetupOrgHeaderWithEORICode(Factory, "eoriCompany");
			companyOrgProxy.OH_Code = "COMPORGPROX";
			var branchOrgProxy = EU.Business.Testing.ExtensionsTest.SetupOrgHeaderWithEORICode(Factory, "eoriBranch");
			branchOrgProxy.OH_Code = "BRANORGPROX";
			Factory.Save();

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = companyOrgProxy.PK;
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchOrgProxy.PK;
			CombineAssertions(() =>
			{
				AssertEquals("Branch Eori Number", "GReoriBranch", TempStorageDecWrapped.NewCustodianEoriNumber);
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
				AssertEquals("Fallback to Company Eroi Number", "GReoriCompany", TempStorageDecWrapped.NewCustodianEoriNumber);
			});
		}

		public void TestOwnerReferenceNumber()
		{
			TempStorageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			TempStorageDec.STH_OwnerReferenceNumber = "ATB150002930220195875";
			CombineAssertions(() =>
			{
				AssertEquals("Owner Reference Number formatted.", "AT/B/15/000293/02/2019/5875", TempStorageDec.FormattedOwnerReferenceNumber);
				AssertEquals("Formatting not on data layer property", "ATB150002930220195875", TempStorageDecWrapped.OwnerReferenceNumber);
			});
		}

		public override void TestStorageLines()
		{
			base.TestStorageLines();
			AssertEquals("Is ICHGTSTTempStorageLine", true, typeof(ICHGTSTTempStorageLine).IsAssignableFrom(TempStorageDecWrapped.StorageLines.First().GetType()));
		}

		protected new ICHGTSTTempStorageDec TempStorageDecWrapped => (ICHGTSTTempStorageDec)base.TempStorageDecWrapped;

		protected new CHGTSTCusTempStorageDec TempStorageDec => (CHGTSTCusTempStorageDec)base.TempStorageDec;

		protected override CusTempStorageDec GetTempStorageDecToTest() => Factory.New<CHGTSTCusTempStorageDec>();

		protected override ITempStorageDec GetTempStorageDecWrapped() => new CHGTSTCusTempStorageDecProvider(TempStorageDec);
	}
}
