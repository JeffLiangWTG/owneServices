using System;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	public abstract class ICS2BaseMessageProviderTest<T> : Customs.Business.Testing.DataProviderTestCase<T>
		where T : ICS2BaseMessageProvider
	{
		public virtual void TestLRN()
		{
			AssertNullOrEmpty(Provider.LRN);

			EUICS2MessageTestHelper.SetManifestHeaderEntryNumberForTesting(manifestHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, "TestLocalReferenceNumber");
			AssertEquals("TestLocalReferenceNumber", Provider.LRN);
		}

		public void TestMRN()
		{
			AssertNullOrEmpty(Provider.MRN);

			manifestHeader.RegistrationNumber = "TestRegistrationNumber";
			AssertEquals("TestRegistrationNumber", Provider.MRN);
		}

		[TestDate(2023, 01, 18, 0, 0, 0)]
		public void TestDocumentIssueDate()
		{
			var documentIssueDate = Provider.CurrentDateTimeUtc;
			AssertEquals("CurrentDateTimeUtc", new DateTime(2023, 01, 18, 0, 0, 0, DateTimeKind.Utc).ToUniversalTime(), documentIssueDate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			manifestHeader = Factory.New<AsycudaManifestHeader>();

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "Name123";
			orgHeader.OH_Code = "OH123";
			var mainAddress = orgHeader.MainAddress;
			mainAddress.Address1 = "My Test Address 123";
			mainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "654321", CountryCodes.Germany);

			manifestHeader.AMA_OA_Declarant = mainAddress.PK;
		}

		protected override T GetProvider()
		{
			return (T)Activator.CreateInstance(typeof(T), manifestHeader);
		}

		protected AsycudaManifestHeader manifestHeader;
	}
}
