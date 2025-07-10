using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CountryWrapper))]
	sealed class CountryWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			CountryWrapper wrapperEmpty = new CountryWrapper("", Factory);
			AssertEquals(ZString.Empty, wrapperEmpty.Code);
			AssertEquals(ZString.Empty, wrapperEmpty.CodeAndName);
			AssertEquals(ZString.Empty, wrapperEmpty.Name);
			AssertEquals(ZString.Empty, wrapperEmpty.EconomicGrouping);
			AssertEquals(ZString.Empty, wrapperEmpty.ISONumericCode);
			AssertEquals(0, wrapperEmpty.WorkingDays.Length);
			AssertEquals(0, wrapperEmpty.NonWorkingDays.Length);
			AssertEquals(0, wrapperEmpty.WeekDays.Length);
		}

		public void TestWrapperMappingAU()
		{
			RefCountry aU = RefCountry.LoadFromCountryCode(Factory, "AU");
			CountryWrapper wrapperAU = new CountryWrapper(aU.RN_Code, Factory);
			AssertEquals(aU.RN_Code, wrapperAU.Code);
			AssertEquals(aU.RN_Code + " - " + aU.RN_Desc, wrapperAU.CodeAndName);
			AssertEquals(aU.RN_Desc, wrapperAU.Name);
			AssertEquals(aU.RN_EconomicGrouping, wrapperAU.EconomicGrouping);
			AssertEquals("036", wrapperAU.ISONumericCode);
		}

		public void TestWrapperMappingJP()
		{
			RefCountry jP = RefCountry.LoadFromCountryCode(Factory, "JP");
			CountryWrapper wrapperJP = new CountryWrapper(jP.RN_Code, Factory);
			AssertEquals(jP.RN_Code, wrapperJP.Code);
			AssertEquals(jP.RN_Code + " - " + jP.RN_Desc, wrapperJP.CodeAndName);
			AssertEquals(jP.RN_Desc, wrapperJP.Name);
			AssertEquals(jP.RN_EconomicGrouping, wrapperJP.EconomicGrouping);
			AssertEquals("392", wrapperJP.ISONumericCode);
		}

		public void TestWrapperMappingInvalid()
		{
			CountryWrapper wrapperInvalid = new CountryWrapper("XXX", Factory);
			AssertEquals("XXX", wrapperInvalid.Code);
			AssertEquals("XXX", wrapperInvalid.CodeAndName);
			AssertEquals("XXX", wrapperInvalid.Name);
			AssertEquals("", wrapperInvalid.EconomicGrouping);
			AssertEquals("", wrapperInvalid.ISONumericCode);
			AssertEquals(0, wrapperInvalid.WorkingDays.Length);
			AssertEquals(0, wrapperInvalid.NonWorkingDays.Length);
			AssertEquals(0, wrapperInvalid.WeekDays.Length);
		}

		public void TestRefCountryConstructorWithNull()
		{
			RefCountry refCountry = null;
			CountryWrapper wrapper = new CountryWrapper(refCountry, Factory);
			AssertEquals("wrapper.Code", ZString.Empty, wrapper.Code);
			AssertEquals("wrapper.Name", ZString.Empty, wrapper.Name);
			AssertEquals("wrapper.CodeAndName", ZString.Empty, wrapper.CodeAndName);
			AssertEquals("wrapper.EconomicGrouping", ZString.Empty, wrapper.EconomicGrouping);
			AssertEquals("wrapper.ISONumericCode", ZString.Empty, wrapper.ISONumericCode);
			AssertEquals(0, wrapper.WorkingDays.Length);
			AssertEquals(0, wrapper.NonWorkingDays.Length);
			AssertEquals(0, wrapper.WeekDays.Length);
		}

		public void TestRefCountryConstructor()
		{
			RefCountry refCountry = Factory.New<RefCountry>();
			refCountry.RN_Code = "AU";
			refCountry.RN_Desc = "SUPER SCOTT LAND";
			refCountry.RN_EconomicGrouping = "EUN";
			refCountry.RN_IsoNumericUNM49Code = "123";
			CountryWrapper wrapper = new CountryWrapper(refCountry, Factory);
			AssertEquals("wrapper.Code", "AU", wrapper.Code);
			AssertEquals("wrapper.Name", "SUPER SCOTT LAND", wrapper.Name);
			AssertEquals("wrapper.CodeAndName", "AU - SUPER SCOTT LAND", wrapper.CodeAndName);
			AssertEquals("wrapper.EconomicGrouping", "EUN", wrapper.EconomicGrouping);
			AssertEquals("wrapper.ISONumericCode", "123", wrapper.ISONumericCode);
		}

		public void TestRefCountryWorkingDays()
		{
			RefCountry aU = RefCountry.LoadFromCountryCode(Factory, "AU");
			CountryWrapper wrapperAU = new CountryWrapper(aU.RN_Code, Factory);
			aU.IsSaturdayNonWorkingDay = true;
			aU.IsSundayNonWorkingDay = true;
			AssertContainsExactElementsInExactOrder(new List<ZString> { "MON", "TUE", "WED", "THU", "FRI" }, wrapperAU.WorkingDays);
			AssertContainsExactElementsInExactOrder(new List<ZString> { "SUN", "SAT" }, wrapperAU.NonWorkingDays);
			AssertContainsExactElementsInExactOrder(new List<ZString> { "SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT" }, wrapperAU.WeekDays);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Country                                   (Default Field: CodeAndName)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Code                                    String
CodeAndName                             String
EconomicGrouping                        String
ISONumericCode                          String
Name                                    String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"Registry : (No Default Field Value Available on Registry)"; }
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			RefCountry refCountry = Factory.New<RefCountry>();
			return new CountryWrapper(refCountry, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new CountryWrapper(ZString.Empty, Factory);
		}
	}
}
