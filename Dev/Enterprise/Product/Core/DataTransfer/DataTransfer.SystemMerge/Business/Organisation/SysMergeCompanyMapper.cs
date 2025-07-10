using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.DataTransfer.SystemMerge.Business.Res;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters
{
	class SysMergeCompanyMapper
	{
		public ZString GetMappedCode(ZString originalCompanyCode)
		{
			ZString result = originalCompanyCode;

			if (!originalCompanyCode.IsEmpty && CompanyCodeMapping.ContainsKey(originalCompanyCode.ToString()))
			{
				result = CompanyCodeMapping[originalCompanyCode.ToString()];
			}

			return result;
		}

		public GlbCompany GetCompanyByCodeThrowingErrorIfNotFound(BusinessObjectFactory factory, ZString companyCode)
		{
			GlbCompany company = factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, companyCode);

			if (company == null)
			{
				string registryLocation = Res.GetString("a0d4cc46-27e7-4ba1-a0dd-4d83fcfc7f31", "System -> Data Import Settings -> System Merge -> Import Company Code Mapping");
				string message =
					Res.GetString("7cce9cfc-132b-4f6f-a88e-4a02db0ba512", "Could not find Company code = [{0}].\r\nPlease create this company or adjust your company code mapping in the registry ({1}).\r\nThen retry the import operation.", companyCode, registryLocation) + "\r\n";

				throw new Exception(message);
			}
			else
			{
				return company;
			}
		}

		#region Company Code Mapping

		Dictionary<string, string> CompanyCodeMapping
		{
			get
			{
				if (companyCodeMapping == null)
				{
					companyCodeMapping = GetCompanyMappingFromRegistry();
				}

				return companyCodeMapping;
			}
		}

		static Dictionary<string, string> GetCompanyMappingFromRegistry()
		{
			Dictionary<string, string> result = new Dictionary<string, string>();
			ReadOnlyCodeDescriptionPairList codeMappingList = SystemDataRegistry.Instance.SystemMergeCompanyCodeMapping.Value;

			if (codeMappingList != null)
			{
				foreach (ICodeDescription mapping in codeMappingList)
				{
					result.Add(mapping.Code, mapping.Description);
				}
			}

			return result;
		}

		Dictionary<string, string> companyCodeMapping;

		#endregion
	}
}
