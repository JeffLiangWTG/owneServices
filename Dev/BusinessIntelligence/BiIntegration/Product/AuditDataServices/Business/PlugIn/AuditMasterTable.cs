namespace Enterprise.AuditDataServices.Business
{
	using System.Collections.Generic;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.Schema;
	using CargoWise.Types;
	using Enterprise.ZArchitecture.Business;

	public class AuditMasterTable
	{
		public AuditMasterTable(BusinessObject businessEntity)
		{
			Argument.NotNull(businessEntity, nameof(businessEntity));
			this.businessEntity = businessEntity;
		}

		readonly BusinessObject businessEntity;
		const string LastEditUserFieldSuffix = "_SystemLastEditUser";

		public BusinessObjectFactory Factory
		{
			get { return businessEntity.Factory; }
		}

		public ZGuid PkValue
		{
			get { return businessEntity.PK; }
		}

		public ZInt ClusterKeyValue => businessEntity is IClusterKeyEntity master ? master.ClusterKeyPty.Value : ZInt.Zero;

		public SchemaColumn ClusterKeySchemaColumn => businessEntity is IClusterKeyEntity master ? TableSchema.GetSchemaColumn(master.ClusterKeyPty.Name) : null;

		public ITableSchema TableSchema
		{
			get { return businessEntity.PKSchemaColumn.TableSchema; }
		}

		public bool HasLastEditField
		{
			get
			{
				if (hasLastEditField == null)
				{
					string lastEditUserFieldName = businessEntity.TablePrefix + LastEditUserFieldSuffix;
					hasLastEditField = (TableSchema.GetSchemaColumn(lastEditUserFieldName) != null);
				}

				return hasLastEditField.Value;
			}
		}
		bool? hasLastEditField;

		public IEnumerable<AuditEntity> AuditEntities
		{
			get
			{
				if (auditEntities == null)
				{
					var auxResult = new List<AuditEntity>();
					auxResult.Add(new AuditEntity(businessEntity.PKSchemaColumn, null));

					var auditParent = businessEntity as IAuditParent;

					if (auditParent != null && auditParent.RelatedAuditChildren != null)
					{
						foreach (var relatedAuditChild in auditParent.RelatedAuditChildren)
						{
							auxResult.Add(new AuditEntity(relatedAuditChild.KeyColumn, relatedAuditChild.InfoColumn));
						}
					}

					auditEntities = auxResult.ToArray();
				}
				return auditEntities;
			}
		}
		AuditEntity[] auditEntities;
	}
}
