using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
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

		public static decimal? NetMassInKilogramsNullableByPreviousDocument(NctsCommonCargoDesc item)
		{
			var netMass = item.NetMassInKilograms;
			var previousDocumentsItem = item is NctsArrivalCargoDesc arrivalItem ? GetPreviousDocumentsN830(arrivalItem.PreviousDocuments) : item is NctsDepartureCargoDesc departureItem ? GetPreviousDocumentsN830(departureItem.PreviousDocuments) : null;
			var previousDocumentsBill = item.Bill is NctsBill bill ? GetPreviousDocumentsN830(bill.PreviousDocuments) : null;
			var previousDocumentsHeader = item.Header is NctsHeader header ? GetPreviousDocumentsN830(header.PreviousDocuments) : null;
			var hasPreviousDocuments = previousDocumentsItem != null || previousDocumentsBill != null || previousDocumentsHeader != null;
			return netMass.IsEmpty && !hasPreviousDocuments ? null : (decimal?)netMass;
		}

		static PreviousDocument GetPreviousDocumentsN830(ICusSupportingInfoCollection<PreviousDocument> previousDocuments) => previousDocuments.Cast<PreviousDocument>().FirstOrDefault(x => x.CSI_Code.EqualsIgnoringCase(NctsConstants.NctsTypeOfPreviousDocument.Codes.N830));
	}
}
