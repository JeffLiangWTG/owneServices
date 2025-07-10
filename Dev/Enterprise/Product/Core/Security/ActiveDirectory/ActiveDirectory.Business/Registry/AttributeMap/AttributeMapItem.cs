using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.ActiveDirectory;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security.ActiveDirectory
{
	[DebuggerDisplay("Enterprise Column: {EnterpriseColumnName}, AD Attribute: {ActiveDirectoryAttributeName}, IsSynced: {IsSynced}")]
	public class AttributeMapItem : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AttributeMapItem()
		{
		}

		public AttributeMapItem(SchemaColumn schemaColumn, string attributeName, bool isSynced)
		{
			Argument.NotNull(schemaColumn, nameof(schemaColumn));
			Argument.NotNullOrEmpty(attributeName, nameof(attributeName));

			using (GetValidationSuspender())
			{
				enterpriseColumnName = schemaColumn.Name;
				schema = schemaColumn;
				ActiveDirectoryAttributeName = attributeName;
				IsSynced = isSynced;
			}
		}

		public AttributeMapItem(string columnName, string attributeName, bool isSynced)
		{
			Argument.NotNullOrEmpty(columnName, nameof(columnName));
			Argument.NotNullOrEmpty(attributeName, nameof(attributeName));

			using (GetValidationSuspender())
			{
				EnterpriseColumnName = columnName;

				ActiveDirectoryAttributeName = attributeName;
				IsSynced = isSynced;
			}
		}

		#region EnterpriseTableColumn

		[ResourceStringData("AttributeMapItem|EnterpriseColumnName", Caption = "Field Name", FullDescription = "The database field name")]
		[MaxLength(Schema.EnterpriseColumnNameMaxLength)]
		public ZString EnterpriseColumnName
		{
			get { return enterpriseColumnName; }
			set
			{
				CheckMaximumLength(EnterpriseColumnNameInfo, value);
				ClearAllNotifications();
				try
				{
					SetNonPersistentPropertyValue(EnterpriseColumnNameInfo, ref enterpriseColumnName, value);
					var table = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchemaFromColumnNamePrefix(enterpriseColumnName.Substring(0, 2)) ?? throw new InvalidSchemaColumnException(value, string.Empty);
					schema = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(enterpriseColumnName, table.TableName);
				}
				catch (InvalidSchemaColumnException)
				{
					schema = null;
					if (value != GlbStaff.Schema.CurrentDRMManager)
					{
						AddRowError(Res.GetString("d24e7ecd-43c7-4924-bd04-929dd9598bb1", "Cannot find field {0} in the {1} database.", enterpriseColumnName, BrandingFactory.Instance.ProductName));
					}
				}
			}
		}
		ZString enterpriseColumnName;
		SchemaColumn schema;

		protected bool EnterpriseColumnName_ReadOnly => true;

		public ZPropertyInfo EnterpriseColumnNameInfo => GetZPropertyInfo(Schema.EnterpriseColumnName);

		public string EnterpriseTableName => schema?.TableName ?? string.Empty;

		#endregion

		#region ActiveDirectoryAttributeName

		[List("AvailableADAttributes")]
		[ResourceStringData("AttributeMapItem|ActiveDirectoryAttributeName", Caption = "Active Directory Attribute", ShortCaption = "AD Attribute", FullDescription = "The attribute name in Active Directory")]
		[MaxLength(Schema.ActiveDirectoryAttributeNameMaxLength)]
		public ZString ActiveDirectoryAttributeName
		{
			get { return activeDirectoryAttributeName; }
			set
			{
				CheckMaximumLength(ActiveDirectoryAttributeNameInfo, value);
				SetNonPersistentPropertyValue(ActiveDirectoryAttributeNameInfo, ref activeDirectoryAttributeName, value);
				ValidateActiveDirectoryAttributeName();
			}
		}
		ZString activeDirectoryAttributeName;

		public ZPropertyInfo ActiveDirectoryAttributeNameInfo => GetZPropertyInfo(Schema.ActiveDirectoryAttributeName);

		public CodeDescriptionPairList AvailableADAttributes
		{
			get
			{
				var list = IsGroupColumn() ? ADAttributeList.Instance.GroupAttributes : ADAttributeList.Instance.UserAttributes;
				if (list.Cast<CodeDescriptionPair>().Any())
				{
					return list;
				}
				else
				{
					// cannot load from Domain, use predefined attributes list
					return ADAttributeList.Instance.PredefinedAttributes;
				}
			}
		}

		bool IsGroupColumn() => EnterpriseColumnName.StartsWith($"{GlbGroupSchema.Constants.Prefix}_", StringComparison.OrdinalIgnoreCase);

		protected bool ActiveDirectoryAttributeName_ReadOnly => Array.Exists(readOnlyEnterpriseColumns, e => e == enterpriseColumnName);

		readonly string[] readOnlyEnterpriseColumns = { GlbGroupSchema.Constants.GG_Desc, GlbStaffSchema.Constants.GS_LoginName, GlbStaff.Schema.CurrentDRMManager };

		void ValidateActiveDirectoryAttributeName()
		{
			if (!IsValidationSuspended)
			{
				ActiveDirectoryAttributeNameInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(ActiveDirectoryAttributeNameInfo);
				ListValidation.ErrorIfInvalidCode(ActiveDirectoryAttributeNameInfo);

				if (((ZString)ActiveDirectoryAttributeNameInfo.Value).EqualsIgnoringCase(ADAttributes.Name))
				{
					if (!EnterpriseColumnName.EqualsIgnoringCase(GlbStaffSchema.GS_FullName.Name) && !EnterpriseColumnName.EqualsIgnoringCase(GlbGroupSchema.GG_Desc.Name))
					{
						ActiveDirectoryAttributeNameInfo.AddError(Res.GetString("EF3BC165-9ACA-4DB1-A4D7-9D6D5860E585", "The '{0}' attribute cannot be mapped to column other than {1} or {2}.",
							ADAttributes.Name,
							GlbStaffSchema.GS_FullName.Name,
							GlbGroupSchema.GG_Desc.Name));
					}
					else if (EnterpriseColumnName.EqualsIgnoringCase(GlbStaffSchema.GS_FullName.Name))
					{
						ActiveDirectoryAttributeNameInfo.AddWarning(Res.GetString("14C1A72F-184B-4ACD-853F-5970D97F37C4", "Mapping {0} to '{1}' attribute could affect the AD user login name including '{2}' and '{3}' attributes, potentially affecting the functionality of user logins and single-sign-on.",
							GlbStaffSchema.GS_FullName.Name,
							ADAttributes.Name,
							ADAttributes.UserPrincipalName,
							ADAttributes.SAMAccountName
							));
					}
				}
			}
		}

		#endregion

		#region IsSynced

		[ResourceStringData("AttributeMapItem|IsSynced", Caption = "Synced", FullDescription = "Whether or not this attribute mapping is actively synchronized.")]
		public ZBool IsSynced
		{
			get { return isSynced; }
			set { SetNonPersistentPropertyValue(IsSyncedInfo, ref isSynced, value); }
		}
		ZBool isSynced;

		public ZPropertyInfo IsSyncedInfo => GetZPropertyInfo(Schema.IsSynced);

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateActiveDirectoryAttributeName();
		}

		public bool Matches(SchemaColumn column) => column == schema;

		public new AttributeMapItem Clone() => new AttributeMapItem(EnterpriseColumnName, ActiveDirectoryAttributeName, IsSynced);

		public override bool Equals(object obj)
		{
			var other = obj as AttributeMapItem;
			return other != null
				&& other.ActiveDirectoryAttributeName == ActiveDirectoryAttributeName
				&& other.EnterpriseColumnName == EnterpriseColumnName
				&& other.IsSynced == IsSynced;
		}

		public override int GetHashCode() => EnterpriseColumnName.GetHashCode() ^ ActiveDirectoryAttributeName.GetHashCode() ^ IsSynced.GetHashCode();

		#region Schema

		static class Schema
		{
			internal const string EnterpriseColumnName = "EnterpriseColumnName";
			internal const int EnterpriseColumnNameMaxLength = 35;
			internal const string ActiveDirectoryAttributeName = "ActiveDirectoryAttributeName";
			internal const int ActiveDirectoryAttributeNameMaxLength = 35;
			internal const string IsSynced = "IsSynced";
		}

		#endregion

		public class MapItemEqualityComparer : EqualityComparer<AttributeMapItem>
		{
			public override bool Equals(AttributeMapItem x, AttributeMapItem y)
			{
				bool activeDirectoryPropertiesMatch = x.ActiveDirectoryAttributeName == y.ActiveDirectoryAttributeName;
				bool schemasMatch = (x.schema != null || y.schema != null) && x.schema == y.schema;
				bool schemasAreNullButColumnsMatch = x.schema == null && y.schema == null && x.EnterpriseColumnName == y.EnterpriseColumnName;

				return activeDirectoryPropertiesMatch && (schemasMatch || schemasAreNullButColumnsMatch) && x.IsSynced == y.IsSynced;
			}

			public override int GetHashCode(AttributeMapItem obj) => obj.EnterpriseColumnName.GetHashCode();
		}
	}
}
