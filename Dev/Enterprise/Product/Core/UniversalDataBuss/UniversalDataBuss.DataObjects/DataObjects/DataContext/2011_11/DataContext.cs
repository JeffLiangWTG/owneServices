using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalDataBussKeyValuePair = Enterprise.UniversalDataBuss.Integration.KeyValuePair;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), NamespaceSpecific(UniversalXmlInfo.Namespace_2011_11)]
	public class DataContext : IDataContextDataObject
	{
		List<IDataSourceDataObject> dataSourceCollection;
		Dictionary<(string typeName, ZString? reference), DataTarget> dataTargetCollection;

		[ReferenceProperty]
		public List<DataSource> DataSourceCollection
		{
			get
			{
				return dataSourceCollection?.Distinct(new DataSourceEqualityComparer()).Select(x =>
				{
					DataSource result;
					if (x is LazyDataSource lazy)
					{
						result = lazy;
					}
					else
					{
						result = (DataSource)x;
					}

					return result;
				}).ToList();
			}
			set
			{
				if (value == null)
				{
					dataSourceCollection = null;
				}
				else
				{
					dataSourceCollection = new List<IDataSourceDataObject>();
					dataSourceCollection.AddRange(value);
				}
			}
		}

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

		public List<RecipientRole> RecipientRoleCollection { get; set; }

		public ZBool? CodesMappedToTarget { get; set; }

		public Company Company { get; set; }

		[MaxLength(3)]
		public ZString? EnterpriseID { get; set; }

		[MaxLength(3)]
		public ZString? ServerID { get; set; }

		[MaxLength(50)]
		public ZString? DataProvider { get; set; }

		public CodeDescriptionPair ActionPurpose { get; set; }
		public CodeDescriptionPair EventType { get; set; }
		[MaxLength(1024)]
		public ZString? EventReference { get; set; }
		public Staff EventUser { get; set; }
		public Branch EventBranch { get; set; }
		public Department EventDepartment { get; set; }
		public ZDateTimeOffset? TriggerDate { get; set; }
		[MaxLength(50)]
		public ZString? TriggerDescription { get; set; }
		[MaxLength(2048)]
		public ZString? TriggerReference { get; set; }
		public TriggerType? TriggerType { get; set; }
		public ZInt? TriggerCount { get; set; }
		public DocumentaryOverride DocumentaryOverride { get; set; }
		public ZLong? Timestamp { get; set; }

		#region IDataContextDataObject Members

		IDocumentaryOverride IDataContextDataObject.DocumentaryOverride
		{
			get { return DocumentaryOverride; }
		}

		#region Getters for Common Consumption

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
			get { return Company?.GetCodeAsUpperCase() ?? ZString.Empty; }
		}

		ZString IDataContextDataObject.CountryCodeToImportInto
		{
			get { return Company?.Country?.GetCodeAsUpperCase() ?? ZString.Empty; }
		}

		ZString IDataContextDataObject.EventBranchCode
		{
			get { return EventBranch?.GetCodeAsUpperCase() ?? ZString.Empty; }
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

		IEnumerable<IDataSourceDataObject> IDataContextDataObject.DataSourceCollection
		{
			get { return dataSourceCollection?.Distinct(new DataSourceEqualityComparer()); }
		}

		IEnumerable<IDataTargetDataObject> IDataContextDataObject.DataTargetCollection
		{
			get { return dataTargetCollection?.Values; }
		}

		IEnumerable<IRecipientRoleDataObject> IDataContextDataObject.RecipientRoleCollection
		{
			get { return RecipientRoleCollection; }
			set
			{
				RecipientRoleCollection = new List<RecipientRole>();
				foreach (var item in value)
				{
					if (item is RecipientRole role)
					{
						RecipientRoleCollection.Add(role);
					}
					else
					{
						throw new NotSupportedException();
					}
				}
			}
		}

		ICodeDescriptionDataObject IDataContextDataObject.ActionPurpose
		{
			get { return ActionPurpose; }
		}

		ICodeDescriptionDataObject IDataContextDataObject.EventType
		{
			get { return EventType; }
		}

		ZString? IDataContextDataObject.EventReference
		{
			get { return EventReference; }
		}

		IEnumerable<UniversalDataBussKeyValuePair> IDataContextDataObject.ContextKeyValuePairs
		{
			get
			{
				return new[]
				{
					new UniversalDataBussKeyValuePair(Res.GetString("12b8b4ca-bdaf-40b1-a3a2-0f91b1f27114", "Data Source Action Purpose"), ActionPurpose.ToStringContents()),
					new UniversalDataBussKeyValuePair(Res.GetString("5c9c032f-8b53-48c4-8255-ca466f6dd122", "Data Source Company"), Company.ToStringContents()),
					new UniversalDataBussKeyValuePair(Res.GetString("ed4b81f7-2296-45b4-82f5-de6d9d9bd440", "Data Source Enterprise ID"), EnterpriseID.GetValueOrDefault()),
					new UniversalDataBussKeyValuePair(Res.GetString("55e65ea7-5767-4832-81cf-e4e39b617b21", "Data Source Server ID"), ServerID.GetValueOrDefault()),
					new UniversalDataBussKeyValuePair(Res.GetString("3d02a3ca-32ae-40ec-b2be-2adf8b2c923f", "Data Source Trigger Count"), TriggerCount.ToString()),
					new UniversalDataBussKeyValuePair(Res.GetString("96cbd582-a8e5-4be2-b826-95adae003a00", "Data Source Trigger Date"), TriggerDate.GetValueOrDefault().ToString()),
					new UniversalDataBussKeyValuePair(Res.GetString("a267b833-1a48-47ae-a690-5d8c15bec14e", "Data Source Trigger Description"), TriggerDescription.GetValueOrDefault()),
					new UniversalDataBussKeyValuePair(Res.GetString("562aae70-b163-4511-bf85-44c80b07fb6d", "Data Source Trigger Event"), EventType.ToStringContents()),
					new UniversalDataBussKeyValuePair(Res.GetString("91ba4ea9-dbae-45e5-89db-068ed1cb32f2", "Data Source Trigger Event User"), EventUser.ToStringContents()),
					new UniversalDataBussKeyValuePair(Res.GetString("2595c5c6-b347-4433-b930-daa25a36bfb0", "Data Source Trigger Reference"), TriggerReference.GetValueOrDefault()),
					new UniversalDataBussKeyValuePair(Res.GetString("7f27defc-a9ab-4eab-90ae-255892702731", "Data Source Trigger Type"), TriggerType.ToString()),
					new UniversalDataBussKeyValuePair(Res.GetString("424cfc8c-dd1c-4357-a61c-c90a604a9c29", "Data Source Event Reference"), EventReference.ToString()),
				};
			}
		}

		ZString IDataContextDataObject.EventDepartmentCode
		{
			get { return EventDepartment?.GetCodeAsUpperCase() ?? ZString.Empty; }
		}

		#endregion

		#region Setters for filling in the details

		void IDataContextDataObject.AddDataSource(IEntityID entityID)
		{
			if (dataSourceCollection == null)
			{
				dataSourceCollection = new List<IDataSourceDataObject>();
			}

			dataSourceCollection.Add(new LazyDataSource(() => entityID.DataContextType.ToString(), () => entityID.DataContextKey));
		}

		void IDataContextDataObject.AddDataSource(IDataSourceDataObject dataSource)
		{
			if (dataSourceCollection == null)
			{
				dataSourceCollection = new List<IDataSourceDataObject>();
			}

			dataSourceCollection.Add(dataSource);
		}

		void IDataContextDataObject.AddDataSource(DataContextType type, ZString reference)
		{
			if (dataSourceCollection == null)
			{
				dataSourceCollection = new List<IDataSourceDataObject>();
			}

			dataSourceCollection.Add(new DataSource() { Type = type.ToString(), Key = reference });
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
			SetDataProviderDetails(registrationKey.EnterpriseCode, registrationKey.ServerCode, Company.New(currentCompany));
		}

		void IDataContextDataObject.AddDataTargetAndSetCompanyAndDataProviderDetails(IUniversalJobLink universalLink)
		{
			((IDataContextDataObject)this).AddDataTarget(universalLink.Context, universalLink.Key);
			SetDataProviderDetails(universalLink.EnterpriseCode, universalLink.ServerCode, new Company() { Code = universalLink.CompanyCode });
		}

		void SetDataProviderDetails(string enterpriseCode, string serverCode, Company company)
		{
			EnterpriseID = enterpriseCode;
			ServerID = serverCode;
			Company = company;
			DataProvider = enterpriseCode + serverCode + company.Code;
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
			EventType = (CodeDescriptionPair)info.EventType;
			if (!info.EventReference.IsEmpty)
			{
				EventReference = info.EventReference;
			}
			EventUser = (Staff)info.EventUser;
			EventBranch = (Branch)info.EventBranch;
			EventDepartment = (Department)info.EventDepartment;
			ActionPurpose = (CodeDescriptionPair)info.ActionPurpose;
			TriggerDescription = info.TriggerDescription;
			TriggerCount = info.TriggerCount;
			if (!info.TriggerDate.IsEmpty)
			{
				TriggerDate = info.TriggerDate;
			}
			if (!info.TriggerReference.IsEmpty)
			{
				TriggerReference = info.TriggerReference;
			}
			TriggerType = info.TriggerType;
			var recipientRoles = info.RecipientRoles;
			if (recipientRoles != null && recipientRoles.Any())
			{
				var recipientRoleCollection = new List<RecipientRole>();
				foreach (var recipientRole in recipientRoles)
				{
					recipientRoleCollection.Add(RecipientRole.New(recipientRole));
				}

				RecipientRoleCollection = recipientRoleCollection;
			}
		}

		void IDataContextDataObject.ClearDataSourceCollection()
		{
			DataSourceCollection = new List<DataSource>();
		}

		void IDataContextDataObject.ClearDataTargetCollection()
		{
			DataTargetCollection = new List<DataTarget>();
		}

		void IDataContextDataObject.SetActionPurpose(ICodeDescriptionDataObject actionPurpose)
		{
			ActionPurpose = actionPurpose as CodeDescriptionPair ?? new CodeDescriptionPair { Code = actionPurpose.Code, Description = actionPurpose.Description };
		}

		class LazyDataSource : IDataSourceDataObject
		{
			readonly Lazy<DataSource> dataSource;

			public LazyDataSource(Func<ZString?> getType, Func<ZString?> getKey)
			{
				this.dataSource = new Lazy<DataSource>(() => new DataSource() { Type = getType.Invoke(), Key = getKey.Invoke() }, System.Threading.LazyThreadSafetyMode.None);
			}

			public static implicit operator DataSource(LazyDataSource lazy)
			{
				return lazy.dataSource.Value;
			}

			ZString? IDataSourceDataObject.Type { get => dataSource.Value.Type; set => dataSource.Value.Type = value; }
			ZString? IDataSourceDataObject.Key { get => dataSource.Value.Key; set => dataSource.Value.Key = value; }
		}

		class DataSourceEqualityComparer : IEqualityComparer<IDataSourceDataObject>
		{
			public bool Equals(IDataSourceDataObject x, IDataSourceDataObject y)
			{
				return x.Type.GetValueOrDefault() == y.Type.GetValueOrDefault() && x.Key.GetValueOrDefault() == y.Key.GetValueOrDefault();
			}

			public int GetHashCode(IDataSourceDataObject obj)
			{
				return obj.Type.GetValueOrDefault().GetHashCode() ^ obj.Key.GetValueOrDefault().GetHashCode();
			}
		}

		#endregion

		#endregion
	}
}
