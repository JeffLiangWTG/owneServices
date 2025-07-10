using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestsSubclassesOf(typeof(G3CommonHeaderWrapper))]
	public abstract class G3CommonHeaderWrapperTest<TWrapper> : DataProviderTestCase<TWrapper> where TWrapper : G3CommonHeaderWrapper
	{
		public void TestLRN()
		{
			AssertEquals("Expected filled LRN", "LRN001", Provider.LRN);
		}

		public void TestCustomsOffice()
		{
			AssertEquals("Expected filled Customs Office", "COF", Provider.CustomsOffice);
		}

		public void TestPersonPresentingGoods()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Person Presenting Goods when Presenter is null", string.Empty, Provider.PersonPresentingGoods);

				header.AMA_OA_Presenter = orgHeader.MainAddress.PK;
				AssertEquals("Expected empty Id", ZString.Empty, Provider.PersonPresentingGoods);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
				AssertEquals("Still Expect empty Id", ZString.Empty, Provider.PersonPresentingGoods);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				AssertEquals("Expected NIF Id", "NIF22222222", Provider.PersonPresentingGoods);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.Government;
				AssertEquals("Expected Country+NIF for non NAT Organizations", "ESNIF22222222", Provider.PersonPresentingGoods);

				OrgCusCode eoriCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				AssertEquals("Expected EORI Id with country code", "FR22222222", Provider.PersonPresentingGoods);

				eoriCusCode.OK_CustomsRegNo = "ES22222222";
				eoriCusCode.OK_RN_NKCodeCountry = "ES";
				AssertEquals("Expected EORI Id with country code not repeated", "ES22222222", Provider.PersonPresentingGoods);
			});
		}

		public void TestDeclarant()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.Addresses.AddNew();
			header.Declarant.OA_OH = orgHeader.PK;

			CombineAssertions(() =>
			{
				var declarant = Provider.Declarant;
				AssertNotNull("Expected filled Declarant", declarant);
				AssertSame("Cached Declarant", Provider.Declarant, declarant);
			});
		}

		public void TestRepresentative()
		{
			var orgAddress = Factory.New<OrgAddress>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgAddress.OA_OH = orgHeader.PK;
			header.AMA_OA_Representative = orgAddress.PK;

			CombineAssertions(() =>
			{
				var representative = Provider.Representative;
				AssertNotNull("Expected filled Representative", representative);
				AssertSame("Cached Representative", Provider.Representative, representative);
			});
		}

		public void TestDeclarationDate()
		{
			AssertEquals("Expected filled Declaration Date", DateTime.MinValue, Provider.DeclarationDate);
		}

		public void TestPresentationDate()
		{
			AssertEquals("Expected filled Presentation Date", DateTime.MinValue, Provider.PresentationDate);
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_CustomsOffice = "COF";
			header.Bills.AddNew();
		}

		protected abstract TWrapper GetProviderCore();

		protected sealed override TWrapper GetProvider()
		{
			return GetProviderCore();
		}

		protected AsycudaManifestHeader header;
		protected string localReferenceNumber = "LRN001";
	}
}
