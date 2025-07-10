using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(CustomsIncoTermOverride))]
	class CustomsIncoTermOverrideTest : RegistryBusinessObjectTemplateTestCase<CustomsIncoTermOverride>
	{
		#region Test Lookup

		public void TestIncotermLookupType()
		{
			var incoTerm = new CustomsIncoTermOverride();
			var actualCodes = incoTerm.InternationalIncoTermList.ToArray().Select(x => x.Code);
			var expectedCodes = (new IncoTermsCodeDescriptionPairList(IncoTermsListType.IncoTerms2020)).ToArray().Select(x => x.Code).Union((new IncoTermsCodeDescriptionPairList(IncoTermsListType.IncoTerms2010)).ToArray().Select(x => x.Code));
			AssertContainsExactElementsInAnyOrder("Incoterms in 2020", expectedCodes, actualCodes);
		}

		#endregion

		#region Test Validations

		public void TestValidateCustomsCode()
		{
			CustomsIncoTermOverrideCollection collection = new CustomsIncoTermOverrideCollection();
			CustomsIncoTermOverride incoTerm = collection.AddNew();
			incoTerm.CustomsCode = "";
			AssertHasErrorContaining(incoTerm.CustomsCodeInfo, MandatoryValidation.MustBeEntered);

			incoTerm.CustomsCode = "AAA";
			AssertNoErrorContaining(incoTerm.CustomsCodeInfo, MandatoryValidation.MustBeEntered);

			CustomsIncoTermOverride incoTerm2 = collection.AddNew();
			incoTerm2.CustomsCode = "AAA";
			AssertHasError(incoTerm2.CustomsCodeInfo, string.Format("The {0} has been duplicated and must be unique.", incoTerm2.CustomsCodeInfo.HumanReadableName));

			incoTerm2.CustomsCode = "AAB";
			AssertNoError(incoTerm2.CustomsCodeInfo, string.Format("The {0} has been duplicated and must be unique.", incoTerm2.CustomsCodeInfo.HumanReadableName));
		}

		public void TestValidateInternationalCode()
		{
			CustomsIncoTermOverride incoTerm = new CustomsIncoTermOverride();
			incoTerm.InternationalCode = "";
			AssertHasErrorContaining(incoTerm.InternationalCodeInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(incoTerm.InternationalCodeInfo, ListValidation.InvalidCodeError);

			incoTerm.InternationalCode = "ZZZ";
			AssertNoErrorContaining(incoTerm.InternationalCodeInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(incoTerm.InternationalCodeInfo, ListValidation.InvalidCodeError);
			AssertNull(incoTerm.InternationalIncoTermList["ZZZ"]);

			incoTerm.InternationalCode = Core.Constants.IncoTerms.FreeOnBoard;
			AssertNoErrorContaining(incoTerm.InternationalCodeInfo, ListValidation.InvalidCodeError);
			AssertNotNull(incoTerm.InternationalIncoTermList[Core.Constants.IncoTerms.FreeOnBoard]);

			incoTerm.InternationalCode = Core.Constants.IncoTerms.DeliveredAtTerminal;
			AssertEquals("InternationalCodeInfo has warning", true, incoTerm.InternationalCodeInfo.HasWarning("This Incoterm is obsolete from 1 January 2020 according to the International Chamber of Commerce rules."));
		}

		#endregion

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override CustomsIncoTermOverride GetBusinessObjectToClone()
		{
			CustomsIncoTermOverrideCollection collection = new CustomsIncoTermOverrideCollection();
			CustomsIncoTermOverride result = collection.AddNew();

			result.CustomsCode = "AAA";
			result.InternationalCode = "BBB";

			return result;
		}

		protected override CustomsIncoTermOverride GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		//protected override void CheckAllPropertiesAreEqual(CustomsIncoTermOverride OriginalBusinessObject, CustomsIncoTermOverride NewBusinessObject, bool IsClone)
		//{
		//    base.CheckAllPropertiesAreEqual(OriginalBusinessObject, NewBusinessObject, IsClone);

		//    CustomsIncoTermOverride OriginalIncoTerm = (CustomsIncoTermOverride)OriginalBusinessObject;
		//    CustomsIncoTermOverride ClonedCharge = (CustomsIncoTermOverride)NewBusinessObject;

		//    AssertEquals("Delivery terms are cloned", OriginalIncoTerm.DeliveryTerms.Count, ClonedCharge.DeliveryTerms.Count);
		//    AssertEquals("Delivery Terms parent", ClonedCharge, ClonedCharge.DeliveryTerms.ParentCustomsIncoTerm);
		//    AssertEquals("Cloned delivery terms", true, ClonedCharge.IsRegisteredEditableChildObject(ClonedCharge.DeliveryTerms));
		//    if (IsClone)
		//    {
		//        AssertEquals("IsSystemGenerated", OriginalIncoTerm.IsSystemGenerated, ClonedCharge.IsSystemGenerated);
		//    }
		//}

		#endregion
	}
}
