using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Text;
using CargoWise.Data;
using CargoWise.IO;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class DocumentsUpgradeTask : EmbeddedUpgradeTask
	{
		public DocumentsUpgradeTask()
			: base(new DocumentsDataFile())
		{
		}

		public DocumentsUpgradeTask(DocumentsDataFile dataFile)
			: base(dataFile)
		{
		}

		/// <summary>
		/// This constructor is used only by the subclass ClientDocumentsUpgradeTask
		/// </summary>
		public DocumentsUpgradeTask(ClientDocumentsDataFile clientDataFile)
			: base(clientDataFile)
		{
		}

		/// <summary>
		/// This constructor is used only DBUpgrade process
		/// </summary>
		public DocumentsUpgradeTask(DocumentsCompleteDataFile documentCompleteDataFile)
			: base(documentCompleteDataFile)
		{
		}

		public override bool ExternalVersionBump
		{
			get { return true; }
		}

		public const string EDocsProviderPlaceholderTag = "<EDocsProviderPlaceholderFor>";

		#region Implementation

		protected override void DoDelete(DataRow targetRow, ref int targetIndex)
		{
			bool doNotDelete = false;

			switch (targetRow.Table.TableName)
			{
				case StmMenuItemSchema.Constants.TableName:
					string filterList = targetRow[StmMenuItemSchema.Constants.SU_FilterList] as string;
					if (!string.IsNullOrEmpty(filterList) &&
						filterList.Contains(EDocsProviderPlaceholderTag))
					{
						doNotDelete = true;
						targetIndex++;
					}
					else
					{
						Guid menuItemPk = (Guid)targetRow[StmMenuItemSchema.Constants.PK];

						DeleteMenuItemChildReferences(menuItemPk);
						DeleteRelatedGlbSecurity(menuItemPk);
						DeleteRelatedReportColumnSettings(menuItemPk);
						//CheckHasNonSystemRelatedItems(menuItemPk);
						if (!(bool)targetRow[StmMenuItemSchema.Constants.SU_IsClientSpecific])
						{
							DeleteClientSpecificRelatedItems(menuItemPk);
						}
					}
					break;
				case RateAttachmentSetSchema.Constants.TableName:
					DeleteTemplateChildRateAttachment((Guid)targetRow[0]);
					break;
				case RefDocTypeSchema.Constants.TableName:
					DeleteNonSystemStmMenuEDocsRowsWithRefDocType((Guid)targetRow[0]);
					RemoveDocTypeFromNonSystemStmMenuTemplatePivot((Guid)targetRow[0]);
					break;
			}

			if (!doNotDelete)
			{
				base.DoDelete(targetRow, ref targetIndex);
			}
		}

		protected override void DoInsert(DataRow sourceRow, DataTable targetTable, ref int targetIndex)
		{
			RemoveDuplicatedRecords(sourceRow);
			base.DoInsert(sourceRow, targetTable, ref targetIndex);
		}

		void RemoveDuplicatedRecords(DataRow sourceRow)
		{
			switch (sourceRow.Table.TableName)
			{
				case StmMenuEDocsSchema.Constants.TableName:
					DeleteMenuEDocsRowsIfTheyAlreadyExist(sourceRow);
					break;

				case StmMenuMenuPivotSchema.Constants.TableName:
					DeleteUserDefinedMenuMenuPivotIfAlreadyExist(sourceRow);
					break;
			}
		}

		void DeleteMenuEDocsRowsIfTheyAlreadyExist(DataRow sourceRow)
		{
			string sqlText = String.Format(
				"DELETE dbo.{0} WHERE {1} = @IsSystemDefined AND {2} = @DocTypePK AND {3} = @MenuPK",
				StmMenuEDocsSchema.Constants.TableName,
				StmMenuEDocsSchema.Constants.SX_IsSystemDefined,
				StmMenuEDocsSchema.Constants.SX_RT_DocType,
				StmMenuEDocsSchema.Constants.SX_SU);
			DbCommand cmd = Db.Connection.Command(sqlText);
			cmd.AddParameterBasedOnDbColumn("@IsSystemDefined", false, StmMenuEDocsSchema.SX_IsSystemDefined);
			cmd.AddParameterBasedOnDbColumn("@DocTypePK", sourceRow[StmMenuEDocsSchema.Constants.SX_RT_DocType], StmMenuEDocsSchema.SX_RT_DocType);
			cmd.AddParameterBasedOnDbColumn("@MenuPK", sourceRow[StmMenuEDocsSchema.Constants.SX_SU], StmMenuEDocsSchema.SX_SU);
			cmd.ExecuteNonQuery();
		}

		void DeleteUserDefinedMenuMenuPivotIfAlreadyExist(DataRow sourceRow)
		{
			string sqlText = String.Format(
				"DELETE dbo.{0} WHERE {1} = @IsSystemDefined AND {2} = @Inwards AND {3} = @Outwards AND {4} = @OverriddenBusinessContext",
				StmMenuMenuPivotSchema.Constants.TableName,
				StmMenuMenuPivotSchema.Constants.SF_IsSystemDefined,
				StmMenuMenuPivotSchema.Constants.SF_SU_Inward,
				StmMenuMenuPivotSchema.Constants.SF_SU_Outward,
				StmMenuMenuPivotSchema.Constants.SF_OverriddenBusinessContext);
			DbCommand cmd = Db.Connection.Command(sqlText);
			cmd.AddParameterBasedOnDbColumn("@IsSystemDefined", false, StmMenuMenuPivotSchema.SF_IsSystemDefined);
			cmd.AddParameterBasedOnDbColumn("@Inwards", sourceRow[StmMenuMenuPivotSchema.Constants.SF_SU_Inward], StmMenuMenuPivotSchema.SF_SU_Inward);
			cmd.AddParameterBasedOnDbColumn("@Outwards", sourceRow[StmMenuMenuPivotSchema.Constants.SF_SU_Outward], StmMenuMenuPivotSchema.SF_SU_Outward);
			cmd.AddParameterBasedOnDbColumn("@OverriddenBusinessContext", sourceRow[StmMenuMenuPivotSchema.Constants.SF_OverriddenBusinessContext], StmMenuMenuPivotSchema.SF_OverriddenBusinessContext);
			cmd.ExecuteNonQuery();
		}

		void DeleteNonSystemStmMenuEDocsRowsWithRefDocType(Guid refDocTypePK)
		{
			string sqlText = String.Format(
				"DELETE dbo.{0} WHERE {1} = @RefDocTypePK AND {2} = 0",
				StmMenuEDocsSchema.Constants.TableName,
				StmMenuEDocsSchema.Constants.SX_RT_DocType,
				StmMenuEDocsSchema.Constants.SX_IsSystemDefined);
			DbCommand cmd = Db.Connection.Command(sqlText);
			cmd.AddParameterBasedOnDbColumn("@RefDocTypePK", refDocTypePK, StmMenuEDocsSchema.SX_RT_DocType);
			cmd.ExecuteNonQuery();
		}

		void RemoveDocTypeFromNonSystemStmMenuTemplatePivot(Guid refDocTypePK)
		{
			string sqlText = String.Format(
				"UPDATE dbo.{0} SET {1} = null WHERE {1} = @RefDocTypePK AND {2} = 0",
				StmMenuTemplatePivotSchema.Constants.TableName,
				StmMenuTemplatePivotSchema.Constants.SI_RT_DocType,
				StmMenuTemplatePivotSchema.Constants.SI_IsSystemDefined);
			DbCommand cmd = Db.Connection.Command(sqlText);
			cmd.AddParameterBasedOnDbColumn("@RefDocTypePK", refDocTypePK, StmMenuTemplatePivotSchema.SI_RT_DocType);
			cmd.ExecuteNonQuery();
		}

		protected void DeleteMenuItemChildReferences(Guid menuItemPk)
		{
			var stringBuilder = new StringBuilder();

			DeleteChildReferences(StmMenuItemSchema.Constants.TableName, "= @MenuItemPk", stringBuilder);

			var command = Db.Connection.Command(stringBuilder.ToString());
			command.AddParameter("@MenuItemPk", SqlDbType.UniqueIdentifier, menuItemPk);
			command.ExecuteNonQuery();
		}

		protected void DeleteRelatedGlbSecurity(Guid menuItemPk)
		{
			const string sql = @"delete dbo.GlbSecurity where GU_ItemGUID = @MenuItemPk";
			DbCommand command = Db.Connection.Command(sql);
			command.AddParameter("@MenuItemPk", SqlDbType.UniqueIdentifier, menuItemPk);
			command.ExecuteNonQuery();
		}

		void DeleteRelatedReportColumnSettings(Guid menuItemPk)
		{
			const string sql = @"DELETE dbo.StmData WHERE SD_Owner = @MenuItemPk";
			var command = Db.Connection.Command(sql);
			command.AddParameter("@MenuItemPk", SqlDbType.UniqueIdentifier, menuItemPk);
			command.ExecuteNonQuery();
		}

#if DEBUG
		internal
#endif
		void DeleteChildReferences(string parentTableName, string parentFilter, StringBuilder stringBuilder)
		{
			foreach (var substanceReference in ExternalMenuReferences[parentTableName])
			{
				if (ExternalMenuReferences.ContainsKey(substanceReference.Schema.TableName))
				{
					var childFilter = $"IN (SELECT {substanceReference.Schema.PK.Name} FROM {substanceReference.Schema.SqlSchemaName}.{substanceReference.Schema.TableName} WHERE {substanceReference.MenuItemColumn} {parentFilter})";

					DeleteChildReferences(substanceReference.Schema.TableName, childFilter, stringBuilder);
				}

				stringBuilder.AppendLine($"DELETE {substanceReference.Schema.SqlSchemaName}.{substanceReference.Schema.TableName} WHERE {substanceReference.MenuItemColumn} {parentFilter};");
			}
		}

		protected void CheckHasNonSystemRelatedItems(Guid menuItemPk)
		{
			bool result = false;

			DbCommand command = Db.Connection.Command("select count(*) from dbo.StmMenuTemplatePivot where SI_SU = @MenuItemPk and SI_IsSystemDefined = 0");
			command.AddParameter("@MenuItemPk", SqlDbType.UniqueIdentifier, menuItemPk);
			result |= (int)command.ExecuteScalar() > 0;

			command = Db.Connection.Command("select count(*) from dbo.StmMenuMenuPivot where (SF_SU_Outward = @MenuItemPk or SF_SU_Inward = @MenuItemPk) and SF_IsSystemDefined = 0");
			command.AddParameter("@MenuItemPk", SqlDbType.UniqueIdentifier, menuItemPk);
			result |= (int)command.ExecuteScalar() > 0;

			if (result)
			{
				throw new ApplicationException(string.Format("System defined menu item with PK='{0}' cannot be deleted as it is referenced by user defined data.", menuItemPk.ToString()));
			}
		}

		protected void DeleteClientSpecificRelatedItems(Guid menuItemPk)
		{
			const string sql =
@"delete dbo.StmMenuTemplatePivot where SI_SU = @MenuItemPk and SI_IsSystemDefined = 1 and SI_IsClientSpecific = 1
delete dbo.StmMenuMenuPivot where (SF_SU_Outward = @MenuItemPk or SF_SU_Inward = @MenuItemPk) and SF_IsSystemDefined = 1 and SF_IsClientSpecific = 1";

			DbCommand command = Db.Connection.Command(sql);
			command.AddParameter("@MenuItemPk", SqlDbType.UniqueIdentifier, menuItemPk);
			command.ExecuteNonQuery();
		}

		protected void DeleteTemplateChildRateAttachment(Guid setPK)
		{
			string sqlText = string.Format("DELETE {0} WHERE {1} = @setPK",
				/* 0 */ RateAttachmentSchema.Constants.TableName,
				/* 1 */ RateAttachmentSchema.Constants.TA_TS);

			using (DbCommand cmd = Db.Connection.Command(sqlText))
			{
				cmd.AddParameter("@setPK", SqlDbType.UniqueIdentifier, setPK);
				cmd.ExecuteNonQuery();
			}
		}

		//Reflect the changes done in this collection to ColumnsToIgnoreForTest [unit tests]
		protected StringCollection ColumnsToIgnore
		{
			get
			{
				if (columnsToIgnore == null)
				{
					columnsToIgnore = new StringCollection();
				}

				if (IsRunningForSetup)
				{
					if (columnsToIgnore.Count > 0)
					{
						columnsToIgnore.Clear();
					}
				}
				else
				{
					if (columnsToIgnore.Count == 0)
					{
						columnsToIgnore.Add(StmMenuItemSchema.SU_IsModifiable.Name);
						columnsToIgnore.Add(StmMenuItemSchema.SU_DeliveryRestrictionType.Name);
						columnsToIgnore.Add(StmMenuItemSchema.SU_DeliveryRestrictionDescription.Name);
						columnsToIgnore.Add(StmMenuItemSchema.SU_DeliveryRestrictionMacro.Name);
						columnsToIgnore.Add(StmMenuItemSchema.SU_IsPublished.Name);
						columnsToIgnore.Add(StmMenuItemSchema.SU_IsLocalDocument.Name);
						columnsToIgnore.Add(StmMenuItemSchema.SU_IsVisibleOnWeb.Name);
						columnsToIgnore.Add(StmMenuItemSchema.SU_EmailSubjectLine.Name);
						columnsToIgnore.Add(StmMenuItemSchema.SU_EmailSenderOverride.Name);
						columnsToIgnore.Add(StmMenuItemSchema.SU_MenuIndex.Name);
						columnsToIgnore.Add(StmMenuItemSchema.SU_SignBy.Name);
						columnsToIgnore.Add(StmMenuItemSchema.SU_DefaultAttachmentType.Name);
						columnsToIgnore.Add(StmMenuItemSchema.SU_IncludeDocInArchive.Name);

						columnsToIgnore.Add(StmMenuEDocsSchema.SX_IsClientSupressed.Name);

						columnsToIgnore.Add(RefDocTypeSchema.RT_IsPublished.Name);
						columnsToIgnore.Add(RefDocTypeSchema.RT_SaveVersions.Name);
						columnsToIgnore.Add(RefDocTypeSchema.RT_IsPublishUpdatable.Name);
						columnsToIgnore.Add(RefDocTypeSchema.RT_LogSystemCreatedDocsToEDocs.Name);
						columnsToIgnore.Add(RefDocTypeSchema.RT_ForceUserToRead.Name);
						columnsToIgnore.Add(RefDocTypeSchema.RT_IsActive.Name);
						columnsToIgnore.Add(RefDocTypeSchema.RT_SE_NKDocumentReceivedEvent.Name);
						columnsToIgnore.Add(RefDocTypeSchema.RT_IsCompanySpecific.Name);
						columnsToIgnore.Add(RefDocTypeSchema.RT_IsBranchSpecific.Name);
						columnsToIgnore.Add(RefDocTypeSchema.RT_IsDepartmentSpecific.Name);
						columnsToIgnore.Add(RefDocTypeSchema.RT_LogMacro.Name);
						columnsToIgnore.Add(RefDocTypeSchema.RT_AllowMultiplePeriodicDocs.Name);
						columnsToIgnore.Add(RefDocTypeSchema.RT_ParseType.Name);

						columnsToIgnore.Add(RateAttachmentSetSchema.TS_IsDefault.Name);
						columnsToIgnore.Add(RateAttachmentSetSchema.TS_IsMandatory.Name);
						columnsToIgnore.Add(RateAttachmentSetSchema.TS_Sequence.Name);

						columnsToIgnore.Add(StmMenuTemplatePivotSchema.SI_IsPasswordProtected.Name);
						columnsToIgnore.Add(StmMenuTemplatePivotSchema.SI_IsPasswordProtectedForOpening.Name);
						columnsToIgnore.Add(StmMenuTemplatePivotSchema.SI_PrintByDefault.Name);

						columnsToIgnore.Add(StmMenuDocumentConfigSchema.S3_ExcludedFromDocPack.Name);
					}
				}

				return columnsToIgnore;
			}
		}

		StringCollection columnsToIgnore;

		//Reflect the changes done in this collection to FormBuilderColumnsToIgnoreForTest [unit tests]
		protected StringCollection FormBuilderColumnsToIgnore
		{
			get
			{
				if (formBuilderColumnsToIgnore == null)
				{
					formBuilderColumnsToIgnore = new StringCollection();
				}

				if (IsRunningForSetup)
				{
					if (formBuilderColumnsToIgnore.Count > 0)
					{
						formBuilderColumnsToIgnore.Clear();
					}
				}
				else
				{
					if (formBuilderColumnsToIgnore.Count == 0)
					{
						formBuilderColumnsToIgnore.Add(StmMenuItemSchema.SU_DeliveryRestrictionType.Name);
						formBuilderColumnsToIgnore.Add(StmMenuItemSchema.SU_DeliveryRestrictionDescription.Name);
						formBuilderColumnsToIgnore.Add(StmMenuItemSchema.SU_DeliveryRestrictionMacro.Name);
						formBuilderColumnsToIgnore.Add(StmMenuItemSchema.SU_IsPublished.Name);
						formBuilderColumnsToIgnore.Add(StmMenuItemSchema.SU_MenuIndex.Name);
						formBuilderColumnsToIgnore.Add(StmMenuItemSchema.SU_EmailSubjectLine.Name);
					}
				}

				return formBuilderColumnsToIgnore;
			}
		}

		StringCollection formBuilderColumnsToIgnore;

		protected override void UpdateColumn(string columnName, DataRow targetRow, DataRow sourceRow)
		{
			RemoveDuplicatedRecords(sourceRow);
			if (!ColumnsToIgnore.Contains(columnName) || IsFormBuilderDocument(sourceRow) && !FormBuilderColumnsToIgnore.Contains(columnName))
			{
				if (!columnName.Equals(StmTemplateSchema.Constants.SO_Template, StringComparison.OrdinalIgnoreCase)
					|| !targetRow[StmTemplateSchema.Constants.SO_Name].ToString().Equals("Customized Document Elements", StringComparison.OrdinalIgnoreCase))
				{
					if (!IsRunningForSetup && columnName.Equals(StmMenuItemSchema.Constants.SU_PreventAutoDelivery))
					{
						string sourceContactType = sourceRow[StmMenuItemSchema.Constants.SU_ContactType].ToString();
						bool sourcePreventAutoDeliveryIsReadonly = sourceContactType == "NCT";

						string targetContactType = targetRow[StmMenuItemSchema.Constants.SU_ContactType, DataRowVersion.Original].ToString();
						bool targetPreventAutoDeliveryIsReadonly = targetContactType == "NCT";

						if (sourcePreventAutoDeliveryIsReadonly || targetPreventAutoDeliveryIsReadonly)
						{
							base.UpdateColumn(columnName, targetRow, sourceRow);
						}
					}
					else if (!IsRunningForSetup && columnName.Equals(StmMenuItemSchema.Constants.SU_IncludeDocInArchive))
					{
						string source = sourceRow[StmMenuItemSchema.Constants.SU_IncludeDocInArchive].ToString();
						bool sourceIsUserDefineable = source == "YES" || string.IsNullOrEmpty(source);

						string target = targetRow[StmMenuItemSchema.Constants.SU_IncludeDocInArchive].ToString();
						bool targetIsUserDefined = target == "YES" || string.IsNullOrEmpty(target);

						if (!sourceIsUserDefineable || !targetIsUserDefined)
						{
							base.UpdateColumn(columnName, targetRow, sourceRow);
						}
					}
					else
					{
						if (columnName.Equals(StmMenuItemSchema.Constants.SU_MenuType, StringComparison.OrdinalIgnoreCase))
						{
							var targetMenuType = targetRow[StmMenuItemSchema.Constants.SU_MenuType].ToString();
							if (targetMenuType != "WEB")
							{
								targetRow[StmMenuItemSchema.Constants.SU_IsVisibleOnWeb] = 0;
							}
						}
						else if (columnName.Equals(StmMenuItemSchema.Constants.SU_SupportsVisualisation, StringComparison.OrdinalIgnoreCase))
						{
							if ((sourceRow[StmMenuItemSchema.Constants.SU_SupportsVisualisation] is string stringy && stringy == "false")
								|| (sourceRow[StmMenuItemSchema.Constants.SU_SupportsVisualisation] is bool booly && !booly))
							{
								targetRow[StmMenuItemSchema.Constants.SU_IsModifiable] = 0;
							}
						}
						base.UpdateColumn(columnName, targetRow, sourceRow);
					}
				}
			}
		}

		bool IsFormBuilderDocument(DataRow dataRow)
		{
			return dataRow.Table.TableName == StmMenuItemSchema.Constants.TableName && dataRow[StmMenuItemSchema.Constants.SU_MenuType].ToString() == "FRM";
		}

		protected override void UpdateVersionNumber()
		{
			base.UpdateVersionNumber();
			PurgeFileSystemTemplateCache();
		}

		void PurgeFileSystemTemplateCache()
		{
			var customisationVersion = DbRegistry.DocumentCustomisationVersionNumber.LoadValue(Db.Connection);
			DbRegistry.DocumentCustomisationVersionNumber.SaveValue(customisationVersion + 1, Db.Connection);

			var cachePath = Path.Combine(Temp.TempPath, "TemplateCache");
			if (Directory.Exists(cachePath))
			{
				foreach (var file in new DirectoryInfo(cachePath).GetFiles("*.xls"))
				{
					try
					{
						file.Delete();
					}
					catch (IOException) { }
					catch (UnauthorizedAccessException) { }
				}
			}
		}

#if DEBUG
		internal
#endif
		static readonly Dictionary<string, (ITableSchema Schema, string MenuItemColumn)[]> ExternalMenuReferences = new Dictionary<string, (ITableSchema schema, string menuItemColumn)[]>
		{
			{
				StmMenuItemSchema.Constants.TableName, new  (ITableSchema schema, string menuItemColumn)[]
				{
					(OrgDocumentSchema.Instance, OrgDocumentSchema.Constants.OD_SU_MenuItem),
					(ProcessTaskNotificationSchema.Instance, ProcessTaskNotificationSchema.Constants.PQ_SU_Document),
					(AccComplianceSequenceSchema.Instance, AccComplianceSequenceSchema.Constants.XD_SU_MenuItem),
					(StmDefaultPrinterSchema.Instance, StmDefaultPrinterSchema.Constants.SDP_SU_Document),
					(OrgSecuritySchema.Instance, OrgSecuritySchema.Constants.OX_SU),
					(StmDocDataOverrideSchema.Instance, StmDocDataOverrideSchema.Constants.DD_SU),
					(JobDocumentDeliverySchema.Instance, JobDocumentDeliverySchema.Constants.JDC_SU_MenuItem),
					(StmDocumentDeliverySchema.Instance, StmDocumentDeliverySchema.Constants.SDL_SU)
				}
			},
			{
				OrgDocumentSchema.Constants.TableName, new (ITableSchema schema, string menuItemColumn)[]
				{
					(OrgDocumentCopyRecipientSchema.Instance, OrgDocumentCopyRecipientSchema.Constants.ODR_OD),
				}
			},
			{
				OrgSecuritySchema.Constants.TableName, new (ITableSchema schema, string menuItemColumn)[]
				{
					(OrgSecurityContactsSchema.Instance, OrgSecurityContactsSchema.Constants.OZ_OX),
				}
			}
		};

		#endregion
	}
}
