using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters.Testing
{
	internal class SysMergeCompanyMapperTest : TestCaseWithFactory
	{
		public void TestGetMappedCode()
		{
			string mappedCode = new SysMergeCompanyMapper().GetMappedCode(null);
			AssertEquals("Mapped Code for [null]", "", mappedCode);

			mappedCode = new SysMergeCompanyMapper().GetMappedCode("");
			AssertEquals("Mapped Code for []", "", mappedCode);

			// No mapping for XXX => return original code
			mappedCode = new SysMergeCompanyMapper().GetMappedCode("XXX");
			AssertEquals("Mapped Code for [XXX]", "XXX", mappedCode);

			// Create mapping XXX -> DEM
			CodeDescriptionPairList mappingList = new CodeDescriptionPairList();
			mappingList.AddPair("XXX", "DEM");
			SystemDataRegistry.Instance.SystemMergeCompanyCodeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappingList);

			mappedCode = new SysMergeCompanyMapper().GetMappedCode("XXX");
			AssertEquals("Mapped Code for XXX", "DEM", mappedCode);
		}

		public void TestGetCompanyByCodeThrowingErrorIfNotFound()
		{
			GlbCompany expectedCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "DEM");
			GlbCompany actualCompany = new SysMergeCompanyMapper().GetCompanyByCodeThrowingErrorIfNotFound(Factory, "DEM");
			AssertEquals("Should pass because company exists.", expectedCompany.PK, actualCompany.PK);

			try
			{
				new SysMergeCompanyMapper().GetCompanyByCodeThrowingErrorIfNotFound(Factory, "~@#");
				Fail("Should throw exception");
			}
			catch (Exception ex)
			{
				AssertEquals("Thrown Exception:", "Could not find Company code = [~@#].\r\nPlease create this company or adjust your company code mapping in the registry (System -> Data Import Settings -> System Merge -> Import Company Code Mapping).\r\nThen retry the import operation.\r\n", ex.Message);
			}
		}
	}
}
