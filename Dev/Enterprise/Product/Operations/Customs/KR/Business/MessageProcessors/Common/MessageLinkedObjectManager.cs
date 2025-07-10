using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public static class MessageLinkedObjectManager
	{
		public static CusEntryHeader GetLinkedObject(BusinessObjectFactory factory, GlbCompany company, ZString entryNumber, ZString entryType)
		{
			CusEntryHeader result = null;
			var companyPK = company?.PK ?? ZGuid.Empty;
			if (!companyPK.IsEmpty)
			{
				var entryNumFilter = new ZQuery(CusEntryNumSchema.CE_EntryNum, entryNumber);
				entryNumFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
				entryNumFilter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.KoreaSouth);
				entryNumFilter.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryHeaderSchema.Constants.TableName);

				foreach (var entryNum in factory.Load<CusEntryNumber>(entryNumFilter))
				{
					var entryHeader = factory.Load(typeof(CusEntryHeader), entryNum.CE_ParentID) as CusEntryHeader;
					if (entryHeader != null)
					{
						var declaration = entryHeader.Declaration;
						if (declaration != null && (declaration.Branch?.GB_GC ?? ZGuid.Empty) == companyPK)
						{
							result = entryHeader;
						}
					}
					if (result != null)
					{
						break;
					}
				}
			}
			return result;
		}

		public static CusEntryHeader GetLinkedJobDeclarationD87(BusinessObjectFactory factory, ZGuid companyPK, ZString carnetCertificateNumber)
		{
			CusEntryHeader result = null;
			if (!companyPK.IsEmpty)
			{
				var declaration = GetDuplicateDeclarationD87(factory, companyPK, carnetCertificateNumber, ZGuid.Empty);
				if (declaration != null)
				{
					var entryQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
					entryQuery.AddToFilter(CusEntryHeaderSchema.CH_JE, declaration.PK);
					var entryHeader = factory.LoadTop1<CusEntryHeader>(entryQuery);
					result = entryHeader;
				}
			}
			return result;
		}

		public static JobDeclaration GetDuplicateDeclarationD87(BusinessObjectFactory factory, ZGuid companyPK, ZString carnetCertificateNumber, ZGuid declarationPK)
		{
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			query.AddToFilter(JobDeclarationSchema.JE_MessageType, ElectronicDocumentTypeList.Codes._D87);
			query.AddToFilter(JobDeclarationSchema.JE_GC, companyPK);
			query.AddToFilter(JobDeclarationSchema.JE_AgentsReference, carnetCertificateNumber);
			if (declarationPK != ZGuid.Empty)
			{
				query.AddToFilter(JobDeclarationSchema.PK, SQLComparisonOperator.NotEqual, declarationPK);
			}

			return factory.LoadTop1<JobDeclaration>(query);
		}

		public static CusMiscRequestHeader GetLinkedMiscRequestHeader(BusinessObjectFactory factory, GlbCompany company, ZString entryNumber, ZString entryType)
		{
			CusMiscRequestHeader result = null;
			var companyPK = company?.PK ?? ZGuid.Empty;
			if (!companyPK.IsEmpty)
			{
				var entryNumFilter = new ZQuery(CusEntryNumSchema.CE_EntryNum, entryNumber);
				entryNumFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
				entryNumFilter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.KoreaSouth);
				entryNumFilter.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusMiscRequestHeaderSchema.Constants.TableName);

				foreach (var entryNum in factory.Load<CusEntryNumber>(entryNumFilter))
				{
					var requestHeader = factory.Load(typeof(CusMiscRequestHeader), entryNum.CE_ParentID) as CusMiscRequestHeader;
					if (requestHeader != null)
					{
						if ((requestHeader.Branch?.GB_GC ?? ZGuid.Empty) == companyPK)
						{
							result = requestHeader;
						}
					}
					if (result != null)
					{
						break;
					}
				}
			}
			return result;
		}

		public static (CusEntryNumber, CusEntryHeader, CusReconDeclaration, EDIMessage) GetLinkedRefundObject(BusinessObjectFactory factory, GlbCompany company, ZString refundNumber, ZString entryType)
		{
			CusEntryNumber entryNum = null;
			CusEntryHeader entry = null;
			CusReconDeclaration declaration = null;
			EDIMessage eDIMessage = null;
			var companyPK = company?.PK ?? ZGuid.Empty;
			if (!companyPK.IsEmpty)
			{
				var entryNumFilter = new ZQuery(CusEntryNumSchema.CE_EntryNum, refundNumber);
				entryNumFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
				entryNumFilter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.KoreaSouth);
				entryNumFilter.AddToFilter(CusEntryNumSchema.CE_ParentTable, new string[] { CusEntryHeader.Schema.TableName, CusReconDeclaration.Schema.TableName });

				foreach (var tmpEntryNum in factory.Load<CusEntryNumber>(entryNumFilter))
				{
					if (tmpEntryNum.CE_ParentTable == CusEntryHeader.Schema.TableName)
					{
						var tmpEntry = factory.Load<CusEntryHeader>(tmpEntryNum.CE_ParentID);
						if (tmpEntry != null && tmpEntry.Declaration != null && tmpEntry.Declaration.Branch.GB_GC == companyPK)
						{
							entry = tmpEntry;
							entryNum = tmpEntryNum;
							eDIMessage = tmpEntry.Messages.GetLastMessageMatching(x => x.EM_MessageType == entryType && x.EM_MessageOwner == refundNumber);
							break;
						}
					}
					else if (tmpEntryNum.CE_ParentTable == CusReconDeclaration.Schema.TableName)
					{
						var tmpDeclaration = factory.Load<CusReconDeclaration>(tmpEntryNum.CE_ParentID);
						if (tmpDeclaration != null && tmpDeclaration.Branch.GB_GC == companyPK)
						{
							declaration = tmpDeclaration;
							entryNum = tmpEntryNum;
							eDIMessage = (EDIMessage)tmpDeclaration.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, entryType);
							break;
						}
					}
				}
			}
			return (entryNum, entry, declaration, eDIMessage);
		}
	}
}
