using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.AsycudaCustoms.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BLNSCustoms.Business.Testing
{
	[TestedType(typeof(ApplicationBusinessProvider))]
	class ApplicationBusinessProviderTest : BaseApplicationBusinessProviderTest
	{
		protected override ZString ExpectedTariffType => Constants.CusTariffCode.Schedule1Part1;

		protected override IEnumerable<ZString> CountriesInGroup
		{
			get
			{
				yield return Enterprise.Core.Constants.CountryCodes.Botswana;
			}
		}

		protected override ZString ExpectedDataSource => Core.Constants.Customs.Universal.DataSetTypes.WTGData;

		protected override Type ApplicationBusinessProviderType => typeof(ApplicationBusinessProvider);

		public void TestIsReciprocalRates()
		{
			AssertIsReciprocalRates(Core.Constants.CountryCodes.Botswana, false);
			AssertIsReciprocalRates(Core.Constants.CountryCodes.Lesotho, false);
			AssertIsReciprocalRates(Core.Constants.CountryCodes.Namibia, false);
			AssertIsReciprocalRates(Core.Constants.CountryCodes.Swaziland, false);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertIsReciprocalRates(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_IsReciprocal);
			}
		}

		void AssertIsReciprocalRates(string countryCode, bool expectValue)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var provider = new ApplicationBusinessProvider();
				var declaration = Factory.New<JobDeclaration>();
				CombineAssertions(() =>
				{
					AssertEquals("IsReciprocalRates for country: " + countryCode, expectValue, provider.IsReciprocalRates(declaration));
					AssertEquals("GlbCompany.CurrentCompany", GlbCompany.CurrentCompany.GC_IsReciprocal, provider.IsReciprocalRates(null));
				});
			}
		}
	}
}
