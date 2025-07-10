using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Edifact.D00A.Segments;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers
{
	public class CargoFactMessageProcessor
	{
		public CargoFactMessageProcessor(CargoFactMessage cargoFact, EDIMessage inboundEdiMessageForAuditing, ILogger iLogger)
		{
			this.cargoFact = cargoFact;
			this.inboundEdiMessageForAuditing = inboundEdiMessageForAuditing;
			this.iLogger = iLogger;
		}

		internal bool DoProcessing()
		{
			var cimParser = GetCargoImpFromCargoFact();
			cimParser.ServiceLogger = iLogger;
			inboundEdiMessageForAuditing.EM_MessageType = "CIM";
			inboundEdiMessageForAuditing.EM_MessageSubType = cimParser.CargoImpCode;

			if (!HandleUnsupportedTypes(cimParser, inboundEdiMessageForAuditing))
			{
				var foundConsignmentAndProcessedOk = cimParser.DoAllProcessingBeforePrinting();
				var needReprocess = false;
				inboundEdiMessageForAuditing.EM_MessageInterpretation = cimParser.MessageInterpretation;
				if (cimParser.Awb != null)
				{
					cimParser.Awb.Messages.Add(inboundEdiMessageForAuditing);
					inboundEdiMessageForAuditing.EM_GB = cimParser.Awb.Branch.PK;
					inboundEdiMessageForAuditing.EM_ApplicationReference = cimParser.Awb.SplitReference;
					if (foundConsignmentAndProcessedOk)
					{
						cimParser.DoPrinting();
					}
					iLogger.Log(LogType.Information, () => $"Parse CIM {cimParser.CargoImpCode} for consignment {cimParser.Awb.ReferenceNumber}, message number {inboundEdiMessageForAuditing.EM_MessageNum}");
				}
				else if (!foundConsignmentAndProcessedOk)
				{
					var interchange = inboundEdiMessageForAuditing.Interchange;
					if (interchange?.EI_RetryCount < MaxRetryAttempts)
					{
						needReprocess = true;
						interchange.EI_RetryCount++;
						inboundEdiMessageForAuditing.EM_HeldUntilDate = ZDateTime.UtcNow.Add(RetryInterval);
						ServiceTaskHelper.NudgeServiceTaskDelay(null, ServiceTask.CcsukServiceTaskConstants.CcsukInterchangePackagerServiceTaskCode, RetryInterval);
					}
					iLogger.Log(LogType.Warning, () => $"CargoFact processor: Could not find or process consignment for message number {inboundEdiMessageForAuditing.EM_MessageNum}{(needReprocess ? " but will retry" : "")}");
				}
				if (!needReprocess)
				{
					inboundEdiMessageForAuditing.EM_Status = foundConsignmentAndProcessedOk ? EDIMessage.Status.Received : EDIMessage.Status.Error;
				}
			}
			return true;  // Even if we did not find a consignment, we could still parse the message
		}

		ZBool HandleUnsupportedTypes(ICimParser parser, EDIMessage inboundEdiMessageForAuditing)
		{
			var unsupportedType = ZBool.False;
			if (messageTypesNotToBeProcessed.Contains(parser.CargoImpCode))
			{
				inboundEdiMessageForAuditing.EM_Status = EDIMessage.Status.Failed;
				iLogger.Log(LogType.Warning, ZString.Format("Message Type '{0}' is an unexpected message type and will not be processed", parser.CargoImpCode));
				unsupportedType = ZBool.True;
			}

			return unsupportedType;
		}

		readonly ZString[] messageTypesNotToBeProcessed = { CIMFSU.Code };

		public static ZInt MaxRetryAttempts => 5;

		public static TimeSpan RetryInterval => TimeSpan.FromMinutes(5);

		public ICimParser GetCargoImpFromCargoFact()
		{
			var cargoFactCode = cargoFact.UNH[0].MessageIdentifier.MessageType;
			var commonAccessReference = cargoFact.UNH[0].CommonAccessReference;
			var textLines = new List<string>();
			foreach (FTXSegment ftx in cargoFact.FTX)
			{
				if (ftx.TextSubjectCodeQualifier == "CIM")
				{
					if (!string.IsNullOrWhiteSpace(ftx.TextLiteral.FreeTextValue1))
					{
						textLines.Add(ftx.TextLiteral.FreeTextValue1);
					}

					if (!string.IsNullOrWhiteSpace(ftx.TextLiteral.FreeTextValue2))
					{
						textLines.Add(ftx.TextLiteral.FreeTextValue2);
					}

					if (!string.IsNullOrWhiteSpace(ftx.TextLiteral.FreeTextValue3))
					{
						textLines.Add(ftx.TextLiteral.FreeTextValue3);
					}

					if (!string.IsNullOrWhiteSpace(ftx.TextLiteral.FreeTextValue4))
					{
						textLines.Add(ftx.TextLiteral.FreeTextValue4);
					}

					if (!string.IsNullOrWhiteSpace(ftx.TextLiteral.FreeTextValue5))
					{
						textLines.Add(ftx.TextLiteral.FreeTextValue5);
					}
				}
			}

			switch (cargoFactCode.Substring(3, 3))
			{
				case CIMFSN.Code:
					return new CIMFSN(textLines, inboundEdiMessageForAuditing);
				case CIMFMA.Code:
					return new CIMFMA(textLines, inboundEdiMessageForAuditing.Factory, cargoFact);
				case CIMFNA.Code:
					return new CIMFNA(textLines, inboundEdiMessageForAuditing.Factory, cargoFact);
				case CIMFSR.Code:
					return new CIMFSR(textLines, inboundEdiMessageForAuditing, commonAccessReference);
				case CIMFRN.Code:
					return new CIMFRN(textLines, inboundEdiMessageForAuditing);
				case CIMFRD.Code:
					return new CIMFRD(textLines, inboundEdiMessageForAuditing, cargoFact);
				case CIMFSU.Code:
					return new CIMFSU(textLines, inboundEdiMessageForAuditing);
				case CcsukTransmissionMessageFunction.CIM.FSA.SubCode:
					return new CIMFSA(textLines, inboundEdiMessageForAuditing, commonAccessReference);
				default:
					throw new NotImplementedException(cargoFactCode + " not yet supported, cannot parse");
			}
		}

		readonly CargoFactMessage cargoFact;
		readonly EDIMessage inboundEdiMessageForAuditing;
		readonly ILogger iLogger;
	}
}
