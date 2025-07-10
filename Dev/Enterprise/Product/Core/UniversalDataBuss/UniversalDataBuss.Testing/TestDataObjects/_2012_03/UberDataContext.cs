using System;
using System.Collections.Generic;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalDataBussKeyValuePair = Enterprise.UniversalDataBuss.Integration.KeyValuePair;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects._2012_11
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), NamespaceSpecific(UniversalXmlInfo.Namespace_2012_11)]
	public class UberDataContext : IDataObject, IDataContextDataObject
	{
		public UberDataSource DataSource { get; set; }
		public UberWorkflow Workflow { get; set; }
		public ZLong? Timestamp { get; set; }

		public List<UberDataTarget> DataTargetCollection { get; set; }
		public UberDocumentaryOverride DocumentaryOverride { get; set; }
		public ImportAction? Action { get; set; }

		#region Safe Object Loaders

		UberWorkflow WorkflowSafe
		{
			get { return Workflow ?? (Workflow = new UberWorkflow()); }
		}

		UberDataSource DataSourceSafe
		{
			get { return DataSource ?? (DataSource = new UberDataSource()); }
		}

		#endregion

		#region IDataContextDataObject Implementation

		IDocumentaryOverride IDataContextDataObject.DocumentaryOverride
		{
			get { return DocumentaryOverride; }
		}

		ICodeDescriptionDataObject IDataContextDataObject.ActionPurpose
		{
			get { return WorkflowSafe.ActionPurpose; }
		}

		ZString IDataContextDataObject.ActionPurposeCode
		{
			get { return WorkflowSafe.ActionPurpose.GetCodeAsUpperCase(); }
		}

		ZString IDataContextDataObject.DataProviderForCodeMapping
		{
			get { return DataSourceSafe.DataProvider.GetCodeAsUpperCase(); }
			set { DataSourceSafe.DataProvider = new UberDataProvider { Code = value }; }
		}

		ZString IDataContextDataObject.CompanyCodeToImportInto
		{
			get { return WorkflowSafe.Company.GetCodeAsUpperCase(); }
		}

		ZString IDataContextDataObject.CountryCodeToImportInto
		{
			get
			{
				var company = WorkflowSafe.Company;
				return company == null ? ZString.Empty : company.Country.GetCodeAsUpperCase();
			}
		}

		ZString IDataContextDataObject.EventBranchCode
		{
			get { throw new NotImplementedException(); }
		}

		ICodeDescriptionDataObject IDataContextDataObject.EventType
		{
			get { return WorkflowSafe.EventType; }
		}

		ZString? IDataContextDataObject.EventReference
		{
			get { throw new NotImplementedException(); }
		}

		EnterpriseServerAndCompanyID IDataContextDataObject.GetEnterpriseServerAndCompanyIDs()
		{
			var dataProvider = DataSource.DataProvider;
			if (dataProvider != null && dataProvider.Type.GetValueOrDefault() == "EnterpriseID")
			{
				ZString enterpriseServerAndCompanyID = dataProvider.Code.GetValueOrDefault().ToUpper();
				return new EnterpriseServerAndCompanyID(
					enterpriseServerAndCompanyID.SubstringSafe(0, 3),
					enterpriseServerAndCompanyID.SubstringSafe(3, 3),
					enterpriseServerAndCompanyID.SubstringSafe(6, 3)
					);
			}

			return new EnterpriseServerAndCompanyID(ZString.Empty, ZString.Empty, ZString.Empty);
		}

		ZBool IDataContextDataObject.CodesMappedToTarget
		{
			get { return ZBool.False; }
			set { throw new NotImplementedException(); }
		}

		IEnumerable<UniversalDataBussKeyValuePair> IDataContextDataObject.ContextKeyValuePairs
		{
			get { throw new NotImplementedException(); }
		}

		IEnumerable<IDataSourceDataObject> IDataContextDataObject.DataSourceCollection
		{
			get { yield return DataSource; }
		}

		IEnumerable<IDataTargetDataObject> IDataContextDataObject.DataTargetCollection
		{
			get { return DataTargetCollection; }
		}

		IEnumerable<IRecipientRoleDataObject> IDataContextDataObject.RecipientRoleCollection
		{
			get { return null; }
			set { }
		}

		public ZString EventDepartmentCode
		{
			get { throw new NotImplementedException(); }
		}

		void IDataContextDataObject.AddDataSource(DataContextType type, ZString reference)
		{
			throw new NotImplementedException();
		}

		void IDataContextDataObject.AddDataTarget(DataContextType type, ZString? reference, IOrganizationAddress owner)
		{
			throw new NotImplementedException();
		}

		void IDataContextDataObject.AddDataSource(IEntityID entityID)
		{
			throw new NotImplementedException();
		}

		void IDataContextDataObject.AddDataSource(IDataSourceDataObject dataSource)
		{
			throw new NotImplementedException();
		}

		void IDataContextDataObject.SetCompanyAndDataProviderDetails(IGlbCompany currentCompany)
		{
			throw new NotImplementedException();
		}

		void IDataContextDataObject.AddDataTargetAndSetCompanyAndDataProviderDetails(IUniversalJobLink universalLink)
		{
			throw new NotImplementedException();
		}

		void IDataContextDataObject.SetDocumentaryOverride(ZString documentName, ZString purposeCode, ICodeDescriptionPairList purposeList, ZBool isSystemDefined, ZInt dataVersion, ZInt submissionVersion)
		{
			DocumentaryOverride = new UberDocumentaryOverride();
			DocumentaryOverride.DocumentName = documentName;

			DocumentaryOverride.Purpose = new UberCodeDescriptionPair();
			DocumentaryOverride.Purpose.Code = purposeCode;

			if (purposeList != null)
			{
				DocumentaryOverride.Purpose.Description = purposeList.GetDescriptionFromCode(purposeCode);
			}

			DocumentaryOverride.IsSystemDefined = isSystemDefined;
			DocumentaryOverride.DataVersion = dataVersion;
			DocumentaryOverride.SubmissionVersion = submissionVersion;
		}

		void IDataContextDataObject.SetWorkflowInfo(WorkflowInfo info)
		{
			throw new NotImplementedException();
		}

		void IDataContextDataObject.ClearDataSourceCollection()
		{
			throw new NotImplementedException();
		}

		void IDataContextDataObject.ClearDataTargetCollection()
		{
			throw new NotImplementedException();
		}

		void IDataContextDataObject.SetActionPurpose(ICodeDescriptionDataObject actionPurpose)
		{
			throw new NotImplementedException();
		}

		public void AddDataTarget(IDataTargetDataObject dataTargetDataObject)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
