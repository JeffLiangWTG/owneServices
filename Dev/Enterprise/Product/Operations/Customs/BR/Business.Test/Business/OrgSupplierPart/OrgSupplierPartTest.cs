using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(OrgSupplierPart))]
	public class OrgSupplierPartTest : Customs.Business.Testing.OrgSupplierPartTest
	{
		public void TestCustomsCountryCodeIsCorrect()
		{
			using (Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var part = Factory.New<OrgSupplierPart>();
				var pivot = part.PivotsForBinding.AddNew();
				AssertEquals(Core.Constants.CountryCodes.Brazil, pivot.CI_RN_NKCountry);
			}
		}

		public void TestUpdateLocalPartNumbersOnPartNumChanged()
		{
			var catalog1 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog1.LocalPartNumbers.AddNew().CGI_Reference = "444";
			var catalog2 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog2.LocalPartNumbers.AddNew().CGI_Reference = "555";
			catalog2.CGC_AuthorityIdentifier = "1";

			AssertLocalPartNumbers("catalog1: No New Local Part Number added", catalog1, new[] { "444" });
			AssertLocalPartNumbers("catalog2: No New Local Part Number added", catalog2, new[] { "555" });

			CombineAssertions(() =>
			{
				var product = Factory.NewWithValidTestData<OrgSupplierPart>();
				product.OP_PartNum = "123";
				var pivot1 = product.PivotsForBinding.AddNew();
				pivot1.CI_CGC_Catalog = catalog1.PK;

				var pivot2 = product.PivotsForBinding.AddNew();
				pivot2.CI_CGC_Catalog = catalog2.PK;
				Factory.Save();
				AssertLocalPartNumbers("catalog1: New Local Part Number added", catalog1, new[] { "444", "123" });
				AssertLocalPartNumbers("catalog2: New Local Part Number added", catalog2, new[] { "555", "123" });

				AssertNull("CID event should not be added when Local Part Number is a new one", catalog1.Logs.MostRecentLogByPostedTime(Events.ChangeOfIdentifier));
				AssertNull("CID event should not be added when Local Part Number is a new one", catalog2.Logs.MostRecentLogByPostedTime(Events.ChangeOfIdentifier));

				product.OP_PartNum = "234";
				Factory.Save();
				AssertLocalPartNumbers("catalog1: Local Part Number updated", catalog1, new[] { "444", "234" });
				AssertLocalPartNumbers("catalog2: Local Part Number updated", catalog2, new[] { "555", "234" });

				AssertNull("CID event should not be added when AuthorityIdentifier is empty", catalog1.Logs.MostRecentLogByPostedTime(Events.ChangeOfIdentifier));
				AssertEquals("|DES=A new Local Part Number was added because the Product Code was changed from 123 to 234.", catalog2.Logs.MostRecentLogByPostedTime(Events.ChangeOfIdentifier).SL_Reference);

				product.Delete();
				Factory.Save();
				AssertLocalPartNumbers("catalog1: Local Part Number deleted", catalog1, new[] { "444" });
				AssertLocalPartNumbers("catalog2: Local Part Number deleted", catalog2, new[] { "555" });
			});
		}

		public void TestUpdateLocalPartNumbersOnPartNumChangedInAnotherCountry()
		{
			var catalog1 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog1.LocalPartNumbers.AddNew().CGI_Reference = "444";
			var catalog2 = Factory.NewWithValidTestData<CusGoodsCatalog>();
			catalog2.LocalPartNumbers.AddNew().CGI_Reference = "555";
			catalog2.CGC_AuthorityIdentifier = "1";

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "999";

			var pivot1 = product.PivotsForBinding.AddNew();
			pivot1.CI_CGC_Catalog = catalog1.PK;

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_CGC_Catalog = catalog2.PK;
			Factory.Save();

			AssertContainsExactElementsInAnyOrder("catalog1: Local Part Number '999' added", new[] { "444", "999" }, catalog1.LocalPartNumbers.Select(x => x.CGI_Reference));
			AssertContainsExactElementsInAnyOrder("catalog2: Local Part Number '999' added", new[] { "555", "999" }, catalog2.LocalPartNumbers.Select(x => x.CGI_Reference));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var otherFactory = new BusinessObjectFactory();
				var productAU = otherFactory.Load<MasterFiles.Business.OrgSupplierPart>(product.PK);
				Assert("Load Product in AU", productAU is Integration.Customs.AU.IOrgSupplierPart);

				catalog1 = otherFactory.Load<CusGoodsCatalog>(catalog1.PK);
				catalog2 = otherFactory.Load<CusGoodsCatalog>(catalog2.PK);

				CombineAssertions(() =>
				{
					productAU.OP_PartNum = "123";
					otherFactory.Save();
					AssertLocalPartNumbers("catalog1: Local Part Number updated", catalog1, new[] { "444", "123" });
					AssertLocalPartNumbers("catalog2: Local Part Number updated", catalog2, new[] { "555", "123" });

					AssertNull("CID event should not be added when AuthorityIdentifier is empty", catalog1.Logs.MostRecentLogByPostedTime(Events.ChangeOfIdentifier));
					AssertEquals("|DES=A new Local Part Number was added because the Product Code was changed from 999 to 123.", catalog2.Logs.MostRecentLogByPostedTime(Events.ChangeOfIdentifier).SL_Reference);
				});
			}
		}

		void AssertLocalPartNumbers(string message, CusGoodsCatalog catalog, params string[] localPartNumbers)
		{
			AssertContainsExactElementsInAnyOrder(message, localPartNumbers, catalog.LocalPartNumbers.Select(x => x.CGI_Reference));
		}

		#region ExpectedClassificationCollectionType

		protected override Type ExpectedClassificationCollectionType
		{
			get
			{
				return typeof(ClassificationCollection<CusClassification>);
			}
		}

		#endregion

		#region GetNewBusinessObject

		protected override BusinessObject GetNewBusinessObject()
		{
			return OrgSupplierPart.New(Factory);
		}

		#endregion
	}
}
