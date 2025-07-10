using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;
using UniversalShipment = Enterprise.Customs.DataTransfer.Universal;

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	public class DeclarationDataObjectWriter : UniversalShipment.DeclarationDataObjectWriter, Integration.Customs.CA.IIIDMessagingDeclarationDataObjectWriter
	{
		protected internal DeclarationDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		public bool IsExportingForIIDMessaging { get; set; }

		protected override UniversalShipment.UniversalDataObjectWriterHelper CreateNewUniversalDataObjectWriterHelper(BaseJobDeclaration declarationBO)
		{
			return new UniversalDataObjectWriterHelper(declarationBO as JobDeclaration);
		}

		protected new UniversalDataObjectWriterHelper helper
		{
			get { return (UniversalDataObjectWriterHelper)base.helper; }
		}

		protected override UniversalShipment.CustomsEntryHeaderDataObjectWriter GetNewCustomsEntryHeaderDataObjectWriter()
		{
			return new CustomsEntryHeaderDataObjectWriter(writeManager, helper);
		}

		protected override List<AddInfo> CreateDeclarationAddInfo(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			var addInfoCollection = base.CreateDeclarationAddInfo(declarationBO, declarationData);
			var declaration = (JobDeclaration)declarationBO;
			PopulateFrenchPreferred(declaration, addInfoCollection);
			PopulateExamLocationName(declaration, addInfoCollection);
			PopulatePARSETA(declaration, addInfoCollection);
			return addInfoCollection;
		}

		void PopulateFrenchPreferred(JobDeclaration declaration, List<AddInfo> addInfoCollection)
		{
			if (CACustomsDataRegistry.Instance.FrenchLanguageIndicator.GetFallBackValueAtAllLevels(Guid.Empty, declaration.EffectiveBranch.PK.ToGuid(), Guid.Empty))
			{
				addInfoCollection.AddIfMissing(Constants.AddInfoKeys.FrenchPreferred, new ZString(Constants.AddInfoBoolValues.True));
			}
		}

		void PopulateExamLocationName(JobDeclaration declaration, List<AddInfo> addInfoCollection)
		{
			if (!declaration.CA_ExamLocationName.IsEmpty || !declaration.CA_ExamLocationCode.IsEmpty)
			{
				var examLocationName = declaration.CA_ExamLocationName;
				if (examLocationName.IsEmpty)
				{
					examLocationName = declaration.Lookups.ExamLocationCodes.GetDescriptionFromCode(declaration.CA_ExamLocationCode);
				}
				addInfoCollection.AddIfMissing(Constants.AddInfoKeys.Declaration.ExamLocationName, examLocationName);
			}
		}

		void PopulatePARSETA(JobDeclaration declaration, List<AddInfo> addInfoCollection)
		{
			if (!declaration.JE_DateOfFirstArrival.IsEmpty)
			{
				helper.Update(addInfoCollection, Constants.AddInfoKeys.Declaration.PARSETA, declaration.JE_DateOfFirstArrival);
			}
		}

		protected override UniversalShipment.CommercialInvoiceHeaderDataObjectWriter GetNewCommercialInvoiceHeaderDataObjectWriter(Customs.Business.CusEntryHeader relatedEntry)
		{
			return new CommercialInvoiceHeaderDataObjectWriter(writeManager, helper, landedCostDataWriter, relatedEntry);
		}

		protected override void PopulateExtraOrganisation(BaseJobDeclaration declarationBO, Shipment declarationData, bool keepExistingData)
		{
			base.PopulateExtraOrganisation(declarationBO, declarationData, keepExistingData);

			var declaration = (JobDeclaration)declarationBO;
			declarationData.AddOrgAddress(writeManager, declaration.NotifyParty, Constants.AddressType.MailTo);
			if (declaration.HasInvoiceLinesWithCFIAPGA)
			{
				PopulateCFIAPaymentParty(declarationBO, declarationData);
			}
			PopulateCustomsBroker(declarationBO, declarationData);
		}

		void PopulateCFIAPaymentParty(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			var declaration = (JobDeclaration)declarationBO;
			var branch = declaration.EffectiveBranch;
			var importer = declaration.EffectiveImporter;
			var paymentMethod = declaration.EffectiveCFIAFeePaymentMethod;
			var staff = PGAContactFromRegistryWithFallback(declaration);
			if (paymentMethod == CFIAPaymentMethods.Codes.Broker)
			{
				var branchOrgProxy = branch?.OrgProxy;
				if (branchOrgProxy != null && DoesOrganizationContainCFIAAccountNumber(branchOrgProxy))
				{
					PopulateContactFromPGAStaff(declarationData.AddOrgAddress(writeManager, branchOrgProxy, Constants.AddressType.CFIAAccountOwner), staff);
				}
				else
				{
					var companyOrgProxy = branch?.Company?.OrgProxy;
					if (companyOrgProxy != null && DoesOrganizationContainCFIAAccountNumber(companyOrgProxy))
					{
						PopulateContactFromPGAStaff(declarationData.AddOrgAddress(writeManager, companyOrgProxy, Constants.AddressType.CFIAAccountOwner), staff);
					}
				}
			}
			else if (paymentMethod == CFIAPaymentMethods.Codes.Importer && DoesOrganizationContainCFIAAccountNumber(importer))
			{
				declarationData.AddOrgAddress(writeManager, importer, Constants.AddressType.CFIAAccountOwner);
			}
		}

		void PopulateContactFromPGAStaff(OrganizationAddress cfiaAddress, GlbStaff pgaStaff)
		{
			if (pgaStaff != null && cfiaAddress != null)
			{
				cfiaAddress.Contact = pgaStaff.GS_FullName;
				cfiaAddress.Email = pgaStaff.GS_EmailAddress;
				cfiaAddress.Fax = !pgaStaff.GS_FaxNum.IsEmpty ? pgaStaff.GS_FaxNum : pgaStaff.HomeBranch?.GB_Fax ?? ZString.Empty;
				cfiaAddress.Mobile = pgaStaff.GS_MobilePhone;
				cfiaAddress.Phone = !pgaStaff.GS_WorkPhone.IsEmpty ? pgaStaff.GS_WorkPhone : pgaStaff.HomeBranch?.GB_Phone ?? ZString.Empty;
			}
		}

		ZBool DoesOrganizationContainCFIAAccountNumber(OrgHeader orgHeader)
		{
			var cfiCode = orgHeader == null ? null : orgHeader.CustomsCodes.GetOrgCusCodesForCodeAndCountry(OrgCusCode.CACodeTypes.CFIAAccountNumber, Core.Constants.CountryCodes.Canada);
			return cfiCode != null && cfiCode.Length > 0;
		}

		void PopulateCustomsBroker(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			var declaration = (JobDeclaration)declarationBO;
			var customsBroker = declaration.GetOrgProxyWithCABusinessNumber();
			var registryPGAContact = PGAContactFromRegistryWithFallback(declaration);
			var orgAddress = new BrokerAddressDataObjectWriter(writeManager, Constants.AddressType.CustomsBroker, registryPGAContact ?? declaration.CusAgent).GetDataObject(customsBroker);

			if (orgAddress != null)
			{
				orgAddress.SetRegistrationNumberCollection(() =>
				{
					var numbers = new List<RegistrationNumber>
					{
						new RegistrationNumber
						{
							Type = new RegistrationNumberType { Code = OrgCusCode.CACodeTypes.AccountSecurityCode, Description = "Account Security Code" },
							CountryOfIssue = new Country { Code = Core.Constants.CountryCodes.Canada, Name = "Canada" },
							Value = CACustomsDataRegistry.Instance.AccountSecurityNo.GetFallBackValueAtAllLevels(declaration.EffectiveBranch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty)
						}
					};

					var codeType = OrgCusCode.CACodeTypes.BusinessNumberCustomsBroker;
					var businessNumber = customsBroker.GetCustomsRegNo(codeType);
					if (businessNumber.IsEmpty)
					{
						codeType = OrgCusCode.CACodeTypes.BusinessNumberForImportExport;
						businessNumber = customsBroker.GetCustomsRegNo(codeType);
					}

					if (!businessNumber.IsEmpty)
					{
						numbers.Add(new RegistrationNumber
						{
							Type = new RegistrationNumberType { Code = codeType, Description = "CA Business Number" },
							CountryOfIssue = new Country { Code = Core.Constants.CountryCodes.Canada, Name = "Canada" },
							Value = businessNumber
						});
					}
					return numbers;
				});

				declarationData.AddOrgAddress(orgAddress);
			}
		}

		GlbStaff PGAContactFromRegistryWithFallback(JobDeclaration declaration)
		{
			var defaultBranchPgaContactPk = (ZGuid)CACustomsDataRegistry.Instance.DefaultBranchPgaContact.GetFallBackValueAtAllLevels(declaration.RegistryCompanyPK, declaration.RegistryBranchPK, Guid.Empty);
			return !defaultBranchPgaContactPk.IsEmpty ? declaration.Factory.Load<GlbStaff>(defaultBranchPgaContactPk) : null;
		}

		protected override void PopulateCountrySpecificData(Shipment declarationData, BaseJobDeclaration declarationBO)
		{
			base.PopulateCountrySpecificData(declarationData, declarationBO);
			var declaration = (JobDeclaration)declarationBO;
			var factory = declaration.Factory;
			if (IsExportingForIIDMessaging)
			{
				PopulateAllocatedContacts(factory, (IDataObject)declarationData, declaration);
				PopulateAllocatedContacts(factory, declarationData.CommercialInfo, declaration);
			}
		}

		void PopulateAllocatedContacts(BusinessObjectFactory factory, IDataObject dataObject, JobDeclaration declaration)
		{
			PopulateAllocatedContacts(factory, dataObject as IAddInfoGroupCollectionParent, declaration);
			PopulateAllocatedContacts(factory, dataObject as IOrganizationAddressCollectionParent, declaration);
		}

		void PopulateAllocatedContacts(BusinessObjectFactory factory, CommercialInfo commercialInfo, JobDeclaration declaration)
		{
			var subGroupCollection = commercialInfo?.SubGroupCollection;
			if (subGroupCollection != null)
			{
				foreach (var subCommercialInfo in subGroupCollection)
				{
					PopulateAllocatedContacts(factory, subCommercialInfo, declaration);
				}
			}

			var commercialInvoiceCollection = commercialInfo?.CommercialInvoiceCollection;
			if (commercialInvoiceCollection != null)
			{
				foreach (var commercialInvoice in commercialInvoiceCollection)
				{
					PopulateAllocatedContacts(factory, (IDataObject)commercialInvoice, declaration);
					var commercialInvoiceLineCollection = commercialInvoice?.CommercialInvoiceLineCollection;
					if (commercialInvoiceLineCollection != null)
					{
						foreach (var commercialInvoiceLine in commercialInvoiceLineCollection)
						{
							PopulateAllocatedContacts(factory, (IDataObject)commercialInvoiceLine, declaration);
						}
					}
				}
			}
		}

		void PopulateAllocatedContacts(BusinessObjectFactory factory, IAddInfoGroupCollectionParent parent, JobDeclaration declaration)
		{
			var addInfoGroupCollection = parent?.AddInfoGroupCollection;
			if (addInfoGroupCollection != null)
			{
				foreach (var addInfoGroup in addInfoGroupCollection)
				{
					PopulateAllocatedContacts(factory, (IDataObject)addInfoGroup, declaration);
				}
			}
		}

		void PopulateAllocatedContacts(BusinessObjectFactory factory, IOrganizationAddressCollectionParent parent, JobDeclaration declaration)
		{
			var organizationAddressCollection = parent?.OrganizationAddressCollection;
			if (organizationAddressCollection != null)
			{
				foreach (var organizationAddress in organizationAddressCollection)
				{
					var addressType = organizationAddress.AddressType.GetValueOrDefault();

					var isCustomsBroker = IsCustomsBrokerAddressType(addressType);
					var isCFIAAddressPaidByOtherThanImporter = IsCFIAAccountOwnerAddressPaidByOtherThanImporter(addressType, declaration.EffectiveCFIAFeePaymentMethod);

					if (!isCustomsBroker && !isCFIAAddressPaidByOtherThanImporter)
					{
						var orgCode = (ZString)organizationAddress.OrganizationCode.GetValueOrDefault();
						if (!orgCode.IsEmpty)
						{
							var orgHeader = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, orgCode);
							if (orgHeader != null)
							{
								var pgaContact = orgHeader.ContactsActive.GetContactForAllocation(OrgConstants.ContactAllocationType.CAPGA);
								UpdateOrganizationAddressWithOrgContact(organizationAddress, pgaContact);
							}
						}
					}
				}
			}
		}

		static bool IsCustomsBrokerAddressType(ZString addressType)
		{
			return addressType == Constants.AddressType.CustomsBroker;
		}

		static bool IsCFIAAccountOwnerAddressPaidByOtherThanImporter(ZString addressType, ZString cfiaPayMethod)
		{
			return addressType == Constants.AddressType.CFIAAccountOwner && cfiaPayMethod != CFIAPaymentMethods.Codes.Importer;
		}

		void UpdateOrganizationAddressWithOrgContact(OrganizationAddress organizationAddress, OrgContact orgContact)
		{
			if (organizationAddress != null && orgContact != null)
			{
				organizationAddress.Contact = orgContact.OC_ContactName;
				organizationAddress.Email = orgContact.OC_Email;
				organizationAddress.Fax = orgContact.OC_Fax;
				organizationAddress.Mobile = orgContact.OC_Mobile;
				organizationAddress.Phone = orgContact.OC_Phone;
			}
		}

		protected override void PopulateDeclarationAddInfoFromRealFieldCore(BaseJobDeclaration declarationBO, List<AddInfo> addInfos)
		{
			base.PopulateDeclarationAddInfoFromRealFieldCore(declarationBO, addInfos);

			addInfos.AddOrUpdate(Constants.AddInfoKeys.Declaration.PortOfClearance, declarationBO.JE_CustomsOffice);
			addInfos.AddIfMissing(Constants.AddInfoKeys.Declaration.SubLocationCode, declarationBO.JE_LocationOfGoods);
			addInfos.AddOrUpdate(Constants.AddInfoKeys.Declaration.CarrierCode, declarationBO.JE_CarrierCode);
		}

		protected sealed override void AddTableFetchHintCreators(IExternalFetchHintSupporter externalFetchHintSupporter)
		{
			base.AddTableFetchHintCreators(externalFetchHintSupporter);
			var declarationPK = DeclarationRowForFetchHint.GetValue(JobDeclarationSchema.PK);
			externalFetchHintSupporter.AddFetchHint(new FetchHint(CusAddInfoSchema.B7_ParentID, declarationPK));
			externalFetchHintSupporter.AddFetchHint(new FetchHint(CusCodeDataSchema.CY_ParentID, declarationPK));
			externalFetchHintSupporter.AddFetchHint(new FetchHint(CusEntryNumSchema.CE_ParentID, declarationPK));
			externalFetchHintSupporter.AddTableFetchHintCreator(CusAddInfoSchema.Instance, GetCusAddInfoRelatedFetchHints);
		}

		protected virtual IEnumerable<IFetchHint> GetCusAddInfoRelatedFetchHints(IColumnIndexer row)
		{
			var addInfoPK = row.GetValue(CusAddInfoSchema.PK);
			yield return new FetchHint(CusAddInfoSchema.B7_ParentID, addInfoPK);
			yield return new FetchHint(CusCodeDataSchema.CY_ParentID, addInfoPK);
		}

		protected override IEnumerable<IFetchHint> GetCusEntryHeaderRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var fetchHint in base.GetCusEntryHeaderRelatedFetchHints(row))
			{
				yield return fetchHint;
			}
			var entryHeaderPK = row.GetValue(CusEntryHeaderSchema.PK);
			yield return new FetchHint(CusAddInfoSchema.B7_ParentID, entryHeaderPK);
			yield return new FetchHint(CusCodeDataSchema.CY_ParentID, entryHeaderPK);
		}

		protected override IEnumerable<IFetchHint> GetCusEntryLineRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var fetchHint in base.GetCusEntryLineRelatedFetchHints(row))
			{
				yield return fetchHint;
			}
			var entryLinePK = row.GetValue(CusEntryLineSchema.PK);
			yield return new FetchHint(CusUnderbondDecSchema.BU_CL, entryLinePK);
		}

		protected override IEnumerable<IFetchHint> GetCusDecHouseBillRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var fetchHint in base.GetCusDecHouseBillRelatedFetchHints(row))
			{
				yield return fetchHint;
			}
			var billPK = row.GetValue(CusDecHouseBillSchema.PK);
			yield return new FetchHint(CusAddInfoSchema.B7_ParentID, billPK);
		}

		protected override IEnumerable<IFetchHint> GetJobComInvoiceHeaderRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var fetchHint in base.GetJobComInvoiceHeaderRelatedFetchHints(row))
			{
				yield return fetchHint;
			}
			var invoicePK = row.GetValue(JobComInvoiceHeaderSchema.PK);
			yield return new FetchHint(CusAddInfoSchema.B7_ParentID, invoicePK);
			yield return new FetchHint(JobDocAddressSchema.E2_ParentID, invoicePK);
			yield return new FetchHint(JobComInvoiceHeaderRefsSchema.J2_JZ, invoicePK);
		}

		protected override IEnumerable<IFetchHint> GetJobComInvoiceLineRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var fetchHint in base.GetJobComInvoiceLineRelatedFetchHints(row))
			{
				yield return fetchHint;
			}
			var invoiceLinePK = row.GetValue(JobComInvoiceLineSchema.PK);
			yield return new FetchHint(CusAddInfoSchema.B7_ParentID, invoiceLinePK);
			yield return new FetchHint(CusCodeDataSchema.CY_ParentID, invoiceLinePK);

			var fetchHint1 = GetFetchHintIfNotEmpty(CACClassSchema.CT_Tariff, row, JobComInvoiceLineSchema.JI_Tariff);
			if (fetchHint1 != null)
			{
				yield return fetchHint1;
			}
		}
	}
}
