using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.GB.Chief.Messaging.Testing
{
	class GbChiefHeaderTest : TestCaseWithFactory
	{
		public void TestConvertFromUnToChiefCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.EUCTY, MapDirectionList.Codes.BTH, "Description", isReadonly: false);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, Core.Constants.CountryCodes.Monaco, Core.Constants.CountryCodes.France,
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, Core.Constants.CountryCodes.PuertoRico, Core.Constants.CountryCodes.UnitedStates,
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, Core.Constants.CountryCodes.SvalbardAndJanMayen, Core.Constants.CountryCodes.Norway,
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF);
			Factory.Save();

			var entry = Factory.NewWithValidTestData<CusEntryHeader>();
			entry.Declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var header = new GbChiefHeaderForTesting(entry);

			AssertEquals("Empty returns Empty", ZString.Empty, header.ConvertFromUnToChiefCountry(ZString.Empty));
			AssertEquals("Unchanged when not in mapping", Core.Constants.CountryCodes.UnitedKingdom, header.ConvertFromUnToChiefCountry(Core.Constants.CountryCodes.UnitedKingdom));
			AssertEquals("MC-FR", Core.Constants.CountryCodes.France, header.ConvertFromUnToChiefCountry(Core.Constants.CountryCodes.Monaco));
			AssertEquals("PR-US", Core.Constants.CountryCodes.UnitedStates, header.ConvertFromUnToChiefCountry(Core.Constants.CountryCodes.PuertoRico));
			AssertEquals("SJ-NO", Core.Constants.CountryCodes.Norway, header.ConvertFromUnToChiefCountry(Core.Constants.CountryCodes.SvalbardAndJanMayen));
		}
	}

	class GbChiefHeaderForTesting : GbChiefHeader
	{
		public GbChiefHeaderForTesting(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		public override ZString JobType => throw new NotImplementedException();

		protected override IDocAddress Customer => throw new NotImplementedException();

		public override ZString HMRC_ASG_CODE(CusDecMessageTypeFunction originalOrReplacementOrDelete)
		{
			throw new NotImplementedException();
		}

		protected override GbLine GetNewGbLine(EU.Business.Declaration.CusEntryLine entryLine)
		{
			throw new NotImplementedException();
		}
	}
}
