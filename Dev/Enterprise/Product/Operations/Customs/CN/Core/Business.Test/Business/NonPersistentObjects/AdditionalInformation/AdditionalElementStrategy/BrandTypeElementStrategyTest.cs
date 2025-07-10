using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business.Testing
{
	class BrandTypeElementStrategyTest : TestCaseWithFactory
	{
		public void TestList()
		{
			var strategy = new BrandTypeElementStrategy();
			AssertEquals("ProvideList", true, strategy.ProvideList);
			AssertType<BrandTypeList>("GetList", strategy.GetList(Factory, EnteringOrExiting.Both));
			var anotherStrategy = new BrandTypeElementStrategy();
			AssertSame("Should have been cached.", strategy.GetList(Factory, EnteringOrExiting.Both), anotherStrategy.GetList(Factory, EnteringOrExiting.Both));
			Assert("Brand type list should be untranslatable", strategy.GetList(Factory, EnteringOrExiting.Both) is UntranslatableCodeDescriptionPairList);
			var enteringList = strategy.GetList(Factory, EnteringOrExiting.Entering);
			AssertEquals("Entering list should have removed code 3", 4, enteringList.Count);
			Assert("Entering list should have removed code 3", !enteringList.ContainsCode(BrandTypeList.Codes._3));
			AssertSame("Entering list should have been cached.", enteringList, anotherStrategy.GetList(Factory, EnteringOrExiting.Entering));
		}

		public void TestValidate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("2713200000", "00422");
			helper.CreateAdditionalElement("00422", "品牌类型");
			Factory.Save();
			var additionalElement = CNRefCusCodeListLoader.GetAdditionalElement(Factory, "00422", ZDateTime.Today);
			var addInfo = new AdditionalElementWrapper(additionalElement, "", new DummyAdditionalInformationWrapperParent(Factory), EnteringOrExiting.Both);
			addInfo.RunPreSaveValidation();
			AssertHasMessageErrorContaining(addInfo.ElementValueInfo, "entered");
			addInfo.ElementValue = "X";
			AssertHasMessageErrorContaining(addInfo.ElementValueInfo, "The code you have selected is not in the list");
			addInfo.ElementValue = "4";
			AssertNoMessageErrorContaining(addInfo.ElementValueInfo, "The code you have selected is not in the list");
		}

		public void TestIsMergeKey()
		{
			AssertEquals("IsMergeKey", true, new BrandTypeElementStrategy().IsMergeKey);
		}
	}

	public class DummyAdditionalInformationWrapperParent : NonPersistentBusinessObject, IAdditionalInformationWrapperParent
	{
		public DummyAdditionalInformationWrapperParent(BusinessObjectFactory factory)
			: base(factory)
		{
		}
		public TariffView UniversalTariff { get; set; }
		public ZString CountryOfOrigin { get; set; }
		public bool ElementValueAllowEmpty { get; set; }
		public AdditionalInformationCollection AdditionalInformationCodes => null;
		public ValidationModes ValidationMode => JobDeclaration.ValidationMode;
		public JobDeclaration JobDeclaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
				}
				return declaration;
			}
		}

		JobDeclaration declaration;
	}
}
