using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CommonGuaranteeLookups))]
	sealed class CommonGuaranteeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestList71NonEcContractingCountriesList()
		{
			var guarantee = Factory.New<JobDeclaration>().Guarantees.AddNew();
			var list = guarantee.Lookups.List71NonEcContractingCountriesList.CodesAsString;
			AssertContains("AD", list);
			AssertContains("CH", list);
			AssertContains("RS", list);
			AssertContains("MK", list);
			AssertNotContains("GB", list);
			AssertNotContains("AU", list);
		}

		public void TestReferenceNumbers()
		{
			var importer = CreateOrgHeader("IMP11111111", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
			var declarant = CreateOrgHeader("DEC22222222", OrgCusCode.SpainCodeTypes.NIF);

			var gua1 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			gua1.CPH_Type = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			gua1.CPH_SubType = "1";
			gua1.CPH_Number = "G1";
			gua1.CPH_OH_PermitHolder = importer.PK;

			var permit1 = Factory.NewWithValidTestData<CusPermitHeader>();
			permit1.CPH_Type = "AAA";
			permit1.CPH_Number = "P1";
			permit1.CPH_OH_PermitHolder = importer.PK;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var gua2 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				gua2.CPH_Type = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
				gua2.CPH_SubType = "1";
				gua2.CPH_Number = "G2";
				gua2.CPH_OH_PermitHolder = importer.PK;

				var gua3 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				gua3.CPH_Type = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
				gua3.CPH_SubType = "1";
				gua3.CPH_Number = "G3";
				gua3.CPH_OH_PermitHolder = declarant.PK;

				var gua4 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				gua4.CPH_Type = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
				gua3.CPH_SubType = "0";
				gua4.CPH_Number = "G4";
				gua4.CPH_OH_PermitHolder = importer.PK;
			}

			var declaration = Factory.New<JobDeclaration>();
			var guarantee = declaration.Guarantees.AddNew();
			declaration.JE_OH_Importer = importer.PK;
			declaration.Declarant.OA_OH = declarant.PK;
			guarantee.PW_BondNumber = "ABC123";
			guarantee.PW_BondType = "GUA";

			CombineAssertions(() =>
			{
				var referenceNumbers = guarantee.Lookups.ReferenceNumbers;
				AssertContainsExactElementsInAnyOrder("Reference numbers will be loaded if they are type GUA and their permit holder is either the importer or the declarant",
					new[] { "G1", "G2", "G3", "G4" }, referenceNumbers.Select(x => x.CPH_Number));
				var filterBusinessObjectDefaults = referenceNumbers.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().Where(x =>
						x.FilterName == Customs.Business.CusGuaranteeHeaderCollection.FilterConstants.GuaranteeHolders ||
						x.FilterName == Customs.Business.CusGuaranteeHeaderCollection.FilterConstants.GuaranteeNumber ||
						x.FilterName == Customs.Business.CusGuaranteeHeaderCollection.FilterConstants.GuaranteeType)
					.GroupBy(x => x.FilterName).ToDictionary(x => x.Key, x => x.ToList());

				AssertEquals(3, filterBusinessObjectDefaults.Count);
				AssertEquals("Guarantee Number is defaulted", "ABC123", filterBusinessObjectDefaults[Customs.Business.CusGuaranteeHeaderCollection.FilterConstants.GuaranteeNumber].Single().Value);
				AssertEquals("Type is defaulted", CusPermitHeaderApplicationCodeList.Codes.Guarantee, filterBusinessObjectDefaults[Customs.Business.CusGuaranteeHeaderCollection.FilterConstants.GuaranteeType].Single().Value);
				var guaranteeHoldersDefaults = filterBusinessObjectDefaults[Customs.Business.CusGuaranteeHeaderCollection.FilterConstants.GuaranteeHolders];
				AssertEquals("PrimaryGuaranteeHolderAddress", importer.PK, guaranteeHoldersDefaults.Single(x => x.PropertyName == "Property1").Value);
				AssertEquals("SecondaryGuaranteeHolderAddress", declarant.PK, guaranteeHoldersDefaults.Single(x => x.PropertyName == "Property2").Value);
				AssertEquals("SecondaryGuaranteeHolderAddress", declarant.PK, guaranteeHoldersDefaults.Single(x => x.PropertyName == "Property2").Value);
			});
		}

		public void TestHolderIdentificationList()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var guarantee = Factory.New<CommonGuarantee>();
				var lookup = new CommonGuaranteeLookups(guarantee);
				var expectedList = new CodeDescriptionPairList();
				AssertEquals(expectedList, lookup.HolderIdentificationList);
			}
		}

		OrgHeader CreateOrgHeader(string code, string type)
		{
			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(type, code);
			return org;
		}
	}
}
