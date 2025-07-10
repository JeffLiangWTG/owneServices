using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC013C_v515.CC013CV1Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC015C_v515.CC015CV1Ent;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.NCTS.Business;

public static class NctsDeclarationComparatorHelper
{
	public static EDIMessage GetLatestEffectiveMessage(NctsHeader header)
	{
		EDIMessage GetLastMessage(string messageType)
			=> header.Messages.GetLastMessage(ZString.Empty, messageType, EDIMessage.Direction.Receive, ZString.Empty, DeclarationMessageSubTypeList.Codes.AcceptedResponse);

		var tquMessage = GetLastMessage(DeclarationMessageTypeList.Codes.TransitNcts5Query);
		var dpdMessage = GetLastMessage(DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration);
		var dpmMessage = GetLastMessage(DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment);

		var lastMessage = new [] { tquMessage, dpdMessage, dpmMessage }.OrderByDescending(x => x?.EM_SystemCreateTimeUtc ?? ZDateTime.MinSmallDateTimeValue).FirstOrDefault();

		return lastMessage == null || lastMessage.EM_MessageType == DeclarationMessageTypeList.Codes.TransitNcts5Query ? lastMessage : MessageProcessorHelper.GetOutgoingMessage(lastMessage);
	}

	public static IComparablePredeclaration GetComparablePredeclaration(EDIMessage ediMessage)
	{
		IComparablePredeclaration result = null;
		switch (ediMessage.EM_MessageType)
		{
			case DeclarationMessageTypeList.Codes.TransitNcts5Query:
				{
					var queryProcessor = new QueryNCTSResponseMessageProcessor(new LoggingInformation());
					result = queryProcessor.GetMessageProvider(ediMessage);
				}
				break;

			case DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration:
				{
					result = DeserializeMessage<Cc015Cv1Ent>(ediMessage, XsdSchemaNameCC015CV1Ent);
				}
				break;

			case DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment:
				{
					result = DeserializeMessage<Cc013Cv1Ent>(ediMessage, XsdSchemaNameCC013CV1Ent);
				}
				break;
		}
		return result;
	}

	public static IComparablePredeclaration GetCurrentNctsHeaderComparable(NctsHeader nctsHeader, NctsMessageSendingObject messageSendingObject)
	{
		var messageType = DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration;
		var builder  = new DepartureNCTSMessageBuilder(new DepartureNCTS5SendMessageWrapper(nctsHeader, messageSendingObject, messageType), messageType, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
		var x = builder.UnsignedMessageText;
		return builder.GetXMLObjectForComparison();
	}

	const string SixDecimalFormat = "0.000000";
	const string ThreeDecimalFormat = "0.000";
	const string TwoDecimalFormat = "0.00";

	public static ZString Compare(IComparablePredeclaration current, IComparablePredeclaration lastMessage)
	{
		var errorReporter = new ComparatorErrorReporter();

		var level = ComparatorErrorLevel.Header;
		var multipleObjectID = ZString.Empty;

		void Compare(string propertyName, ZString val1, ZString val2)
		{
			if (val1 != val2)
			{
				errorReporter.AddError(level, propertyName, multipleObjectID);
			}
		}

		Compare(nameof(current.ConsignorID), current.ConsignorID, lastMessage.ConsignorID);
		Compare(nameof(current.ConsigneeID), current.ConsigneeID, lastMessage.ConsigneeID);
		Compare(nameof(current.DeclarationType), current.DeclarationType, lastMessage.DeclarationType);
		Compare(nameof(current.TIRCarnetNum), current.TIRCarnetNum, lastMessage.TIRCarnetNum);
		Compare(nameof(current.ReducedDatasetIndicator), current.ReducedDatasetIndicator, lastMessage.ReducedDatasetIndicator);
		Compare(nameof(current.DispatchCountry), current.DispatchCountry, lastMessage.DispatchCountry);
		Compare(nameof(current.DestinationCountry), current.DestinationCountry, lastMessage.DestinationCountry);
		Compare(nameof(current.GrossWeight), current.GrossWeight.ToString(SixDecimalFormat), lastMessage.GrossWeight.ToString(SixDecimalFormat));
		Compare(nameof(current.UnloadingPlace), current.UnloadingPlace, lastMessage.UnloadingPlace);
		Compare(nameof(current.Circumstance), current.Circumstance, lastMessage.Circumstance);
		Compare(nameof(current.UniqueConsignmentReference), current.UniqueConsignmentReference, lastMessage.UniqueConsignmentReference);
		Compare(nameof(current.MethodOfPayment), current.MethodOfPayment, lastMessage.MethodOfPayment);
		Compare(nameof(current.CarrierID), current.CarrierID, lastMessage.CarrierID);
		Compare(nameof(current.CustomsOfficeOfDeparture), current.CustomsOfficeOfDeparture, lastMessage.CustomsOfficeOfDeparture);
		Compare(nameof(current.CustomsOfficeOfDestination), current.CustomsOfficeOfDestination, lastMessage.CustomsOfficeOfDestination);

		void CompareCollection<TInterface>(string propertyName, List<TInterface> list1, List<TInterface> list2, Func<TInterface, ZString> getKey) where TInterface : class
		{
			if (list1.Count != list2.Count)
			{
				errorReporter.AddError(level, propertyName, multipleObjectID);
				return;
			}

			for (var i = 0; i < list1.Count; i++)
			{
				var value1 = getKey.Invoke(list1[i]);
				var value2 = getKey.Invoke(list2[i]);
				if (value1 != value2)
				{
					errorReporter.AddError(level, propertyName, multipleObjectID);
					break;
				}
			}
		}

		CompareCollection(nameof(current.CustomsOfficeOfTransit),
						current.CustomsOfficeOfTransit.OrderBy(x => x.SequenceNumber).ToList(),
						lastMessage.CustomsOfficeOfTransit.OrderBy(x => x.SequenceNumber).ToList(),
						x => x.SequenceNumber + x.ReferenceNumber);

		CompareCollection(nameof(current.CustomsOfficeOfExitForTransit),
						current.CustomsOfficeOfExitForTransit.OrderBy(x => x.SequenceNumber).ToList(),
						lastMessage.CustomsOfficeOfExitForTransit.OrderBy(x => x.SequenceNumber).ToList(),
						x => x.SequenceNumber + x.ReferenceNumber);

		CompareCollection(nameof(current.Guarantees),
						current.Guarantees.OrderBy(x => x.GuaranteeType + x.GuaranteeReference).ToList(),
						lastMessage.Guarantees.OrderBy(x => x.GuaranteeType + x.GuaranteeReference).ToList(),
						x => x.GuaranteeType + x.GuaranteeReference.OrderBy(x => x.SequenceNumber).Select(x => (ZString)(x.SequenceNumber + x.GRN + x.AccessCode + x.AmountToBeCovered.ToString(TwoDecimalFormat))).JoinAsString());

		CompareCollection(nameof(current.Authorizations),
						current.Authorizations.OrderBy(x => x.Type + x.ReferenceNumber).ToList(),
						lastMessage.Authorizations.OrderBy(x => x.Type + x.ReferenceNumber).ToList(),
						x => x.Type + x.ReferenceNumber);

		CompareCollection(nameof(current.SupportingDocuments),
						current.SupportingDocuments.OrderBy(x => x.SequenceNumber).ToList(),
						lastMessage.SupportingDocuments.OrderBy(x => x.SequenceNumber).ToList(),
						x => x.SequenceNumber + x.Name + x.Number);

		CompareCollection(nameof(current.AdditionalDocumentsTD),
						current.AdditionalDocumentsTD.OrderBy(x => x.SequenceNumber).ToList(),
						lastMessage.AdditionalDocumentsTD.OrderBy(x => x.SequenceNumber).ToList(),
						x => x.SequenceNumber + x.Name + x.Number);

		CompareCollection(nameof(current.AdditionalDocumentsAR),
						current.AdditionalDocumentsAR.OrderBy(x => x.SequenceNumber).ToList(),
						lastMessage.AdditionalDocumentsAR.OrderBy(x => x.SequenceNumber).ToList(),
						x => x.SequenceNumber + x.Name + x.Number);

		CompareCollection(nameof(current.AdditionalDocumentsAI),
						current.AdditionalDocumentsAI.OrderBy(x => x.SequenceNumber).ToList(),
						lastMessage.AdditionalDocumentsAI.OrderBy(x => x.SequenceNumber).ToList(),
						x => x.SequenceNumber + x.Code + x.Text);

		CompareCollection(nameof(current.CountriesOfRouting),
						current.CountriesOfRouting.OrderBy(x => x.SequenceNumber).ToList(),
						lastMessage.CountriesOfRouting.OrderBy(x => x.SequenceNumber).ToList(),
						x => x.SequenceNumber + x.Country);

		CompareCollection(nameof(current.SupplyChainActors),
						current.SupplyChainActors.OrderBy(x => x.Role + x.IdentificationNumber).ToList(),
						lastMessage.SupplyChainActors.OrderBy(x => x.Role + x.IdentificationNumber).ToList(),
						x => x.Role + x.IdentificationNumber);

		if (current.HouseConsignments.Count != lastMessage.HouseConsignments.Count)
		{
			level = ComparatorErrorLevel.Consignment;
			errorReporter.AddError(ComparatorErrorLevel.Header, nameof(IComparablePredeclaration.HouseConsignments), multipleObjectID);
		}
		else
		{
			var houseList1 = current.HouseConsignments.OrderBy(x => x.HouseNumber).ToList();
			var houseList2 = lastMessage.HouseConsignments.OrderBy(x => x.HouseNumber).ToList();

			for (var i = 0; i < houseList1.Count; i++)
			{
				var house1 = houseList1[i];
				var house2 = houseList2[i];

				level = ComparatorErrorLevel.Consignment;
				multipleObjectID = house1.HouseNumber;

				Compare(nameof(house1.HouseNumber), house1.HouseNumber, house2.HouseNumber);
				Compare(nameof(house1.TotalGrossWeight), house1.TotalGrossWeight.ToString(SixDecimalFormat), house2.TotalGrossWeight.ToString(SixDecimalFormat));

				CompareCollection(nameof(house1.SupportingDocuments),
						house1.SupportingDocuments.OrderBy(x => x.Name + x.Number).ToList(),
						house2.SupportingDocuments.OrderBy(x => x.Name + x.Number).ToList(),
						x => x.Name + x.Number);

				CompareCollection(nameof(house1.AdditionalDocumentsTD),
								house1.AdditionalDocumentsTD.OrderBy(x => x.Name + x.Number).ToList(),
								house2.AdditionalDocumentsTD.OrderBy(x => x.Name + x.Number).ToList(),
								x => x.Name + x.Number);

				CompareCollection(nameof(house1.AdditionalDocumentsAR),
								house1.AdditionalDocumentsAR.OrderBy(x => x.Name + x.Number).ToList(),
								house2.AdditionalDocumentsAR.OrderBy(x => x.Name + x.Number).ToList(),
								x => x.Name + x.Number);

				CompareCollection(nameof(house1.AdditionalDocumentsAI),
						house1.AdditionalDocumentsAI.OrderBy(x => x.Code + x.Text).ToList(),
						house2.AdditionalDocumentsAI.OrderBy(x => x.Code + x.Text).ToList(),
						x => x.Code + x.Text);

				CompareCollection(nameof(house1.PreviousDocuments),
								house1.PreviousDocuments.OrderBy(x => x.Name + x.Number).ToList(),
								house2.PreviousDocuments.OrderBy(x => x.Name + x.Number).ToList(),
								x => x.Name + x.Number);

				CompareCollection(nameof(house1.SupplyChainActors),
						house1.SupplyChainActors.OrderBy(x => x.Role + x.IdentificationNumber).ToList(),
						house2.SupplyChainActors.OrderBy(x => x.Role + x.IdentificationNumber).ToList(),
						x => x.Role + x.IdentificationNumber);

				level = ComparatorErrorLevel.Goods;
				if (house1.GoodsItems.Count != house2.GoodsItems.Count)
				{
					errorReporter.AddError(ComparatorErrorLevel.Consignment, nameof(IComparableHouseConsignment.GoodsItems), multipleObjectID);
				}
				else
				{
					var goodsList1 = house1.GoodsItems.OrderBy(x => x.ItemNumber).ToList();
					var goodsList2 = house2.GoodsItems.OrderBy(x => x.ItemNumber).ToList();

					for (var j = 0; j < goodsList1.Count; j++)
					{
						var goods1 = goodsList1[j];
						var goods2 = goodsList2[j];

						multipleObjectID = goods1.DeclarationItemNumber;

						Compare(nameof(goods1.ItemNumber), goods1.ItemNumber, goods2.ItemNumber);
						Compare(nameof(goods1.DeclarationItemNumber), goods1.DeclarationItemNumber, goods2.DeclarationItemNumber);
						Compare(nameof(goods1.DeclarationType), goods1.DeclarationType, goods2.DeclarationType);
						Compare(nameof(goods1.OriginCountry), goods1.OriginCountry, goods2.OriginCountry);
						Compare(nameof(goods1.DestinationCountry), goods1.DestinationCountry, goods2.DestinationCountry);
						Compare(nameof(goods1.CommercialReference), goods1.CommercialReference, goods2.CommercialReference);
						Compare(nameof(goods1.ConsigneeID), goods1.ConsigneeID, goods2.ConsigneeID);
						Compare(nameof(goods1.GoodsDescription), goods1.GoodsDescription, goods2.GoodsDescription);
						Compare(nameof(goods1.CusCode), goods1.CusCode, goods2.CusCode);
						Compare(nameof(goods1.CommodityCode), goods1.CommodityCode, goods2.CommodityCode);
						Compare(nameof(goods1.GrossWeight), goods1.GrossWeight.ToString(SixDecimalFormat), goods2.GrossWeight.ToString(SixDecimalFormat));
						Compare(nameof(goods1.NetWeight), goods1.NetWeight.ToString(SixDecimalFormat), goods2.NetWeight.ToString(SixDecimalFormat));
						Compare(nameof(goods1.SupplementaryQty), goods1.SupplementaryQty.ToString(SixDecimalFormat), goods2.SupplementaryQty.ToString(SixDecimalFormat));

						CompareCollection(nameof(goods1.DangerousGoods),
										goods1.DangerousGoods.OrderBy(x => x.UNNumber).ToList(),
										goods2.DangerousGoods.OrderBy(x => x.UNNumber).ToList(),
										x => x.UNNumber);

						CompareCollection(nameof(goods1.SupplyChainActors),
										goods1.SupplyChainActors.OrderBy(x => x.Role + x.IdentificationNumber).ToList(),
										goods2.SupplyChainActors.OrderBy(x => x.Role + x.IdentificationNumber).ToList(),
										x => x.Role + x.IdentificationNumber);

						CompareCollection(nameof(goods1.PackagesAndVehicles),
										goods1.PackagesAndVehicles.OrderBy(x => x.SequenceNumber).ToList(),
										goods2.PackagesAndVehicles.OrderBy(x => x.SequenceNumber).ToList(),
										x => x.SequenceNumber + x.NumberOfPackages + x.PackageType + x.Marks);

						CompareCollection(nameof(goods1.PreviousDocuments),
										goods1.PreviousDocuments.OrderBy(x => x.Name + x.Number).ToList(),
										goods2.PreviousDocuments.OrderBy(x => x.Name + x.Number).ToList(),
										x => x.Name + x.Number + x.ComplementaryInformation + x.GoodsItemNumber + x.MeasurementUnitAndQualifier + (x.QuantitySpecified ? x.Quantity.ToString(ThreeDecimalFormat) : string.Empty));

						CompareCollection(nameof(goods1.SupportingDocuments),
										goods1.SupportingDocuments.OrderBy(x => x.Name + x.Number).ToList(),
										goods2.SupportingDocuments.OrderBy(x => x.Name + x.Number).ToList(),
										x => x.Name + x.Number);

						CompareCollection(nameof(goods1.AdditionalDocumentsTD),
										goods1.AdditionalDocumentsTD.OrderBy(x => x.Name + x.Number).ToList(),
										goods2.AdditionalDocumentsTD.OrderBy(x => x.Name + x.Number).ToList(),
										x => x.Name + x.Number);

						CompareCollection(nameof(goods1.AdditionalDocumentsAR),
										goods1.AdditionalDocumentsAR.OrderBy(x => x.Name + x.Number).ToList(),
										goods2.AdditionalDocumentsAR.OrderBy(x => x.Name + x.Number).ToList(),
										x => x.Name + x.Number);

						CompareCollection(nameof(goods1.AdditionalDocumentsAI),
										goods1.AdditionalDocumentsAI.OrderBy(x => x.Code + x.Text).ToList(),
										goods2.AdditionalDocumentsAI.OrderBy(x => x.Code + x.Text).ToList(),
										x => x.Code + x.Text);
					}
				}
			}
		}

		return errorReporter.GetFormattedReport();
	}

	const string XsdSchemaNameCC015CV1Ent = "CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.Outgoing.CC015CV1Ent.xsd";
	const string XsdSchemaNameCC013CV1Ent = "CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.Outgoing.CC013CV1Ent.xsd";

	static TProvider DeserializeMessage<TProvider>(EDIMessage message, string xsdSchemaEmbeddedResourceName) where TProvider : class, IComparablePredeclaration
	{
		using var textReader = message.GetEM_MessageTextReader();
		using var bodyTextReader = XMLResponseMessageHelper.GetXmlBody(textReader);
		return ESXmlObjectSerializer.DeserializeWithoutValidation<TProvider>(xsdSchemaEmbeddedResourceName, bodyTextReader, isAES: false, isNCTS: true);
	}
}
