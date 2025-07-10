using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED801ConsigneeMessageProcessor : EmcsMessageProcessor<EmcsInboundEDIMessage<IED801>, IED801>
	{
		public ED801ConsigneeMessageProcessor(LoggingInformation logger)
		: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("8F6E32DD-17EE-41B0-BC12-1BC9D44763BE", "EMCS ED801 Consignee Message Processor");

		protected override bool MustHaveLinkedObject => false;

		protected override BusinessObject GetLinkedObject(EmcsInboundEDIMessage<IED801> message)
		{
			BusinessObject result = null;
			var provider = message.DataProvider;
			if (provider != null)
			{
				result = GetDeclarationFromEADNumber(message, provider.ExciseMovement.AdministrativeReferenceCode, provider.MessageGroup, provider.ExciseMovement.SequenceNumber);
			}
			return result;
		}

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EmcsInboundEDIMessage<IED801> message)
		{
			var provider = message.DataProvider;
			if (provider != null)
			{
				messageGroup = provider.MessageGroup;
				var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
				if (emcsDeclaration == null)
				{
					emcsDeclaration = factory.New<EMCSJobDeclaration>();
					message.EM_LinkedObject = emcsDeclaration;
					emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
				}
				emcsDeclaration.JE_MessageStatus = EDIMessage.Status.Received;
				emcsDeclaration.JE_EntryStatus = EntryStatusList.Codes.REG;
				message.EM_Status = EDIMessage.Status.ProcessedOK;

				UpdateDeclarationFromMessage(factory, provider, emcsDeclaration);

				message.SetLogbookLocalReferenceNumber(provider.LocalReferenceNumber);
				message.SetLogbookRegistrationNumber(provider.ExciseMovement.AdministrativeReferenceCode);

				emcsDeclaration.OnSuccessfulSaveDo(declaration =>
				{
					GenerateHtmlEmailAndSendToOriginalOrGroup(factory
						, declaration
						, Res.GetString("D1CDF730-BE96-4090-B8D6-A963454B5F38", "Incoming EMCS e-AD")
						, GetEmailBodyHeader(declaration, provider)
						, false
						, message.Branch
						, declaration
						, () => declaration.Messages.LastOutgoingMessage);
				});
			}
		}

		void UpdateDeclarationFromMessage(BusinessObjectFactory factory, IED801 provider, EMCSJobDeclaration emcsDeclaration)
		{
			emcsDeclaration.ZG_JourneyTime = provider.JourneyTime;
			emcsDeclaration.JE_MessageSubType = provider.DestinationTypeCode;
			emcsDeclaration.ZG_TransportArrangement = provider.TransportArrangement;
			emcsDeclaration.JE_DateAtOrigin = provider.DispatchTime;
			emcsDeclaration.ZG_OriginType = provider.OriginTypeCode;
			emcsDeclaration.JE_OwnerRef = provider.LocalReferenceNumber;
			emcsDeclaration.InvoiceNumber = provider.InvoiceNumber;
			emcsDeclaration.InvoiceDate = provider.InvoiceDate;
			emcsDeclaration.ZG_CCTMSA = provider.MemberStateCode;
			emcsDeclaration.ZG_CertOfExemption = provider.CertificateOfExemption;

			emcsDeclaration.ZG_GuarantorType = provider.GuarantorTypeCode;
			emcsDeclaration.JE_TransportMode = new EU.EMCS.Business.TransportModeTranslator().TranslateToCargoWiseCode(provider.TransportModeCode);
			emcsDeclaration.SpecialInstructions = provider.ComplementaryInfo;

			CreateCustomsOffices(emcsDeclaration, provider);
			UpdateOrCreateEADNumber(emcsDeclaration, provider);
			CreateImportSADNumbers(emcsDeclaration, provider);
			CreateDocumentCertificates(emcsDeclaration, provider);

			emcsDeclaration.CreateOrUpdateGuarantor(provider.Guarantor);
			CreateConsignee(factory, emcsDeclaration, provider.Consignee);
			CreateConsignor(factory, emcsDeclaration, provider.Consignor);
			CreatePlaceOfDispatch(factory, emcsDeclaration, provider.PlaceOfDispatch);
			emcsDeclaration.CreateOrUpdateDeliveryPlace(provider.DeliveryPlace);
			emcsDeclaration.CreateOrUpdateCarrierAgent(provider.TransportArranger);
			emcsDeclaration.CreateOrUpdateTransporter(provider.FirstTransporter);
			emcsDeclaration.CreateOrUpdateTransportDetails(provider.TransportDetails);
			CreateInvoiceLinesAndPackages(emcsDeclaration, provider.Lines);
			LinkLinePackagePivots(emcsDeclaration, provider.Lines);
		}

		void CreateCustomsOffices(EMCSJobDeclaration emcsDeclaration, IED801 provider)
		{
			emcsDeclaration.CustomsOffices.RemoveAndDeleteAll();
			UpdateOrCreateCustomsOffice(emcsDeclaration, OfficeCodes_EMCS.Codes.CompetentAuthorityOfArrival, provider.MessageSender);

			if (!provider.DispatchImportOffice.IsEmpty)
			{
				UpdateOrCreateCustomsOffice(emcsDeclaration, EuOfficeCodesTypes.Codes.OfficeOfDispatch, provider.DispatchImportOffice);
			}
			if (!provider.DeliveryPlaceCustomsOffice.IsEmpty)
			{
				UpdateOrCreateCustomsOffice(emcsDeclaration, EuOfficeCodesTypes.Codes.OfficeOfDelivery, provider.DeliveryPlaceCustomsOffice);
			}
			if (!provider.CompetentAuthorityDispatchOffice.IsEmpty)
			{
				UpdateOrCreateCustomsOffice(emcsDeclaration, EuOfficeCodesTypes.Codes.CompetentAuthorityOfDispatch, provider.CompetentAuthorityDispatchOffice);
			}
		}

		void UpdateOrCreateCustomsOffice(EMCSJobDeclaration emcsDeclaration, ZString officeType, ZString office)
		{
			var customsOffice = emcsDeclaration.CustomsOffices.Cast<EMCSOfficeCode>().SingleOrDefault(x => x.CY_Code == officeType) ?? emcsDeclaration.CustomsOffices.AddNew();
			customsOffice.CY_Code = officeType;
			customsOffice.CY_Data = office;
		}

		void CreateImportSADNumbers(EMCSJobDeclaration emcsDeclaration, IED801 provider)
		{
			emcsDeclaration.ImportSADNumbers.RemoveAndDeleteAll();
			foreach (var sadNumber in provider.ImportSadNumbers)
			{
				var newSadNumber = emcsDeclaration.ImportSADNumbers.AddNew();
				newSadNumber.CSI_Description = sadNumber;
			}
		}

		void UpdateOrCreateEADNumber(EMCSJobDeclaration emcsDeclaration, IED801 provider)
		{
			var entryNumber = emcsDeclaration.LoadCusEntryNumber(true);
			entryNumber.CE_EntryNum = provider.ExciseMovement.AdministrativeReferenceCode;
			entryNumber.CE_EntryLineReference = provider.ExciseMovement.SequenceNumber;
			entryNumber.CE_IssueDate = provider.DateAndTimeOfValidationOfEadEsad;
		}
		void CreateConsignee(BusinessObjectFactory factory, EMCSJobDeclaration emcsDeclaration, IEMCSPartyConsignee consigneeInMessage)
		{
			emcsDeclaration.ImporterDocumentaryAddress.Delete();
			if (consigneeInMessage != null)
			{
				var consignee = emcsDeclaration.ImporterDocumentaryAddress;
				var traderId = consigneeInMessage.TraderId;
				var orgHeader = factory.GetOrgHeaderByCustomsRegNo(consigneeInMessage.Country, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, traderId);
				if (orgHeader != null)
				{
					consignee.OrganisationPK = orgHeader.PK;
				}
				else
				{
					consignee.E2_AddressOverride = true;
					if (!traderId.IsEmpty())
					{
						consignee.E2_GovRegNum = traderId;
						consignee.E2_GovRegNumType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber;
					}
					UpdatePartyAddress(consignee, consigneeInMessage);
				}
			}
		}

		void CreateConsignor(BusinessObjectFactory factory, EMCSJobDeclaration emcsDeclaration, IEMCSPartyConsignor consignorInMessage)
		{
			emcsDeclaration.SupplierDocumentaryAddress.Delete();
			if (consignorInMessage != null)
			{
				var consignor = emcsDeclaration.SupplierDocumentaryAddress;
				var traderExciseNumber = consignorInMessage.TraderExciseNumber;
				var orgHeader = factory.GetOrgHeaderByCustomsRegNo(consignorInMessage.Country, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, traderExciseNumber);
				if (orgHeader != null)
				{
					consignor.OrganisationPK = orgHeader.PK;
				}
				else
				{
					consignor.E2_AddressOverride = true;
					if (!traderExciseNumber.IsEmpty())
					{
						consignor.E2_GovRegNum = traderExciseNumber;
						consignor.E2_GovRegNumType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber;
					}
					UpdatePartyAddress(consignor, consignorInMessage);
				}
			}
		}

		void CreatePlaceOfDispatch(BusinessObjectFactory factory, EMCSJobDeclaration emcsDeclaration, IEMCSPartyPlaceOfDispatch placeOfDispatchInMessage)
		{
			emcsDeclaration.DispatchWarehouseDocumentaryAddress.Delete();
			if (placeOfDispatchInMessage != null)
			{
				var dispatchWarehouse = emcsDeclaration.DispatchWarehouseDocumentaryAddress;
				var referenceOfTaxWarehouse = placeOfDispatchInMessage.ReferenceOfTaxWarehouse;
				var orgAddress = factory.GetOrgAddressByCustomsRegNo(placeOfDispatchInMessage.Country, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, referenceOfTaxWarehouse);
				if (orgAddress != null)
				{
					dispatchWarehouse.OrganisationPK = orgAddress.OA_OH;
					dispatchWarehouse.E2_OA_Address = orgAddress.PK;
				}
				else
				{
					dispatchWarehouse.E2_AddressOverride = true;
					if (!referenceOfTaxWarehouse.IsEmpty())
					{
						dispatchWarehouse.E2_GovRegNum = referenceOfTaxWarehouse;
						dispatchWarehouse.E2_GovRegNumType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID;
					}
					UpdatePartyAddress(dispatchWarehouse, placeOfDispatchInMessage);
				}
			}
		}

		void UpdatePartyAddress(JobDocAddress jobDocAddress, IEMCSPartyAddress addressInMessage)
		{
			jobDocAddress.E2_RN_NKCountryCode = addressInMessage.Language;
			jobDocAddress.E2_City = addressInMessage.City;
			jobDocAddress.E2_Address1 = addressInMessage.Address;
			jobDocAddress.E2_Postcode = addressInMessage.Postcode;
			jobDocAddress.E2_CompanyName = addressInMessage.Name;
		}

		void CreateDocumentCertificates(EMCSJobDeclaration emcsDeclaration, IED801 provider)
		{
			emcsDeclaration.Documents.RemoveAndDeleteAll();
			foreach (var certificate in provider.DocumentCertificates)
			{
				var newCertificate = emcsDeclaration.Documents.AddNew();
				newCertificate.CSI_ReferenceNumber = certificate.Reference;
				newCertificate.CSI_Description = certificate.Description;
				newCertificate.CSI_SubType = certificate.Type;
			}
		}

		void CreateInvoiceLinesAndPackages(EMCSJobDeclaration emcsDeclaration, IReadOnlyCollection<IED801Line> lines)
		{
			emcsDeclaration.EMCSPackages.RemoveAndDeleteAll();
			emcsDeclaration.FilteredInvoiceLines.RemoveAndDeleteAll();
			foreach (var line in lines.OrderBy(x => x.BodyRecordUniqueReference))
			{
				var newInvoiceLine = emcsDeclaration.InvoiceHeader.InvoiceLines.AddNew();
				newInvoiceLine.ZG_ExciseProductCode = line.ExciseProductCode;
				newInvoiceLine.JI_Tariff = line.CnCode;
				newInvoiceLine.ZG_FiscalMarkUsed = line.FiscalMarkUsedFlag;
				newInvoiceLine.ZG_FiscalMark = line.FiscalMark;
				newInvoiceLine.ZG_Origin = line.DesignationOfOrigin;
				newInvoiceLine.JI_NDescription = line.CommercialDescription;
				newInvoiceLine.JI_BrandName = line.BrandNameOfProducts;
				newInvoiceLine.JI_CustomsQuantity = line.Quantity;
				newInvoiceLine.ZG_DeclaredValue = line.Quantity;
				newInvoiceLine.JI_Weight = line.GrossWeight;
				newInvoiceLine.JI_NetWeight = line.NetWeight;
				newInvoiceLine.ZG_AlcoholicStrength = line.AlcoholicStrength;
				newInvoiceLine.ZG_DegreePlato = line.DegreePlato;
				newInvoiceLine.ZG_Density = line.Density;
				newInvoiceLine.ZG_SizeOfProducer = line.SizeOfProducer;
				newInvoiceLine.ZG_GrowingZone = line.WineGrowingZoneCode;
				newInvoiceLine.ZG_WineCategory = line.WineProductCategory;
				newInvoiceLine.ZG_WineCountryOrigin = line.WineProductThirdCountryOfOrigin;
				newInvoiceLine.JI_WineDetailsComments = line.WineProductOtherInfo;
				newInvoiceLine.ZG_MaturationPeriodOrAgeOfProducts = line.MaturationPeriodOrAgeOfProducts;
				foreach (var operationCode in line.WineOperationCodes)
				{
					var newOperationCode = newInvoiceLine.OperationCodeDataCollection.AddNew();
					newOperationCode.CY_Code = operationCode;
				}
				CreatePackages(newInvoiceLine, line.Packages);
			}

			void CreatePackages(EMCSJobComInvoiceLine emcsInvoiceLine, IReadOnlyCollection<IEMCSPackageInComing> packages)
			{
				foreach (var package in packages)
				{
					if (package.NumberOfPackages > 0)
					{
						var newPackage = emcsDeclaration.EMCSPackages.AddNew();
						newPackage.B5_UnitType = package.KindOfPackages;
						newPackage.B5_UnitCount = package.NumberOfPackages;
						newPackage.B5_SealNumber = package.SealNumber;
						newPackage.B5_SealComment = package.SealInformation;
						newPackage.B5_MarksAndNumbers = package.ShippingMarks;
					}
					else if (package.NumberOfPackages == 0 && !package.IsNumberOfPackagesProvided)
					{
						var newPackage = emcsDeclaration.EMCSPackages.AddNew();
						newPackage.B5_UnitType = package.KindOfPackages;
						newPackage.B5_UnitCount = package.NumberOfPackages;
						newPackage.B5_SealNumber = package.SealNumber;
						newPackage.B5_SealComment = package.SealInformation;
						newPackage.B5_MarksAndNumbers = package.ShippingMarks;
					}
				}
			}
		}

		void LinkLinePackagePivots(EMCSJobDeclaration emcsDeclaration, IReadOnlyCollection<IED801Line> lines)
		{
			foreach (var line in lines.OrderBy(x => x.BodyRecordUniqueReference))
			{
				var invoiceLine = emcsDeclaration.FilteredInvoiceLines.Cast<EMCSJobComInvoiceLine>().SingleOrDefault(x => x.JI_LineNo == line.BodyRecordUniqueReference);
				foreach (var package in line.Packages)
				{
					var packagePivot = invoiceLine.EMCSPackagePivots.Cast<NonPersistentPackagePivot>().SingleOrDefault(x => x.UnitCount == package.NumberOfPackages && x.MarksAndNumbers == package.ShippingMarks);

					if (package.NumberOfPackages > 0 && packagePivot != null)
					{
						packagePivot.IsForInvoiceLine = true;
						invoiceLine.ZG_IsMainPack = true;
					}
					else if (package.NumberOfPackages == 0 && !package.IsNumberOfPackagesProvided && packagePivot != null)
					{
						packagePivot.IsForInvoiceLine = true;
						invoiceLine.ZG_IsMainPack = true;
					}
					else if (package.NumberOfPackages == 0 && package.IsNumberOfPackagesProvided)
					{
						packagePivot = invoiceLine.EMCSPackagePivots.Cast<NonPersistentPackagePivot>().SingleOrDefault(x => x.UnitCount > ZLong.Zero && x.MarksAndNumbers == package.ShippingMarks);
						if (packagePivot != null)
						{
							packagePivot.IsForInvoiceLine = true;
						}
					}
				}
			}
		}

		ZString GetEmailBodyHeader(EMCSJobDeclaration emcsDeclaration, IED801 dataProvider)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("D86594A6-B82F-45B9-B1F4-9F7A9B639E9C", "An incoming EMCS Declaration created Job {0}. For details please follow the link to the Job.", emcsDeclaration.JE_DeclarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("946BA292-ECC9-4D4D-81F3-D3F8C0FBF4CB", "ARC: {0}", dataProvider.ExciseMovement.AdministrativeReferenceCode));

			return htmlBody.ToString();
		}
	}
}
