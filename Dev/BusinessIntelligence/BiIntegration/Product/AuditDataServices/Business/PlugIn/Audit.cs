[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(CargoWise.Bi.Common.BiConstants))]

namespace Enterprise.AuditDataServices.Business
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Globalization;
	using System.Linq;
	using CargoWise.Bi.Common;
	using CargoWise.Common;
	using CargoWise.ComponentModel;
	using CargoWise.Data;
	using CargoWise.EntityFramework;
	using CargoWise.Integration;
	using CargoWise.Types;
	using Enterprise.ChangeDataCapture.Common;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Security;

	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class Audit : NonPersistentBusinessObject
	{
		public Audit(BusinessObject businessObject, string auditServer) : base(businessObject?.Factory)
		{
			Argument.NotNull(businessObject, nameof(businessObject));
			Argument.NotNullOrEmpty(auditServer, nameof(auditServer));

			this.businessObject = businessObject;
			this.auditServer = auditServer;
			using (SuspendSettingHasChanges())
			{
				this.FilterSourceEntity = SourceEntities.FirstOrDefault()?.Code;
			}
		}

		readonly BusinessObject businessObject;
		readonly string auditServer;

		AuditMasterTable MasterTable => masterTable ??= new AuditMasterTable(businessObject);
		AuditMasterTable masterTable;

		protected BusinessObjectFactory AuditServerFactory => auditServerFactory ??= IsAuditServerSameAsFactoryServerName ? businessObject.Factory : new BusinessObjectFactory(Db.NewExtraConnectionWithMainDbCredentials(auditServer, Db.AuditDatabaseName));
		BusinessObjectFactory auditServerFactory;

		protected virtual bool IsAuditServerSameAsFactoryServerName => auditServer.Equals(((IDbConnected)businessObject.Factory).Connection.ServerName, StringComparison.OrdinalIgnoreCase);

		public bool IsMasterTableEnabledForCdc()
		{
			var cdcTable = new CdcTable(MasterTable.TableSchema.SqlSchemaName, MasterTable.TableSchema.TableName);
			return cdcTable.IsCdcEnabled(((IDbConnected)Factory).Connection);
		}

		#region IReadOnlySecurity

		const string FilterTimeLocalFromProperty = "FilterTimeLocalFrom";
		const string FilterTimeLocalToProperty = "FilterTimeLocalTo";

		public bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			var propertyName = property.Name;
			bool shouldBeReadOnly;
			if (propertyName == FilterTimeLocalFromProperty || propertyName == FilterTimeLocalToProperty)
			{
				shouldBeReadOnly = false;
			}
			else
			{
				shouldBeReadOnly =
					!GetSecurityCheckpointForReadOnlySecurity().IsAllowed
					|| CargoWise.ComponentModel.MetaData.GetReadOnlyExcludingMethodProvider(this, property);
			}
			return shouldBeReadOnly;
		}

		SecurityCheckpoint GetSecurityCheckpointForReadOnlySecurity()
		{
			return Env.Security.AuditServices;
		}

		#endregion

		#region Filters

		#region FilterTimeLocalFrom and FilterTimeLocalTo

		public ZDateTime FilterTimeLocalFrom
		{
			get
			{
				return filterTimeLocalFrom == ZDateTime.Empty ? ZDateTime.Now.AddMonths(-1).AddDays(1) : filterTimeLocalFrom;
			}
			set
			{
				if (filterTimeLocalFrom != value)
				{
					SetNonPersistentPropertyValue(FilterTimeLocalFromInfo, ref filterTimeLocalFrom, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateFilterTimeLocalFrom();
						Validation.ValidateFilterTimeLocalTo();
					}
				}
			}
		}
		ZDateTime filterTimeLocalFrom;

		public ZPropertyInfo FilterTimeLocalFromInfo
		{
			get { return GetZPropertyInfo(nameof(FilterTimeLocalFrom)); }
		}

		public ZDateTime FilterTimeLocalTo
		{
			get
			{
				return filterTimeLocalTo == ZDateTime.Empty ? ZDateTime.Now : filterTimeLocalTo;
			}
			set
			{
				if (filterTimeLocalTo != value)
				{
					SetNonPersistentPropertyValue(FilterTimeLocalToInfo, ref filterTimeLocalTo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateFilterTimeLocalFrom();
						Validation.ValidateFilterTimeLocalTo();
					}
				}
			}
		}
		ZDateTime filterTimeLocalTo;

		public ZPropertyInfo FilterTimeLocalToInfo
		{
			get { return GetZPropertyInfo(nameof(FilterTimeLocalTo)); }
		}

		#endregion

		#region FilterUserCode

		[List("UserCollection")]
		[ReadOnlyMember(nameof(ReadOnlyUserFilter))]
		public ZString FilterUserCode
		{ get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test Cases")]
		bool ReadOnlyUserFilter
		{
			get { return !MasterTable.HasLastEditField; }
		}

		public GlbStaffCollection UserCollection
		{
			get { return new GlbStaffCollection(Factory); }
		}

		#endregion

		#region FilterSourceEntity

		[List("SourceEntities")]
		[ReadOnlyMember(nameof(ReadOnlySourceEntityFilter))]
		public ZString FilterSourceEntity
		{
			get
			{
				return filterSourceEntity;
			}

			set
			{
				if (filterSourceEntity != value)
				{
					SetNonPersistentPropertyValue(FilterSourceEntityInfo, ref filterSourceEntity, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateFilterSourceEntity();
					}
				}
			}
		}
		ZString filterSourceEntity;

		public ZPropertyInfo FilterSourceEntityInfo
		{
			get { return GetZPropertyInfo(nameof(FilterSourceEntity)); }
		}

		public List<SourceEntityWrapper> SourceEntities
		{
			get
			{
				if (sourceEntities == null)
				{
					var auxList = new List<SourceEntityWrapper>();

					if (MasterTable.AuditEntities.Skip(1).Any())
					{
						auxList.Add(new SourceEntityWrapper(null));
					}

					foreach (var auditEntity in MasterTable.AuditEntities)
					{
						auxList.Add(new SourceEntityWrapper(auditEntity));
					}

					sourceEntities = auxList;
				}

				return sourceEntities;
			}
		}
		List<SourceEntityWrapper> sourceEntities;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test Cases")]
		bool ReadOnlySourceEntityFilter
		{
			get { return !MasterTable.AuditEntities.Skip(1).Any(); }
		}

		public class SourceEntityWrapper : ICodeDescription
		{
			public SourceEntityWrapper(AuditEntity sourceEntity)
			{
				this.sourceEntity = sourceEntity;
			}

			public AuditEntity SourceEntity
			{
				get { return sourceEntity; }
			}
			readonly AuditEntity sourceEntity;

			public string Code
			{
				get { return code ?? (code = (sourceEntity == null) ? AllRelatedEntitiesCode : sourceEntity.ToString()); }
			}
			string code;

			object ICodeDescription.PK { get { return null; } }
			string ICodeDescription.Description { get { return null; } }

			public const string AllRelatedEntitiesCode = "ALL";
		}

		#endregion

		AuditValidation Validation
		{
			get { return validation ?? (validation = new AuditValidation(this)); }
		}
		AuditValidation validation;

		public void ValidateFilters()
		{
			Validation.ValidateAll();
		}

		#endregion

		#region Min and Max audit data times

		public void RefreshMinAndMaxAuditDataTime()
		{
			SetBoundaryAuditDataUtcFields();
			MinAuditDataLocalTime = minAuditLocalTime.ToBestReadableDateTimeString();
			MinAuditDataLocalTimeInfo.RefreshBinding();
			LastProcessedLocalTime = maxAuditLocalTime.ToBestReadableDateTimeString();
			LastProcessedLocalTimeInfo.RefreshBinding();
		}

		void SetBoundaryAuditDataUtcFields()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT min(TranEndTimeUtc), max(TranEndTimeUtc) FROM [{0}].[{1}].LsnTimeMapping WITH (READPAST, READCOMMITTEDLOCK)", // This is an SQL query,
				/*0*/Db.AuditDatabaseName,
				/*1*/BiConstants.BiAdminSchemaName
			);

			ZDateTime minAuditUtc = ZDateTime.Empty;
			ZDateTime maxAuditUtc = ZDateTime.Empty;

			using (var cmd = ((IDbConnected)AuditServerFactory).Connection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					var rawValue = reader[0];

					if (rawValue != DBNull.Value)
					{
						minAuditUtc = reader.GetDateTime(0);
						maxAuditUtc = reader.GetDateTime(1);
					}
				}
			}

			minAuditLocalTime = minAuditUtc.ToLocalBranchTime(Factory);
			maxAuditLocalTime = maxAuditUtc.ToLocalBranchTime(Factory);
		}

		ZDateTime minAuditLocalTime;
		ZDateTime maxAuditLocalTime;

		#region Min Audit Data Time

		public ZString MinAuditDataLocalTime
		{ get; private set; }

		public ZPropertyInfo MinAuditDataLocalTimeInfo
		{
			get { return GetZPropertyInfo(nameof(MinAuditDataLocalTime)); }
		}

		#endregion

		#region Last Processed Time

		public ZString LastProcessedLocalTime
		{ get; private set; }

		public ZPropertyInfo LastProcessedLocalTimeInfo
		{
			get { return GetZPropertyInfo(nameof(LastProcessedLocalTime)); }
		}

		#endregion

		#endregion

		#region Audit Header - Change events

		public AuditEventCollection AuditEventData
		{
			get
			{
				return auditEventData ??
				  (auditEventData = new AuditEventCollection(AuditServerFactory, MasterTable));
			}
		}
		AuditEventCollection auditEventData;

		public void ReloadAuditEvents()
		{
			RefreshMinAndMaxAuditDataTime();
			var currentSort = AuditEventData.SortInformation;

			if (maxAuditLocalTime.IsValid)
			{
				ZDateTime cappedFilterTimeLocalTo = (FilterTimeLocalTo > maxAuditLocalTime) ? maxAuditLocalTime : FilterTimeLocalTo;

				AuditEventData.Reload(
					SourceEntities.FirstOrDefault(st => st.Code == FilterSourceEntity)?.SourceEntity,
					FilterUserCode,
					FilterTimeLocalFrom.ToUniversalBranchTime(Factory),
					cappedFilterTimeLocalTo.ToUniversalBranchTime(Factory)
				);
			}

			if (currentSort != null)
			{
				AuditEventData.Sort(currentSort);
			}
		}

		#endregion

		#region ChangeOperation Enum

		public enum ChangeOperation
		{
			None = 0,
			Delete = 1,
			Insert = 2,
			BeforeUpdate = 3,
			AfterUpdate = 4,
			Transaction = -1,
		}

		#endregion
	}
}
