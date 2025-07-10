using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceCommodityDetail : AutoComplianceCommodityDetail
	{
		public ComplianceCommodityDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("ComplianceRiskStatus")]
		public override ZGuid CCD_COR_ComplianceRisk
		{
			get => base.CCD_COR_ComplianceRisk;
			set => base.CCD_COR_ComplianceRisk = value;
		}

		public ComplianceRiskStatus ComplianceRiskStatus => Factory.Load<ComplianceRiskStatus>(CCD_COR_ComplianceRisk.IsValid ? CCD_COR_ComplianceRisk : (ZGuid)CCD_COR_ComplianceRiskInfo.OriginalValue);

		public override bool IsSavedByFactory => CommodityType != CommodityType.RelatedJobLink && base.IsSavedByFactory;

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo)
		{
			var result = base.IsValidationEnabledCore(propertyInfo);
			if (ReadOnly && (propertyInfo.Name != CCD_HarmonizedCodeInfo.Name && propertyInfo.Name != ConditionsInfo.Name))
			{
				result = false;
			}

			return result;
		}

		[ReadOnlyMember(nameof(CCD_HarmonizedCode_ReadOnly))]
		public override ZString CCD_HarmonizedCode
		{
			get => base.CCD_HarmonizedCode;
			set
			{
				if (base.CCD_HarmonizedCode != value)
				{
					NeedResetStatus = true;
					BlockedByComplianceRule = false;
					base.CCD_HarmonizedCode = value;

					LegalBookLinkInfo.RefreshBinding();
					HarmonizedBorderWiseTextualInfo.RefreshBinding();
				}
			}
		}

		public bool NeedResetStatus { get; set; }

		bool CCD_HarmonizedCode_ReadOnly => ReadOnly || CommodityType is CommodityType.RelatedJobLink or CommodityType.FetchDataEntry;

		public bool AssessmentInitialized => CommodityType == CommodityType.RelatedJobLink ? RelatedJobIsAssessmentInitialized : (!IsDeleted && !IsDeleting && (ComplianceRiskStatus?.IsAssessmentInitialized ?? false));

		public bool RelatedJobIsAssessmentInitialized { get; set; }

		public ZPropertyInfo ConditionsInfo
		{
			get { return GetZPropertyInfo(nameof(Conditions)); }
		}

		[ReadOnly(true)]
		public ZString CommoditySource { get; set; }

		[ReadOnly(true)]
		public ZString Source { get; set; }

		[ReadOnly(true)]
		public ZString NomenclatureCondition => CCD_NomenclatureCondition ? Res.GetString("332BFB25-5BD1-4A6C-9652-EEC3DFD8E282", "Yes") : ZString.Empty;

		[ReadOnly(true)]
		public ZString SpecificCondition => CCD_SpecificCondition ? Res.GetString("332BFB25-5BD1-4A6C-9652-EEC3DFD8E282", "Yes") : ZString.Empty;

		[ReadOnly(true)]
		public ZString Description { get; set; }

		ZString conditions = "";

		[ReadOnly(true)]
		public ZString Conditions
		{
			get => conditions;
			set
			{
				conditions = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCondtionsInfo();
				}
				ConditionsInfo.RefreshBinding();
			}
		}

		internal bool IsComplianceDecisionChanged { get; set; }

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(AssessmentPropertyReadOnly))]
		public override ZString CCD_RiskStatus
		{
			get => base.CCD_RiskStatus;
			set
			{
				if (base.CCD_RiskStatus != value)
				{
					base.CCD_RiskStatus = value;
					TakeSnapshotWhenComplianceDecisionChanged();
				}
			}
		}

		bool AssessmentPropertyReadOnly => ReadOnly || !AssessmentInitialized;

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(ComplianceCommodityDetailLookups.CommodityRiskStatusCodeList))]
		[ReadOnlyMember(nameof(AssessmentPropertyReadOnly))]
		public ZString CCD_RiskStatusDescription
		{
			get
			{
				return AssessmentInitialized ? Lookups.CommodityRiskStatusCodeList.GetDescriptionFromCode(CCD_RiskStatus.ToString()) : ComplianceRiskStatusCodeList.Descriptions.AssessmentNotInitialized.ToString();
			}
			set
			{
				var riskStatus = Lookups.CommodityRiskStatusCodeList.GetCodeFromDescription(value.ToString());
				if (riskStatus != null)
				{
					CCD_RiskStatus = riskStatus;
				}
				CCD_RiskStatusDescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CCD_RiskStatusDescriptionInfo => GetZPropertyInfo(nameof(CCD_RiskStatusDescription));

		ZString importAlertsForExportJob;

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		public ZString ImportAlertsForExportJobDescription
		{
			get
			{
				return AssessmentInitialized ? Lookups.CommodityImportAlertForExportCodeList.GetDescriptionFromCode(importAlertsForExportJob) : ComplianceRiskStatusCodeList.Descriptions.AssessmentNotInitialized.ToString();
			}
			set
			{
				importAlertsForExportJob = value;
				ImportAlertsForExportJobDescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ImportAlertsForExportJobDescriptionInfo => GetZPropertyInfo(nameof(ImportAlertsForExportJobDescription));

		[ReadOnlyMember(nameof(AssessmentPropertyReadOnly))]
		public override ZString CCD_AssessmentNotes
		{
			get => base.CCD_AssessmentNotes;
			set
			{
				if (base.CCD_AssessmentNotes != value)
				{
					base.CCD_AssessmentNotes = value;
					TakeSnapshotWhenComplianceDecisionChanged();
				}
			}
		}

		void TakeSnapshotWhenComplianceDecisionChanged()
		{
			IsComplianceDecisionChanged = true;
			if (CCD_RiskStatus.HasBlockedOrReleased())
			{
				ComplianceRiskStatus?.TakeSnapshotWhenComplianceDecisionChanged();
			}
		}

		public ZString LegalBookLink
		{
			get
			{
				return ShowLegalBookLink ? Res.GetString("283583a2-b64f-4927-a5b9-1ba9dad9057e", "View") : ZString.Empty;
			}
		}

		public ZPropertyInfo LegalBookLinkInfo => GetZPropertyInfo(nameof(LegalBookLink));

		public ZString HarmonizedBorderWiseTextual
		{
			get
			{
				return ShowLegalBookLink ? Res.GetString("0DE74654-7DF2-429E-948D-8079D0E1683D", "View Compliance Alerts for {0}", CCD_HarmonizedCode) : string.Empty;
			}
		}

		public ZPropertyInfo HarmonizedBorderWiseTextualInfo => GetZPropertyInfo(nameof(HarmonizedBorderWiseTextual));

		internal bool ShowLegalBookLink => CommodityType != CommodityType.RelatedJobLink
											&& !string.IsNullOrEmpty(CCD_HarmonizedCode)
											&& BorderWiseCheckStatus == BorderWiseCheckStatus.Viewable;

		public CommodityType CommodityType { get; set; }

		public override bool CanDelete => base.CanDelete && CommodityType == CommodityType.UserDataEntry;

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("5ac26713-9f69-424a-acab-9367678de568", "Cannot delete the commodity not added by the user.");

		[ReadOnlyMember(nameof(CCD_HarmonizedCode_ReadOnly))]
		public override ZString CCD_Description
		{
			get => base.CCD_Description;
			set
			{
				if (value.Length > CCD_DescriptionInfo.MaxLength)
				{
					value = value.Substring(0, CCD_DescriptionInfo.MaxLength);
				}

				if (value != base.CCD_Description)
				{
					if (CCD_RiskStatus.HasBlockedOrReleased())
					{
						NeedResetStatus = true;
					}
					base.CCD_Description = value;
				}
			}
		}

		[ReadOnlyMember(nameof(CCD_HarmonizedCode_ReadOnly))]
		public override ZString CCD_RN_NKOrigin
		{
			get => base.CCD_RN_NKOrigin;
			set
			{
				if (value != base.CCD_RN_NKOrigin)
				{
					NeedResetStatus = true;
					base.CCD_RN_NKOrigin = value;
				}
			}
		}

		#region BorderWise

		public string[] Origins => OriginOfGoods.ToString().Split([", "], StringSplitOptions.RemoveEmptyEntries).ToArray();

		[ReadOnly(true)]
		public ZString OriginOfGoods { get; set; }

		public bool BorderWiseCheckInProgress { get; set; }

		public BorderWiseCheckStatus BorderWiseCheckStatus { get; set; }

		public bool BlockedByComplianceRule { get; set; }

		public bool IsValidHsCode { get; set; }

		public string MatchedHsCode { get; set; }

		public bool HasMatchedHsCode => !string.IsNullOrEmpty(MatchedHsCode);

		[BusinessObjectTestExclude]
		public (string CountryCode, bool Supported)[] Countries { get; set; }

		public bool AllCountriesUnsupported => Countries?.Length > 0 && Countries.All(u => !u.Supported);

		public bool HasUnsupportedCountries => Countries?.Length > 0 && Countries.Any(u => !u.Supported);

		#endregion

		public override void OnSaving()
		{
			base.OnSaving();

			if (IsComplianceDecisionChanged && CCD_RiskStatus.HasBlockedOrReleased())
			{
				ComplianceRiskStatus?.AddNewComplianceEventLogIfComplianceDecisionChanged();
				IsComplianceDecisionChanged = false;
			}
		}

		#region Validation

		protected override ComplianceCommodityDetailValidation GetNewValidation()
		{
			return new ComplianceCommodityDetailValidationReal(this);
		}

		public new ComplianceCommodityDetailValidationReal Validation
		{
			get { return (ComplianceCommodityDetailValidationReal)base.Validation; }
		}

		#endregion

		public override void Delete()
		{
			if (IsInDatabase && (ComplianceRiskStatus?.IsAssessmentInitialized ?? false))
			{
				var dateTime = (ZDateTime)CCD_SystemCreateTimeUtcInfo.OriginalValue;
				ComplianceRiskStatus.CommodityEventHelper.AddRemovedCommodity(
					Commodity.GetCommodity
					(
						code: (ZString)CCD_HarmonizedCodeInfo.OriginalValue,
						conditions: Conditions,
						hsCodeDescription: Description,
						riskStatus: (ZString)CCD_RiskStatusInfo.OriginalValue,
						nomenclatureCondition: NomenclatureCondition,
						specificCondition: SpecificCondition,
						source: Source,
						commoditySource: CommoditySource,
						notes: (ZString)CCD_AssessmentNotesInfo.OriginalValue,
						originOfGoods: (ZString)CCD_RN_NKOriginInfo.OriginalValue,
						goodsDescription: (ZString)CCD_DescriptionInfo.OriginalValue,
						isAssessmentInitiated: AssessmentInitialized,
						dateAddedUtc: dateTime.IsValid ? dateTime.ToDateTime() : DateTime.MinValue
					));
			}

			base.Delete();
		}

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get
			{
				if (uniqueIndexFailureHandlers == null)
				{
					uniqueIndexFailureHandlers = new List<IUniqueIndexFailureHandler>();
					uniqueIndexFailureHandlers.Add(new ComplianceCommodityDetailUniqueIndexFailureHandler(this));
				}

				return uniqueIndexFailureHandlers;
			}
		}

		List<IUniqueIndexFailureHandler> uniqueIndexFailureHandlers;

		class ComplianceCommodityDetailUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public ComplianceCommodityDetailUniqueIndexFailureHandler(ComplianceCommodityDetail commodityDetail)
			{
				this.CommodityDetail = commodityDetail;
			}
			readonly ComplianceCommodityDetail CommodityDetail;

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return ComplianceCommodityDetailSchema.Constants.Indexes.NR_UX__CCD_COR_ComplianceRisk_CCD_HarmonizedCode_CCD_DescriptionBIN2_CCD_RN_NKOrigin_CCD_CountryOrGrouping; }
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				notifier?.ReportInformation(Res.GetString("CAD8D78B-3800-4030-A5FC-C96F64419A5F", "While you have been working with this form, another user has made changes.\r\n\r\nThe system will now try to combine your changes with those of the other user.\r\nPlease review the form carefully before clicking the 'Save' button again.\r\n\r\nDuplicate Commodity: Harmonized Code('{0}')|Description('{1}')|Origin Of Goods('{2}') must be unique on Compliance Commodity.", CommodityDetail.CCD_HarmonizedCode, CommodityDetail.CCD_Description, CommodityDetail.CCD_RN_NKOrigin), Res.GetString("21C5B348-D9A8-44D0-B9F3-CDA0D30B3FA0", "Duplicate Commodity"));
				AttemptToResolveDuplicateCommodity();
			}

			void AttemptToResolveDuplicateCommodity()
			{
				var complianceRiskStatus = CommodityDetail.ComplianceRiskStatus;
				var complianceCommodityCollection = complianceRiskStatus.CommodityDetailCollection;
				var query = new ZDBOnlyQuery(typeof(ComplianceCommodityDetail));
				query.IgnoreDbQueryCache = true;
				query.AddToFilter(ComplianceCommodityDetailSchema.CCD_COR_ComplianceRisk, complianceRiskStatus.PK);
				query.AddToFilter(ComplianceCommodityDetailSchema.CCD_HarmonizedCode, CommodityDetail.CCD_HarmonizedCode);
				query.AddToFilter(ComplianceCommodityDetailSchema.CCD_Description, CommodityDetail.CCD_Description);
				query.AddToFilter(ComplianceCommodityDetailSchema.CCD_RN_NKOrigin, CommodityDetail.CCD_RN_NKOrigin);
				query.AddToFilter(ComplianceCommodityDetailSchema.CCD_CountryOrGrouping, CommodityDetail.CCD_CountryOrGrouping);
				var duplicateCommodityInDb = complianceRiskStatus.Factory.LoadTop1<ComplianceCommodityDetail>(query);
				if (duplicateCommodityInDb != null)
				{
					duplicateCommodityInDb.CommodityType = CommodityDetail.CommodityType;
					duplicateCommodityInDb.Source = CommodityDetail.Source;
					duplicateCommodityInDb.CommoditySource = CommodityDetail.CommoditySource;
					duplicateCommodityInDb.BorderWiseCheckStatus = CommodityDetail.BorderWiseCheckStatus;
					complianceCommodityCollection.RemoveAndDelete(CommodityDetail);
					complianceCommodityCollection.Add(duplicateCommodityInDb);
				}
			}
		}
	}
}
