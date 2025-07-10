using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(CusOtherLawReference))]
	public class CusOtherLawReferenceTest : CusReferenceAbstractTest<CusOtherLawReference>
	{
		public void TestLookupsType()
		{
			AssertType<CusOtherLawReferenceLookups>(cusOtherLawReference.Lookups);
		}

		public virtual void TestValidationType()
		{
			AssertType<CusOtherLawReferenceValidation>(cusOtherLawReference.Validation);
		}

		public virtual void TestGetCodeDescription()
		{
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws, "TestDescritpion", Core.Constants.CountryCodes.Japan);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws, "C1", "Description1", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			newFactory.Save();

			cusOtherLawReference.CFR_Reference = "C1";
			AssertEquals("Description1", cusOtherLawReference.CodeDescription);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Other Law", cusOtherLawReference.HumanReadableName);
		}

		protected override IEnumerable<CusOtherLawReference> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<BaseJobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var collection = new CusOtherLawReferenceCollection<CusOtherLawReference>(instruction);
			var otherLaws1 = collection.AddNew();
			otherLaws1.CFR_Reference = "11";
			yield return otherLaws1;

			var otherLaws2 = collection.AddNew();
			otherLaws2.CFR_Reference = "11";
			yield return otherLaws2;
		}

		public void TestIsRoadTransportVehicleLaw()
		{
			var otherLaw = Factory.New<CusOtherLawReference>();
			Assert(!otherLaw.IsRoadTranportVehicleLaw);

			otherLaw.CFR_Reference = "MS";
			Assert(otherLaw.IsRoadTranportVehicleLaw);

			otherLaw.CFR_Reference = "MM";
			Assert(otherLaw.IsRoadTranportVehicleLaw);

			otherLaw.CFR_Reference = "AB";
			Assert(!otherLaw.IsRoadTranportVehicleLaw);
		}

		protected override void SetUp()
		{
			cusOtherLawReference = BusinessObject as CusOtherLawReference;
		}

		protected CusOtherLawReference cusOtherLawReference;
	}
}
