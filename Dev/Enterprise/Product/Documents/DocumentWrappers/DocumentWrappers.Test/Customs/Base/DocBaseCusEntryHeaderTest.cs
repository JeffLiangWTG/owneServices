using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	[Enterprise.Customs.Business.Testing.AsycudaCustomsCountries(Enterprise.Core.Constants.CountryCodes.Namibia)]
	sealed class DocBaseCusEntryHeaderTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestStaticNewReturnsCountrySpecificEntryHeader()
		{
			CombineAssertions(() =>
			{
				AssertDocCusEntryHeaderCountrySpecificType(Core.Constants.CountryCodes.Australia, typeof(AU.DocCusEntryHeader));
				AssertDocCusEntryHeaderCountrySpecificType(Core.Constants.CountryCodes.NewZealand, typeof(NZ.FormalEntry.DocCusEntryHeader));
				AssertDocCusEntryHeaderCountrySpecificType(Core.Constants.CountryCodes.Japan, null);
			});
		}

		void AssertDocCusEntryHeaderCountrySpecificType(ZString countryCode, Type docEntryHeaderType)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var entryHeader = Factory.New<CusEntryHeader>();
				var docEntryHeader = DocBaseCusEntryHeader.New(entryHeader, Factory);
				AssertEquals(countryCode, docEntryHeaderType, docEntryHeader?.GetType());
			}
		}
	}
}
