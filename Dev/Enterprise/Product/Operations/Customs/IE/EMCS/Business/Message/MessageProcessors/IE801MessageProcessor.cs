using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.EMCS.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class IE801MessageProcessor : EMCSMessageProcessor<IIE801>
	{
		public IE801MessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override bool MustHaveLinkedObject => false;

		protected override string MessageFriendlyNameCore => Res.GetString("B3F72554-F60B-4ED1-9B73-7F88C288C27C", "EMCS IE801 Message Processor");

		protected override Type MessageInterpreterType => typeof(IE801MessageInterpreter);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IIE801 provider)
		{
			var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
			if (emcsDeclaration != null && emcsDeclaration.JE_DeclarantType == EMCSEntryTypeList.Codes.Consignor)
			{
				ProcessMessageForConsignor(message, provider, emcsDeclaration);
			}
			else
			{
				ProcessMessageForConsignee(factory, message, provider, emcsDeclaration);
			}
		}

		protected override ZString ReplaceExtraXMLElementIfNeeded(ZString text)
		{
			return text.Replace(":ExciseMovementEad>", ":ExciseMovement>").Replace(":GrossWeight>", ":GrossMass>")
				.Replace(":NetWeight>", ":NetMass>").Replace(":EADContainer>", ":EADESADContainer>").Replace(":Ead>", ":EadEsad>")
				.Replace(":HeaderEad>", ":HeaderEadEsad>").Replace(":BodyEad>", ":BodyEadEsad>").Replace(":DateAndTimeOfValidationOfEad>", ":DateAndTimeOfValidationOfEadEsad>");
		}

		void UpdateOrCreateEADNumber(EMCSJobDeclaration emcsDeclaration, IIE801 provider)
		{
			var entryNumber = emcsDeclaration.LoadCusEntryNumber(true);
			entryNumber.CE_EntryNum = provider.ExciseMovementEad.AdministrativeReferenceCode;
			entryNumber.CE_EntryLineReference = provider.ExciseMovementEad.SequenceNumber;
			entryNumber.CE_IssueDate = provider.DateAndTimeOfValidationOfEad;
		}

		#region Consignor

		void ProcessMessageForConsignor(EMCSInboundEDIMessage message, IIE801 provider, EMCSJobDeclaration emcsDeclaration)
		{
			emcsDeclaration.JE_MessageStatus = EDIMessage.Status.Received;
			emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.REG;

			UpdateInvoiceLines(emcsDeclaration);
			UpdateOrCreateEADNumber(emcsDeclaration, provider);

			linkedEMCSDeclaration = emcsDeclaration;
			SendEmailNotification(message, Res.GetString("0C35F21B-2E4D-4F17-9C89-A2248657240D", "EMCS e-AD registered"), false, provider, null, GetEmailBodyForConsignor);
		}

		string GetEmailBodyForConsignor(EMCSJobDeclaration declaration, IEMCSInboundProvider dataProvider, IEMCSEvent iEvent)
		{
			var provider = (IIE801)dataProvider;
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("1799E470-A896-4A19-BF8D-117AAD52C361", "Your EMCS Declaration for Job {0} has been registered. For details please follow the Link to the Job.", declaration.JE_DeclarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("3BF9CA8B-3A08-472B-A06B-054092CD108D", "ARC: {0}", provider.ExciseMovementEad.AdministrativeReferenceCode));

			return htmlBody.ToString();
		}

		void UpdateInvoiceLines(EMCSJobDeclaration declaration)
		{
			foreach (var line in declaration.InvoiceLines.Cast<EMCSJobComInvoiceLine>().ToArray())
			{
				line.ZG_DeclaredValue = line.JI_CustomsQuantity;
			}
		}

		#endregion

		#region Consignee

		void ProcessMessageForConsignee(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IIE801 provider, EMCSJobDeclaration emcsDeclaration)
		{
			if (emcsDeclaration == null)
			{
				using (DisposableEnvironment.ForBranch(message.Branch.PK.ToGuid()))
				{
					emcsDeclaration = factory.New<EMCSJobDeclaration>();
					message.EM_LinkedObject = emcsDeclaration;
					emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;
					UpdateCertificate(factory, emcsDeclaration, message);
				}
			}
			emcsDeclaration.JE_MessageStatus = EDIMessage.Status.Received;
			emcsDeclaration.JE_EntryStatus = EntryStatusList.Codes.REG;

			UpdateDeclarationFromMessage(factory, provider, emcsDeclaration);

			linkedEMCSDeclaration = emcsDeclaration;
			emcsDeclaration.OnSuccessfulSaveDo(declaration =>
			{
				SendEmailNotification(message, Res.GetString("B55AF3A1-667C-463B-BFCB-2FD9F2BEAAFF", "Incoming EMCS e-AD"), false, provider, null, GetEmailBodyForConsignee);
			});
		}

		string GetEmailBodyForConsignee(EMCSJobDeclaration declaration, IEMCSInboundProvider dataProvider, IEMCSEvent iEvent)
		{
			var provider = (IIE801)dataProvider;
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("97A19B38-1C70-4031-ACA7-E25CC26A2DD0", "An incoming EMCS Declaration created Job {0}. For details please follow the link to the Job.", declaration.JE_DeclarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("7A2A1E19-06F1-422B-A416-48653E964A4C", "ARC: {0}", provider.ExciseMovementEad.AdministrativeReferenceCode));

			return htmlBody.ToString();
		}

		void UpdateDeclarationFromMessage(BusinessObjectFactory factory, IIE801 provider, EMCSJobDeclaration emcsDeclaration)
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
			LinkExtraLinePackagePivots(emcsDeclaration, provider.Lines);
		}

		void UpdateCertificate(BusinessObjectFactory factory, EMCSJobDeclaration emcsDeclaration, EMCSInboundEDIMessage message)
		{
			var credential = factory.Load<EMCSGlbCompanyCredential>(message.EM_GP);
			emcsDeclaration.JE_CustomsProfile = credential?.GP_MailBoxID.Left(EMCSJobDeclaration.Schema.JE_CustomsProfileMaxLength) ?? ZString.Empty;
		}

		void CreateCustomsOffices(EMCSJobDeclaration emcsDeclaration, IIE801 provider)
		{
			emcsDeclaration.CustomsOffices.RemoveAndDeleteAll();
			UpdateOrCreateCustomsOffice(emcsDeclaration, OfficeCodes_EMCS.Codes.OfficeOfDestination, ZString.Empty);

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
			var customsOffice = emcsDeclaration.CustomsOffices.Cast<OfficeCode>().SingleOrDefault(x => x.CY_Code == officeType) ?? emcsDeclaration.CustomsOffices.AddNew();
			customsOffice.CY_Code = officeType;
			customsOffice.CY_Data = office;
		}

		void CreateImportSADNumbers(EMCSJobDeclaration emcsDeclaration, IIE801 provider)
		{
			emcsDeclaration.ImportSADNumbers.RemoveAndDeleteAll();
			foreach (var sadNumber in provider.ImportSadNumbers)
			{
				var newSadNumber = emcsDeclaration.ImportSADNumbers.AddNew();
				newSadNumber.CSI_Description = sadNumber;
			}
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
					consignee.UpdatePartyAddress(consigneeInMessage);
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
					consignor.UpdatePartyAddress(consignorInMessage);
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
					dispatchWarehouse.UpdatePartyAddress(placeOfDispatchInMessage);
				}
			}
		}

		void CreateDocumentCertificates(EMCSJobDeclaration emcsDeclaration, IIE801 provider)
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

		void CreateInvoiceLinesAndPackages(EMCSJobDeclaration emcsDeclaration, IReadOnlyCollection<IIE801Line> lines)
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
				newInvoiceLine.ZG_IndependentSmallProducersDeclaration = line.IndependentSmallProducersDeclaration;

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
					if (package.NumberOfPackages > 0 || (package.NumberOfPackages == 0 && !package.IsNumberOfPackagesProvided))
					{
						var newPackage = emcsDeclaration.EMCSPackages.AddNew();
						newPackage.B5_UnitType = package.KindOfPackages;
						newPackage.B5_UnitCount = package.NumberOfPackages;
						newPackage.B5_SealNumber = package.SealNumber;
						newPackage.B5_SealComment = package.SealInformation;
						newPackage.B5_MarksAndNumbers = package.ShippingMarks;

						var packagePivot = emcsInvoiceLine.EMCSPackagePivots.Cast<NonPersistentPackagePivot>().SingleOrDefault(x => x.Package.PK == newPackage.PK);
						if (packagePivot != null)
						{
							packagePivot.IsForInvoiceLine = true;
							emcsInvoiceLine.ZG_IsMainPack = true;
						}
					}
				}
			}
		}

		void LinkExtraLinePackagePivots(EMCSJobDeclaration emcsDeclaration, IReadOnlyCollection<IIE801Line> lines)
		{
			foreach (var line in lines.OrderBy(x => x.BodyRecordUniqueReference))
			{
				var invoiceLine = emcsDeclaration.FilteredInvoiceLines.Cast<EMCSJobComInvoiceLine>().SingleOrDefault(x => x.JI_LineNo == line.BodyRecordUniqueReference);
				foreach (var package in line.Packages)
				{
					if (package.NumberOfPackages == 0 && package.IsNumberOfPackagesProvided)
					{
						var packagePivot = invoiceLine.EMCSPackagePivots.Cast<NonPersistentPackagePivot>().FirstOrDefault(x => x.UnitCount > ZLong.Zero && x.MarksAndNumbers == package.ShippingMarks);
						if (packagePivot != null)
						{
							packagePivot.IsForInvoiceLine = true;
						}
					}
				}
			}
		}

		#endregion
	}
}
