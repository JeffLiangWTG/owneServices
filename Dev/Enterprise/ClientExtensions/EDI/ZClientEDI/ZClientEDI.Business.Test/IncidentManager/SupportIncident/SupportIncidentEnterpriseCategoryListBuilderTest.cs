using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	class SupportIncidentEnterpriseCategoryListBuilderTest : TestCaseWithFactory
	{
		public void TestBuild()
		{
			SetupProductAndModuleRegistryMappings();

			var builder = new SupportIncidentEnterpriseMenuSectionListBuilder();

			var actualModules = builder.Build("ENT", ZString.Empty);
			var expectedCodes = new string[] { "AR1", "CU1", "ZZZ" };
			AssertModuleCodes(actualModules, expectedCodes);

			actualModules = builder.Build("ENT", ProductAreaList.Codes.ARC);
			expectedCodes = new string[] { "AR1" };
			AssertModuleCodes(actualModules, expectedCodes);
		}

		#region Implementation

		void AssertModuleCodes(CodeDescriptionPairList actualModules, IEnumerable<string> expectedCodes)
		{
			AssertContainsExactElementsInAnyOrder(expectedCodes, actualModules.Cast<ICodeDescription>().Select(module => module.Code).Where(x => expectedCodes.Contains(x)));
		}

		void SetupProductAndModuleRegistryMappings()
		{
			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("AR1", "ARC Module 1", ProductAreaList.Codes.ARC, false);
			product.ModuleMappings.AddNew("CU1", "CUS Module 1", ProductAreaList.Codes.CUS, false);
			product.ModuleMappings.AddNew("ZZZ", "No Area Defined Module 1", "", false);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		#endregion
	}
}