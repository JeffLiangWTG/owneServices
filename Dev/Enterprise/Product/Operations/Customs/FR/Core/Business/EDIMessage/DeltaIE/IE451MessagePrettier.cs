using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE451;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IE451MessagePrettier : DeltaIEMessagePrettier<CC451BType>
	{
		public IE451MessagePrettier(IE451MessageDataObject messageDataObject)
			: base(messageDataObject)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Core strings")]
		protected override ZString GetMessageInterpretationCore(CC451BType messageObject)
		{
			var importOperation = messageObject.ImportOperation;
			var declarationStatus = messageObject.DeclarationStatus;

			var result = ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				("Decision Date", importOperation?.DecisionDate ?? ZString.Empty),
				("Decision Reason", importOperation?.DecisionReason ?? ZString.Empty),
				("Declaration Status", declarationStatus?.State ?? ZString.Empty),
				("Status Date", declarationStatus?.StateDateTime ?? ZString.Empty),
				("Previous Status", declarationStatus?.PreviousState ?? ZString.Empty),
				("Event", declarationStatus?.Event ?? ZString.Empty)
			});

			result += "<style> table, th, td {border: 1px solid black; border-collapse: collapse;} th, td { padding: 10px; text-align: left;}</style><table>";
			var content = ZString.Empty;

			if (messageObject.ControlResult != null)
			{
				var controlResult = messageObject.ControlResult;
				var pendingSamplingResults = controlResult?.PendingSamplingResults ?? ZString.Empty;
				var controlResultCode = controlResult?.Code ?? string.Empty;
				var controlResultDescription = GetFRCodeDescription(controlResultCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ControlResult);
				content = ToTd(ToStrongIfNotEmpty(ToLargerPIfNotEmpty("Control Result")), 4, 1, "lightgray");
				content += ToTd("Code", 1, 3);
				content += ToTd(controlResultCode + " - " + controlResultDescription, 1, 1);
				EndOfARow(ref result, ref content);
				content += ToTd("Date", 1, 3);
				content += ToTd(controlResult?.Date ?? ZString.Empty, 1, 1);
				EndOfARow(ref result, ref content);
				content += ToTd("Remarks", 1, 3);
				content += ToTd(controlResult?.Remarks ?? ZString.Empty, 1, 1);
				EndOfARow(ref result, ref content);
				content += ToTd("Pending Sampling Results", 1, 3);
				content += ToTd(pendingSamplingResults.IsEmpty() ? ZString.Empty : pendingSamplingResults == "1" ? "Yes" : "No", 1, 1);
				EndOfARow(ref result, ref content);
			}

			if (messageObject.ControlResults != null && messageObject.ControlResults.Count > 0)
			{
				var totalRows = 0;
				foreach (var controlResult in messageObject.ControlResults)
				{
					totalRows += 3;
					if (controlResult.ResultsOfControl != null)
					{
						var resultsOfControl = controlResult.ResultsOfControl;
						foreach (var resultOfControl in resultsOfControl)
						{
							totalRows += 5;
							if (resultOfControl.ControlDetails != null)
							{
								totalRows += resultOfControl.ControlDetails.Count * 6;
							}
						}
					}
					content = ToTd(ToStrongIfNotEmpty(ToLargerPIfNotEmpty("Control Results")), totalRows, 1, "lightgray");

					var controlResultCode = controlResult.ControlResultCode ?? string.Empty;
					var controlResultDescription = GetFRCodeDescription(controlResultCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ControlResult);

					content += ToTd("Sequence Number", 1, 3);
					content += ToTd(controlResult?.SequenceNumber ?? ZString.Empty, 1, 1);
					EndOfARow(ref result, ref content);
					content += ToTd("Declaration Goods Item Number", 1, 3);
					content += ToTd(controlResult?.DeclarationGoodsItemNumber ?? ZString.Empty, 1, 1);
					EndOfARow(ref result, ref content);
					content += ToTd("Control Result Code", 1, 3);
					content += ToTd(controlResultCode + " - " + controlResultDescription, 1, 1);
					EndOfARow(ref result, ref content);

					if (controlResult.ResultsOfControl != null && controlResult.ResultsOfControl.Count > 0)
					{
						var resultsOfControl = controlResult.ResultsOfControl;
						totalRows = 0;
						foreach (var resultOfControl in resultsOfControl)
						{
							totalRows = 5;
							if (resultOfControl.ControlDetails != null)
							{
								totalRows += resultOfControl.ControlDetails.Count * 6;
							}
							content = ToTd(ToStrongIfNotEmpty(ToLargerPIfNotEmpty("Results of Control")), totalRows, 1, "lightgray");

							var riskAreaCode = resultOfControl?.RiskAreaCode ?? ZString.Empty;
							var riskAreaCodeDescription = GetFRCodeDescription(riskAreaCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.RiskAreaCode);
							riskAreaCodeDescription = riskAreaCodeDescription.IsEmpty ? GetEUNCodeDescription(riskAreaCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.RiskAreaCode) : riskAreaCodeDescription;
							var resultOfControlType = resultOfControl?.ControlType ?? ZString.Empty;
							var resultOfControlTypeDescription = GetFRCodeDescription(resultOfControlType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType);

							content += ToTd("Sequence Number", 1, 2);
							content += ToTd(resultOfControl?.SequenceNumber ?? ZString.Empty, 1, 1);
							EndOfARow(ref result, ref content);
							content += ToTd("Risk Area Code", 1, 2);
							content += ToTd(riskAreaCode + " - " + riskAreaCodeDescription, 1, 1);
							EndOfARow(ref result, ref content);
							content += ToTd("Control Type", 1, 2);
							content += ToTd(resultOfControlType + " - " + resultOfControlTypeDescription, 1, 1);
							EndOfARow(ref result, ref content);
							content += ToTd("Control Date", 1, 2);
							content += ToTd(resultOfControl?.ControlDate ?? ZString.Empty, 1, 1);
							EndOfARow(ref result, ref content);
							content += ToTd("Remarks", 1, 2);
							content += ToTd(resultOfControl?.Remarks ?? ZString.Empty, 1, 1);
							EndOfARow(ref result, ref content);

							if (resultOfControl.ControlDetails != null && resultOfControl.ControlDetails.Count > 0)
							{
								var controlDetails = resultOfControl.ControlDetails;

								foreach (var controlDetail in controlDetails)
								{
									content = ToTd(ToStrongIfNotEmpty(ToLargerPIfNotEmpty("Control Details")), 6, 1, "lightgray");

									var controlDetailType = controlDetail?.TypeOfDiscrepancies ?? ZString.Empty;
									var controlDetailTypeDescription = GetFRCodeDescription(controlDetailType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfDiscrepancies);

									content += ToTd("", 1, 2);
									EndOfARow(ref result, ref content);
									content += ToTd("Sequence Number", 1, 1);
									content += ToTd(controlDetail?.SequenceNumber ?? ZString.Empty, 1, 1);
									EndOfARow(ref result, ref content);
									content += ToTd("Type Of discrepancies", 1, 1);
									content += ToTd(controlDetailType + " - " + controlDetailTypeDescription, 1, 1);
									EndOfARow(ref result, ref content);
									content += ToTd("Attribute Pointer", 1, 1);
									content += ToTd(controlDetail?.AttributePointer ?? ZString.Empty, 1, 1);
									EndOfARow(ref result, ref content);
									content += ToTd("Corrected Value", 1, 1);
									content += ToTd(controlDetail?.CorrectedValue ?? ZString.Empty, 1, 1);
									EndOfARow(ref result, ref content);
									content += ToTd("Remarks", 1, 1);
									content += ToTd(controlDetail?.Remarks ?? ZString.Empty, 1, 1);
									EndOfARow(ref result, ref content);
								}
							}
						}
					}
				}
			}

			result += "<table>";
			return result;
		}

		ZString GetFRCodeDescription(string code, string codeType)
		{
			return string.IsNullOrEmpty(code) ? ZString.Empty : (ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(MessageDataObject.Factory, code, Core.Constants.CountryCodes.France, codeType, ZDateTime.Today)?.ZZD_Description ?? ZString.Empty);
		}

		ZString GetEUNCodeDescription(string code, string codeType)
		{
			return string.IsNullOrEmpty(code) ? ZString.Empty : (ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(MessageDataObject.Factory, code, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, codeType, ZDateTime.Today)?.ZZD_Description ?? ZString.Empty);
		}
	}
}
