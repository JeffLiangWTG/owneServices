using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CustomsEntryWrapperFromPkgPackage))]
	sealed class CustomsEntryWrapperFromPkgPackageTest : CustomsEntryWrapperTest
	{
		public override void TestEntryCategory()
		{
			var wrapperEmpty = (CustomsEntryWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapperEmpty.EntryCategory", "CUS", wrapperEmpty.EntryCategory);
		}

		public void TestGetCountry()
		{
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var wrapper = new CustomsEntryWrapperFromPkgPackage(entryNumber, Factory);

			AssertEquals(Core.Constants.CountryCodes.Australia, wrapper.Country);

			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, wrapper.Country);
		}

		public override void TestWrapperMappingFull()
		{
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryNum = "34345678";
			entryNumber.CE_EntryType = "IOT";
			entryNumber.CE_Category = "OTH";
			entryNumber.CE_EntryLineReference = "Info";
			entryNumber.CE_IssueDate = new ZDateTime(2009, 2, 2);
			var wrapperFull = new CustomsEntryWrapperFromPkgPackage(entryNumber, Factory);
			AssertEquals("wrapperFull.ToString()", "34345678", wrapperFull.ToString());
			AssertEquals("wrapperFull.EntryNumber", "34345678", wrapperFull.EntryNumber);
			AssertEquals("wrapperFull.EntryType.Code", "IOT", wrapperFull.EntryType.Code);
			AssertEquals("wrapperFull.EntryType.CodeAndDescription", "IOT - Internet of Things", wrapperFull.EntryType.CodeAndDescription);
			AssertEquals("wrapperFull.EntryCategory", "OTH", wrapperFull.EntryCategory);
			AssertEquals("wrapperFull.Information", "Info", wrapperFull.Information);
			AssertEquals("wrapperFull.IssueDate", new ZDateTime(2009, 2, 2), wrapperFull.IssueDate);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
EntryType : IOT - Internet of Things
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = "IOT";
			return new CustomsEntryWrapperFromPkgPackage(entryNumber, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new CustomsEntryWrapperFromPkgPackage(null, Factory);
		}
	}
}
