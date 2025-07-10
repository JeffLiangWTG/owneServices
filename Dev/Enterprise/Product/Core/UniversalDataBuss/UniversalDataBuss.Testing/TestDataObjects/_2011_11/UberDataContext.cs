using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalDataBussKeyValuePair = Enterprise.UniversalDataBuss.Integration.KeyValuePair;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects._2011_11
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), NamespaceSpecific(UniversalXmlInfo.Namespace_2011_11)]
	public class UberDataContext : IDataObject, IDataContextDataObject
	{
		public List<UberDataSource> DataSourceCollection { get; set; }

		public List<UberDataTarget> DataTargetCollection { get; set; }

		public ImportAction? Action { get; set; }

		public ZBool? CodesMappedToTarget { get; set; }

		public UberCompany Company { get; set; }

		[MaxLength(3)]
		public ZString? EnterpriseID { get; set; }

		[MaxLength(3)]
		public ZString? ServerID { get; set; }

		[MaxLength(50)]
		public ZString? DataProvider { get; set; }

		public UberCodeDescriptionPair ActionPurpose { get; set; }
		public UberCodeDescriptionPair EventType { get; set; }
		public UberStaff EventUser { get; set; }
		public ZDateTime? TriggerDate { get; set; }
		[MaxLength(50)]
		public ZString? TriggerDescription { get; set; }
		[MaxLength(2048)]
		public ZString? TriggerReference { get; set; }
		public TriggerType? TriggerType { get; set; }
		public ZInt? TriggerCount { get; set; }
		public ZLong? Timestamp { get; set; }
		public UberDocumentaryOverride DocumentaryOverride { get; set; }

		#region IDataContextDataObject Implementation

		IDocumentaryOverride IDataContextDataObject.DocumentaryOverride
		{
			get { return DocumentaryOverride; }
		}

		ICodeDescriptionDataObject IDataContextDataObject.ActionPurpose
		{
			get { return ActionPurpose; }
		}

		ZString IDataContextDataObject.ActionPurposeCode
		{
			get { return ActionPurpose.GetCodeAsUpperCase(); }
		}

		ZString IDataContextDataObject.DataProviderForCodeMapping
		{
			get { return DataProvider.GetValueOrDefault(); }
			set { DataProvider = value; }
		}

		ZString IDataContextDataObject.CompanyCodeToImportInto
		{
			get { return Company.GetCodeAsUpperCase(); }
		}

		ZString IDataContextDataObject.CountryCodeToImportInto
		{
			get { return Company == null ? ZString.Empty : Company.Country.GetCodeAsUpperCase(); }
		}

		EnterpriseServerAndCompanyID IDataContextDataObject.GetEnterpriseServerAndCompanyIDs()
		{
			return new EnterpriseServerAndCompanyID(EnterpriseID.GetValueOrDefault(), ServerID.GetValueOrDefault(), Company.GetCodeAsUpperCase());
		}

		ZBool IDataContextDataObject.CodesMappedToTarget
		{
			get { return CodesMappedToTarget.GetValueOrDefault(); }
			set { CodesMappedToTarget = value; }
		}

		IEnumerable<UniversalDataBussKeyValuePair> IDataContextDataObject.ContextKeyValuePairs
		{
			get { throw new NotImplementedException(); }
		}

		IEnumerable<IDataSourceDataObject> IDataContextDataObject.DataSourceCollection
		{
			get { return DataSourceCollection; }
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

		ICodeDescriptionDataObject IDataContextDataObject.EventType
		{
			get { return EventType; }
		}

		ZString? IDataContextDataObject.EventReference
		{
			get { throw new NotImplementedException(); }
		}

		ZString IDataContextDataObject.EventBranchCode
		{
			get { throw new NotImplementedException(); }
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

		void IDataContextDataObject.AddDataTarget(IDataTargetDataObject dataTargetDataObject)
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

		#endregion
	}
}
