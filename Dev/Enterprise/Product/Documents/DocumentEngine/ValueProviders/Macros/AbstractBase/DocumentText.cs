using System;
using System.Text.RegularExpressions;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	abstract class DocumentText : ValueProvider
	{
		[CodeStringFinderHint(typeof(DocumentTextReplacer), "GetDocumentOpenCloseTextProvider")]
		protected override object GetReplacementCore(string macro, Report report)
		{
			Match match = Regex.Match(macro);

			string fieldList = match.Groups["fieldList"].ToString().Trim();
			string modePrefix = match.Groups["modePrefix"].ToString().Trim();

			return new DocumentTextReplacer(fieldList, modePrefix, report, this).GetRegistryValue();
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "regular expression")]
		protected const string regexParameters = @"[^\(\)]*[\(]?(?<fieldList>[^,\(\)]*)(|,\s*(?<modePrefix>[^,\(\)]+))[\)]?";

		protected abstract DocumentTextType DocumentTextType { get; }

		class DocumentTextReplacer
		{
			internal DocumentTextReplacer(string fieldList, string modePrefix, Report report, DocumentText parentMacro)
			{
				this.report = report;
				this.parentMacro = parentMacro;

				if (report != null && report.MacroTranslator != null)
				{
					if (!string.IsNullOrEmpty(fieldList))
					{
						ZStringBuilder registryKey = new ZStringBuilder();
						foreach (var fieldName in fieldList.Split(' '))
						{
							if (fieldName.Equals("ReportName", StringComparison.InvariantCultureIgnoreCase))
							{
								registryKey.Append(report.Name);
							}
							else
							{
								registryKey.Append(report.MacroTranslator.GetValue("<" + fieldName + ">", Passes.FirstPass).ToString());
							}
						}

						this.registryKey = registryKey.ToStringWithDelimiterBetweenAppends(" ").Trim();
					}
					else
					{
						this.registryKey = "";
					}

					if (!string.IsNullOrEmpty(modePrefix))
					{
						this.modePrefix = report.MacroTranslator.GetValue("<" + modePrefix + ">", Passes.FirstPass).ToString().ToUpperInvariant();
					}
				}
			}

			readonly string modePrefix;
			readonly string registryKey;
			readonly Report report;
			readonly DocumentText parentMacro;

			DocumentTextType documentTextType
			{
				get { return parentMacro.DocumentTextType; }
			}

			DocumentDirection direction
			{
				get { return report.Direction; }
			}

			ZString businessContext
			{
				get { return report.MenuItem != null ? report.MenuItem.SU_BusinessContext : ZString.Empty; }
			}

			internal string GetRegistryValue()
			{
				bool resultFound = false;
				DocumentOpenCloseText resultValue = new DocumentOpenCloseText();
				if (!string.IsNullOrEmpty(registryKey))
				{
					if (!string.IsNullOrEmpty(modePrefix))
					{
						resultFound = GetDocumentOpenCloseTextProvider(modePrefix + " " + registryKey, ref resultValue);
					}

					if (!resultFound)
					{
						resultFound = GetDocumentOpenCloseTextProvider(registryKey, ref resultValue);
					}

					if (!resultFound)
					{
						DocumentOpenCloseText airValue, seaValue;
						airValue = seaValue = resultValue;
						if (GetDocumentOpenCloseTextProvider("AIR " + registryKey, ref airValue)
							&& GetDocumentOpenCloseTextProvider("SEA " + registryKey, ref seaValue)
							&& airValue.Text(documentTextType) == seaValue.Text(documentTextType))
						{
							resultFound = true;
							resultValue = airValue;
						}
					}
				}

				if (!resultFound || string.IsNullOrEmpty(resultValue.Text(documentTextType)))
				{
					resultValue = Env.Registry.DefaultDocumentText;
				}

				return GetTextFrom(resultValue);
			}

			string GetTextFrom(DocumentOpenCloseText documentOpenCloseText)
			{
				return documentOpenCloseText.Text(documentTextType).Replace("\r", "");
			}

			bool GetDocumentOpenCloseTextProvider(string fullRegistryKey, ref DocumentOpenCloseText result)
			{
				switch (fullRegistryKey.ToUpperInvariant())
				{
					#region AIR Freight Docs

					case Constants.DocumentNames.AirPreAlert:
						result = Env.Registry.ImportAirPreAlert;
						return true;

					case Constants.DocumentNames.AirArrivalNotice:
						result = Env.Registry.ImportAirArrivalNotice;
						return true;

					case Constants.DocumentNames.AirShippingAdvice:
						result = Env.Registry.ImportAirShippingAdvice;
						return true;

					case Constants.DocumentNames.AirDeliveryOrder:
						result = Env.Registry.ImportAirFreightDeliveryOrder;
						return true;

					case Constants.DocumentNames.AirUltimateConsigneePreAlert:
						result = Env.Registry.ImportAirUltimateConsigneePreAlert;
						return true;

					case Constants.DocumentNames.AirUltimateConsigneeArrivalNotice:
						result = Env.Registry.ImportAirUltimateConsigneeArrivalNotice;
						return true;

					case Constants.DocumentNames.AirOutturnReport:
						result = Env.Registry.ImportAirOutturnReport;
						return true;

					case Constants.DocumentNames.AirLetterToOverseasAgent:
						result = Env.Registry.ExportAirLetterToOverseasAgent;
						return true;

					case Constants.DocumentNames.AirShipperDepartureNotice:
						result = Env.Registry.ExportAirFreightShipperDepartureNotice;
						return true;

					case Constants.DocumentNames.AirAgentsInstruction:
						result = Env.Registry.AirAgentsInstruction;
						return true;

					case Constants.DocumentNames.AirBookingConfirmation:
						result = Env.Registry.ExportAirBookingConfirmation;
						return true;

					case Constants.DocumentNames.AirCargoWeightsAndMeasurementsReport:
						result = Env.Registry.AirWeightsAndMeasurements;
						return true;

					case Constants.DocumentNames.AirAgentDepartureNotice:
						result = Env.Registry.ExportAirFreightAgentDepartureNotice;
						return true;

					#endregion

					#region SEA Freight Documents

					case Constants.DocumentNames.SeaPreAlert:
						result = Env.Registry.ImportSeaFreightPreAlert;
						return true;

					case Constants.DocumentNames.SeaArrivalNotice:
						result = Env.Registry.ImportSeaFreightArrivalNotice;
						return true;

					case Constants.DocumentNames.SeaShippingAdvice:
						result = Env.Registry.ImportSeaShippingAdvice;
						return true;

					case Constants.DocumentNames.SeaDeliveryOrder:
						result = Env.Registry.ImportSeaFreightDeliveryOrder;
						return true;

					case Constants.DocumentNames.SeaUltimateConsigneePreAlert:
						result = Env.Registry.ImportSeaUltimateConsigneePreAlert;
						return true;

					case Constants.DocumentNames.SeaUltimateConsigneeArrivalNotice:
						result = Env.Registry.ImportSeaUltimateConsigneeArrivalNotice;
						return true;

					case Constants.DocumentNames.SeaAgentsInstruction:
						result = Env.Registry.SeaAgentsInstruction;
						return true;

					case Constants.DocumentNames.SeaLetterToOverseasAgent:
						result = Env.Registry.ExportSeaLetterToOverseasAgent;
						return true;

					case Constants.DocumentNames.SeaShipperDepartureNotice:
						result = Env.Registry.ExportSeaFreightShipperDepartureNotice;
						return true;

					case Constants.DocumentNames.SeaAgentDepartureNotice:
						result = Env.Registry.ExportSeaFreightAgentDepartureNotice;
						return true;

					case Constants.DocumentNames.SeaOutturnReport:
						result = Env.Registry.ImportSeaOutturnReport;
						return true;

					case Constants.DocumentNames.SeaBookingConfirmation:
						result = Env.Registry.ExportSeaBookingConfirmation;
						return true;

					case Constants.DocumentNames.SeaCargoWeightsAndMeasurementsReport:
						result = Env.Registry.SeaWeightsAndMeasurements;
						return true;

					case Constants.DocumentNames.AgencyShipmentArrivalNotice:
						result = DocumentsDataRegistry.Instance.AgencyShipmentArrivalNotice;
						return true;

					#endregion

					#region Cartage Advices

					case Constants.DocumentNames.ShipmentCartageAdvice:
					case Constants.DocumentNames.ShipmentCartageAdviceWithReceipt:
					case Constants.DocumentNames.ShipmentCartageAdviceWithRouting:
					case Constants.DocumentNames.ShipmentCartageAdviceWithRoutingWithReceipt:
					case Constants.DocumentNames.ConfirmationCartageAdvice:
					case Constants.DocumentNames.ConfirmationCartageAdviceWithReceipt:
						result = (direction == DocumentDirection.ARV) ? Env.Registry.CartageAdviceImport : Env.Registry.CartageAdviceExport;
						return true;

					case Constants.DocumentNames.CartageAdviceTimeSlotRequest:
						result = (direction == DocumentDirection.ARV) ? new DocumentOpenCloseText(Env.Registry.CartageAdviceTimeSlotRequestImportOpeningText, "") : new DocumentOpenCloseText(Env.Registry.CartageAdviceTimeSlotRequestExportOpeningText, "");
						return true;

					case Constants.DocumentNames.CartageAdviceTimeSlotConfirmation:
						result = new DocumentOpenCloseText(Env.Registry.CartageAdviceTimeSlotConfirmationOpeningText, Env.Registry.CartageAdviceTimeSlotConfirmationClosingText);
						return true;

					case Constants.DocumentNames.CFSCartageAdvice:
					case Constants.DocumentNames.CFSCartageAdviceWithReceipt:
					case Constants.DocumentNames.CFSShipmentCartageAdvice:
					case Constants.DocumentNames.CFSShipmentCartageAdviceWithReceipt:
						result = Env.Registry.CFSCartageAdvice;
						return true;

					case Constants.DocumentNames.BookingCartageAdvice:
					case Constants.DocumentNames.BookingCartageAdviceWithReceipt:
						result = Env.Registry.BookingCartageAdvice;
						return true;

					//Old
					case Constants.DocumentNames.LocalCartageAdviceWithReceipt:
					case Constants.DocumentNames.ContainerLegCartageAdvice:
					case Constants.DocumentNames.ContainerLegCartageAdviceWithReceipt:
					case Constants.DocumentNames.CartageLegCartageAdvice:
					//DocBuilder
					case Constants.DocumentNames.LocalCartageAdvice:
					case Constants.DocumentNames.LocalMultiContainerCartageAdvice:
						result = Env.Registry.LocalCartageCartageAdvice;
						return true;

					case Constants.DocumentNames.CustomsDeclarationCartageAdvice:
					case Constants.DocumentNames.CustomsDeclarationCartageAdviceWithReceipt:
					case Constants.DocumentNames.CustomsDeclarationMultiContainerCartageAdvice:
					case Constants.DocumentNames.CustomsDeclarationMultiContainerCartageAdviceWithReceipt:
						result = Env.Registry.CustomsDeclarationCartageAdvice;
						return true;

					case Constants.DocumentNames.CartageAdvice:
					case Constants.DocumentNames.CartageAdviceWithReceipt:
						result = new DocumentOpenCloseText(DocumentsDataRegistry.Instance.TransportBookingCartageAdviceOpeningText.Value, DocumentsDataRegistry.Instance.TransportBookingCartageAdviceClosingText.Value);
						return true;

					#endregion

					#region Declaration Docs

					case Constants.DocumentNames.RequestForMissingDocuments:
						if (businessContext == nameof(BusinessContext.Customs))
						{
							result = Env.Registry.CustomsRequestForMissingDocuments;
						}
						else
						{
							result = Env.Registry.ShipmentRequestForMissingDocuments;
						}
						return true;

					case Constants.DocumentNames.RequestForCollectCharges:
						result = new DocumentOpenCloseText(DocumentsDataRegistry.Instance.RequestForCollectChargesOpeningText.Value, DocumentsDataRegistry.Instance.RequestForCollectChargesClosingText.Value);
						return true;

					case Constants.DocumentNames.EFTRequest:
						result = new DocumentOpenCloseText(DocumentsDataRegistry.Instance.EFTRequest.OpeningText, DocumentsDataRegistry.Instance.EFTRequest.ClosingText);
						return true;

					#endregion

					#region Agency Docs

					case Constants.DocumentNames.ContainerRelease:
					case Constants.DocumentNames.ContainerReleaseAuthorization:
						result = DocumentsDataRegistry.Instance.ContainerRelease;
						return true;

					case Constants.DocumentNames.DetentionAdvice:
						result = new DocumentOpenCloseText(DocumentsDataRegistry.Instance.DetentionAdviceOpeningText.Value, DocumentsDataRegistry.Instance.DetentionAdviceClosingText.Value);
						return true;

					case Constants.DocumentNames.AgencyBookingConfirmation:
						result = new DocumentOpenCloseText(DocumentsDataRegistry.Instance.AgencyBookingConfirmationOpeningText.Value, DocumentsDataRegistry.Instance.AgencyBookingConfirmationClosingText.Value);
						return true;

					#endregion

					#region Freight Docs

					case Constants.DocumentNames.CarrierBookingRequest:
						result = new DocumentOpenCloseText(DocumentsDataRegistry.Instance.CarrierBookingRequestOpeningText.Value, DocumentsDataRegistry.Instance.CarrierBookingRequestClosingText.Value);
						return true;

					case Constants.DocumentNames.DeliveryInformation:
						result = new DocumentOpenCloseText(DocumentsDataRegistry.Instance.DeliveryInformationOpeningText.Value, DocumentsDataRegistry.Instance.DeliveryInformationClosingText.Value);
						return true;

					case Constants.DocumentNames.ShippingOrder:
						result = new DocumentOpenCloseText("", DocumentsDataRegistry.Instance.ShippingOrderClosingText.Value);
						return true;

					case Constants.DocumentNames.ContainerDetentionReminder:
						result = new DocumentOpenCloseText(DocumentsDataRegistry.Instance.ContainerDetentionReminderOpeningText.Value, "");
						return true;

					case Constants.DocumentNames.DelayAlert:
						result = new DocumentOpenCloseText(DocumentsDataRegistry.Instance.DelayAlertOpeningText.Value, DocumentsDataRegistry.Instance.DelayAlertClosingText.Value);
						return true;

					case Constants.DocumentNames.LetterOfIndemnity:
						result = DocumentsDataRegistry.Instance.LetterOfIndemnity;
						return true;

					case Constants.DocumentNames.ExportCertification:
						result = new DocumentOpenCloseText(Env.Registry.ExportCertificationOpeningText, Env.Registry.ExportCertificationClosingText);
						return true;

					case Constants.DocumentNames.FOBAndienung:
					case Constants.DocumentNames.Versicherungsanmeldung:
						result = new DocumentOpenCloseText("", DocumentsDataRegistry.Instance.FOBAndienungClosingText.Value);
						return true;

					case Constants.DocumentNames.RequestForService:
						result = new DocumentOpenCloseText(DocumentsDataRegistry.Instance.RequestForServiceOpeningText.Value, DocumentsDataRegistry.Instance.RequestForServiceClosingText.Value);
						return true;

					case Constants.DocumentNames.AuthorisationForService:
						result = new DocumentOpenCloseText(DocumentsDataRegistry.Instance.AuthorisationForServiceOpeningText.Value, DocumentsDataRegistry.Instance.AuthorisationForServiceClosingText.Value);
						return true;

					case Constants.DocumentNames.RoutingOrder:
						result = new DocumentOpenCloseText(Env.Registry.RoutingOrderOpeningText, Env.Registry.RoutingOrderClosingText);
						return true;

					#endregion

					#region Rating / Quotation docs

					case Constants.DocumentNames.OneOffPricingPage:
					case Constants.DocumentNames.OneOffPricingPageMultipleCarriers:
						result = new DocumentOpenCloseText(DocumentsDataRegistry.Instance.QuoteOpeningText.Value, DocumentsDataRegistry.Instance.QuoteClosingText.Value);
						return true;

					case Constants.DocumentNames.QuotationAcceptance:
						result = new DocumentOpenCloseText(DocumentsDataRegistry.Instance.AcceptancePageOpeningText.Value, DocumentsDataRegistry.Instance.AcceptancePageClosingText.Value);
						return true;

					#endregion

					#region Order Docs

					case Constants.DocumentNames.ImportOrderAdvice:
						result = Env.Registry.ImportOrderAdvice;
						return true;

					case Constants.DocumentNames.ExportOrderAdvice:
						result = Env.Registry.ExportOrderAdvice;
						return true;

					case Constants.DocumentNames.ImportOrderNotification:
						result = Env.Registry.ImportOrderNotification;
						return true;

					case Constants.DocumentNames.ExportOrderNotification:
						result = Env.Registry.ExportOrderNotification;
						return true;

					case Constants.DocumentNames.ImportOrderStatus:
						result = Env.Registry.ImportOrderStatus;
						return true;

					case Constants.DocumentNames.ExportOrderStatus:
						result = Env.Registry.ExportOrderStatus;
						return true;

					case Constants.DocumentNames.OrderShippedOnBoardAdvice:
						result = Env.Registry.ImportOrderShippedOnBoardAdvice;
						return true;

					case Constants.DocumentNames.OrderAmendmentToBooking:
						result = Env.Registry.ImportOrderAmendmentToBooking;
						return true;

					#endregion

					#region Dangerous Goods

					case Constants.DocumentNames.IMODangerousGoodsDeclaration:
					case Constants.DocumentNames.DangerousPackingCertificate:
						result = new DocumentOpenCloseText("", DocumentsDataRegistry.Instance.DangerousGoodsStatement.Value);
						return true;

					#endregion

					#region Domestic Transport

					case Constants.DocumentNames.ConsignmentRequestForService:
						result = Env.Registry.ConsignmentRequestForService;
						return true;

					case Constants.DocumentNames.ConsignmentAuthorizationForService:
						result = Env.Registry.ConsignmentAuthorizationForService;
						return true;

					#endregion

					#region Default

					default:
						return false;

						#endregion
				}
			}
		}
	}
}
