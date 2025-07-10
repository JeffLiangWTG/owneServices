using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using static Enterprise.Customs.CA.DataTransfer.Universal.Constants;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using AddInfoGroup = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.AddInfoGroup;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	partial class DeclarationDataObjectReaderTest
	{
		public void TestImportLPCO()
		{
			var lpcoApplicant = CreateOrganisation("applicant", "ABC#@1");
			var lpcoHolder = CreateOrganisation("holder", "ABC#@2");

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var gacIndAddInfo = new AddInfo();
			gacIndAddInfo.Key = "GACInd";
			gacIndAddInfo.Value = YesNoList.Codes.Yes;
			var gacDataObject = new AddInfoGroup();
			gacDataObject.Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.CAGACPGAHeader };

			var lpcoAddInfo = new AddInfoGroup(DefaultDataObjectWriterStrategy.TestInstance);
			lpcoAddInfo.Type = new CodeDescriptionPair() { Code = AddInfoKeys.CusCALPCO.CusAddInfoType };

			gacDataObject.AddInfoGroupCollection = new List<AddInfoGroup>
			{
				lpcoAddInfo
			};
			lpcoAddInfo.AddInfoCollection = new List<AddInfo>
			{
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.Type, Value = "1" },
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.RefNo, Value = "2" },
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.SecondaryRefNo, Value = "3" },
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.DIFRefNumberOrLocation, Value = "4" },
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.LPCOStartDate, Value = "2021-03-23" },
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.LPCOEndDate, Value = "2021-03-25" },
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.LPCOIssueDate, Value = "2021-03-24" },
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.CountryOfIssuance, Value = "CA" },
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.CountryOfOrigin, Value = "US" },
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.AuthorizationCountry, Value = "CN" },
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.IsMixedCountryOfOrigin, Value = "N" },
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.CommodityTypeCode, Value = "5" },
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.Qty, Value = "6" },
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.LPCOHolderType, Value = "OTH" },
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.IsHolderOverridden, Value = "Y" },
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.LPCOHolderName, Value = "1" },
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.AuthorizedPartyContactEmail, Value = "2" },
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.AuthorizedPartyContactName, Value = "3" },
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.AuthorizedPartyContactPhone, Value = "4" },
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.LPCOApplicant, Value = "OTH" },
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.IsApplicantOverridden, Value = "Y" },
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.LPCOApplicantName, Value = "5" },
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.ApplicantContactEmail, Value = "6" },
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.ApplicantContactName, Value = "7" },
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.ApplicantContactPhone, Value = "8" },
				new AddInfo() { Key = AddInfoKeys.CusCALPCO.RN_NKSmeltAndPourCountryCode, Value = "AE" }
			};
			lpcoAddInfo.AddOrgAddress(writeManager, lpcoApplicant, AddressType.LPCOApplicant);
			lpcoAddInfo.AddOrgAddress(writeManager, lpcoHolder, AddressType.LPCOHolder);

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, "B00001001");

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				OwnerRef = "OW234#$#",
				MessageType = new CodeDescriptionPair() { Code = "IMP" },

				CommercialInfo = new CommercialInfo()
				{
					Name = "GROUP",
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							InvoiceNumber = "INV23",
						}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
							new DataObjectList<CommercialInvoiceLine>(new[]
							{
								new CommercialInvoiceLine()
								{
									LineNo = 1,
									AddInfoCollection = new List<AddInfo>(new[]
									{
										gacIndAddInfo
									}),
									AddInfoGroupCollection = new List<AddInfoGroup>(new[]
									{
										gacDataObject
									})
								}
							})))
					})
				}
			};
			declarationDataObject.SetAddInfoGroupCollection(() =>
			{
				var lpcoAddInfoGroup = new AddInfoGroup(DefaultDataObjectWriterStrategy.TestInstance);
				lpcoAddInfoGroup.Type = new CodeDescriptionPair() { Code = AddInfoKeys.CusCALPCO.CusAddInfoType };
				return new List<AddInfoGroup>() { lpcoAddInfoGroup };
			});

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001001";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			var lpco1 = declaration.LPCOViews.AddNew();
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;
			var header = invoiceLine.GACPGAHeader;
			var lpco2 = header.LPCOViews.AddNew();

			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
				ProcessMessage(message, new ServiceTaskLogForTesting());
				declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
				Assert("LPCO should be deleted.", !declaration.LPCOViews.Any());

				AssertEquals("declaration.InvoiceLines.Count", 1, declaration.InvoiceLines.Count);
				invoiceLine = declaration.InvoiceLines[0];
				header = invoiceLine.GACPGAHeader;
				AssertNotNull("GACHeader", header);

				var lpco = header.LPCOViews[0];
				AssertNotEquals("LPCO should be deleted.", lpco2.PK, lpco.PK);
				AssertEquals("CLP_Type", "1", lpco.CLP_Type);
				AssertEquals("CLP_RefNo", "2", lpco.CLP_RefNo);
				AssertEquals("CLP_SecondaryRefNo", "3", lpco.CLP_SecondaryRefNo);
				AssertEquals("CLP_DIFRefNumberOrLocation", "4", lpco.CLP_DIFRefNumberOrLocation);
				AssertEquals("CLP_StartDate", new DateTime(2021, 3, 23), lpco.CLP_StartDate);
				AssertEquals("CLP_EndDate", new DateTime(2021, 3, 25), lpco.CLP_EndDate);
				AssertEquals("CLP_IssueDate", new DateTime(2021, 3, 24), lpco.CLP_IssueDate);
				AssertEquals("CLP_RN_NKIssuanceCountryCode", "CA", lpco.CLP_RN_NKIssuanceCountryCode);
				AssertEquals("CLP_RN_NKOriginCountryCode", "US", lpco.CLP_RN_NKOriginCountryCode);
				AssertEquals("CLP_RN_NKAuthorizationCountry", "CN", lpco.CLP_RN_NKAuthorizationCountry);
				AssertEquals("CLP_IsMixedCountryOfOrigin", false, lpco.CLP_IsMixedCountryOfOrigin);
				AssertEquals("CLP_CommodityTypeCode", "5", lpco.CLP_CommodityTypeCode);
				AssertEquals("CLP_AlternativeQuotaQuantity", (decimal)0, lpco.CLP_AlternativeQuotaQuantity);
				AssertEquals("CLP_AlternativeQuotaUQ", "", lpco.CLP_AlternativeQuotaUQ);
				AssertEquals("CLP_HolderType", "OTH", lpco.CLP_HolderType);
				AssertEquals("CLP_IsHolderOverridden", true, lpco.CLP_IsHolderOverridden);
				AssertEquals("CLP_HolderName", "1", lpco.CLP_HolderName);
				AssertEquals("CLP_HolderContactEmail", "2", lpco.CLP_HolderContactEmail);
				AssertEquals("CLP_HolderContactName", "3", lpco.CLP_HolderContactName);
				AssertEquals("CLP_HolderContactPhone", "4", lpco.CLP_HolderContactPhone);
				AssertEquals("CLP_ApplicantType", "OTH", lpco.CLP_ApplicantType);
				AssertEquals("CLP_IsApplicantOverridden", true, lpco.CLP_IsApplicantOverridden);
				AssertEquals("CLP_ApplicantName", "5", lpco.CLP_ApplicantName);
				AssertEquals("CLP_ApplicantContactEmail", "6", lpco.CLP_ApplicantContactEmail);
				AssertEquals("CLP_ApplicantContactName", "7", lpco.CLP_ApplicantContactName);
				AssertEquals("CLP_ApplicantContactName", "8", lpco.CLP_ApplicantContactPhone);
				AssertEquals("Applicant", lpcoApplicant.MainAddress.PK, lpco.CLP_OA_Applicant);
				AssertEquals("Holder", lpcoHolder.MainAddress.PK, lpco.CLP_OA_Holder);
				AssertEquals("CLP_RN_NKSmeltAndPourCountryCode", "AE", lpco.CLP_RN_NKSmeltAndPourCountryCode);
			});
		}

		public void TestCLP_OA_ApplicantAndHolder()
		{
			var lpcoApplicant = CreateOrganisation("applicant", "ABC#@1");
			var lpcoHolder = CreateOrganisation("holder", "ABC#@2");

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var gacIndAddInfo = new AddInfo();
			gacIndAddInfo.Key = "GACInd";
			gacIndAddInfo.Value = YesNoList.Codes.Yes;
			var gacDataObject = new AddInfoGroup();
			gacDataObject.Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.CAGACPGAHeader };

			var lpcoAddInfo = new AddInfoGroup(DefaultDataObjectWriterStrategy.TestInstance);
			lpcoAddInfo.Type = new CodeDescriptionPair() { Code = AddInfoKeys.CusCALPCO.CusAddInfoType };

			gacDataObject.AddInfoGroupCollection = new List<AddInfoGroup>();
			gacDataObject.AddInfoGroupCollection.Add(lpcoAddInfo);
			lpcoAddInfo.AddInfoCollection = new List<AddInfo>();
			lpcoAddInfo.AddInfoCollection.Add(new AddInfo() { Key = AddInfoKeys.CusCALPCO.Type, Value = "1" });
			lpcoAddInfo.AddInfoCollection.Add(new AddInfo() { Key = AddInfoKeys.CusCALPCO.LPCOHolderType, Value = "IMP" });
			lpcoAddInfo.AddInfoCollection.Add(new AddInfo() { Key = AddInfoKeys.CusCALPCO.LPCOApplicant, Value = "EXP" });
			lpcoAddInfo.AddOrgAddress(writeManager, lpcoApplicant, AddressType.LPCOApplicant);
			lpcoAddInfo.AddOrgAddress(writeManager, lpcoHolder, AddressType.LPCOHolder);

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, "B00001001");

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				OwnerRef = "OW234#$#",
				MessageType = new CodeDescriptionPair() { Code = "IMP" },

				CommercialInfo = new CommercialInfo()
				{
					Name = "GROUP",
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							InvoiceNumber = "INV23",
						}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
							new DataObjectList<CommercialInvoiceLine>(new[]
							{
								new CommercialInvoiceLine()
								{
									LineNo = 1,
									AddInfoCollection = new List<AddInfo>(new[]
									{
										gacIndAddInfo
									}),
									AddInfoGroupCollection = new List<AddInfoGroup>(new[]
									{
										gacDataObject
									})
								}
							})))
					})
				}
			};
			declarationDataObject.SetAddInfoGroupCollection(() =>
			{
				var lpcoAddInfoGroup = new AddInfoGroup(DefaultDataObjectWriterStrategy.TestInstance);
				lpcoAddInfoGroup.Type = new CodeDescriptionPair() { Code = AddInfoKeys.CusCALPCO.CusAddInfoType };
				return new List<AddInfoGroup>() { lpcoAddInfoGroup };
			});

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001001";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			var lpco1 = declaration.LPCOViews.AddNew();
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;
			var header = invoiceLine.GACPGAHeader;

			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
				ProcessMessage(message, new ServiceTaskLogForTesting());
				declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
				Assert("LPCO should be deleted.", !declaration.LPCOViews.Any());

				AssertEquals("declaration.InvoiceLines.Count", 1, declaration.InvoiceLines.Count);
				invoiceLine = declaration.InvoiceLines[0];
				header = invoiceLine.GACPGAHeader;
				AssertNotNull("GACHeader", header);

				var lpco = header.LPCOViews[0];
				AssertEquals("CLP_Type", "1", lpco.CLP_Type);
				AssertEquals("CLP_HolderType", "IMP", lpco.CLP_HolderType);
				AssertEquals("CLP_ApplicantType", "EXP", lpco.CLP_ApplicantType);
				AssertEquals("Applicant", Guid.Empty, lpco.CLP_OA_Applicant);
				AssertEquals("Holder", Guid.Empty, lpco.CLP_OA_Holder);
			});
		}

		public void TestCLP_IsApplicantOverriddenAndIsHolderOverridden()
		{
			var lpcoApplicant = CreateOrganisation("applicant", "ABC#@1");
			var lpcoHolder = CreateOrganisation("holder", "ABC#@2");

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var gacIndAddInfo = new AddInfo();
			gacIndAddInfo.Key = "GACInd";
			gacIndAddInfo.Value = YesNoList.Codes.Yes;
			var gacDataObject = new AddInfoGroup();
			gacDataObject.Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.CAGACPGAHeader };

			var lpcoAddInfo = new AddInfoGroup(DefaultDataObjectWriterStrategy.TestInstance);
			lpcoAddInfo.Type = new CodeDescriptionPair() { Code = AddInfoKeys.CusCALPCO.CusAddInfoType };

			gacDataObject.AddInfoGroupCollection = new List<AddInfoGroup>();
			gacDataObject.AddInfoGroupCollection.Add(lpcoAddInfo);
			lpcoAddInfo.AddInfoCollection = new List<AddInfo>();
			lpcoAddInfo.AddInfoCollection.Add(new AddInfo() { Key = AddInfoKeys.CusCALPCO.Type, Value = "1" });
			lpcoAddInfo.AddInfoCollection.Add(new AddInfo() { Key = AddInfoKeys.CusCALPCO.LPCOHolderType, Value = "OTH" });
			lpcoAddInfo.AddInfoCollection.Add(new AddInfo() { Key = AddInfoKeys.CusCALPCO.LPCOApplicant, Value = "OTH" });
			lpcoAddInfo.AddOrgAddress(writeManager, lpcoApplicant, AddressType.LPCOApplicant);
			lpcoAddInfo.AddOrgAddress(writeManager, lpcoHolder, AddressType.LPCOHolder);
			foreach (var organizationAddress in lpcoAddInfo.OrganizationAddressCollection)
			{
				organizationAddress.AddressOverride = true;
			}

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, "B00001001");

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				OwnerRef = "OW234#$#",
				MessageType = new CodeDescriptionPair() { Code = "IMP" },

				CommercialInfo = new CommercialInfo()
				{
					Name = "GROUP",
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							InvoiceNumber = "INV23",
						}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
							new DataObjectList<CommercialInvoiceLine>(new[]
							{
								new CommercialInvoiceLine()
								{
									LineNo = 1,
									AddInfoCollection = new List<AddInfo>(new[]
									{
										gacIndAddInfo
									}),
									AddInfoGroupCollection = new List<AddInfoGroup>(new[]
									{
										gacDataObject
									})
								}
							})))
					})
				}
			};
			declarationDataObject.SetAddInfoGroupCollection(() =>
			{
				var lpcoAddInfoGroup = new AddInfoGroup(DefaultDataObjectWriterStrategy.TestInstance);
				lpcoAddInfoGroup.Type = new CodeDescriptionPair() { Code = AddInfoKeys.CusCALPCO.CusAddInfoType };
				return new List<AddInfoGroup>() { lpcoAddInfoGroup };
			});

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001001";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			var lpco1 = declaration.LPCOViews.AddNew();
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;
			var header = invoiceLine.GACPGAHeader;

			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
				ProcessMessage(message, new ServiceTaskLogForTesting());
				declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
				Assert("LPCO should be deleted.", !declaration.LPCOViews.Any());

				AssertEquals("declaration.InvoiceLines.Count", 1, declaration.InvoiceLines.Count);
				invoiceLine = declaration.InvoiceLines[0];
				header = invoiceLine.GACPGAHeader;
				AssertNotNull("GACHeader", header);

				var lpco = header.LPCOViews[0];
				AssertEquals("CLP_Type", "1", lpco.CLP_Type);
				AssertEquals("CLP_HolderType", "OTH", lpco.CLP_HolderType);
				AssertEquals("CLP_ApplicantType", "OTH", lpco.CLP_ApplicantType);
				AssertEquals("Applicant", lpcoApplicant.MainAddress.PK, lpco.CLP_OA_Applicant);
				AssertEquals("Holder", lpcoHolder.MainAddress.PK, lpco.CLP_OA_Holder);
				AssertEquals("ApplicantOverridden", true, lpco.CLP_IsApplicantOverridden);
				AssertEquals("HolderOverridden", true, lpco.CLP_IsHolderOverridden);
			});
		}

		public void TestOAFiledShouldNotBePopulatedWhenTypeIsNotOTH()
		{
			var lpcoApplicant = CreateOrganisation("applicant", "ABC#@1");
			var lpcoHolder = CreateOrganisation("holder", "ABC#@2");

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var gacIndAddInfo = new AddInfo();
			gacIndAddInfo.Key = "GACInd";
			gacIndAddInfo.Value = YesNoList.Codes.Yes;
			var gacDataObject = new AddInfoGroup();
			gacDataObject.Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.CAGACPGAHeader };

			var lpcoAddInfo = new AddInfoGroup(DefaultDataObjectWriterStrategy.TestInstance);
			lpcoAddInfo.Type = new CodeDescriptionPair() { Code = AddInfoKeys.CusCALPCO.CusAddInfoType };

			gacDataObject.AddInfoGroupCollection = new List<AddInfoGroup>();
			gacDataObject.AddInfoGroupCollection.Add(lpcoAddInfo);
			lpcoAddInfo.AddInfoCollection = new List<AddInfo>();
			lpcoAddInfo.AddInfoCollection.Add(new AddInfo() { Key = AddInfoKeys.CusCALPCO.Type, Value = "1" });
			lpcoAddInfo.AddInfoCollection.Add(new AddInfo() { Key = AddInfoKeys.CusCALPCO.LPCOHolderType, Value = "IMP" });
			lpcoAddInfo.AddInfoCollection.Add(new AddInfo() { Key = AddInfoKeys.CusCALPCO.LPCOApplicant, Value = "SUP" });
			lpcoAddInfo.AddOrgAddress(writeManager, lpcoApplicant, AddressType.LPCOApplicant);
			lpcoAddInfo.AddOrgAddress(writeManager, lpcoHolder, AddressType.LPCOHolder);

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, "B00001001");

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				OwnerRef = "OW234#$#",
				MessageType = new CodeDescriptionPair() { Code = "IMP" },

				CommercialInfo = new CommercialInfo()
				{
					Name = "GROUP",
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							InvoiceNumber = "INV23",
						}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
							new DataObjectList<CommercialInvoiceLine>(new[]
							{
								new CommercialInvoiceLine()
								{
									LineNo = 1,
									AddInfoCollection = new List<AddInfo>(new[]
									{
										gacIndAddInfo
									}),
									AddInfoGroupCollection = new List<AddInfoGroup>(new[]
									{
										gacDataObject
									})
								}
							})))
					})
				}
			};
			declarationDataObject.SetAddInfoGroupCollection(() =>
			{
				var lpcoAddInfoGroup = new AddInfoGroup(DefaultDataObjectWriterStrategy.TestInstance);
				lpcoAddInfoGroup.Type = new CodeDescriptionPair() { Code = AddInfoKeys.CusCALPCO.CusAddInfoType };
				return new List<AddInfoGroup>() { lpcoAddInfoGroup };
			});

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001001";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			var lpco1 = declaration.LPCOViews.AddNew();
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;
			var header = invoiceLine.GACPGAHeader;

			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
				ProcessMessage(message, new ServiceTaskLogForTesting());
				declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
				Assert("LPCO should be deleted.", !declaration.LPCOViews.Any());

				AssertEquals("declaration.InvoiceLines.Count", 1, declaration.InvoiceLines.Count);
				invoiceLine = declaration.InvoiceLines[0];
				header = invoiceLine.GACPGAHeader;
				AssertNotNull("GACHeader", header);

				var lpco = header.LPCOViews[0];
				AssertEquals("CLP_Type", "1", lpco.CLP_Type);
				AssertEquals("CLP_HolderType", "IMP", lpco.CLP_HolderType);
				AssertEquals("CLP_ApplicantType", "SUP", lpco.CLP_ApplicantType);
				AssertEquals("Applicant", ZGuid.Empty, lpco.CLP_OA_Applicant);
				AssertEquals("Holder", ZGuid.Empty, lpco.CLP_OA_Holder);
			});
		}

		public void TestIsOverriddenWhenTypeIsOTHAndAddressIsNull_IsOverriddenIsTrue()
		{
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var gacIndAddInfo = new AddInfo();
			gacIndAddInfo.Key = "GACInd";
			gacIndAddInfo.Value = YesNoList.Codes.Yes;
			var gacDataObject = new AddInfoGroup();
			gacDataObject.Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.CAGACPGAHeader };

			var lpcoAddInfo = new AddInfoGroup(DefaultDataObjectWriterStrategy.TestInstance);
			lpcoAddInfo.Type = new CodeDescriptionPair() { Code = AddInfoKeys.CusCALPCO.CusAddInfoType };

			gacDataObject.AddInfoGroupCollection = new List<AddInfoGroup>();
			gacDataObject.AddInfoGroupCollection.Add(lpcoAddInfo);
			lpcoAddInfo.AddInfoCollection = new List<AddInfo>();
			lpcoAddInfo.AddInfoCollection.Add(new AddInfo() { Key = AddInfoKeys.CusCALPCO.Type, Value = "1" });
			lpcoAddInfo.AddInfoCollection.Add(new AddInfo() { Key = AddInfoKeys.CusCALPCO.LPCOHolderType, Value = "OTH" });
			lpcoAddInfo.AddInfoCollection.Add(new AddInfo() { Key = AddInfoKeys.CusCALPCO.LPCOApplicant, Value = "OTH" });
			lpcoAddInfo.AddInfoCollection.Add(new AddInfo() { Key = AddInfoKeys.CusCALPCO.IsHolderOverridden, Value = "Y" });
			lpcoAddInfo.AddInfoCollection.Add(new AddInfo() { Key = AddInfoKeys.CusCALPCO.IsApplicantOverridden, Value = "Y" });

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, "B00001001");

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				OwnerRef = "OW234#$#",
				MessageType = new CodeDescriptionPair() { Code = "IMP" },

				CommercialInfo = new CommercialInfo()
				{
					Name = "GROUP",
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							InvoiceNumber = "INV23",
						}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
							new DataObjectList<CommercialInvoiceLine>(new[]
							{
								new CommercialInvoiceLine()
								{
									LineNo = 1,
									AddInfoCollection = new List<AddInfo>(new[]
									{
										gacIndAddInfo
									}),
									AddInfoGroupCollection = new List<AddInfoGroup>(new[]
									{
										gacDataObject
									})
								}
							})))
					})
				}
			};
			declarationDataObject.SetAddInfoGroupCollection(() =>
			{
				var lpcoAddInfoGroup = new AddInfoGroup(DefaultDataObjectWriterStrategy.TestInstance);
				lpcoAddInfoGroup.Type = new CodeDescriptionPair() { Code = AddInfoKeys.CusCALPCO.CusAddInfoType };
				return new List<AddInfoGroup>() { lpcoAddInfoGroup };
			});

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001001";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			var lpco1 = declaration.LPCOViews.AddNew();
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;
			var header = invoiceLine.GACPGAHeader;

			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
				ProcessMessage(message, new ServiceTaskLogForTesting());
				declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
				Assert("LPCO should be deleted.", !declaration.LPCOViews.Any());

				AssertEquals("declaration.InvoiceLines.Count", 1, declaration.InvoiceLines.Count);
				invoiceLine = declaration.InvoiceLines[0];
				header = invoiceLine.GACPGAHeader;
				AssertNotNull("GACHeader", header);

				var lpco = header.LPCOViews[0];
				AssertEquals("CLP_Type", "1", lpco.CLP_Type);
				AssertEquals("CLP_HolderType", "OTH", lpco.CLP_HolderType);
				AssertEquals("CLP_ApplicantType", "OTH", lpco.CLP_ApplicantType);
				AssertEquals("CLP_IsHolderOverridden", true, lpco.CLP_IsHolderOverridden);
				AssertEquals("CLP_IsApplicantOverridden", true, lpco.CLP_IsApplicantOverridden);
				AssertEquals("Applicant", ZGuid.Empty, lpco.CLP_OA_Applicant);
				AssertEquals("Holder", ZGuid.Empty, lpco.CLP_OA_Holder);
			});
		}

		public void TestIsOverriddenWhenTypeIsOTHAndAddressIsNull_IsOverriddenIsFalse()
		{
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var gacIndAddInfo = new AddInfo();
			gacIndAddInfo.Key = "GACInd";
			gacIndAddInfo.Value = YesNoList.Codes.Yes;
			var gacDataObject = new AddInfoGroup();
			gacDataObject.Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.CAGACPGAHeader };

			var lpcoAddInfo = new AddInfoGroup(DefaultDataObjectWriterStrategy.TestInstance);
			lpcoAddInfo.Type = new CodeDescriptionPair() { Code = AddInfoKeys.CusCALPCO.CusAddInfoType };

			gacDataObject.AddInfoGroupCollection = new List<AddInfoGroup>();
			gacDataObject.AddInfoGroupCollection.Add(lpcoAddInfo);
			lpcoAddInfo.AddInfoCollection = new List<AddInfo>();
			lpcoAddInfo.AddInfoCollection.Add(new AddInfo() { Key = AddInfoKeys.CusCALPCO.Type, Value = "1" });
			lpcoAddInfo.AddInfoCollection.Add(new AddInfo() { Key = AddInfoKeys.CusCALPCO.LPCOHolderType, Value = "OTH" });
			lpcoAddInfo.AddInfoCollection.Add(new AddInfo() { Key = AddInfoKeys.CusCALPCO.LPCOApplicant, Value = "OTH" });

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, "B00001001");

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				OwnerRef = "OW234#$#",
				MessageType = new CodeDescriptionPair() { Code = "IMP" },

				CommercialInfo = new CommercialInfo()
				{
					Name = "GROUP",
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							InvoiceNumber = "INV23",
						}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
							new DataObjectList<CommercialInvoiceLine>(new[]
							{
								new CommercialInvoiceLine()
								{
									LineNo = 1,
									AddInfoCollection = new List<AddInfo>(new[]
									{
										gacIndAddInfo
									}),
									AddInfoGroupCollection = new List<AddInfoGroup>(new[]
									{
										gacDataObject
									})
								}
							})))
					})
				}
			};
			declarationDataObject.SetAddInfoGroupCollection(() =>
			{
				var lpcoAddInfoGroup = new AddInfoGroup(DefaultDataObjectWriterStrategy.TestInstance);
				lpcoAddInfoGroup.Type = new CodeDescriptionPair() { Code = AddInfoKeys.CusCALPCO.CusAddInfoType };
				return new List<AddInfoGroup>() { lpcoAddInfoGroup };
			});

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001001";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			var lpco1 = declaration.LPCOViews.AddNew();
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;
			var header = invoiceLine.GACPGAHeader;

			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
				ProcessMessage(message, new ServiceTaskLogForTesting());
				declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
				Assert("LPCO should be deleted.", !declaration.LPCOViews.Any());

				AssertEquals("declaration.InvoiceLines.Count", 1, declaration.InvoiceLines.Count);
				invoiceLine = declaration.InvoiceLines[0];
				header = invoiceLine.GACPGAHeader;
				AssertNotNull("GACHeader", header);

				var lpco = header.LPCOViews[0];
				AssertEquals("CLP_Type", "1", lpco.CLP_Type);
				AssertEquals("CLP_HolderType", "OTH", lpco.CLP_HolderType);
				AssertEquals("CLP_ApplicantType", "OTH", lpco.CLP_ApplicantType);
				AssertEquals("CLP_IsHolderOverridden", false, lpco.CLP_IsHolderOverridden);
				AssertEquals("CLP_IsApplicantOverridden", false, lpco.CLP_IsApplicantOverridden);
				AssertEquals("Applicant", ZGuid.Empty, lpco.CLP_OA_Applicant);
				AssertEquals("Holder", ZGuid.Empty, lpco.CLP_OA_Holder);
			});
		}
	}
}
