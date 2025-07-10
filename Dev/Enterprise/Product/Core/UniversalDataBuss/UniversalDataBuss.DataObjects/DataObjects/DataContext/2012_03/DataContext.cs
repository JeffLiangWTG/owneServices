using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalDataBussKeyValuePair = Enterprise.UniversalDataBuss.Integration.KeyValuePair;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), NamespaceSpecific(UniversalXmlInfo.Namespace_2012_11)]
	public class DataContext : IDataContextDataObject
	{
		LazyDataSource dataSource;
		Dictionary<(string typeName, ZString? reference), DataTarget> dataTargetCollection;

		[ReferenceProperty]
		public DataSource DataSource { get => dataSource; set => dataSource = value != null ? new LazyDataSource(value.DataProvider, () => value.Type, () => value.Key) : null; }

		public Workflow Workflow { get; set; }

		public ZLong? Timestamp { get; set; }

		public List<DataTarget> DataTargetCollection
		{
			get
			{
				return dataTargetCollection?.Values.ToList();
			}
			set
			{
				if (value == null)
				{
					dataTargetCollection = null;
				}
				else
				{
					dataTargetCollection = new Dictionary<(string typeName, ZString? reference), DataTarget>();
					foreach (var dataTarget in value)
					{
						var uniqueKey = (dataTarget.Type.GetValueOrDefault(), dataTarget.Key.GetValueOrDefault());
						if (!dataTargetCollection.ContainsKey(uniqueKey))
						{
							dataTargetCollection.Add(uniqueKey, dataTarget);
						}
					}
				}
			}
		}

		public ImportAction? Action { get; set; }

		public DocumentaryOverride DocumentaryOverride { get; set; }

		#region Safe Object Loaders

		Workflow WorkflowSafe
		{
			get { return Workflow ?? (Workflow = new Workflow()); }
		}

		IDataSourceDataObject DataSourceSafe
		{
			get
			{
				return dataSource ?? (DataSource = new DataSource());
			}
		}

		#endregion

		#region IDataContextDataObject Members

		IDocumentaryOverride IDataContextDataObject.DocumentaryOverride
		{
			get { return DocumentaryOverride; }
		}

		#region Getters for Common Consumption

		ZString IDataContextDataObject.ActionPurposeCode
		{
			get { return WorkflowSafe.ActionPurpose.GetCodeAsUpperCase(); }
		}

		ZString IDataContextDataObject.DataProviderForCodeMapping
		{
			get { return ((DataSource)DataSourceSafe).DataProvider.GetCodeAsUpperCase(); }
			set { ((DataSource)DataSourceSafe).DataProvider = new DataProvider { Code = value }; }
		}

		ZString IDataContextDataObject.CompanyCodeToImportInto
		{
			get { return WorkflowSafe.Company?.GetCodeAsUpperCase() ?? ZString.Empty; }
		}

		ZString IDataContextDataObject.EventBranchCode
		{
			get {  return WorkflowSafe.EventBranch?.GetCodeAsUpperCase() ?? ZString.Empty; }
		}

		ZString IDataContextDataObject.CountryCodeToImportInto
		{
			get
			{
				return WorkflowSafe.Company?.Country?.GetCodeAsUpperCase() ?? ZString.Empty;
			}
		}

		EnterpriseServerAndCompanyID IDataContextDataObject.GetEnterpriseServerAndCompanyIDs()
		{
			var dataProvider = DataSource == null ? null : DataSource.DataProvider;
			if (dataProvider != null && dataProvider.Type == DataProviderType.EnterpriseID)
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
			get { return WorkflowSafe.CodesMappedToTarget.GetValueOrDefault(); }
			set { WorkflowSafe.CodesMappedToTarget = value; }
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
			get { return WorkflowSafe.RecipientRoleCollection; }
			set
			{
				WorkflowSafe.SetRecipientRoleCollection(() =>
				{
					var result = new List<RecipientRole>();
					foreach (var item in value)
					{
						if (item is RecipientRole role)
						{
							result.Add(role);
						}
						else
						{
							throw new NotSupportedException();
						}
					}
					return result;
				});
			}
		}

		ICodeDescriptionDataObject IDataContextDataObject.ActionPurpose
		{
			get { return WorkflowSafe.ActionPurpose; }
		}

		ICodeDescriptionDataObject IDataContextDataObject.EventType
		{
			get { return WorkflowSafe.EventType; }
		}

		ZString? IDataContextDataObject.EventReference
		{
			get { return WorkflowSafe.EventReference; }
		}
		IEnumerable<UniversalDataBussKeyValuePair> IDataContextDataObject.ContextKeyValuePairs
		{
			get
			{
				yield return new UniversalDataBussKeyValuePair(Res.GetString("12b8b4ca-bdaf-40b1-a3a2-0f91b1f27114", "Data Source Action Purpose"), WorkflowSafe.ActionPurpose.ToStringContents());
				yield return new UniversalDataBussKeyValuePair(Res.GetString("5c9c032f-8b53-48c4-8255-ca466f6dd122", "Data Source Company"), WorkflowSafe.Company.ToStringContents());
				yield return new UniversalDataBussKeyValuePair(Res.GetString("ed4b81f7-2296-45b4-82f5-de6d9d9bd441", "Data Source Data Provider"), ((DataSource)DataSourceSafe).DataProvider.GetCodeAndType());
				yield return new UniversalDataBussKeyValuePair(Res.GetString("3d02a3ca-32ae-40ec-b2be-2adf8b2c923f", "Data Source Trigger Count"), WorkflowSafe.TriggerCount.ToString());
				yield return new UniversalDataBussKeyValuePair(Res.GetString("96cbd582-a8e5-4be2-b826-95adae003a00", "Data Source Trigger Date"), WorkflowSafe.TriggerDate.GetValueOrDefault().ToString());
				yield return new UniversalDataBussKeyValuePair(Res.GetString("a267b833-1a48-47ae-a690-5d8c15bec14e", "Data Source Trigger Description"), WorkflowSafe.TriggerDescription.GetValueOrDefault());
				yield return new UniversalDataBussKeyValuePair(Res.GetString("562aae70-b163-4511-bf85-44c80b07fb6d", "Data Source Trigger Event"), WorkflowSafe.EventType.ToStringContents());
				yield return new UniversalDataBussKeyValuePair(Res.GetString("91ba4ea9-dbae-45e5-89db-068ed1cb32f2", "Data Source Trigger Event User"), WorkflowSafe.EventUser.ToStringContents());
				yield return new UniversalDataBussKeyValuePair(Res.GetString("2595c5c6-b347-4433-b930-daa25a36bfb0", "Data Source Trigger Reference"), WorkflowSafe.TriggerReference.GetValueOrDefault());
				yield return new UniversalDataBussKeyValuePair(Res.GetString("7f27defc-a9ab-4eab-90ae-255892702731", "Data Source Trigger Type"), WorkflowSafe.TriggerType.ToString());
				yield return new UniversalDataBussKeyValuePair(Res.GetString("424cfc8c-dd1c-4357-a61c-c90a604a9c29", "Data Source Event Reference"), WorkflowSafe.EventReference.ToString());
			}
		}

		public ZString EventDepartmentCode
		{
			get { return Workflow?.EventDepartment?.GetCodeAsUpperCase() ?? ZString.Empty; }
		}

		#endregion

		#region Setters for filling in the details

		void IDataContextDataObject.AddDataSource(IEntityID entityID)
		{
			dataSource = new LazyDataSource(((DataSource)DataSourceSafe).DataProvider, () => entityID.DataContextType.ToString(), () => entityID.DataContextKey);
		}

		void IDataContextDataObject.AddDataSource(IDataSourceDataObject dataSourceToAdd)
		{
			dataSource = new LazyDataSource(((DataSource)DataSourceSafe).DataProvider, () => dataSourceToAdd.Type.GetValueOrDefault(), () => dataSourceToAdd.Key.GetValueOrDefault());
		}

		void IDataContextDataObject.AddDataSource(DataContextType type, ZString reference)
		{
			dataSource = new LazyDataSource(((DataSource)DataSourceSafe).DataProvider, () => type.ToString(), () => reference);
		}

		void IDataContextDataObject.AddDataTarget(DataContextType type, ZString? reference, IOrganizationAddress owner)
		{
			var uniqueKey = (type.ToString(), reference);
			if (dataTargetCollection == null)
			{
				dataTargetCollection = new Dictionary<(string typeName, ZString? reference), DataTarget>();
			}

			if (!dataTargetCollection.ContainsKey(uniqueKey))
			{
				dataTargetCollection.Add(uniqueKey, new DataTarget() { Type = type.ToString(), Key = reference, Owner = (OrganizationAddress)owner });
			}
		}

		void IDataContextDataObject.AddDataTarget(IDataTargetDataObject dataTargetDataObject)
		{
			var uniqueKey = (dataTargetDataObject.Type?.ToString(), dataTargetDataObject.Key?.ToString());
			if (dataTargetCollection == null)
			{
				dataTargetCollection = new Dictionary<(string typeName, ZString? reference), DataTarget>();
			}

			if (!dataTargetCollection.ContainsKey(uniqueKey) && dataTargetDataObject is DataTarget dataTarget)
			{
				dataTargetCollection.Add(uniqueKey, dataTarget);
			}
		}

		void IDataContextDataObject.SetCompanyAndDataProviderDetails(IGlbCompany currentCompany)
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var enterpriseCode = registrationKey.EnterpriseCode;
			var serverCode = registrationKey.ServerCode;
			SetDataProviderDetails(enterpriseCode, serverCode, Company.New(currentCompany));
		}

		void IDataContextDataObject.AddDataTargetAndSetCompanyAndDataProviderDetails(IUniversalJobLink universalLink)
		{
			((IDataContextDataObject)this).AddDataTarget(universalLink.Context, universalLink.Key);
			SetDataProviderDetails(universalLink.EnterpriseCode, universalLink.ServerCode, new Company() { Code = universalLink.CompanyCode });
		}

		void SetDataProviderDetails(string enterpriseCode, string serverCode, Company company)
		{
			var dataSourceToSet = (DataSource)DataSourceSafe;
			dataSourceToSet.DataProvider = new DataProvider() { Code = enterpriseCode + serverCode + company.Code, Type = DataProviderType.EnterpriseID };
			dataSource = new LazyDataSource(dataSourceToSet.DataProvider, () => dataSourceToSet.Type, () => dataSourceToSet.Key);
			WorkflowSafe.Company = company;
		}

		void IDataContextDataObject.SetDocumentaryOverride(ZString documentName, ZString purposeCode, ICodeDescriptionPairList purposeList, ZBool isSystemDefined, ZInt dataVersion, ZInt submissionVersion)
		{
			DocumentaryOverride = new DocumentaryOverride();
			DocumentaryOverride.DocumentName = documentName;

			DocumentaryOverride.Purpose = new CodeDescriptionPair();
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
			var workFlow = WorkflowSafe;
			workFlow.EventType = (CodeDescriptionPair)info.EventType;
			if (!info.EventReference.IsEmpty)
			{
				workFlow.EventReference = info.EventReference;
			}
			workFlow.EventUser = (Staff)info.EventUser;
			workFlow.EventBranch = (Branch)info.EventBranch;
			workFlow.EventDepartment = (Department)info.EventDepartment;
			workFlow.ActionPurpose = (CodeDescriptionPair)info.ActionPurpose;
			workFlow.TriggerDescription = info.TriggerDescription;
			workFlow.TriggerCount = info.TriggerCount;
			if (!info.TriggerDate.IsEmpty)
			{
				workFlow.TriggerDate = info.TriggerDate;
			}
			else if (workFlow.TriggerType == TriggerType.Trigger)
			{
				ErrorReporter.ReportOnce("b4095e5f-f1ad-462b-b47d-f4c3a0261096", "There should always be a trigger date when the Trigger Type is Trigger.");
			}

			if (!info.TriggerReference.IsEmpty)
			{
				workFlow.TriggerReference = info.TriggerReference;
			}
			workFlow.TriggerType = info.TriggerType;
			var recipientRoles = info.RecipientRoles;
			if (recipientRoles != null && recipientRoles.Any())
			{
				workFlow.RecipientRoleCollection = new List<RecipientRole>();

				foreach (var recipientRole in recipientRoles)
				{
					workFlow.RecipientRoleCollection.Add(RecipientRole.New(recipientRole));
				}
			}
		}

		void IDataContextDataObject.SetActionPurpose(ICodeDescriptionDataObject actionPurpose)
		{
			WorkflowSafe.ActionPurpose = actionPurpose as CodeDescriptionPair ?? new CodeDescriptionPair { Code = actionPurpose.Code, Description = actionPurpose.Description };
		}

		void IDataContextDataObject.ClearDataSourceCollection()
		{
			dataSource = null;
		}

		void IDataContextDataObject.ClearDataTargetCollection()
		{
			dataTargetCollection = null;
		}

		#endregion

		#endregion

		class LazyDataSource : IDataSourceDataObject
		{
			readonly Lazy<DataSource> dataSource;

			public LazyDataSource(DataProvider dataProvider, Func<ZString?> getType, Func<ZString?> getKey)
			{
				this.dataSource = new Lazy<DataSource>(() => new DataSource() { DataProvider = dataProvider, Type = getType.Invoke(), Key = getKey.Invoke() }, System.Threading.LazyThreadSafetyMode.None);
			}

			public static implicit operator DataSource(LazyDataSource lazy)
			{
				return lazy?.dataSource?.Value;
			}

			public DataProvider DataProvider { get => dataSource.Value.DataProvider; set => dataSource.Value.DataProvider = value; }
			ZString? IDataSourceDataObject.Type { get => dataSource.Value.Type; set => dataSource.Value.Type = value; }
			ZString? IDataSourceDataObject.Key { get => dataSource.Value.Key; set => dataSource.Value.Key = value; }
		}
	}
}
