using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.Manifest.Business.Testing
{
	class IcsCustomsOfficeRequirementHelperTest : TestCaseWithFactory
	{
		public void TestOtherRequirements()
		{
			AssertCustomsOfficeRequirementEquals("OOA", new CustomsOfficeRequirement("OOA", false, false)
			{
				OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent }
			}, officeHelper.OtherRequirements.Single(x => x.OfficeRole == "OOA"));

			AssertCustomsOfficeRequirementEquals("OOF", new CustomsOfficeRequirement("OOF", true, false)
			{
				OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent }
			}, officeHelper.OtherRequirements.Single(x => x.OfficeRole == "OOF"));

			AssertCustomsOfficeRequirementEquals("OOS", new CustomsOfficeRequirement("OOS", false, false)
			{
				OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent }
			}, officeHelper.OtherRequirements.Single(x => x.OfficeRole == "OOS"));
		}

		void AssertCustomsOfficeRequirementEquals(ZString shortComment, CustomsOfficeRequirement first, CustomsOfficeRequirement second)
		{
			CombineAssertions(() =>
			{
				AssertEquals(shortComment + " Type", first.GetType(), second.GetType());
				AssertEquals(shortComment + " Office Role", first.OfficeRole, second.OfficeRole);
				AssertEquals(shortComment + " Office Roles For Lookup", first.OfficeRolesForLookup.First(), second.OfficeRolesForLookup.First());
				AssertEquals(shortComment + " Is Mandatory", first.IsMandatory, second.IsMandatory);
				AssertEquals(shortComment + " Is Local Country Only", first.IsLocalCountryOnly, second.IsLocalCountryOnly);
				AssertEquals(shortComment + " Friendly Name", first.FriendlyName, second.FriendlyName);
				AssertEquals(shortComment + " Max #offices supported", first.MaxOfficeCountLimit, second.MaxOfficeCountLimit);
				AssertEquals(shortComment + " Is Foreign Country Only", first.IsForeignCountryOnly, second.IsForeignCountryOnly);
			});
		}

		public void TestMainOffice()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var officeHelper = header.CustomsOfficeRequirementHelper;

			AssertNull(officeHelper.MainOffice);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
			officeHelper = header.CustomsOfficeRequirementHelper;
		}
		AsycudaManifestHeader header;
		CustomsOfficeRequirementHelper officeHelper;
	}
}
