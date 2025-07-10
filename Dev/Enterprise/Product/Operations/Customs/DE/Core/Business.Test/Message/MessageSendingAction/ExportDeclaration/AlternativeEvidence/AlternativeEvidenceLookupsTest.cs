using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants.RefCusCodeList.Codes;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.DE.Business.Testing
{
	class AlternativeEvidenceLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypeList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.AlternativeEvidenceTypeList;
				AssertEquals("CodesAsString", "11, 14, 15, 16, 17, 30, 31", list.CodesAsString);
				AssertSame("Cached", list, lookups.AlternativeEvidenceTypeList);
			});
		}

		public void TestDocTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Code_TD44E, Code_TD44E + " DESC");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Code_TD44E, "01", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Code_TD44E, "02", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Code_TD44E, Code_9ZZX, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Code_TD44E, Code_9ZZY, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			var list = (CodeDescriptionPairList)lookups.TransportDocumentTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "01, 02", list.CodesAsString);
				AssertSame("Cached", list, lookups.TransportDocumentTypeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var alternativeEvidence = new AlternativeEvidence(Factory);
			lookups = new AlternativeEvidenceLookups(alternativeEvidence);
		}
		AlternativeEvidenceLookups lookups;
	}
}
