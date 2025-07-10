using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CustomsEntryWrapperFromCusEntryNumber))]
	sealed class CustomsEntryWrapperFromCusEntryNumberTest : CustomsEntryWrapperTest
	{
		public override void TestEntryCategory()
		{
			CustomsEntryWrapper wrapperEmpty = (CustomsEntryWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapperEmpty.EntryCategory", "CUS", wrapperEmpty.EntryCategory);
		}

		public void TestGetCountry()
		{
			CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			CustomsEntryWrapper wrapper = new CustomsEntryWrapperFromCusEntryNumber(entryNumber, Factory);

			AssertEquals(Core.Constants.CountryCodes.Australia, wrapper.Country);

			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, wrapper.Country);
		}

		public override void TestWrapperMappingFull()
		{
			CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryNum = "34345678";
			entryNumber.CE_EntryType = "ZXZ";
			entryNumber.CE_Category = "CUS";
			entryNumber.CE_EntryLineReference = "Info";
			entryNumber.CE_IssueDate = new ZDateTime(2009, 2, 2);
			CustomsEntryWrapper wrapperFull = new CustomsEntryWrapperFromCusEntryNumber(entryNumber, Factory);
			AssertEquals("wrapperFull.ToString()", "34345678", wrapperFull.ToString());
			AssertEquals("wrapperFull.EntryNumber", "34345678", wrapperFull.EntryNumber);
			AssertEquals("wrapperFull.EntryType.Code", "ZXZ", wrapperFull.EntryType.Code);
			AssertEquals("wrapperFull.EntryType.CodeAndDescription", "ZXZ", wrapperFull.EntryType.CodeAndDescription);
			AssertEquals("wrapperFull.EntryCategory", "CUS", wrapperFull.EntryCategory);
			AssertEquals("wrapperFull.Information", "Info", wrapperFull.Information);
			AssertEquals("wrapperFull.IssueDate", new ZDateTime(2009, 2, 2), wrapperFull.IssueDate);

			CusEntryNumber entryNumber2 = Factory.New<CusEntryNumber>();
			entryNumber2.CE_EntryNum = "11111111";
			entryNumber2.CE_EntryType = "RRN";
			entryNumber2.CE_Category = "CUS";
			entryNumber2.CE_EntryLineReference = "Info";
			entryNumber2.CE_IssueDate = new ZDateTime(2009, 2, 2);
			entryNumber2.CE_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedStates;
			CustomsEntryWrapper wrapperFull2 = new CustomsEntryWrapperFromCusEntryNumber(entryNumber2, Factory);
			AssertEquals("wrapperFull.ToString()", "11111111", wrapperFull2.ToString());
			AssertEquals("wrapperFull.EntryNumber", "11111111", wrapperFull2.EntryNumber);
			AssertEquals("wrapperFull.EntryType.Code", "RRN", wrapperFull2.EntryType.Code);
			AssertEquals("wrapperFull.EntryType.CodeAndDescription", "RRN - Rail Reference Number", wrapperFull2.EntryType.CodeAndDescription);
			AssertEquals("wrapperFull.EntryCategory", "CUS", wrapperFull2.EntryCategory);
			AssertEquals("wrapperFull.Information", "Info", wrapperFull2.Information);
			AssertEquals("wrapperFull.IssueDate", new ZDateTime(2009, 2, 2), wrapperFull2.IssueDate);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
EntryType : QWE
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = "QWE";
			return new CustomsEntryWrapperFromCusEntryNumber(entryNumber, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new CustomsEntryWrapperFromCusEntryNumber(null, Factory);
		}
	}
}
