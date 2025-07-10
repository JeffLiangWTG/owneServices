using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.MasterFiles.Testing
{
	[TestedType(typeof(CusClassification))]
	public class CusClassificationTest : Customs.Business.Testing.BaseCusClassificationTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Analyzer suggests BaseSupplementaryCode.Loader, which is less readible")]
		public void TestISupplementaryCodeSupporterProperties()
		{
			var classification = Factory.New<CusClassification>();
			var supporter = classification as ISupplementaryCodeSupporter;

			var additionalSupplementaryCode = classification.AdditionalSupplementaryCodes.AddNew();
			additionalSupplementaryCode.CY_Code = "1";
			var loader = new SupplementaryCode.Loader(Factory);
			var supplementaryCode1 = loader.LoadOrCreate<SupplementaryCode, CusClassification>(classification, 1);

			classification.CC_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			CombineAssertions(() =>
			{
				AssertEquals("SupplementaryCodesFieldType", nameof(FieldType.Text), supporter.SupplementaryCodesFieldType);
				AssertArrayEqualsByElements("SupplementaryCodes", new ZGuid[] { additionalSupplementaryCode.PK, supplementaryCode1.PK }, supporter.SupplementaryCodes.Select(x => x.PK).ToArray());
				AssertNull("Tariff", supporter.Tariff);
				AssertNull("RateSelectionCriteria", supporter.RateSelectionCriteria);
				AssertEquals("GetCountryCodeForSupplementaryCodeProvider", Core.Constants.CountryCodes.France, supporter.GetCountryCodeForCodeProvider());
				AssertNull("CachedListOfAdditionalCodeDescriptions", supporter.CachedListOfAdditionalCodeDescriptions);
				AssertNull("SupplementaryCodeCaption", supporter.SupplementaryCodeCaption);
			});
		}

		public void TestNumberOfSupplementaryCodesAllowed()
		{
			var classification = Factory.New<CusClassification>();
			var supplementaryCodeProvider = SupplementaryCodeProvider.GetBySupplementaryCodeSupporter(classification);
			for (int i = 0; i < supplementaryCodeProvider.NumberOfCodes; i++)
			{
				classification.AdditionalSupplementaryCodes.AddNew();
			}
			Assert("Should not be able to add more supplementary codes", !classification.AdditionalSupplementaryCodes.AllowNew);
		}

		public void TestAdditionalSupplementaryCodesCalculatedField()
		{
			var classification = Factory.New<CusClassification>();
			var collection = classification.AdditionalSupplementaryCodes;
			collection.AddNew("ABCD");
			collection.AddNew("EFGH");
			AssertEquals("ABCD,EFGH", classification.CC_EcAdditionalSupplements);
		}

		public void TestDefaultValues()
		{
			AssertEquals("Country is set", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Classification.CC_RN_NKCountryCode);
			AssertEquals("Type is set", CusClassification.ClassificationType.Both, Classification.CC_ClassificationType);
		}

		public void TestTypeDecider()
		{
			Assert("Update Customs.Business.BaseCusClassification to include a decider for this class", Factory.New(typeof(BaseCusClassification)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestGetDescriptionFromTariffUsingRefCusTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
				.ZZZ_ZZZ_Grouping = helper.CreateNewOrGetExistingDataGrouping("EUN").PK;
			Factory.Save();

			var impTariffTypePK = helper.CreateNewOrGetExistingTariffType("EUN", "IMP").PK;
			var expTariffTypePK = helper.CreateNewOrGetExistingTariffType("EUN", "EXP").PK;
			Factory.Save();

			helper.CreateTariff("EUN", impTariffTypePK, "6107110000", ZDateTime.Now.AddYears(-5), ZDateTime.Now.AddYears(5), "Clothes");
			helper.CreateTariff("EUN", impTariffTypePK, "0101210000", ZDateTime.Now.AddYears(-5), ZDateTime.Now.AddYears(5), "Horses");
			helper.CreateTariff("EUN", expTariffTypePK, "61071100", ZDateTime.Now.AddYears(-5), ZDateTime.Now.AddYears(5), "Clothes E");
			helper.CreateTariff("EUN", expTariffTypePK, "01012100", ZDateTime.Now.AddYears(-5), ZDateTime.Now.AddYears(5), "Horses E");

			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = "BTH";
			classification.CC_TariffNum = "6107110000";
			AssertEquals("Clothes", classification.CC_Description);
			classification.CC_TariffNum = "0101210000";
			AssertEquals("Horses", classification.CC_Description);
			classification.CC_ClassificationType = "EXP";
			AssertEquals("", classification.CC_Description);
			classification.CC_TariffNum = "01012100";
			AssertEquals("Horses E", classification.CC_Description);
			classification.CC_TariffNum = "61071100";
			AssertEquals("Clothes E", classification.CC_Description);
			classification.CC_Description = "Customised description";
			classification.CC_TariffNum = "01012100";
			AssertEquals("Desc is unchanged if it's the user's own", "Customised description", classification.CC_Description);
		}

		public override void TestITariffFormatProvider()
		{
			var classification = Factory.New<CusClassification>();
			AssertType<TariffFormatterThirteen>("TariffFormatter", ((Common.ITariffFormatProvider)classification).TariffFormatter);
		}

		public void TestSupplementaryCodes()
		{
			var classification = Factory.New<CusClassification>();

			AssertEquals("When NO Supplementary Codes are entered, SupplementaryCodes Count", 0, classification.SupplementaryCodes.Count());

			classification.CC_EcSupplement1 = "S001";
			AssertEquals("SupplementaryCodes Count", 1, classification.SupplementaryCodes.Count());
			AssertContainsExactElementsInAnyOrder("", new ZString[] { "S001" }, classification.SupplementaryCodes.Select(x => x.CY_Code).ToArray());

			classification.CC_EcSupplement2 = "S002";

			var additionalSupplementaryCode3 = classification.AdditionalSupplementaryCodes.AddNew();
			additionalSupplementaryCode3.CY_Code = "S003";
			additionalSupplementaryCode3.CY_Order = 3;
			var additionalSupplementaryCode4 = classification.AdditionalSupplementaryCodes.AddNew();
			additionalSupplementaryCode4.CY_Code = "";
			additionalSupplementaryCode3.CY_Order = 4;

			AssertEquals("SupplementaryCodes Count", 4, classification.SupplementaryCodes.Count());
			AssertContainsExactElementsInAnyOrder("", new ZString[] { "S001", "S002", "S003", "" }, classification.SupplementaryCodes.Select(x => x.CY_Code).ToArray());
		}

		public void TestGetCountryCodeFromAdditionalCode()
		{
			var classification = Factory.New<CusClassification>();

			AssertNullOrEmpty(classification.GetCountryCodeFromAdditionalCode("AD"), ZString.Empty);
		}

		#region Implementation

		protected new CusClassification Classification
		{
			get { return (CusClassification)base.Classification; }
		}

		#endregion
	}
}
