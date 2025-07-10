using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(ApplicationBusinessProvider))]
	class ApplicationBusinessProviderTest : BaseApplicationBusinessProviderTest
	{
		public override void TestGetApplicationBusinessProvider()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.CoteDivoire))
			{
				base.TestGetApplicationBusinessProvider();
			}
		}

		protected override Type ApplicationBusinessProviderType => typeof(ApplicationBusinessProvider);

		protected override ZString ExpectedTariffType => Universal.Constants.TariffTypes.HarmonizedSystem;

		protected override IEnumerable<ZString> CountriesInGroup
		{
			get
			{
				yield return Core.Constants.CountryCodes.Bangladesh;
			}
		}

		protected override ZString ExpectedDataSource => Core.Constants.Customs.Universal.DataSetTypes.OWNData;

		public void TestIsReciprocalRates()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.CoteDivoire))
			{
				var provider = new ApplicationBusinessProvider();
				var declaration = Factory.New<JobDeclaration>();
				CombineAssertions(() =>
				{
					AssertEquals("declaration.Company", declaration.Company.GC_IsReciprocal, provider.IsReciprocalRates(declaration));
					AssertEquals("GlbCompany.CurrentCompany", GlbCompany.CurrentCompany.GC_IsReciprocal, provider.IsReciprocalRates(null));
				});
			}
		}
	}
}
