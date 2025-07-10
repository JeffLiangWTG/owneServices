using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public static class ComplementaryJobISTFinder
	{
		public static CusTempStorageJobHeader FindFromPreviousDocuments(BusinessObjectFactory factory, IEnumerable<EU.Business.Declaration.MultiLineAddInfos.PreviousDocument> previousDocuments, ZString complementaryDocumentCode, ZString country)
		{
			CusTempStorageJobHeader complementaryIST = null;
			if (!complementaryDocumentCode.IsEmpty)
			{
				var previousReferenceNumber = previousDocuments.FirstOrDefault(x => x.CSI_Code == complementaryDocumentCode)?.CSI_ReferenceNumber ?? ZString.Empty;
				complementaryIST = FindFromReferenceNumber(factory, previousReferenceNumber, country);
			}
			return complementaryIST;
		}

		public static CusTempStorageJobHeader FindFromReferenceNumber(BusinessObjectFactory factory, ZString referenceNumber, ZString country)
		{
			CusTempStorageJobHeader parent = null;
			if (!referenceNumber.IsEmpty)
			{
				if (referenceNumber.StartsWith(CusTempStorageJobHeader.Schema.JobReferencePrefix))
				{
					parent = factory.LoadTop1<CusTempStorageJobHeader>(new ZQuery(CusTempStorageJobHeaderSchema.SJH_JobReference, referenceNumber));
				}
				else
				{
					ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.France.DDT);
					query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, country);
					query.AddToFilter(CusEntryNumSchema.CE_EntryNum, referenceNumber);
					query.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusTempStorageJobHeader.Schema.TableName);
					var cusEntryNum = factory.LoadTop1<CusEntryNumber>(query);
					if (cusEntryNum != null)
					{
						parent = cusEntryNum.Parent as CusTempStorageJobHeader;
					}
				}
			}
			return parent;
		}
	}
}
