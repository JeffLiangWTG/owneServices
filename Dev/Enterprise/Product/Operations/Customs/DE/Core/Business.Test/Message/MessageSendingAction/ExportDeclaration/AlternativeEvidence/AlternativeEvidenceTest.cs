using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(AlternativeEvidence))]
	class AlternativeEvidenceTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLookups() => AssertType<AlternativeEvidenceLookups>(alternativeEvidence.Lookups);

		public void TestValidation() => AssertType<AlternativeEvidenceValidation>(alternativeEvidence.Validation);

		public void TestDocType_ClearReference()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Code_TD44E, Code_TD44E + " DESC");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Code_TD44E, "01", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Code_TD44E, "02", "02 DESC", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date,
				UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference, ZString.Empty);
			Factory.Save();

			CombineAssertions(() =>
			{
				alternativeEvidence.Reference = "XX";

				alternativeEvidence.DocType = "02";
				AssertEquals("Reference isn't ReadOnly", "XX", alternativeEvidence.Reference);

				alternativeEvidence.DocType = "01";
				AssertEquals("Reference is ReadOnly", ZString.Empty, alternativeEvidence.Reference);
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

			CombineAssertions(() =>
			{
				AssertEquals("DocType is empty", true, alternativeEvidence.ReferenceInfo.ReadOnly);

				alternativeEvidence.DocType = "XX";
				AssertEquals("DocType is invalid", true, alternativeEvidence.ReferenceInfo.ReadOnly);

				alternativeEvidence.DocType = "01";
				AssertEquals("DocType is valid, no 'Reference attribute'", true, alternativeEvidence.ReferenceInfo.ReadOnly);

				alternativeEvidence.DocType = "02";
				AssertEquals("DocType is valid, has 'Reference attribute'", false, alternativeEvidence.ReferenceInfo.ReadOnly);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AlternativeEvidence(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();

			alternativeEvidence = new AlternativeEvidence(Factory);
		}
		AlternativeEvidence alternativeEvidence;
	}
}
