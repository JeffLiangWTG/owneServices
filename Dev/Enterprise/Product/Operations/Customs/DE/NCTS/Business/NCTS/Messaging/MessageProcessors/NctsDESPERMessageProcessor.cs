using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using AdditionalInfo = Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo;
using EUNctsSupportingDocument = Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsDESPERMessageProcessor : NctsMessageProcessor<AtlasInboundEDIMessage<IDESPER>, IDESPER>
	{
		public NctsDESPERMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("CBC33166-98C3-4B93-BB2C-5BB21822A597", "NCTS DESPER Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<IDESPER> message)
		{
			BusinessObject result = null;
			var dataProvider = message.DataProvider;
			if (dataProvider != null)
			{
				result = GetLinkedObjectFromOriginalMessage(message.Factory, dataProvider.ReferencedMessageIdentifier);
			}
			return result;
		}

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<IDESPER> message)
		{
			var dataProvider = message.DataProvider;
			var header = (NctsHeader)message.EM_LinkedObject;
			var arrivalMovementHeader = header.ArrivalMovementHeader;

			message.EM_Status = EDIMessage.Status.ProcessedOK;
			message.SetLogbookRegistrationNumber(dataProvider.MovementReferenceNumber);

			arrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
			arrivalMovementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Arrival;
			arrivalMovementHeader.BM_EntryDate = dataProvider.DeclarationAcceptanceDate;

			header.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;

			MapHeader(header, dataProvider);

			emailSubjectSuffixProvider = new NCTSEmailSubjectSuffixProvider(arrivalMovementHeader);

			GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory
							, header
							, Res.GetString("6DEADA9C-CDB4-4142-B81F-97C43BDC2A1C", "NCTS Arrival Status Update.")
							, CreateEmailBody(header)
							, isFailure: false
							, message.Branch
							, header
							, dataProvider.ReferencedMessageIdentifier);
		}

		string CreateEmailBody(NctsHeader nctsHeader)
		{
			var emailBody = new StringBuilder();

			emailBody.Append(ZString.Format(Res.GetString("025F9B87-5A35-40B0-A1D2-A6BE445FE8D2",
				"Your NCTS Arrival Declaration for Job {0} has received the Unloading Permission. For details please follow the Link to the Job.",
				nctsHeader.BH_JobReference)));

			emailBody.Append((NoResString)@"<br/><br/>");
			var emailTable = new HtmlTableCreator();
			emailTable.WriteRow(Res.GetString("46307A02-70FE-4C1E-A104-18307D80C323", "MRN"), nctsHeader.MovementReferenceNumber);
			emailTable.WriteRow(Res.GetString("AA6EF452-95C7-406D-B948-B8CE58F23EFE", "Status Update"), Res.GetString("E13AD24D-0230-4CD8-BF31-43B41FDE826C", "Unloading Permission received"));

			emailBody.Append(emailTable.ToHtml());

			return emailBody.ToString();
		}

		NCTSEmailSubjectSuffixProvider emailSubjectSuffixProvider;

		protected override EmailDef GenerateEmail(string uri, string jobNumber, string messageTypeInSubject, string body, bool isFailure, IGlbBranch branchForEmailLogo)
		{
			var email = base.GenerateEmail(uri, jobNumber, messageTypeInSubject, body, isFailure, branchForEmailLogo);
			emailSubjectSuffixProvider.SetEmailSubjectSuffix(email);
			return email;
		}

		static void MapHeader(NctsHeader header, IDESPER dataProvider)
		{
			MapArrivalMovementHeader(header.ArrivalMovementHeader, dataProvider);
			MapTransportEquipments(header, dataProvider);
			MapPreviousDocuments(header.PreviousDocuments, dataProvider.PreviousDocuments);
		}

		static void MapArrivalMovementHeader(NctsArrivalMovementHeader arrivalMovementHeader, IDESPER dataProvider)
		{
			arrivalMovementHeader.BM_InlandTransportMode = dataProvider.InlandModeOfTransport;
			arrivalMovementHeader.BM_GrossWeight = dataProvider.GrossMass;

			MapDepartureTransportMeans(arrivalMovementHeader.ArrivalTransportInfos, dataProvider.DepartureTransportMeans);
			MapSupportingDocuments(arrivalMovementHeader.SupportingDocuments, dataProvider.SupportingDocuments);
			MapAdditionalReferences(arrivalMovementHeader.AdditionalDocuments, dataProvider.AdditionalReferences);
			MapAdditionalInformation(arrivalMovementHeader.AdditionalDocuments, dataProvider.AdditionalInformations);
			MapTransportDocuments(arrivalMovementHeader.AdditionalDocuments, dataProvider.TransportDocuments);
			MapHouseConsignments(arrivalMovementHeader.Header, dataProvider.HouseConsignments);
		}

		static void MapHouseConsignments(NctsHeader header, IReadOnlyCollection<IDESPERHouseConsignment> houseConsignments)
		{
			header.Bills.DeleteAll();
			if (houseConsignments == null)
			{
				return;
			}

			foreach (var houseConsignment in houseConsignments)
			{
				var bill = header.Bills.AddNew();
				bill.B0_Weight = houseConsignment.GrossMass;

				var movementDetail = bill.MovementDetail;
				movementDetail.B9_UnloadedState = NctsUnloadedStateListForHouseConsignment.Codes.DEC;
				movementDetail.B9_SeqNo = houseConsignment.SequenceNumber.ToString(CultureInfo.InvariantCulture);

				MapConsignmentItems(bill.ArrivalGoodsItems, houseConsignment.ConsignmentItems);
				MapDepartureTransportMeans(bill.ArrivalTransportInfos, houseConsignment.DepartureTransportMeans);
				MapPreviousDocuments(bill.PreviousDocuments, houseConsignment.PreviousDocuments);
				MapSupportingDocuments(bill.SupportingDocuments, houseConsignment.SupportingDocuments);
				MapTransportDocuments(bill.AdditionalDocuments, houseConsignment.TransportDocuments);
				MapAdditionalReferences(bill.AdditionalDocuments, houseConsignment.AdditionalReferences);
				MapAdditionalInformation(bill.AdditionalDocuments, houseConsignment.AdditionalInformations);
			}
		}

		static void MapConsignmentItems(INctsArrivalCargoDescCollection<NctsArrivalCargoDesc> arrivalGoodsItems, IReadOnlyCollection<IDESPERConsignmentItem> consignmentItems)
		{
			if (consignmentItems is null)
			{
				return;
			}

			foreach (var consignmentItem in consignmentItems)
			{
				var goodsItem = arrivalGoodsItems.AddNew();
				goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				goodsItem.BY_LineNo = (ZShort)consignmentItem.SequenceNumber;
				goodsItem.BY_DeclarationGoodsItemNumber = consignmentItem.DeclarationGoodsItemNumber;
				goodsItem.BY_Description = consignmentItem.DescriptionOfGoods;
				goodsItem.BY_CusC4Number = consignmentItem.CusCode;
				goodsItem.BY_HarmonisedTariff = consignmentItem.HarmonizedSystemSubHeadingCode + consignmentItem.CombinedNomenclatureCode;
				goodsItem.BY_GrossWeight = consignmentItem.GrossMass.GetValueOrDefault();
				goodsItem.BY_NetWeight = consignmentItem.NetMass.GetValueOrDefault();

				MapPackages(consignmentItem.Packages, goodsItem.Packages);
				MapSupportingDocuments(goodsItem.SupportingDocuments, consignmentItem.SupportingDocuments);
				MapAdditionalReferences(goodsItem.AdditionalInfos, consignmentItem.AdditionalReferences);
				MapAdditionalInformation(goodsItem.AdditionalInfos, consignmentItem.AdditionalInformations);
			}
		}

		static void MapPackages(IReadOnlyCollection<IDESPERPackaging> sourcePackages, NctsPackageCollection packages)
		{
			if (sourcePackages is null)
			{
				return;
			}

			foreach (var sourcePackage in sourcePackages)
			{
				var package = packages.AddNew();
				package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
				package.B5_SequenceNumber = (ZShort)sourcePackage.SequenceNumber;
				package.B5_UnitType = sourcePackage.Kind;
				package.B5_UnitCount = sourcePackage.Quantity.GetValueOrDefault();
				package.B5_MarksAndNumbers = sourcePackage.MarksNumber;
			}
		}

		static void MapAdditionalInformation(ICusSupportingInfoCollection<AdditionalInfo> additionalDocuments, IReadOnlyCollection<INCTSAdditionalInformation> additionalInformation)
		{
			const string docSubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			var existingDocuments = additionalDocuments.Where(x => x.CSI_SubType == docSubType).ToArray();
			existingDocuments.ForEach(additionalDocuments.RemoveAndDelete);
			if (additionalInformation is null)
			{
				return;
			}

			foreach (var additionalInformationItem in additionalInformation)
			{
				var supportingDocument = additionalDocuments.AddNew();
				supportingDocument.CSI_SubType = docSubType;
				supportingDocument.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				supportingDocument.CSI_Code = additionalInformationItem.Code;
				supportingDocument.CSI_ReferenceNumber = additionalInformationItem.Text;
			}
		}

		static void MapAdditionalReferences(ICusSupportingInfoCollection<AdditionalInfo> additionalDocuments, IReadOnlyCollection<INCTSDocument> additionalReferences)
		{
			const string docSubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			var existingDocuments = additionalDocuments.Where(x => x.CSI_SubType == docSubType).ToArray();
			existingDocuments.ForEach(additionalDocuments.RemoveAndDelete);
			if (additionalReferences is null)
			{
				return;
			}

			foreach (var additionalReference in additionalReferences)
			{
				CreateAdditionalDocument(additionalDocuments, additionalReference, docSubType);
			}
		}

		static void MapTransportDocuments(ICusSupportingInfoCollection<AdditionalInfo> additionalDocuments, IReadOnlyCollection<INCTSDocument> transportDocuments)
		{
			const string docSubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			var existingDocuments = additionalDocuments.Where(x => x.CSI_SubType == docSubType).ToArray();
			existingDocuments.ForEach(additionalDocuments.RemoveAndDelete);
			if (transportDocuments is null)
			{
				return;
			}

			foreach (var transportDocument in transportDocuments)
			{
				CreateAdditionalDocument(additionalDocuments, transportDocument, docSubType);
			}
		}

		static void MapSupportingDocuments(INctsSupportingDocumentCollection<EUNctsSupportingDocument> supportingDocuments, IReadOnlyCollection<INCTSDocument> documents)
		{
			supportingDocuments.RemoveAndDeleteAll();
			if (documents is null)
			{
				return;
			}

			foreach (var dataProviderSupportingDocument in documents)
			{
				var supportingDocument = supportingDocuments.AddNew();
				supportingDocument.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				supportingDocument.CSI_Code = dataProviderSupportingDocument.Type;
				supportingDocument.CSI_ReferenceNumber = dataProviderSupportingDocument.ReferenceNumber;
				supportingDocument.CSI_ReferenceNumber2 = dataProviderSupportingDocument.ComplementOfInformation;
				var documentLineItemNumber = dataProviderSupportingDocument.DocumentLineItemNumber;
				if (documentLineItemNumber != null)
				{
					supportingDocument.CSI_LineNo = (ZInt)documentLineItemNumber;
				}
			}
		}

		static void MapPreviousDocuments(ICommonPreviousDocumentCollection<CommonPreviousDocument> previousDocuments, IReadOnlyCollection<INCTSDocument> documents)
		{
			previousDocuments.RemoveAndDeleteAll();
			if (documents is null)
			{
				return;
			}

			foreach (var dataProviderPreviousDocument in documents)
			{
				var previousDocument = previousDocuments.AddNew();
				previousDocument.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				previousDocument.CSI_Code = dataProviderPreviousDocument.Type;
				previousDocument.CSI_ReferenceNumber = dataProviderPreviousDocument.ReferenceNumber;
				previousDocument.CSI_ReferenceNumber2 = dataProviderPreviousDocument.ComplementOfInformation;
			}
		}

		static void MapDepartureTransportMeans(IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans> arrivalTransportMeans, IReadOnlyCollection<INCTSDepartureTransportMeans> departureTransportMeans)
		{
			arrivalTransportMeans.RemoveAndDeleteAll();
			if (departureTransportMeans is null)
			{
				return;
			}

			foreach (var departureTransportMean in departureTransportMeans)
			{
				var arrivalTransportInfo = arrivalTransportMeans.AddNew();
				arrivalTransportInfo.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
				arrivalTransportInfo.TPM_SequenceNumber = (ZShort)departureTransportMean.SequenceNumber;
				arrivalTransportInfo.TPM_TypeOfIdentification = departureTransportMean.TypeOfIdentification;
				arrivalTransportInfo.TPM_IdentificationNumber = departureTransportMean.IdentificationNumber;
				arrivalTransportInfo.TPM_RN_NKTransportNationality = departureTransportMean.Nationality;
			}
		}

		static void MapTransportEquipments(NctsHeader header, IDESPER dataProvider)
		{
			header.ArrivalHeaderContainers.RemoveAndDeleteAll();
			if (dataProvider.TransportEquipments is null)
			{
				return;
			}

			foreach (var transportEquipment in dataProvider.TransportEquipments)
			{
				var arrivalHeaderContainer = header.ArrivalHeaderContainers.AddNew();
				arrivalHeaderContainer.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				arrivalHeaderContainer.BC_SequenceNumber = (ZShort)transportEquipment.SequenceNumber;
				arrivalHeaderContainer.BC_ContainerNum = transportEquipment.ContainerIdentificationNumber;
				arrivalHeaderContainer.BC_Mode = dataProvider.ContainerIndicator == 1
					? Core.Constants.ContainerModes.Containerised
					: Core.Constants.ContainerModes.NonContainerised;

				MapSeals(transportEquipment.Seals, arrivalHeaderContainer.Seals);
				MapGoodsReferences(header, transportEquipment);
			}
		}

		static void MapSeals(IReadOnlyCollection<IDESPERTransportEquipmentSeal> seals, CusSealCollection additionalSeals)
		{
			if (seals is null)
			{
				return;
			}

			foreach (var seal in seals)
			{
				var additionalSeal = additionalSeals.AddNew();
				additionalSeal.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
				additionalSeal.BK_SequenceNumber = (ZShort)seal.SequenceNumber;
				additionalSeal.BK_SealNumber = seal.Identifier;
			}
		}

		static void MapGoodsReferences(NctsHeader header, IDESPERTransportEquipment transportEquipment)
		{
			var packages = header.Bills
				.SelectMany(b => b.ArrivalGoodsItems)
				.Where(agi => transportEquipment.GoodsReferences.Contains(agi.BY_DeclarationGoodsItemNumber))
				.SelectMany(agi => agi.Packages.Cast<NctsPackage>());
			foreach (var package in packages)
			{
				var containerPivot = package.ContainersPivotsForBindingOnly.Cast<NonPersistentContainerPivotPhase5>()
					.SingleOrDefault(cp => cp.Container.BC_SequenceNumber == transportEquipment.SequenceNumber);
				if (containerPivot != null)
				{
					containerPivot.ContainerSelected = true;
				}
			}
		}

		static void CreateAdditionalDocument(ICusSupportingInfoCollection<AdditionalInfo> additionalDocuments, INCTSDocument document, string subType)
		{
			var additionalDocument = additionalDocuments.AddNew();
			additionalDocument.CSI_SubType = subType;
			additionalDocument.CSI_Status = NctsUnloadedStateList.Codes.DEC;
			additionalDocument.CSI_Code = document.Type;
			additionalDocument.CSI_ReferenceNumber = document.ReferenceNumber;
		}
	}
}
