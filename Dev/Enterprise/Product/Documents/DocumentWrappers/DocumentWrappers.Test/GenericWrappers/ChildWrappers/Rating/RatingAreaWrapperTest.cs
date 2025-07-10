using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(RatingAreaWrapper))]
	sealed class RatingAreaWrapperTest : GenericWrapperTest
	{
		public void TestAreaType()
		{
			RefUNLOCO aubne = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");
			RefCountry au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			RefZoneHeader anz = Factory.New<RefZoneHeader>();
			anz.FZ_Code = "ANZX";
			anz.Countries.Add(au);

			RatingAreaWrapper portWrapper = new RatingAreaWrapper(aubne, Factory);
			RatingAreaWrapper countryWrapper = new RatingAreaWrapper(au, Factory);
			RatingAreaWrapper zoneWrapper = new RatingAreaWrapper(anz, Factory);

			AssertEquals("portWrapper.IsPort", true, portWrapper.IsPort);
			AssertEquals("portWrapper.IsCountry", false, portWrapper.IsCountry);
			AssertEquals("portWrapper.IsZone", false, portWrapper.IsZone);
			AssertEquals("countryWrapper.IsPort", false, countryWrapper.IsPort);
			AssertEquals("countryWrapper.IsCountry", true, countryWrapper.IsCountry);
			AssertEquals("countryWrapper.IsZone", false, countryWrapper.IsZone);
			AssertEquals("zoneWrapper.IsPort", false, zoneWrapper.IsPort);
			AssertEquals("zoneWrapper.IsCountry", false, zoneWrapper.IsCountry);
			AssertEquals("zoneWrapper.IsZone", true, zoneWrapper.IsZone);
		}

		public void TestByPort()
		{
			RatingAreaWrapper wrapper = new RatingAreaWrapper("AUBNE", Factory);
			AssertEquals("AUBNE", wrapper.Code);
			AssertEquals("Brisbane", wrapper.Name);
			AssertEquals("AUBNE", wrapper.Port.UNLOCO);
			AssertEquals("AU", wrapper.Country.Code);
		}

		public void TestByCountry()
		{
			RatingAreaWrapper wrapper = new RatingAreaWrapper("AU", Factory);
			AssertEquals("AU", wrapper.Code);
			AssertEquals("Australia", wrapper.Name);
			AssertEquals("", wrapper.Port.UNLOCO);
			AssertEquals("AU", wrapper.Country.Code);
		}

		public void TestByZoneWithSingleCountry()
		{
			RefZoneHeader zone = Factory.New<RefZoneHeader>();
			zone.FZ_Code = "BLAT";
			zone.FZ_Description = "Blaticus";
			zone.FZ_ZoneType = "RAT";
			zone.UNLOCOs.Add(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE"));
			zone.UNLOCOs.Add(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD"));

			Factory.Save();

			RatingAreaWrapper wrapper = new RatingAreaWrapper("BLAT", Factory);
			AssertEquals("BLAT", wrapper.Code);
			AssertEquals("Blaticus", wrapper.Name);
			AssertEquals("", wrapper.Port.UNLOCO);
			AssertEquals("AU", wrapper.Country.Code);
		}

		public void TestByZoneWithMultiCountry()
		{
			RefZoneHeader zone = Factory.New<RefZoneHeader>();
			zone.FZ_Code = "BLAT";
			zone.FZ_Description = "Blaticus";
			zone.FZ_ZoneType = "RAT";
			zone.UNLOCOs.Add(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE"));
			zone.UNLOCOs.Add(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "MYBAG"));

			Factory.Save();

			RatingAreaWrapper wrapper = new RatingAreaWrapper("BLAT", Factory);
			AssertEquals("BLAT", wrapper.Code);
			AssertEquals("Blaticus", wrapper.Name);
			AssertEquals("", wrapper.Port.UNLOCO);
			AssertEquals("", wrapper.Country.Code);
		}

		public override void TestWrapperMappingsEmpty()
		{
			RatingAreaWrapper wrapper = new RatingAreaWrapper("", Factory);
			AssertEquals("", wrapper.Code);
			AssertEquals("", wrapper.Name);
			AssertEquals("", wrapper.Port.UNLOCO);
			AssertEquals("", wrapper.Country.Code);
		}

		#region Implementation

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Country : AU - Australia
Port : AUBNE - Brisbane
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new RatingAreaWrapper("AUBNE", Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Rating Area                                      (Default Field: Name)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Country                                 Country
Port                                    Location
Code                                    String
IsCountry                               Bool
IsPort                                  Bool
IsZone                                  Bool
Name                                    String
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new RatingAreaWrapper("AUBNE", Factory);
		}

		#endregion
	}
}
