using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Customs.BE.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(AlternativeEvidence))]
class AlternativeEvidenceTest : NonPersistentBusinessObjectTestCase
{
	public void TestDocType_ClearReference()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeType(Code_TD44E, Code_TD44E + " DESC");
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Code_TD44E, "01", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Code_TD44E, "02", "02 DESC", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date,
			UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference, ZString.Empty);
		Factory.Save();

		var alternativeEvidence = new AlternativeEvidence(Factory);
		CombineAssertions(() =>
		{
			alternativeEvidence.Reference = "XX";
			AssertEquals("Reference is ReadOnly", "XX", alternativeEvidence.Reference);
		});
	}

	public void TestReference_ReadOnly()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeType(Code_TD44E, Code_TD44E + " DESC");
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Code_TD44E, "01", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Code_TD44E, "02", "02 DESC", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date,
			UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
		Factory.Save();

		var alternativeEvidence = new AlternativeEvidence(Factory);
		CombineAssertions(() =>
		{
			AssertEquals("DocType is empty", true, alternativeEvidence.ReferenceInfo.ReadOnly);

			alternativeEvidence.DocType = "XX";
			AssertEquals("DocType is invalid", true, alternativeEvidence.ReferenceInfo.ReadOnly);

			alternativeEvidence.DocType = "01";
			AssertEquals("DocType is valid, no 'Reference attribute'", true, alternativeEvidence.ReferenceInfo.ReadOnly);
		});
	}

	public void TestLookups() => AssertType<AlternativeEvidenceLookups>(new AlternativeEvidence(Factory).Lookups);

	public void TestValidation() => AssertType<AlternativeEvidenceValidation>(new AlternativeEvidence(Factory).Validation);
}
