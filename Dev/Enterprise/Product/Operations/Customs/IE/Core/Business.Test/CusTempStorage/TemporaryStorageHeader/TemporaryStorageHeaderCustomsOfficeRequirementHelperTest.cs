using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.CusTempStorage.Testing
{
	sealed class TemporaryStorageHeaderCustomsOfficeRequirementHelperTest : TestCaseWithFactory
	{
		public void TestCreateOtherRequirements()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var helper = new TemporaryStorageHeaderCustomsOfficeRequirementHelperForTest_Exposed(header);

			var officeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, EuOfficeCodesTypes.Codes.OfficeOfLodgementEntry, EuOfficeCodesTypes.Codes.CustomsOfficeForTemporaryStorage };

			CombineAssertions(() =>
			{
				AssertEquals("first office role", helper.OtherRequirements_Exposed.First().OfficeRole, "PRE");
				AssertSequencesEqual("first lookup", helper.OtherRequirements_Exposed.First().OfficeRolesForLookup, officeRolesForLookup);
				AssertEquals("sencond office role", helper.OtherRequirements_Exposed.Last().OfficeRole, "LOD");
				AssertSequencesEqual("sencond lookup", helper.OtherRequirements_Exposed.Last().OfficeRolesForLookup, officeRolesForLookup);
			});
		}

		class TemporaryStorageHeaderCustomsOfficeRequirementHelperForTest_Exposed : TemporaryStorageHeaderCustomsOfficeRequirementHelper
		{
			public TemporaryStorageHeaderCustomsOfficeRequirementHelperForTest_Exposed(IEuOfficeCodeProvider officeCodeProvider) : base(officeCodeProvider)
			{
			}

			public IEnumerable<CustomsOfficeRequirement> OtherRequirements_Exposed => CreateOtherRequirements();
		}
	}
}
