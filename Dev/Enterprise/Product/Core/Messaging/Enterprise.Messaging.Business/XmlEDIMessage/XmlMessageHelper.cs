using System;
using System.IO;
using System.Xml;
using CargoWise.Common;
using Enterprise.Messaging.Integration;

namespace Enterprise.Messaging.Business.XmlMessaging
{
	public static class XmlMessageHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "fixed tag name expected to be present in xml files produced by verbose export")]
		public static string GetMessageSubTypeFromXml(Stream stream)
		{
			stream.Position = 0;
			XmlTextReader reader = new XmlTextReader(stream);

			try
			{
				reader.ReadToFollowing("Payload");
				while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.Element)
					{
						break;
					}
				}

				if (reader.EOF)
				{
					return EDIMessageSubTypeList.Codes.Unknown;
				}

				return GetMessageSubType(reader.LocalName);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				return EDIMessageSubTypeList.Codes.Unknown;
			}
		}

		public static string GetMessageSubType(string nodeName)
		{
			return GetMessageSubType(nodeName, string.Empty);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Unable to reference DataTransfer to use constant, XML element name")]
		public static string GetMessageSubType(string nodeName, string targetType)
		{
			switch (nodeName)
			{
				case EDIMessageSubTypeXMLElementList.Descriptions.Consols:
					if (targetType.Equals("Declaration"))
					{
						return EDIMessageSubTypeXMLElementList.Codes.Brokerage;
					}
					if (targetType.Equals(EDIMessageSubTypeXMLElementList.Descriptions.CFSLoadList))
					{
						return EDIMessageSubTypeXMLElementList.Codes.CFSLoadList;
					}
					else
					{
						return EDIMessageSubTypeXMLElementList.Codes.Consols;
					}
				case EDIMessageSubTypeXMLElementList.Descriptions.CartageJobs:
					switch (targetType)
					{
						case EDIMessageSubTypeXMLElementList.Descriptions.LocalCartageStatus:
							return EDIMessageSubTypeXMLElementList.Codes.LocalCartageStatus;
						default:
							return EDIMessageSubTypeXMLElementList.Codes.LocalCartageBooking;
					}
				case "Native":
				case "ReferenceData":
					return GetMessageSubTypeForNative(targetType);
				case "":
					return "";
				default:
					return new EDIMessageSubTypeXMLElementList().GetCodeFromDescription(nodeName) ?? EDIMessageSubTypeXMLElementList.Codes.Unknown;
			}
		}

		public static string GetMessageSubTypeForNative(string targetType)
		{
			return new EDIMessageSubTypeXMLElementList().GetCodeFromDescription(targetType) ?? EDIMessageSubTypeXMLElementList.Codes.Unknown;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static string GetMessageSchemaName(EDIInterchange interchange)
		{
			switch (interchange.EI_ApplicationCode)
			{
				case ApplicationCodeList.Codes.XMS:
					return GetSchemaNameFromMessageOrEmptyString(interchange, message => GetMessageSchemaNameForXMS(message.EM_MessageSubType));

				case ApplicationCodeList.Codes.CIM:
					return GetSchemaNameFromMessageOrEmptyString(interchange, message =>
					{
						switch (message.EM_MessageType)
						{
							case EDIMessageTypeList.Codes.FHL:
								return EDIMessageSchemaNameList.Descriptions.FHL;

							case EDIMessageTypeList.Codes.FWB:
								return EDIMessageSchemaNameList.Descriptions.FWB;

							default:
								return string.Empty;
						}
					});

				case ApplicationCodeList.Codes.Inttra:
				case ApplicationCodeList.Codes.ShippingLineEHubMessaging:
					return EDIMessageSchemaNameList.Descriptions.InttraEdifact;

				case ApplicationCodeList.Codes.ZACustoms:
					return EDIMessageSchemaNameList.Descriptions.ZACustoms; // TODO: We will decide to use the schemaname or skip the prevalidate in EHubMessageBuilder.PreCheck() when developing the GateWay process, we use this temporary for now.

				case ApplicationCodeList.Codes.USeManifest:
				case ApplicationCodeList.Codes.USAMA:
				case ApplicationCodeList.Codes.USAMS:
				case ApplicationCodeList.Codes.USCustomsExport:
				case ApplicationCodeList.Codes.USCustomsImport:
				case ApplicationCodeList.Codes.USExportManifest:
				case ApplicationCodeList.Codes.StowPlan:
					return GetSchemaNameFromMessageOrEmptyString(interchange, message => message.EM_MessageType);

				case ApplicationCodeList.Codes.SYS:
				case ApplicationCodeList.Codes.UniversalDataMessaging:
				case ApplicationCodeList.Codes.USeBond:
				case ApplicationCodeList.Codes.AirCargoAdvanceScreening:
					return EDIMessageSchemaNameList.Descriptions.UniversalDataMessaging;

				case ApplicationCodeList.Codes.NativeDataMessaging:
					return EDIMessageSchemaNameList.Descriptions.NativeDataMessaging;

				case ApplicationCodeList.Codes.CustomsWare:
					return EDIMessageSchemaNameList.Descriptions.CustomsWare;

				case ApplicationCodeList.Codes.NZCustoms:
				case ApplicationCodeList.Codes.NZMAFeBACCa:
					return EDIMessageSchemaNameList.Descriptions.NZCustoms;

				case ApplicationCodeList.Codes.AUCMR:
					return EDIMessageSchemaNameList.Descriptions.AUCustoms;

				case ApplicationCodeList.Codes.USCustomsDIS:
					return EDIMessageSchemaNameList.Descriptions.USDDIS;

				case ApplicationCodeList.Codes.CACustoms:
				case ApplicationCodeList.Codes.CAACI:
				case ApplicationCodeList.Codes.CAIMP:
				case ApplicationCodeList.Codes.CAEXP:
					return EDIMessageSchemaNameList.Descriptions.CanadianCustoms;
				case ApplicationCodeList.Codes.ChinaInterfaceMapping:
					return EDIMessageSchemaNameList.Descriptions.ChinaInterface;

				case ApplicationCodeList.Codes.HKTraxon:
					return EDIMessageSchemaNameList.Descriptions.HKCustoms;
				case ApplicationCodeList.Codes.GbCustomsDeclarationServices:
					return EDIInterchangeTypeList.Descriptions.GBCustoms;
				case ApplicationCodeList.Codes.GlobalElectronicInvoice:
					return EDIInterchangeTypeList.Descriptions.GlobalElectronicInvoice;
				case ApplicationCodeList.Codes.GlobalElectronicPayment:
					return EDIInterchangeTypeList.Descriptions.GlobalElectronicPayment;
				case ApplicationCodeList.Codes.GenericMessageDelivery:
					return EDIInterchangeTypeList.Descriptions.GenericMessageDelivery;
				case ApplicationCodeList.Codes.SGCustomsCMD:
					return EDIMessageSchemaNameList.Descriptions.SGCustomsCMD;
				case ApplicationCodeList.Codes.AUCustomsNEXDOC:
					return EDIInterchangeTypeList.Descriptions.AUCustomsNEXDOC;
				case ApplicationCodeList.Codes.ITCustoms:
					return EDIInterchangeTypeList.Descriptions.ITCustoms;
				case ApplicationCodeList.Codes.ESCustomsMessage:
					return EDIInterchangeTypeList.Descriptions.ESCustoms;
				case ApplicationCodeList.Codes.TRCustoms:
					return EDIInterchangeTypeList.Descriptions.TRCustomsGlobalManifest;
				case ApplicationCodeList.Codes.TRETrade:
					return EDIInterchangeTypeList.Descriptions.TRCustomsETradeRegistration;
				default:
					return string.Empty;
			}
		}

		static string GetSchemaNameFromMessageOrEmptyString(EDIInterchange interchange, Func<EDIMessage, string> getSchemaNameFromMessage)
		{
			if (interchange.ContainedMessages.Count == 0)
			{
				return string.Empty;
			}

			return getSchemaNameFromMessage(interchange.ContainedMessages[0]);
		}

		static string GetMessageSchemaNameForXMS(string messageSubType)
		{
			switch (messageSubType)
			{
				case EDIMessageSubTypeList.Codes.AgencyBillsOfLading: return EDIMessageSchemaNameList.Descriptions.AgencyBillsOfLading;
				case EDIMessageSubTypeList.Codes.BankStatements: return EDIMessageSchemaNameList.Descriptions.BankStatements;
				case EDIMessageSubTypeList.Codes.Consols: return EDIMessageSchemaNameList.Descriptions.Consols;
				case EDIMessageSubTypeList.Codes.ContainerMovements: return EDIMessageSchemaNameList.Descriptions.ContainerMovements;
				case EDIMessageSubTypeList.Codes.Events: return EDIMessageSchemaNameList.Descriptions.Events;
				case EDIMessageSubTypeList.Codes.Organizations: return EDIMessageSchemaNameList.Descriptions.Organizations;
				case EDIMessageSubTypeList.Codes.FinancialTransactions: return EDIMessageSchemaNameList.Descriptions.FinancialTransactions;
				case EDIMessageSubTypeList.Codes.NettingClearingJournals: return EDIMessageSchemaNameList.Descriptions.NettingClearingJournals;
				case EDIMessageSubTypeList.Codes.Invoices: return EDIMessageSchemaNameList.Descriptions.Invoices;
				case EDIMessageSubTypeList.Codes.ISFs: return EDIMessageSchemaNameList.Descriptions.ISFs;
				case EDIMessageSubTypeList.Codes.Orders: return EDIMessageSchemaNameList.Descriptions.Orders;
				case EDIMessageSubTypeList.Codes.Products: return EDIMessageSchemaNameList.Descriptions.Products;
				case EDIMessageSubTypeList.Codes.Schedules: return EDIMessageSchemaNameList.Descriptions.Schedules;
				case EDIMessageSubTypeList.Codes.Shipments: return EDIMessageSchemaNameList.Descriptions.Shipments;
				case EDIMessageSubTypeList.Codes.ShipmentBookings: return EDIMessageSchemaNameList.Descriptions.ShipmentBookings;
				case EDIMessageSubTypeList.Codes.WhsDockets: return EDIMessageSchemaNameList.Descriptions.WhsDockets;
				case EDIMessageSubTypeList.Codes.LocalCartageBooking: return EDIMessageSchemaNameList.Descriptions.LocalCartageBooking;
				case EDIMessageSubTypeList.Codes.LocalCartageStatus: return EDIMessageSchemaNameList.Descriptions.LocalCartageStatus;
				case EDIMessageSubTypeList.Codes.DocumentMessages: return EDIMessageSchemaNameList.Descriptions.DocumentMessages;
			}
			return string.Empty;
		}
	}
}
