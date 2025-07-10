using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentWrappers.Testing;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	public abstract class CustomsEntryWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			CustomsEntryWrapper wrapperEmpty = (CustomsEntryWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapperEmpty.ToString()", ZString.Empty, wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.EntryNumber", ZString.Empty, wrapperEmpty.EntryNumber);
			AssertEquals("wrapperEmpty.EntryType.Code", ZString.Empty, wrapperEmpty.EntryType.Code);
		}

		public virtual void TestEntryCategory()
		{
			CustomsEntryWrapper wrapperEmpty = (CustomsEntryWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapperEmpty.EntryCategory", ZString.Empty, wrapperEmpty.EntryCategory);
		}

		public void TestSearcheableCusEntryNumber()
		{
			CustomsEntryWrapperForTest customsEntryWrapper = new CustomsEntryWrapperForTest(Factory.New<CusEntryNumber>(), Factory);

			customsEntryWrapper.GetEntryTypeImplementation = () => new CodeAndDescriptionWrapper("AAA", new DummyListForTesting(), null);
			customsEntryWrapper.GetEntryNumberImplementation = () => "11111";
			customsEntryWrapper.GetEntryCategoryImplementation = () => "OTH";
			customsEntryWrapper.GetCountryImplementation = () => "AU";

			ISearcheableCusEntryNumber searcheableCusEntryNumber = customsEntryWrapper;

			AssertEquals("AAA", searcheableCusEntryNumber.CE_EntryType);
			AssertEquals("11111", searcheableCusEntryNumber.CE_EntryNum);
			AssertEquals("OTH", searcheableCusEntryNumber.CE_Category);
			AssertEquals("AU", searcheableCusEntryNumber.CE_RN_NKCountryCode);
		}

		public abstract void TestWrapperMappingFull();

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
CustomsEntry                              (Default Field: EntryNumber)
======================================================================
Name                                    Type
----------------------------------------------------------------------
EntryType                               CodeAndDescription
Country                                 String
EntryCategory                           String
EntryNumber                             String
Information                             String
IssueDate                               DateTime
";
			}
		}

		class CustomsEntryWrapperForTest : CustomsEntryWrapper
		{
			public CustomsEntryWrapperForTest(BusinessObject objectToWrap, BusinessObjectFactory factory)
				: base(objectToWrap, factory)
			{
			}

			public Func<CodeAndDescriptionWrapper> GetEntryTypeImplementation { get; set; }
			protected override CodeAndDescriptionWrapper GetEntryType()
			{
				return GetEntryTypeImplementation != null ? GetEntryTypeImplementation() : null;
			}

			public Func<ZString> GetEntryNumberImplementation { get; set; }
			protected override ZString GetEntryNumber()
			{
				return GetEntryNumberImplementation != null ? GetEntryNumberImplementation() : ZString.Empty;
			}

			public Func<ZString> GetInformationImplementation { get; set; }
			protected override ZString GetInformation()
			{
				return GetInformationImplementation != null ? GetInformationImplementation() : ZString.Empty;
			}

			public Func<ZDateTime> GetIssueDateImplementation { get; set; }
			protected override ZDateTime GetIssueDate()
			{
				return GetIssueDateImplementation != null ? GetIssueDateImplementation() : ZDateTime.Empty;
			}

			public Func<ZString> GetCountryImplementation { get; set; }
			protected override ZString GetCountry()
			{
				return GetCountryImplementation != null ? GetCountryImplementation() : ZString.Empty;
			}

			public Func<ZString> GetEntryCategoryImplementation { get; set; }
			protected override ZString GetEntryCategory()
			{
				return GetEntryCategoryImplementation != null ? GetEntryCategoryImplementation() : ZString.Empty;
			}
		}
	}
}
