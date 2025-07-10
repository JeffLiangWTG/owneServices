using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Common.CA;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	partial class DeclarationDataObjectWriterTest
	{
		public void TestAdditionalAddInfoGroupCollectionDataObjectWriterForILPCOCollectionParent()
		{
			var org1 = CreateOrganisation("ORG1", "ABC#@1");
			var address1 = org1.MainAddress;
			address1.OA_CompanyNameOverride = "111";
			address1.OA_Email = "222@wisetechglobal.com";
			address1.OA_Phone = "333";
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "444";
			var allocation1 = contact1.Allocations.AddNew();
			allocation1.PC_Type = OrgConstants.ContactAllocationType.CAPGA;
			var org2 = CreateOrganisation("ORG2", "ABC#@2");
			var address2 = org2.MainAddress;
			address2.OA_CompanyNameOverride = "666";
			address2.OA_Email = "777@wisetechglobal.com";
			address2.OA_Phone = "888";
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "999";
			var allocation2 = contact2.Allocations.AddNew();
			allocation2.PC_Type = OrgConstants.ContactAllocationType.CAPGA;
			Factory.SaveForTesting();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001001";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.JE_MessageType = CAJobMessageTypeList.Codes.Import;
			var lpco1 = declaration.LPCOViews.AddNew();
			lpco1.CLP_Type = "1";
			lpco1.CLP_RefNo = "2";
			lpco1.CLP_SecondaryRefNo = "3";
			lpco1.CLP_DIFRefNumberOrLocation = "4";
			lpco1.CLP_StartDate = new ZDate(2021, 3, 23);
			lpco1.CLP_EndDate = new ZDate(2021, 3, 25);
			lpco1.CLP_IssueDate = new ZDate(2021, 3, 24);
			lpco1.CLP_RN_NKIssuanceCountryCode = "CA";
			lpco1.CLP_RN_NKOriginCountryCode = "US";
			lpco1.CLP_RN_NKAuthorizationCountry = "CN";
			lpco1.CLP_IsMixedCountryOfOrigin = true;
			lpco1.CLP_CommodityTypeCode = "5";
			lpco1.CLP_AlternativeQuotaQuantity = 6;
			lpco1.CLP_AlternativeQuotaUQ = "KG";
			lpco1.CLP_HolderType = LPCOHolderPartyTypeCodes.Codes.Other;
			lpco1.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Other;
			lpco1.LPCOHolderOrgPK = org1.PK;
			lpco1.LPCOApplicantOrgPK = org2.PK;
			lpco1.CLP_IsHolderOverridden = true;
			lpco1.CLP_HolderName = "7";
			lpco1.CLP_HolderContactName = "8";
			lpco1.CLP_HolderContactEmail = "9@wisetechglobal.com";
			lpco1.CLP_HolderContactPhone = "10";
			lpco1.CLP_IsApplicantOverridden = true;
			lpco1.CLP_ApplicantName = "11";
			lpco1.CLP_ApplicantContactName = "12";
			lpco1.CLP_ApplicantContactEmail = "13@wisetechglobal.com";
			lpco1.CLP_ApplicantContactPhone = "14";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;
			var header = invoiceLine.GACPGAHeader;
			var lpco2 = header.LPCOViews.AddNew();
			lpco2.CLP_Type = "21";
			lpco2.CLP_RefNo = "22";
			lpco2.CLP_SecondaryRefNo = "23";
			lpco2.CLP_DIFRefNumberOrLocation = "24";
			lpco2.CLP_StartDate = new ZDate(2021, 3, 23);
			lpco2.CLP_EndDate = new ZDate(2021, 3, 25);
			lpco2.CLP_IssueDate = new ZDate(2021, 3, 24);
			lpco2.CLP_RN_NKIssuanceCountryCode = "CA";
			lpco2.CLP_RN_NKOriginCountryCode = "US";
			lpco2.CLP_RN_NKAuthorizationCountry = "CN";
			lpco2.CLP_IsMixedCountryOfOrigin = true;
			lpco2.CLP_CommodityTypeCode = "25";
			lpco2.CLP_AlternativeQuotaQuantity = 26;
			lpco2.CLP_AlternativeQuotaUQ = "KG";
			lpco2.CLP_HolderType = LPCOHolderPartyTypeCodes.Codes.Other;
			lpco2.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Other;
			lpco2.LPCOHolderOrgPK = org1.PK;
			lpco2.LPCOApplicantOrgPK = org2.PK;
			lpco2.CLP_RN_NKSmeltAndPourCountryCode = "AE";

			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LineNo = 2;
			invoiceLine1.CA_GACInd = YesNoList.Codes.Yes;
			var header1 = invoiceLine1.GACPGAHeader;
			var lpco3 = header1.LPCOViews.AddNew();

			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			CombineAssertions(() =>
			{
				var lpcoAddInfoGroup = declarationData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.Code.GetValueOrDefault() == Constants.AddInfoKeys.CusCALPCO.CusAddInfoType);
				AssertEquals("should have 24 AddInfo", 26, lpcoAddInfoGroup.AddInfoCollection.Count);
				foreach (var addInfo in lpcoAddInfoGroup.AddInfoCollection)
				{
					switch (addInfo.Key)
					{
						case Constants.AddInfoKeys.CusCALPCO.Type:
							AssertEquals("CLP_Type", "1", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.RefNo:
							AssertEquals("CLP_RefNo", "2", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.SecondaryRefNo:
							AssertEquals("CLP_SecondaryRefNo", "3", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.DIFRefNumberOrLocation:
							AssertEquals("CLP_DIFRefNumberOrLocation", "4", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.LPCOStartDate:
							AssertEquals("CLP_StartDate", "2021-03-23 00:00:00.000", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.LPCOEndDate:
							AssertEquals("CLP_EndDate", "2021-03-25 00:00:00.000", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.LPCOIssueDate:
							AssertEquals("CLP_IssueDate", "2021-03-24 00:00:00.000", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.CountryOfIssuance:
							AssertEquals("CLP_RN_NKIssuanceCountryCode", "CA", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.CountryOfOrigin:
							AssertEquals("CLP_RN_NKOriginCountryCode", "US", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.AuthorizationCountry:
							AssertEquals("CLP_RN_NKAuthorizationCountry", "CN", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.IsMixedCountryOfOrigin:
							AssertEquals("CLP_IsMixedCountryOfOrigin", "Y", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.CommodityTypeCode:
							AssertEquals("CLP_CommodityTypeCode", "5", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.Qty:
							AssertEquals("CLP_AlternativeQuotaQuantity", "6", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.UQ:
							AssertEquals("CLP_AlternativeQuotaUQ", "KG", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.LPCOHolderType:
							AssertEquals("CLP_HolderType", "OTH", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.LPCOApplicant:
							AssertEquals("CLP_ApplicantType", "OTH", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.LPCOHolderName:
							AssertEquals("CLP_HolderName", "7", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.AuthorizedPartyContactName:
							AssertEquals("CLP_HolderContactName", "8", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.AuthorizedPartyContactEmail:
							AssertEquals("CLP_HolderContactEmail", "9@wisetechglobal.com", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.AuthorizedPartyContactPhone:
							AssertEquals("CLP_HolderContactPhone", "10", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.LPCOApplicantName:
							AssertEquals("CLP_ApplicantName", "11", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.ApplicantContactName:
							AssertEquals("CLP_ApplicantContactName", "12", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.ApplicantContactEmail:
							AssertEquals("CLP_ApplicantContactEmail", "13@wisetechglobal.com", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.ApplicantContactPhone:
							AssertEquals("CLP_ApplicantContactPhone", "14", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.IsHolderOverridden:
							AssertEquals("CLP_IsHolderOverridden", "Y", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.IsApplicantOverridden:
							AssertEquals("CLP_IsApplicantOverridden", "Y", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.RN_NKSmeltAndPourCountryCode:
							AssertEquals("RN_NKSmeltAndPourCountryCode", "AE", addInfo.Value);
							break;
					}
				}

				var holder1 = lpcoAddInfoGroup.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.Equals(Constants.AddressType.LPCOHolder));
				AssertEquals("CompanyName", "111", holder1.CompanyName);
				AssertEquals("AddressOverride", ZBool.True, holder1.AddressOverride);
				AssertEquals("Email", "222@wisetechglobal.com", holder1.Email);
				AssertEquals("Phone", "333", holder1.Phone);
				var applicant1 = lpcoAddInfoGroup.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.Equals(Constants.AddressType.LPCOApplicant));
				AssertEquals("CompanyName", "666", applicant1.CompanyName);
				AssertEquals("AddressOverride", ZBool.True, applicant1.AddressOverride);
				AssertEquals("Email", "777@wisetechglobal.com", applicant1.Email);
				AssertEquals("Phone", "888", applicant1.Phone);
			});

			CombineAssertions(() =>
			{
				var lpcoAddInfoGroup = declarationData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection.FirstOrDefault(x => x.LineNo == 1).
				AddInfoGroupCollection.FirstOrDefault(group => group.Type.Code.GetValueOrDefault() == CusAddInfoTypeAttribute.Codes.CAGACPGAHeader).
				AddInfoGroupCollection.FirstOrDefault(group => group.Type.Code.GetValueOrDefault() == Constants.AddInfoKeys.CusCALPCO.CusAddInfoType);
				AssertEquals("should have 25 AddInfo", 25, lpcoAddInfoGroup.AddInfoCollection.Count);
				foreach (var addInfo in lpcoAddInfoGroup.AddInfoCollection)
				{
					switch (addInfo.Key)
					{
						case Constants.AddInfoKeys.CusCALPCO.Type:
							AssertEquals("CLP_Type", "21", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.RefNo:
							AssertEquals("CLP_RefNo", "22", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.SecondaryRefNo:
							AssertEquals("CLP_SecondaryRefNo", "23", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.DIFRefNumberOrLocation:
							AssertEquals("CLP_DIFRefNumberOrLocation", "24", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.LPCOStartDate:
							AssertEquals("CLP_StartDate", "2021-03-23 00:00:00.000", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.LPCOEndDate:
							AssertEquals("CLP_EndDate", "2021-03-25 00:00:00.000", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.LPCOIssueDate:
							AssertEquals("CLP_IssueDate", "2021-03-24 00:00:00.000", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.CountryOfIssuance:
							AssertEquals("CLP_RN_NKIssuanceCountryCode", "CA", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.CountryOfOrigin:
							AssertEquals("CLP_RN_NKOriginCountryCode", "US", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.AuthorizationCountry:
							AssertEquals("CLP_RN_NKAuthorizationCountry", "CN", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.IsMixedCountryOfOrigin:
							AssertEquals("CLP_IsMixedCountryOfOrigin", "Y", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.CommodityTypeCode:
							AssertEquals("CLP_CommodityTypeCode", "25", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.Qty:
							AssertEquals("CLP_AlternativeQuotaQuantity", "26", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.UQ:
							AssertEquals("CLP_AlternativeQuotaUQ", "KG", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.LPCOHolderType:
							AssertEquals("CLP_HolderType", "OTH", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.LPCOApplicant:
							AssertEquals("CLP_ApplicantType", "OTH", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.LPCOHolderName:
							AssertEquals("CLP_HolderName", "111", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.AuthorizedPartyContactName:
							AssertEquals("CLP_HolderContactName", "444", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.AuthorizedPartyContactEmail:
							AssertEquals("CLP_HolderContactEmail", "222@wisetechglobal.com", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.AuthorizedPartyContactPhone:
							AssertEquals("CLP_HolderContactPhone", "333", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.LPCOApplicantName:
							AssertEquals("CLP_ApplicantName", "666", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.ApplicantContactName:
							AssertEquals("CLP_ApplicantContactName", "999", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.ApplicantContactEmail:
							AssertEquals("CLP_ApplicantContactEmail", "777@wisetechglobal.com", addInfo.Value);
							break;
						case Constants.AddInfoKeys.CusCALPCO.ApplicantContactPhone:
							AssertEquals("CLP_ApplicantContactPhone", "888", addInfo.Value);
							break;
					}
				}

				var holder = lpcoAddInfoGroup.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.Equals(Constants.AddressType.LPCOHolder));
				AssertEquals("CompanyName", "111", holder.CompanyName);
				AssertEquals("AddressOverride", ZBool.False, holder.AddressOverride);
				AssertEquals("Email", "222@wisetechglobal.com", holder.Email);
				AssertEquals("Phone", "333", holder.Phone);
				var applicant = lpcoAddInfoGroup.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.Equals(Constants.AddressType.LPCOApplicant));
				AssertEquals("CompanyName", "666", applicant.CompanyName);
				AssertEquals("AddressOverride", ZBool.False, applicant.AddressOverride);
				AssertEquals("Email", "777@wisetechglobal.com", applicant.Email);
				AssertEquals("Phone", "888", applicant.Phone);
			});

			var lpcoAddInfoGroup1 = declarationData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection.FirstOrDefault(x => x.LineNo == 2).
			AddInfoGroupCollection.FirstOrDefault(group => group.Type.Code.GetValueOrDefault() == CusAddInfoTypeAttribute.Codes.CAGACPGAHeader).
			AddInfoGroupCollection.FirstOrDefault(group => group.Type.Code.GetValueOrDefault() == Constants.AddInfoKeys.CusCALPCO.CusAddInfoType);
			AssertNull("should have 0 AddInfo", lpcoAddInfoGroup1.AddInfoCollection);
		}
	}
}
