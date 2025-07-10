using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Environment
{
	public static class RegistryLoadingHelper
	{
		public static DynamicBusinessObjectCollection GetJoinedCountryCompanyBranchCollection(BusinessObjectFactory factory)
		{
			var result = new DynamicBusinessObjectCollection(factory);
			var sqlToFire = "select " + RefCountrySchema.PK.Name + ", " + GlbCompanySchema.PK.Name + ", " + GlbBranchSchema.PK.Name +
							" from " + RefCountrySchema.Constants.SqlSchemaName + "." + RefCountrySchema.Constants.TableName +
							" join " + GlbCompanySchema.Constants.SqlSchemaName + "." + GlbCompanySchema.Constants.TableName +
							" on " + RefCountrySchema.RN_Code.Name + " = " + GlbCompanySchema.GC_RN_NKCountryCode.Name +
							" left join " + GlbBranchSchema.Constants.SqlSchemaName + "." + GlbBranchSchema.Constants.TableName +
							" on " + GlbCompanySchema.PK.Name + " = " + GlbBranchSchema.GB_GC.Name;
			result.Load(sqlToFire);
#if DEBUG
			RegistryLoadCount++;
#endif
			return result;
		}

		public static bool IsVisible(Guid companyPk, Guid branchPk, IEnumerable<Guid> countryFilterPKs, DynamicBusinessObjectCollection joinedCountryCompanyBranchCollection)
		{
			var currentCountryPk = Guid.Empty;
			if (joinedCountryCompanyBranchCollection is { Count: > 0 })
			{
				DynamicBusinessObject dynamicBizO = null;
				if (companyPk != Guid.Empty)
				{
					dynamicBizO = joinedCountryCompanyBranchCollection.FirstOrDefault(bizO => (ZGuid)bizO[GlbCompanySchema.PK.Name] == companyPk);
				}
				else if (branchPk != Guid.Empty)
				{
					dynamicBizO = joinedCountryCompanyBranchCollection.FirstOrDefault(bizO => (ZGuid)bizO[GlbBranchSchema.PK.Name] == branchPk);
				}

				currentCountryPk = dynamicBizO != null ? ((ZGuid)dynamicBizO[RefCountrySchema.PK.Name]).ToGuid() : Guid.Empty;
			}

			return countryFilterPKs.Contains(currentCountryPk);
		}

#if DEBUG
		public static int RegistryLoadCount
		{
			get;
			set;
		}
#endif
	}
}
