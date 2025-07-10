using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public static class NctsDataRetrieveMethods
	{
		public static List<UNDGDataItem> GetUNDGDataItems(BusinessObject parent)
		{
			var query = new ZQuery(UNDGDataItemSchema.DI_ParentID, parent.PK);
			query.AddToFilter(UNDGDataItemSchema.DI_ParentTableCode, parent.TablePrefix);
			return parent.Factory.Load<UNDGDataItem>(query).ToList();
		}

		public static List<CusReference> GetCusReferences(BusinessObjectFactory factory, ZGuid parentPK, ZString? type = null)
		{
			var result = new List<CusReference>();
			if (!parentPK.IsEmpty)
			{
				var query = new ZQuery(CusReferenceSchema.CFR_ParentID, parentPK);
				if (!string.IsNullOrEmpty(type))
				{
					query.AddToFilter(CusReferenceSchema.CFR_Type, type);
				}
				result = factory.Load<CusReference>(query).ToList();
			}
			return result;
		}

		public static IReadOnlyCollection<CusSupportingInfo> GetCusSupportingInfo(BusinessObjectFactory factory, ZGuid parentPK, ZString type, ZString? subType = null)
		{
			if (!parentPK.IsEmpty && !type.IsEmpty)
			{
				var query = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, parentPK);
				query.AddToFilter(CusSupportingInfoSchema.CSI_Type, type);
				if (!string.IsNullOrEmpty(subType))
				{
					query.AddToFilter(CusSupportingInfoSchema.CSI_SubType, subType);
				}

				return factory.Load<CusSupportingInfo>(query).ToArray();
			}
			return Array.Empty<CusSupportingInfo>();
		}

		public static List<OrgCusCode> GetOrgCusCode(JobDocAddress jobDocAddress, string type)
		{
			return jobDocAddress.Organisation?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(type).ToList();
		}

		public static CusPermitHeader[] GetCusPermitHeader(BusinessObjectFactory factory, OrgAddress orgAddress)
			=> orgAddress != null ? factory.Load<CusPermitHeader>(new ZQuery(CusPermitHeaderSchema.CPH_OH_PermitHolder, orgAddress.OA_OH)) : null;

		public static JobDocAddress GetJobDocAddress(BusinessObject parent, ZString addressType)
		{
			JobDocAddress result = null;
			if (parent != null && !addressType.IsEmpty)
			{
				var query = new ZQuery(JobDocAddressSchema.E2_ParentID, parent.PK);
				query.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, parent.TablePrefix);
				query.AddToFilter(JobDocAddressSchema.E2_AddressType, addressType);
				result = parent.Factory.LoadTop1<JobDocAddress>(query);
			}
			return result;
		}
	}
}
