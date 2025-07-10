using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;
using Container = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;
using ReceiveTransmitList = Enterprise.Messaging.Integration.ReceiveTransmitList.Codes;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.JP.AFR.Business
{
	public static class MessageInterpretationGenerator
	{
		#region const

		const string TableClass = "table";
		public const string ParsingFailureResult = "<html><b>Failed parsing the message</B><br/>Please refer to 'Message Text' tab for more detailed information.</html>";
		const IEnumerable<string> EmptyColumnNames = null;

		const string NoContainerContained = "No Container was included in this bill";
		const string CompletionMessage = "<b>The Master Bill has been marked as Registration Completed through '{0}' message.</b>";
		const string OutgoingMessageFooter = "<br/>For more detailed information, please refer to the 'Message Text' tab.<br/>";
		const string MessageTypeDescription = "The message type is : '{0}'.<br/>";
		const string ActionPurposeMeaning = "The response indicates your {0} has been {1}<br/>";
		const string RACMeaning = "The response indicates your previous Risk Assessment Result '{0}' has been canceled.<br/>";
		const string RARMeaning = "The response indicates you have received a Risk Assessment Result '{0}'.<br/>";

		#endregion

		#region Predefined Formats

		static NameValueCollection FittingWidthFormat => fittingWidthFormat ?? (fittingWidthFormat = new NameValueCollection { { "width", "100%" } });

		[ThreadStatic]
		static NameValueCollection fittingWidthFormat;

		static NameValueCollection FixWidthColumnFormat => fixWidthColumnFormat ?? (fixWidthColumnFormat = new NameValueCollection { { "width", "150px" } });

		[ThreadStatic]
		static NameValueCollection fixWidthColumnFormat;

		static NameValueCollection CrossColumnFormat => crossColumnFormat ?? (crossColumnFormat = new NameValueCollection { { "colspan", "2" } });

		[ThreadStatic]
		static NameValueCollection crossColumnFormat;

		#endregion

		public static ZString GetInterpretatedHTML(JPAFRMessage message)
		{
			var messageDetail = ZString.Empty;
			try
			{
				Argument.NotNull(message, "message");

				switch (message.EM_ReceiveTransmit)
				{
					case ReceiveTransmitList.Receive:
						var eventDeserializer = new XmlEventDeserializer();
						var xmlEvent = eventDeserializer.Parse(message.EM_MessageText);
						messageDetail = GenerateOutputForUniversalEvent(xmlEvent, message.Factory);
						break;
					case ReceiveTransmitList.Transmit:
						var parsedUniversalShipment = message.GetEM_MessageTextReader().Parse<UniversalShipment>();
						if (message.EM_MessageOwner == MessagingTypeList.Codes.CancelBLL || message.EM_MessageOwner == MessagingTypeList.Codes.RegisterBLL)
						{
							messageDetail = GenerateOutputForUniversalShipmentForBLLFunction(parsedUniversalShipment, message.Factory);
						}
						else
						{
							messageDetail = GenerateOutputForUniversalShipment(parsedUniversalShipment, true, message.Factory);
						}
						break;
				}

				if (!messageDetail.IsEmpty)
				{
					messageDetail = FormatOutputInTemplate(messageDetail);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{ }

			if (messageDetail.IsEmpty)
			{
				messageDetail = ParsingFailureResult;
			}
			return messageDetail;
		}

		#region UniversalEventsProcessing

		public static ZString GenerateOutputForUniversalEvent(IXmlEventValueObject xmlEvent, BusinessObjectFactory factory)
		{
			Argument.NotNull(xmlEvent, "xmlEvent");
			Argument.NotNull(factory, "factory");

			var result = ZString.Empty;
			var htmlCreator = new HtmlTableCreator(TableClass, EmptyColumnNames, FittingWidthFormat);
			htmlCreator.EnableHTMLEncoding = false;
			htmlCreator.WriteRow(new CellWithFormatting("Response Details", true));

			var dataContext = xmlEvent.DataContext;
			var eventReference = xmlEvent.EventReference;
			var actionPurposeCode = dataContext == null ? ZString.Empty : xmlEvent.DataContext.ActionPurposeCode;
			var actionPurposeDescription = ListHelper.GetDescription(dataContext.ActionPurposeCode, factory.GetCachedValue<MessagingTypeList>());
			var actionPurposeMeaning = GetActionPurposeMeaning(eventReference, actionPurposeCode);

			htmlCreator.WriteRow(MessageDisplayConstants.AResponseMessage +
				string.Format(MessageTypeDescription, actionPurposeCode + (string.IsNullOrEmpty(actionPurposeDescription) ? string.Empty : " - ") + actionPurposeDescription) +
				actionPurposeMeaning + MessageDisplayConstants.LineChange + MessageDisplayConstants.ShownBelow);

			var context = xmlEvent.Context;
			if (context != null)
			{
				htmlCreator.WriteRow(context.NotificationDetails);
			}
			return htmlCreator.ToHtml();
		}

		public static ZString GetActionPurposeMeaning(ZString eventReference, ZString actionPurposeCode)
		{
			var result = ZString.Empty;
			var references = eventReference.ToString().Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
			if (references.Length > 0)
			{
				switch (actionPurposeCode)
				{
					case MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse:
						if (references.Length > 1)
						{
							result = string.Format(ActionPurposeMeaning, "House Bill Registration", references[1]);
						}
						break;
					case MessagingTypeList.Codes.UpdateAdvanceCargoInformationRegistrationHouse:
						if (references.Length > 1)
						{
							result = string.Format(ActionPurposeMeaning, "House Bill Amendment", references[1]);
						}
						break;
					case MessagingTypeList.Codes.AdvanceCargoInformationRegistrationMaster:
						if (references.Length > 1)
						{
							result = string.Format(ActionPurposeMeaning, "Master Bill Registration", references[1]);
						}
						break;
					case MessagingTypeList.Codes.UpdateRegisteredAdvanceCargoInformationMaster:
						if (references.Length > 1)
						{
							result = string.Format(ActionPurposeMeaning, "Master Bill Amendment", references[1]);
						}
						break;
					case MessagingTypeList.Codes.DepartureTimeRegistration:
						if (references.Length > 1)
						{
							result = string.Format(ActionPurposeMeaning, "Departure Time Registration", references[1]);
						}
						break;
					case MessagingTypeList.Codes.RiskAssessmentCancellation:
						result = string.Format(RACMeaning, references[0]);
						break;
					case MessagingTypeList.Codes.RiskAssessmentResult:
						result = string.Format(RARMeaning, references[0]);
						break;
				}
			}
			return result;
		}

		#endregion

		#region UniversalShipmentProcessing

		internal static ZString GenerateOutputForUniversalShipmentForBLLFunction(UniversalShipment shipment, BusinessObjectFactory factory)
		{
			Argument.NotNull(shipment, "shipment");
			Argument.NotNull(factory, "factory");

			var htmlCreator = new HtmlTableCreator(TableClass, EmptyColumnNames, FittingWidthFormat) { EnableHTMLEncoding = false };
			htmlCreator.WriteRow(new CellWithFormatting("BLL Function Information", CrossColumnFormat, true));

			var actionCode = shipment.DataContext.ActionPurposeCode;
			var actionDescription = ListHelper.GetDescription(actionCode, factory.GetCachedValue<MessagingTypeList>());
			htmlCreator.WriteRow("Action Type", actionCode + (string.IsNullOrEmpty(actionDescription) ? string.Empty : " - " + actionDescription));
			var functionCode = shipment.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.BLLFunctionCode) ?? ZString.Empty;
			if (Enum.TryParse(functionCode.ToString(), out BLLFunctionCode bllfunctionCode))
			{
				var functionCodeDescription = bllfunctionCode.GetCaption();
				htmlCreator.WriteRow("Function Code", Invariant($"{functionCode} - {functionCodeDescription}"));
			}
			else
			{
				htmlCreator.WriteRow("Function Code", ZString.Empty);
			}

			var changeReasonCode = shipment.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.BLLChangeReasonCode) ?? ZString.Empty;
			var changeReasonDescription = new ZString(ListHelper.GetDescription(changeReasonCode, factory.GetCachedValue<AFRBLLChangeReasonCodeList>()));
			htmlCreator.WriteRow("Change Reason", changeReasonDescription.IsEmpty ? changeReasonCode.ToString() : Invariant($"{changeReasonCode} - {changeReasonDescription}"));

			var origGroup = shipment.AddInfoGroupCollection.FirstOrDefault(x => x.Type.Code.GetValueOrDefault() == AddInfoConstants.Bill.BLLOriginalBillNumbers);
			if (origGroup != null)
			{
				var origNumbers = GetBLLBillNumbers(origGroup);
				if (origNumbers.Any())
				{
					if (origNumbers.Length == 1)
					{
						htmlCreator.WriteRow(new CellWithFormatting("Original Bill Number", CrossColumnFormat, true));
						htmlCreator.WriteRow("Original Bill", origNumbers[0]);
					}
					else if (origNumbers.Length > 1)
					{
						htmlCreator.WriteRow(new CellWithFormatting("Original Bill Numbers", CrossColumnFormat, true));
						for (var index = 0; index < origNumbers.Length; index++)
						{
							htmlCreator.WriteRow(Invariant($"Original Bill {index + 1}"), origNumbers[index]);
						}
					}
				}
			}

			var newGroup = shipment.AddInfoGroupCollection.FirstOrDefault(x => x.Type.Code.GetValueOrDefault() == AddInfoConstants.Bill.BLLNewBillNumbers);
			if (newGroup != null)
			{
				var newNumbers = GetBLLBillNumbers(newGroup);
				if (newNumbers.Any())
				{
					if (newNumbers.Length == 1)
					{
						htmlCreator.WriteRow(new CellWithFormatting("New Bill Number", CrossColumnFormat, true));
						htmlCreator.WriteRow("New Bill", newNumbers[0]);
					}
					else if (newNumbers.Length > 1)
					{
						htmlCreator.WriteRow(new CellWithFormatting("New Bill Numbers", CrossColumnFormat, true));
						for (var index = 0; index < newNumbers.Length; index++)
						{
							htmlCreator.WriteRow(Invariant($"New Bill {index + 1}"), newNumbers[index]);
						}
					}
				}
			}

			return htmlCreator.ToHtml() + OutgoingMessageFooter;
		}

		static ZString[] GetBLLBillNumbers(AddInfoGroup group)
		{
			return group.AddInfoCollection
				.Where(x => x.Key.GetValueOrDefault().StartsWith(AddInfoConstants.Bill.BLLBillNumber, StringComparison.OrdinalIgnoreCase))
				.Select(x => x.Value.GetValueOrDefault())
				.ToArray();
		}

		public static ZString GenerateOutputForUniversalShipment(UniversalShipment shipment, bool isMasterLevel, BusinessObjectFactory factory)
		{
			Argument.NotNull(shipment, "shipment");
			Argument.NotNull(factory, "factory");

			var result = ZString.Empty;
			var actionCode = shipment.DataContext.ActionPurposeCode;
			var actionDescription = ListHelper.GetDescription(actionCode, factory.GetCachedValue<FunctionTypeList>());
			var isShippingLineManifesting = MessagingTypeList.IsShippingLineManifestingMessaging(actionCode);
			var isShippingLineDepartureTime = MessagingTypeList.IsShippingLineDepartureTimeMessaging(actionCode);

			var htmlCreator = new HtmlTableCreator(isMasterLevel ? TableClass : string.Empty, EmptyColumnNames, FittingWidthFormat) { EnableHTMLEncoding = false };
			var tableHeader = isMasterLevel && actionCode == MessagingTypeList.Codes.BlanketVesselChange ? "Original Vessel Details" : (isMasterLevel ? (isShippingLineDepartureTime ? "Departure Time Information" : "AFR Header Information") : "AFR Bill Information");
			htmlCreator.WriteRow(new CellWithFormatting(tableHeader, CrossColumnFormat, true));
			if (!isMasterLevel)
			{
				htmlCreator.WriteRow("Action Type", actionCode + (string.IsNullOrEmpty(actionDescription) ? string.Empty : (" - " + actionDescription)));
				if (actionCode == FunctionTypeList.Codes.Delete)
				{
					htmlCreator.WriteRow("Delete Reason Code", shipment.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.DeleteReasonCode));
					htmlCreator.WriteRow("Delete Reason Text", shipment.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.DeleteReasonText));
				}
				var specialCargoCode = shipment.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.SpecialCargoCode);
				if (specialCargoCode.HasValue && !specialCargoCode.Value.IsEmpty)
				{
					htmlCreator.WriteRow("Special Cargo Code", specialCargoCode);
				}
			}
			if (!isShippingLineManifesting && !isShippingLineDepartureTime && shipment.WayBillType != null)
			{
				var billDescription = shipment.WayBillType.Description;
				htmlCreator.WriteRow(billDescription, shipment.WayBillNumber);
			}
			if (isMasterLevel)
			{
				GenerateOutputForVesselInformation(actionCode, htmlCreator, shipment);
				if (isShippingLineDepartureTime)
				{
					GenerateOutputForATD(shipment, htmlCreator);
				}
				else
				{
					GenerateOutputForSubshipments(htmlCreator, shipment.SubShipmentCollection, factory);
				}
			}
			else
			{
				if (actionCode != FunctionTypeList.Codes.BlanketVesselChange)
				{
					GenerateOutputForContainers(htmlCreator, shipment.ContainerCollection);
				}
			}

			result = htmlCreator.ToHtml() + (isMasterLevel ? OutgoingMessageFooter : string.Empty);
			return result;
		}

		static void GenerateOutputForVesselInformation(ZString actionCode, HtmlTableCreator outputCreator, UniversalShipment shipment)
		{
			var portOfLoading = shipment.PortOfLoading;
			var portOfDischarge = shipment.PortOfDischarge;
			var addInfoCollection = shipment.AddInfoCollection;
			outputCreator.WriteRow(new CellWithFormatting("Port of Loading", FixWidthColumnFormat),
				new CellWithFormatting(portOfLoading == null ? ZString.Empty : portOfLoading.Code));
			outputCreator.WriteRow(new CellWithFormatting("Port of Discharge", FixWidthColumnFormat),
				new CellWithFormatting(portOfDischarge == null ? ZString.Empty : portOfDischarge.Code));
			outputCreator.WriteRow(new CellWithFormatting("Carrier Code", FixWidthColumnFormat),
				new CellWithFormatting(addInfoCollection == null ? ZString.Empty : addInfoCollection.GetZStringValue(AddInfoConstants.Header.CarrierCode)));
			outputCreator.WriteRow(new CellWithFormatting("Vessel Name", FixWidthColumnFormat),
				new CellWithFormatting(shipment.VesselName));
			outputCreator.WriteRow(new CellWithFormatting("Vessel Call Sign", FixWidthColumnFormat),
				new CellWithFormatting(addInfoCollection == null ? ZString.Empty : addInfoCollection.GetZStringValue(AddInfoConstants.Header.VesselCallSign)));
			outputCreator.WriteRow(new CellWithFormatting("Voyage Number", FixWidthColumnFormat),
				new CellWithFormatting(addInfoCollection == null ? ZString.Empty : shipment.VoyageFlightNo));
			var vesselDetailsChanged = addInfoCollection?.GetZStringValue(AddInfoConstants.Header.VesselDetailsChanged);
			if (vesselDetailsChanged.HasValue && !vesselDetailsChanged.Value.IsEmpty)
			{
				outputCreator.WriteRow(new CellWithFormatting("Vessel Details Changed", FixWidthColumnFormat), new CellWithFormatting(vesselDetailsChanged));
			}
			var operationalCarrierVoyageNo = addInfoCollection?.GetZStringValue(AddInfoConstants.Header.OperationalCarrierVoyageNo);
			if (operationalCarrierVoyageNo.HasValue && !operationalCarrierVoyageNo.Value.IsEmpty)
			{
				outputCreator.WriteRow(new CellWithFormatting("Operational Carrier Voyage No", FixWidthColumnFormat), new CellWithFormatting(operationalCarrierVoyageNo));
			}

			if (actionCode == MessagingTypeList.Codes.BlanketVesselChange)
			{
				outputCreator.WriteRow(new CellWithFormatting("New Vessel Details", CrossColumnFormat, true));
				GenerateOutputForNewVesselInformation(outputCreator, addInfoCollection);
			}
		}

		static void GenerateOutputForNewVesselInformation(HtmlTableCreator outputCreator, List<AddInfo> addInfoCollection)
		{
			outputCreator.WriteRow(new CellWithFormatting("New Carrier Code", FixWidthColumnFormat),
				new CellWithFormatting(addInfoCollection.GetZStringValue(AddInfoConstants.Header.CarrierCodeNew) ?? ZString.Empty));
			outputCreator.WriteRow(new CellWithFormatting("New Vessel Name", FixWidthColumnFormat),
				new CellWithFormatting(addInfoCollection.GetZStringValue(AddInfoConstants.Header.VesselNameNew) ?? ZString.Empty));
			outputCreator.WriteRow(new CellWithFormatting("New Vessel Call Sign", FixWidthColumnFormat),
				new CellWithFormatting(addInfoCollection.GetZStringValue(AddInfoConstants.Header.VesselCallSignNew) ?? ZString.Empty));
			var newOperatorVoyage = addInfoCollection.GetZStringValue(AddInfoConstants.Header.OperationalCarrierVoyageNoNew) ?? ZString.Empty;
			if (!newOperatorVoyage.IsEmpty)
			{
				outputCreator.WriteRow(new CellWithFormatting("New Operator Voyage", FixWidthColumnFormat), new CellWithFormatting(newOperatorVoyage));
			}
			outputCreator.WriteRow(new CellWithFormatting("New Port of Loading Code", FixWidthColumnFormat),
				new CellWithFormatting(addInfoCollection.GetZStringValue(AddInfoConstants.Header.PortOfLoadingCodeNew) ?? ZString.Empty));
			outputCreator.WriteRow(new CellWithFormatting("New ETD", FixWidthColumnFormat),
				new CellWithFormatting(addInfoCollection.GetZStringValue(AddInfoConstants.Header.EstimatedDateTimeOfDepartureNew) ?? ZString.Empty));
		}

		static void GenerateOutputForATD(UniversalShipment shipment, HtmlTableCreator htmlCreator)
		{
			var etd = shipment.DateCollection.FirstOrDefault(DateType.Departure, ZBool.False);
			var etdValue = etd?.Value;
			htmlCreator.WriteRow(new CellWithFormatting("Departure Time", FixWidthColumnFormat), new CellWithFormatting(etdValue == null ? string.Empty : etdValue.ToString()));
		}

		public static void GenerateOutputForContainers(HtmlTableCreator htmlCreator, DataObjectList<Container> containersList)
		{
			string result = null;
			if (containersList != null && containersList.Count > 0)
			{
				var tableCreator = new HtmlTableCreator("", new[] { "Manifested Container Number", "Seal 1", "Seal2" }, FittingWidthFormat);//BasicTableFormat);
				foreach (var container in containersList)
				{
					tableCreator.WriteRow(
						new CellWithFormatting(container.ContainerNumber, FixWidthColumnFormat),
						new CellWithFormatting(container.Seal),
						new CellWithFormatting(container.SecondSeal)
						);
				}
				result = tableCreator.ToHtml();
			}
			htmlCreator.WriteRow(new CellWithFormatting((result ?? NoContainerContained), CrossColumnFormat));
		}

		public static void GenerateOutputForSubshipments(HtmlTableCreator outputCreator, DataObjectList<UniversalShipment> shipmentsList, BusinessObjectFactory factory)
		{
			Argument.NotNull(outputCreator, "outputCreator");
			Argument.NotNull(factory, "factory");

			if (shipmentsList != null)
			{
				var isRegisterCompletion = false;
				var messageAction = string.Empty;
				if (shipmentsList.Count == 1)
				{
					var addInfoCollection = shipmentsList[0] == null ? null : shipmentsList[0].AddInfoCollection;
					messageAction = ListHelper.GetDescription(shipmentsList[0] == null ? FunctionTypeList.Codes.Registration : shipmentsList[0].DataContext.ActionPurposeCode.ToString(), factory.GetCachedValue<FunctionTypeList>());
					var completionFlag = addInfoCollection == null ? null : addInfoCollection.GetZBoolValue(AddInfoConstants.Bill.HouseBillRegisterCompletion);
					isRegisterCompletion = completionFlag.HasValue && completionFlag.Value;
				}
				if (isRegisterCompletion)
				{
					outputCreator.WriteRow(
						new CellWithFormatting("Register Completion", FixWidthColumnFormat),
						new CellWithFormatting(string.Format(CompletionMessage, messageAction))
						);
				}
				else
				{
					foreach (var shipment in shipmentsList)
					{
						outputCreator.WriteRow(
							new CellWithFormatting("Included Bill", FixWidthColumnFormat),
							new CellWithFormatting(GenerateOutputForUniversalShipment(shipment, false, factory))
							);
					}
				}
			}
		}

		#endregion

		public static ZString FormatOutputInTemplate(string detailBody)
		{
			var result = ZString.Empty;
			using (Stream stream = typeof(MessageInterpretationGenerator).Assembly.GetManifestResourceStream("Enterprise.Customs.JP.AFR.Business.BusinessObject.EDIMessage.MessageDetailTemplate.htm"))
			{
				result = new StreamReader(stream).ReadToEnd();
			}

			result = result.Replace("<!--Style Sheet Section-->", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
			result = result.Replace("<!--Message Detail Section-->", detailBody);
			return result;
		}
	}
}
