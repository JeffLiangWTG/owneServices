using System.Collections.Generic;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IDataContextDataObject : IDataObject
	{
		ImportAction? Action { get; }

		ICodeDescriptionDataObject ActionPurpose { get; }
		ZString ActionPurposeCode { get; }
		ZString CompanyCodeToImportInto { get; }
		ZString CountryCodeToImportInto { get; }
		ZString EventBranchCode { get; }
		ZString EventDepartmentCode { get; }
		ZString DataProviderForCodeMapping { get; set; }

		ICodeDescriptionDataObject EventType { get; }

		ZString? EventReference { get; }

		IEnumerable<IDataSourceDataObject> DataSourceCollection { get; }
		IEnumerable<IDataTargetDataObject> DataTargetCollection { get; }
		IEnumerable<IRecipientRoleDataObject> RecipientRoleCollection { get; set; }

		IDocumentaryOverride DocumentaryOverride { get; }

		ZBool CodesMappedToTarget { get; set; }

		IEnumerable<KeyValuePair> ContextKeyValuePairs { get; }

		ZLong? Timestamp { get; set; }

		void AddDataSource(IEntityID entityID);
		void AddDataSource(IDataSourceDataObject dataSource);
		void AddDataSource(DataContextType type, ZString reference);
		void ClearDataSourceCollection();
		void SetCompanyAndDataProviderDetails(IGlbCompany currentCompany);
		void AddDataTargetAndSetCompanyAndDataProviderDetails(IUniversalJobLink universalLink);
		void SetDocumentaryOverride(ZString documentName, ZString purposeCode, ICodeDescriptionPairList purposeList, ZBool isSystemDefined, ZInt dataVersion, ZInt submissionVersion);
		void SetWorkflowInfo(WorkflowInfo info);
		void SetActionPurpose(ICodeDescriptionDataObject actionPurpose);
		void AddDataTarget(DataContextType type, ZString? reference, IOrganizationAddress owner = null);
		void AddDataTarget(IDataTargetDataObject dataTargetDataObject);
		void ClearDataTargetCollection();
		EnterpriseServerAndCompanyID GetEnterpriseServerAndCompanyIDs();
	}

	public class KeyValuePair
	{
		public KeyValuePair(ZString key, ZString value)
		{
			Key = key;
			Value = value;
		}

		public readonly ZString Key;
		public readonly ZString Value;
	}

	public class EnterpriseServerAndCompanyID
	{
		public EnterpriseServerAndCompanyID(ZString enterpriseID, ZString serverID, ZString companyCode)
		{
			EnterpriseID = enterpriseID;
			ServerID = serverID;
			CompanyCode = companyCode;
		}

		public readonly ZString EnterpriseID;
		public readonly ZString ServerID;
		public readonly ZString CompanyCode;
	}
}
