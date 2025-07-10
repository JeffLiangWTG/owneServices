using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	[TestedType(typeof(AdditionalInfoLookups))]
	sealed class AdditionalInfoLookupsTest : TestCaseWithFactory
	{
		public void TestCodeList()
		{
			var countryCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(countryCode, "To test AdditionalInfoLookups");
			var additionalInformationCode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;

			helper.CreateNewOrGetExistingCusCodeType(additionalInformationCode, "AdditionalInformation");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Direction", "Desc.", additionalInformationCode, countryCode);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Level", "Desc.", additionalInformationCode, countryCode);

			var addInfo = helper.CreateCusCodeList(countryCode, additionalInformationCode, "IMITM", "Import Item", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			addInfo.Attributes.AddNew("Direction", "IMPORT");
			addInfo.Attributes.AddNew("Level", "ITEM");

			Factory.Save();

			var codeList = (CodeDescriptionPairList)additionalInfo.Lookups.CodeList;

			Assert(codeList.GetAllCodes().Contains("IMITM"));
			AssertEquals("Import Item", codeList.GetDescriptionFromCode("IMITM"));
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();

			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			additionalInfo = bill.AdditionalInfos.AddNew();

			helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			var uk = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, "Customs Declaration Service", uk);

			Factory.Save();
		}

		AdditionalInfo additionalInfo;
		UniversalReferenceTestDataHelper helper;
		#endregion
	}
}
