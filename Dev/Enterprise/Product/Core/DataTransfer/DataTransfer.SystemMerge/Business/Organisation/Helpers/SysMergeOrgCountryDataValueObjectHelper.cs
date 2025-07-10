using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters
{
	class SysMergeOrgCountryDataValueObjectHelper
	{
		public SysMergeOrgCountryDataValueObjectHelper()
		{
		}

		#region Export

		public void ExportToValueObjectCollection(OrgHeaderForDataTransfer org, Xsd.SysMergeOrgCountryDataCollection xsdOrgCountryDataCollection, INotifications notifications)
		{
			ZQuery query = new ZQuery(OrgCountryDataSchema.OV_OH_OrgHeader, org.PK);
			OrgCountryData[] orgCountryDataArray = org.Factory.Load<OrgCountryData>(query);

			for (int i = 0; i < orgCountryDataArray.Length; i++)
			{
				OrgCountryData orgCountryData = orgCountryDataArray[i];
				Xsd.SysMergeOrgCountryData xsdOrgCountryData = GetExportValueObject(orgCountryData, notifications);

				if (xsdOrgCountryData != null)
				{
					xsdOrgCountryDataCollection.Add(xsdOrgCountryData);
				}
			}
		}

		Xsd.SysMergeOrgCountryData GetExportValueObject(OrgCountryData orgCountryData, INotifications notifications)
		{
			Xsd.SysMergeOrgCountryData xsdOrgCountryData = null;

			if (
				!orgCountryData.PK.IsEmpty &&
				!orgCountryData.OV_OH_OrgHeader.IsEmpty &&
				!orgCountryData.OV_RN_NKClientCountryRelation.IsEmpty)
			{
				RefCountry country = orgCountryData.ClientCountryRelation;
				if (country != null)
				{
					xsdOrgCountryData = new Xsd.SysMergeOrgCountryData();

					xsdOrgCountryData.OH_OrgHeader_PK = orgCountryData.OV_OH_OrgHeader.ToString();
					xsdOrgCountryData.OH_OrgHeader_PKSpecified = true;

					xsdOrgCountryData.RN_ClientCountryRelationship_NK = country.Code;
					xsdOrgCountryData.RN_ClientCountryRelationship_NKSpecified = true;

					PopulateValueObjectDetail(xsdOrgCountryData, orgCountryData, notifications);
				}
			}

			return xsdOrgCountryData;
		}

		void PopulateValueObjectDetail(Xsd.SysMergeOrgCountryData xsdOrgCountryData, OrgCountryData orgCountryData, INotifications notifications)
		{
			if (!orgCountryData.OV_OA_ApprovedLocation.IsEmpty)
			{
				xsdOrgCountryData.OA_ApprovedLocation_PK = orgCountryData.OV_OA_ApprovedLocation.ToString();
				xsdOrgCountryData.OA_ApprovedLocation_PKSpecified = true;
			}

			if (!orgCountryData.OV_ImportEntryPaymentPreference.IsEmpty)
			{
				xsdOrgCountryData.ImportEntryPaymentPreference = orgCountryData.OV_ImportEntryPaymentPreference;
				xsdOrgCountryData.ImportEntryPaymentPreferenceSpecified = true;
			}

			if (!orgCountryData.OV_ImportQuarantinePaymentPreference.IsEmpty)
			{
				xsdOrgCountryData.ImportQuarantinePaymentPreference = orgCountryData.OV_ImportQuarantinePaymentPreference;
				xsdOrgCountryData.ImportQuarantinePaymentPreferenceSpecified = true;
			}

			if (!orgCountryData.OV_EXApprovedOrMajorExporter.IsEmpty)
			{
				xsdOrgCountryData.EXApprovedOrMajorExporter = orgCountryData.OV_EXApprovedOrMajorExporter;
				xsdOrgCountryData.EXApprovedOrMajorExporterSpecified = true;
			}

			if (!orgCountryData.OV_EXApprovalMethod.IsEmpty)
			{
				xsdOrgCountryData.EXApprovalMethod = orgCountryData.OV_EXApprovalMethod;
				xsdOrgCountryData.EXApprovalMethodSpecified = true;
			}

			if (!orgCountryData.OV_EXApprovalNumber.IsEmpty)
			{
				xsdOrgCountryData.EXApprovalNumber = orgCountryData.OV_EXApprovalNumber;
				xsdOrgCountryData.EXApprovalNumberSpecified = true;
			}

			if (!orgCountryData.OV_EXPermitNumber.IsEmpty)
			{
				xsdOrgCountryData.EXPermitNumber = orgCountryData.OV_EXPermitNumber;
				xsdOrgCountryData.EXPermitNumberSpecified = true;
			}

			if (!orgCountryData.OV_EXExportPermissionDetails.IsEmpty)
			{
				xsdOrgCountryData.EXExportPermissionDetails = orgCountryData.OV_EXExportPermissionDetails;
				xsdOrgCountryData.EXExportPermissionDetailsSpecified = true;
			}

			if (!orgCountryData.OV_CustomsEconomicGroupAddInfo.IsEmpty)
			{
				xsdOrgCountryData.CustomsEconomicGroupAddInfo = orgCountryData.OV_CustomsEconomicGroupAddInfo;
				xsdOrgCountryData.CustomsEconomicGroupAddInfoSpecified = true;
			}

			if (!orgCountryData.OV_ImportCustomsDefaultAddInfo.IsEmpty)
			{
				xsdOrgCountryData.ImportCustomsDefaultAddInfo = orgCountryData.OV_ImportCustomsDefaultAddInfo;
				xsdOrgCountryData.ImportCustomsDefaultAddInfoSpecified = true;
			}

			if (!orgCountryData.OV_GS_NKReviewedByUser.IsEmpty)
			{
				xsdOrgCountryData.GS_NKReviewedByUser = orgCountryData.OV_GS_NKReviewedByUser;
				xsdOrgCountryData.GS_NKReviewedByUserSpecified = true;
			}

			if (!orgCountryData.OV_SystemCreateUser.IsEmpty)
			{
				xsdOrgCountryData.SystemCreateUser = orgCountryData.OV_SystemCreateUser;
				xsdOrgCountryData.SystemCreateUserSpecified = true;
			}

			if (!orgCountryData.OV_SystemLastEditUser.IsEmpty)
			{
				xsdOrgCountryData.SystemLastEditUser = orgCountryData.OV_SystemLastEditUser;
				xsdOrgCountryData.SystemLastEditUserSpecified = true;
			}

			if (!orgCountryData.OV_EXE3Signed.IsEmpty)
			{
				xsdOrgCountryData.EXE3Signed = orgCountryData.OV_EXE3Signed;
				xsdOrgCountryData.EXE3SignedSpecified = true;
			}

			if (!orgCountryData.OV_MakePartsBothImportAndExport.IsEmpty)
			{
				xsdOrgCountryData.MakePartsBothImportAndExport = orgCountryData.OV_MakePartsBothImportAndExport;
				xsdOrgCountryData.MakePartsBothImportAndExportSpecified = true;
			}

			// DateTime fields

			if (!orgCountryData.OV_EXSiteInspectionDate.IsEmpty)
			{
				xsdOrgCountryData.EXSiteInspectionDate = orgCountryData.OV_EXSiteInspectionDate.ToDateTime();
				xsdOrgCountryData.EXSiteInspectionDateSpecified = true;
			}

			if (!orgCountryData.OV_LastReviewedOn.IsEmpty)
			{
				xsdOrgCountryData.LastReviewedOn = orgCountryData.OV_LastReviewedOn.ToDateTime();
				xsdOrgCountryData.LastReviewedOnSpecified = true;
			}

			if (!orgCountryData.OV_SystemCreateTimeUtc.IsEmpty)
			{
				xsdOrgCountryData.SystemCreateTime = orgCountryData.OV_SystemCreateTimeUtc.ToDateTime();
				xsdOrgCountryData.SystemCreateTimeSpecified = true;
			}

			if (!orgCountryData.OV_SystemLastEditTimeUtc.IsEmpty)
			{
				xsdOrgCountryData.SystemLastEditTime = orgCountryData.OV_SystemLastEditTimeUtc.ToDateTime();
				xsdOrgCountryData.SystemLastEditTimeSpecified = true;
			}
		}

		#endregion

		#region Import

		public void ImportFromValueObjectCollection(Xsd.SysMergeOrgCountryDataCollection xsdOrgCountryDataCollection, OrgHeaderForDataTransfer org, IValueObjectImportContext context)
		{
			if (xsdOrgCountryDataCollection.IsSpecified)
			{
				for (int i = 0; i < xsdOrgCountryDataCollection.Count; i++)
				{
					Xsd.SysMergeOrgCountryData xsdOrgCountryData = xsdOrgCountryDataCollection[i];

					OrgCountryData orgCountryData = org.Factory.New<OrgCountryData>();
					orgCountryData.OV_OH_OrgHeader = org.PK;
					orgCountryData.OV_RN_NKClientCountryRelation = xsdOrgCountryData.RN_ClientCountryRelationship_NK;

					PopulateBusinessObjectProperties(xsdOrgCountryData, orgCountryData, context);
				}
			}
		}

		public void PopulateBusinessObjectProperties(Xsd.SysMergeOrgCountryData xsdOrgCountryData, OrgCountryData orgCountryData, IValueObjectImportContext context)
		{
			if (!xsdOrgCountryData.OA_ApprovedLocation_PK.IsEmpty)
			{
				orgCountryData.OV_OA_ApprovedLocation = new Guid(xsdOrgCountryData.OA_ApprovedLocation_PK);
			}

			context.SetPropertyInfoValueIfValueNotEmpty(orgCountryData.OV_ImportEntryPaymentPreferenceInfo, xsdOrgCountryData.ImportEntryPaymentPreference);
			context.SetPropertyInfoValueIfValueNotEmpty(orgCountryData.OV_ImportQuarantinePaymentPreferenceInfo, xsdOrgCountryData.ImportQuarantinePaymentPreference);
			context.SetPropertyInfoValueIfValueNotEmpty(orgCountryData.OV_EXApprovedOrMajorExporterInfo, xsdOrgCountryData.EXApprovedOrMajorExporter);
			context.SetPropertyInfoValueIfValueNotEmpty(orgCountryData.OV_EXApprovalMethodInfo, xsdOrgCountryData.EXApprovalMethod);
			context.SetPropertyInfoValueIfValueNotEmpty(orgCountryData.OV_EXApprovalNumberInfo, xsdOrgCountryData.EXApprovalNumber);
			context.SetPropertyInfoValueIfValueNotEmpty(orgCountryData.OV_EXPermitNumberInfo, xsdOrgCountryData.EXPermitNumber);
			context.SetPropertyInfoValueIfValueNotEmpty(orgCountryData.OV_EXExportPermissionDetailsInfo, xsdOrgCountryData.EXExportPermissionDetails);
			context.SetPropertyInfoValueIfValueNotEmpty(orgCountryData.OV_CustomsEconomicGroupAddInfoInfo, xsdOrgCountryData.CustomsEconomicGroupAddInfo);
			context.SetPropertyInfoValueIfValueNotEmpty(orgCountryData.OV_ImportCustomsDefaultAddInfoInfo, xsdOrgCountryData.ImportCustomsDefaultAddInfo);
			context.SetPropertyInfoValueIfValueNotEmpty(orgCountryData.OV_GS_NKReviewedByUserInfo, xsdOrgCountryData.GS_NKReviewedByUser);
			context.SetPropertyInfoValueIfValueNotEmpty(orgCountryData.OV_SystemCreateUserInfo, xsdOrgCountryData.SystemCreateUser);
			context.SetPropertyInfoValueIfValueNotEmpty(orgCountryData.OV_SystemLastEditUserInfo, xsdOrgCountryData.SystemLastEditUser);
			context.SetPropertyInfoValueIfValueNotEmpty(orgCountryData.OV_EXE3SignedInfo, xsdOrgCountryData.EXE3Signed.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(orgCountryData.OV_MakePartsBothImportAndExportInfo, xsdOrgCountryData.MakePartsBothImportAndExport.ToString());

			// DateTime fields

			if (xsdOrgCountryData.EXSiteInspectionDate != null)
			{
				orgCountryData.OV_EXSiteInspectionDate = (ZDateTime)xsdOrgCountryData.EXSiteInspectionDate;
			}

			if (xsdOrgCountryData.LastReviewedOn != null)
			{
				orgCountryData.OV_LastReviewedOn = (ZDateTime)xsdOrgCountryData.LastReviewedOn;
			}

			if (xsdOrgCountryData.SystemCreateTime != null)
			{
				orgCountryData.OV_SystemCreateTimeUtc = (ZDateTime)xsdOrgCountryData.SystemCreateTime;
			}

			if (xsdOrgCountryData.SystemLastEditTime != null)
			{
				orgCountryData.OV_SystemLastEditTimeUtc = (ZDateTime)xsdOrgCountryData.SystemLastEditTime;
			}
		}

		#endregion
	}
}
