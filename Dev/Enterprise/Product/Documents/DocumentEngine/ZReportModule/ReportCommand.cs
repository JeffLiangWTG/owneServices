using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine
{
	public class ReportCommand : StmMenuItemBase
	{
		public ReportCommand(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool IsApplicableCore()
		{
			if (!SU_IsPublished && SU_IsSystemDefined)
			{
				return false;
			}

			var result = true;
			if (!SU_FilterList.IsEmpty)
			{
				result = false;

				if (ZExpressionEvaluator.IsInnermostRegexMatch(SU_FilterList))
				{
					return ZExpressionEvaluator.Evaluate(SU_FilterList, null, BODocDataProvider.Get(this));
				}

				var filterValues = SU_FilterList.Split('=');
				string filterTypeString = filterValues[0];
				var hasNotEquals = filterTypeString.EndsWith("!");
				if (hasNotEquals)
				{
					filterTypeString = filterTypeString.TrimEnd(new char[] { '!' });
				}

				var recognisedFilterType = false;
				if (Enum.IsDefined(typeof(DocumentFilters), filterTypeString))
				{
					recognisedFilterType = true;
					var filterType = (DocumentFilters)Enum.Parse(typeof(DocumentFilters), filterTypeString);
					switch (filterType)
					{
						case DocumentFilters.CTY:
							for (int index = 1; index < filterValues.Length; index++)
							{
								result = GlbCompany.CurrentCompany.GC_RN_NKCountryCode == filterValues[index].ToString();
								if (result)
								{
									break;
								}
							}
							break;

						case DocumentFilters.BKRCTY:
							var customsCountry = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
							for (int index = 1; index < filterValues.Length; index++)
							{
								result = customsCountry == filterValues[index].ToString();
								if (result)
								{
									break;
								}
							}
							break;

						case DocumentFilters.CTYEG:
							result = GlbCompany.CurrentCompany.Country.RN_EconomicGrouping == filterValues[1].ToString();
							break;

						case DocumentFilters.EUGB:
							bool isAllowed = GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.UnitedKingdom
								|| GlbCompany.CurrentCompany.Country.RN_EconomicGrouping == EconomicGroupList.Codes.EuropeanUnion;
							if (filterValues.Length > 1)
							{
								result = isAllowed && filterValues[1].ToString() == "Y";
							}
							break;

						case DocumentFilters.CO:
							result = GlbCompany.CurrentCompany.GC_Code == filterValues[1].ToString();
							break;

						case DocumentFilters.CMP:
							isAllowed = GlbCompany.CurrentCompany.Country.SupportComplianceSubType;
							if (filterValues.Length > 1)
							{
								result = isAllowed && filterValues[1].ToString() == "Y";
							}
							break;

						case DocumentFilters.ERS:
							result = GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.China || (filterValues[1].ToString() == "Y" && AccountingMasterFilesRegistry.Instance.EnableReportSetup.Value);
							break;

						case DocumentFilters.NCTS:  // We cannot use CTYEG for NCTS because non-EU countries like Norway participate. So flag with a yes and then check the current country code. 
							result = filterValues[1].ToString() == "Y" && ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsCountryEuOrCtCountry(GlbCompany.CurrentCompany.Country.RN_Code);
							break;

						case DocumentFilters.CSHB:
							result = filterValues[1].ToString() == "Y" && GlbCompany.CurrentCompany.GC_IsGSTCashBasis;
							break;

						case DocumentFilters.COMAP:
							isAllowed = GlbCompany.CurrentCompany.Country.SupportComplianceSubType && GlbCompany.CurrentCompany.Country.ComplianceSubTypeIncludeAPLedger;
							if (filterValues.Length > 1)
							{
								result = isAllowed && filterValues[1].ToString() == "Y";
							}
							break;

						case DocumentFilters.COMNOAP:
							isAllowed = GlbCompany.CurrentCompany.Country.SupportComplianceSubType && !GlbCompany.CurrentCompany.Country.ComplianceSubTypeIncludeAPLedger;
							if (filterValues.Length > 1)
							{
								result = isAllowed && filterValues[1].ToString() == "Y";
							}
							break;

						case DocumentFilters.LoginName:
							result = string.Compare(GlbStaff.CurrentUser.GS_LoginName, filterValues[1].ToString(), StringComparison.OrdinalIgnoreCase) == 0;
							break;

						case DocumentFilters.IsComplianceDocumentModuleEnabled:
							result = filterValues[1].ToString() == "Y" && AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value;
							break;

						case DocumentFilters.TF:
							isAllowed = GlbCompany.CurrentCompany.IsEnabledForTaxFrameworkConfiguration(Factory);
							if (filterValues.Length > 1)
							{
								result = isAllowed && filterValues[1].ToString() == "Y";
							}
							break;

						case DocumentFilters.HasHVLVClearance:
							result = filterValues[1].ToString() == "Y" && HVLVDataRegistry.HasHVLVClearance;
							break;

						case DocumentFilters.TSD:
							var temporaryStorageSettings = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ITemporaryStorageSettings>();
							result = filterValues[1].ToString() == "Y" && (temporaryStorageSettings.IsUsingUCC5 || temporaryStorageSettings.IsUsingUCC6);
							break;

						default:
							recognisedFilterType = false;
							break;
					}
				}

				if (recognisedFilterType && hasNotEquals)
				{
					result = !result;
				}
			}

			return result;
		}

		[ChildEditable(true)]
		public ReportScheduleTaskDependentCollection Schedules
		{
			get
			{
				if (schedules == null)
				{
					schedules = new ReportScheduleTaskDependentCollection(this);
					schedules.Load();
					RegisterEditableChildObject(schedules);
				}

				return schedules;
			}
		}
		ReportScheduleTaskDependentCollection schedules;

		protected override void OnSavingForDelete()
		{
			RemoveConfigurations();
			base.OnSavingForDelete();
		}

		public override void Delete()
		{
			Schedules.RemoveAndDeleteAll();
			base.Delete();
		}

		public override MultilingualString GetWarningBeforeBeingDeleted()
		{
			if (Schedules.Count != 0)
			{
				return ResString.GetMultilingualString("b95d8bab-48dc-446c-b283-56aab2a53db5", "This report has been scheduled, all related schedules will be deleted.");
			}
			return base.GetWarningBeforeBeingDeleted();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:DoNotUseDbConnectionMethods", Justification = "Necessary")]
		void RemoveConfigurations()
		{
			var sql = $@"DELETE dbo.StmData WHERE SD_Owner = @MenuItemID and SD_Name like @RegistryKeyPrefix";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@MenuItemID", SqlDbType.UniqueIdentifier, PK.ToGuid());
				cmd.AddParameter("@RegistryKeyPrefix", SqlDbType.VarChar, $"{ReportColumnSettingRegistryPrefixHelper.RegistryKeyPrefix}%");
				cmd.ExecuteNonQuery();
			}
		}

		protected override StmMenuItemValidation GetNewValidation()
		{
			return new ReportCommandValidation(this);
		}

		#region SU_IsVisibleOnWeb

		public override ZBool SU_IsVisibleOnWeb
		{
			get => !SU_Calc_IsWebSupportable ? ZBool.False : base.SU_IsVisibleOnWeb;
			set
			{
				if (value)
				{
					SU_Calc_IsWebSupportable = true;
				}

				base.SU_IsVisibleOnWeb = value;
			}
		}

		public bool SU_IsVisibleOnWeb_ReadOnly => !SU_Calc_IsWebSupportable;

		#endregion

		public override StmMenuTemplatePivotBaseCollection Documents
		{
			get
			{
				var documents = base.Documents;
				documents.MaxCountValidationEnable(1, Res.GetString("5d6b0cc8-fad5-4276-ae62-707649bcfbd6", "Reports can only have a single template."));
				return documents;
			}
		}

		public Report GetReport()
		{
			var printset = new ReportPrintSet(this);
			DocumentPack pack = printset[0];
			Report report = pack.GetFirstReport();
			if (report != null)
			{
				report.PrepareForRender();
			}

			return report;
		}

		bool hasReportBeenAnalyzed;

		void AnalyzeReport()
		{
			if (!hasReportBeenAnalyzed)
			{
				reportHasColumnHeaders = false;
				disableXLSXExport = false;
				using (var report = GetReport())
				{
					if (report != null)
					{
						reportHasColumnHeaders = !report.ColumnHeadingManager.IsEmpty;
						disableXLSXExport = report.Analyser != null && report.Analyser.Config != null && report.Analyser.Config.DisableXLSXExport;
						disableCSVExport = report.Analyser != null && report.Analyser.Config != null && report.Analyser.Config.DisableCSVExport;
						excludedAttachmentTypes = report.Analyser?.GetExcludedAttachmentTypes();
					}
				}
				hasReportBeenAnalyzed = true;
			}
		}
		public bool ReportHasColumnHeaders
		{
			get
			{
				AnalyzeReport();
				return reportHasColumnHeaders.Value;
			}
		}
		bool? reportHasColumnHeaders;

		public bool DisableXLSXExport
		{
			get
			{
				AnalyzeReport();
				return disableXLSXExport ?? false;
			}
		}
		bool? disableXLSXExport;

		public IReadOnlyCollection<string> ExcludedAttachmentTypes
		{
			get
			{
				AnalyzeReport();
				return excludedAttachmentTypes ?? Array.Empty<string>();
			}
		}
		string[] excludedAttachmentTypes;

		public bool DisableCSVExport
		{
			get
			{
				AnalyzeReport();
				return disableCSVExport ?? false;
			}
		}
		bool? disableCSVExport;

		protected override ICodeDescriptionPairList GetAttachmentTypesCore()
		{
			if (attachmentTypes == null)
			{
				attachmentTypes = new CodeDescriptionPairList();

				attachmentTypes.AddPair(AttachmentTypeList.Codes.Xls, AttachmentTypeList.Descriptions.Xls);
				attachmentTypes.AddPair(AttachmentTypeList.Codes.Xlsx, AttachmentTypeList.Descriptions.Xlsx);
				attachmentTypes.AddPair(AttachmentTypeList.Codes.Csv, AttachmentTypeList.Descriptions.Csv);
				attachmentTypes.AddPair(AttachmentTypeList.Codes.Pdf, AttachmentTypeList.Descriptions.Pdf);
				attachmentTypes.AddPair(AttachmentTypeList.Codes.Pdfa, AttachmentTypeList.Descriptions.Pdfa);
				attachmentTypes.AddPair(AttachmentTypeList.Codes.Tif, AttachmentTypeList.Descriptions.Tif);
				attachmentTypes.AddPair(AttachmentTypeList.Codes.Html, AttachmentTypeList.Descriptions.Html);
				attachmentTypes.AddPair(AttachmentTypeList.Codes.Htmf, AttachmentTypeList.Descriptions.Htmf);
				attachmentTypes.AddPair(AttachmentTypeList.Codes.Txt_Semi, AttachmentTypeList.Descriptions.Txt_Semi);
				attachmentTypes.AddPair(AttachmentTypeList.Codes.Txt_Comm, AttachmentTypeList.Descriptions.Txt_Comm);
				attachmentTypes.AddPair(AttachmentTypeList.Codes.Txt_Pipe, AttachmentTypeList.Descriptions.Txt_Pipe);

				if (!DisableCSVExport && ReportHasColumnHeaders)
				{
					attachmentTypes.AddPair(AttachmentTypeList.Codes.CsvWithHeadings, AttachmentTypeList.Descriptions.CsvWithHeadings);
					attachmentTypes.AddPair(AttachmentTypeList.Codes.Xml, AttachmentTypeList.Descriptions.Xml);
				}

				foreach (var typeCode in ExcludedAttachmentTypes)
				{
					attachmentTypes.RemoveCode(typeCode);
				}
			}
			return attachmentTypes;
		}

		CodeDescriptionPairList attachmentTypes;
	}
}
