using System;
using System.Collections.Generic;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Intrastat.Business.Testing
{
	[TestedType(typeof(IntrastatCustomsDataRegistry))]
	sealed class IntrastatCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<IntrastatCustomsDataRegistry>
	{
		public void TestIsForProductivityWise()
		{
			AssertEquals(false, IntrastatCustomsDataRegistry.Instance.IsForProductivityWise);
		}

		public void TestEnableIntrastatFunctions()
		{
			TestRegistryItem(IntrastatCustomsDataRegistry.Instance.EnableIntrastatFunctions,
				"EnableIntraStatFunctions",
				CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_Intrastat,
				"Enable Intrastat Functions",
				"Set to YES to enable Intrastat Functions.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestIsIntrastatEnabled()
		{
			IntrastatCustomsDataRegistry.Instance.EnableIntrastatFunctions.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, IntrastatCustomsDataRegistry.Instance.IsIntrastatEnabled);

			IntrastatCustomsDataRegistry.Instance.EnableIntrastatFunctions.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, IntrastatCustomsDataRegistry.Instance.IsIntrastatEnabled);
		}

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				yield return "EnableIntraStatFunctions";
			}
		}
	}
}
