using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.UCC5;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IE.Business
{
	public static class MessageInterpreterHelper
	{
		public static string GetCodeAndDescription(
			BusinessObjectFactory factory,
			ZString code,
			ZString codeType,
			ZDateTime date,
			string dataGrouping = Core.Constants.CountryCodes.Ireland
		) => GetCodeAndDescription(code, RefCusCodeListTypes.GetCachedList(factory, dataGrouping, codeType, date));

		public static string GetCodeAndDescription(string code, ICodeDescriptionPairList list)
		{
			var result = new ZStringBuilder(code);
			result.AppendIfNotEmpty(list.GetDescriptionFromCode(code));
			return result.ToStringWithDelimiterBetweenAppends(" - ");
		}

		public static IEnumerable<TResult> UnevenZipExtension<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first,
					IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> operation)
		{
			using (var iter1 = first.GetEnumerator())
			{
				using (var iter2 = second.GetEnumerator())
				{
					while (iter1.MoveNext())
					{
						if (iter2.MoveNext())
						{
							yield return operation(iter1.Current, iter2.Current);
						}
						else
						{
							yield return operation(iter1.Current, default(TSecond));
						}
					}
					while (iter2.MoveNext())
					{
						yield return operation(default(TFirst), iter2.Current);
					}
				}
			}
		}

		public static IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetFunctionalErrorDetailsUCC5(this IEnumerable<FunctionalErrorTypeProvider> providers, ZDate messageCreatedDate, BusinessObjectFactory factory)
			=> GetFunctionalErrorDetails(providers, messageCreatedDate, factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.IEUcc5);

		public static IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetFunctionalErrorDetails(this IEnumerable<FunctionalErrorTypeProvider> providers, ZDate messageCreatedDate, BusinessObjectFactory factory)
			=> GetFunctionalErrorDetails(providers, messageCreatedDate, factory, Core.Constants.CountryCodes.Ireland);

		static IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetFunctionalErrorDetails(this IEnumerable<FunctionalErrorTypeProvider> providers, ZDate messageCreatedDate, BusinessObjectFactory factory, string dataGrouping)
		{
			var index = 1;
			foreach (var provider in providers)
			{
				yield return GetSummaryForFunctionalError(
					provider,
					index++,
					GetDescriptionFromCode(factory, provider.ErrorType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ErrorCode, messageCreatedDate, dataGrouping)
				);
			}
		}

		public static string GetDescriptionFromCode(BusinessObjectFactory factory, ZString code, ZString codeType, ZDateTime date, string country = Core.Constants.CountryCodes.Ireland)
		{
			var list = RefCusCodeListTypes.GetCachedList(factory, country, codeType, date);
			return list.GetDescriptionFromCode(code);
		}

		static (string summary, IEnumerable<(string key, string value)>) GetSummaryForFunctionalError(FunctionalErrorTypeProvider functionalErrorProvider, int sequenceNumber, string errorCodeDescription)
		{
			var details = new (string, string)[]
			{
				(CommonResStrings.ErrorReason, functionalErrorProvider.ErrorReason),
				(CommonResStrings.ErrorType, functionalErrorProvider.ErrorType),
				(CommonResStrings.ErrorTypeDescription, errorCodeDescription),
				(CommonResStrings.ErrorMessage, functionalErrorProvider.ErrorMessage),
				(CommonResStrings.OriginalAttributeValue, functionalErrorProvider.OriginalAttributeValue),
				(CommonResStrings.ErrorPointer, functionalErrorProvider.ErrorPointer),
			};

			return (GetFunctionalSummary(sequenceNumber), details);
		}

		public static string GetFunctionalSummary(int functionalErrorSequenceNumber) => Res.GetString("E9333F5E-E68D-4D59-9AEA-1F6045B23BE3", "Functional Error: {0}", functionalErrorSequenceNumber);

		internal static IEnumerable<(string key, string value)> GetMessageDetails(this MFunctionalError01Provider functionalError, string errorCodeDescription)
		{
			return new (string, string)[]
			{
				(CommonResStrings.FunctionalError, functionalError.SequenceNumber),
				(CommonResStrings.ErrorPointer, functionalError.ErrorPointer),
				(CommonResStrings.ErrorCode, functionalError.ErrorCode),
				(CommonResStrings.ErrorCodeDescription, errorCodeDescription),
				(CommonResStrings.ErrorReason, functionalError.ErrorReason),
				(CommonResStrings.Remarks, functionalError.Remarks),
				(CommonResStrings.OriginalAttributeValue, functionalError.OriginalAttributeValue)
			};
		}

		public static IEnumerable<(string key, string value)> GetFunctionalErrorDetails(this FunctionalErrorTypeProvider functionalError, string errorCodeDescription)
		{
			return new (string, string)[]
			{
				(CommonResStrings.FunctionalError, string.Empty),
				(CommonResStrings.ErrorReason, functionalError.ErrorReason),
				(CommonResStrings.ErrorType, functionalError.ErrorType),
				(CommonResStrings.ErrorTypeDescription, errorCodeDescription),
				(CommonResStrings.ErrorMessage, functionalError.ErrorMessage),
				(CommonResStrings.OriginalAttributeValue, functionalError.OriginalAttributeValue),
				(CommonResStrings.ErrorPointer, functionalError.ErrorPointer),
			};
		}

		internal static IEnumerable<(string key, string value)> GetMessageDetails(this INegativeAcknowledgementError negativeAcknowledgementError)
		{
			return new (string, string)[]
			{
				(CommonResStrings.ErrorLineNumber, negativeAcknowledgementError.LineNumber),
				(CommonResStrings.ErrorReason, negativeAcknowledgementError.Reason),
				(CommonResStrings.ErrorColumnNumber, negativeAcknowledgementError.ColumnNumber),
			};
		}

		internal static IEnumerable<(string key, string value)> GetMessageDetails(this Messaging.AIS.ItemControlResultsProvider controlResult, Func<ZString, string> getRiskAreaCodeDescription, Func<ZString, string> getControlTypeDescription)
		{
			var result = new List<(string key, string value)>
			{
				(CommonResStrings.ControlResults, controlResult.SequenceNumber),
				(OneDash + CommonResStrings.DeclarationGoodsItemNumber, controlResult.DeclarationGoodsItemNumber),
				(OneDash + CommonResStrings.ControlResultCode, controlResult.ControlResultCode),
			};

			foreach (var resultOfControl in controlResult.ResultsOfControl)
			{
				result.AddRange(new (string key, string value)[]
				{
					(OneDash + CommonResStrings.ResultsOfControl, resultOfControl.SequenceNumber),
					(TwoDashes + CommonResStrings.RiskAreaCode, resultOfControl.RiskAreaCode),
					(TwoDashes + CommonResStrings.RiskAreaCodeDescription, getRiskAreaCodeDescription(resultOfControl.RiskAreaCode)),
					(TwoDashes + CommonResStrings.ControlType, resultOfControl.ControlType),
					(TwoDashes + CommonResStrings.ControlTypeDescription, getControlTypeDescription(resultOfControl.ControlType)),
					(TwoDashes + CommonResStrings.ControlDate, resultOfControl.ControlDate.ToShortDateString()),
					(TwoDashes + CommonResStrings.Remarks, resultOfControl.Remarks),
				});

				foreach (var controlDetail in resultOfControl.ControlDetails)
				{
					result.AddRange(new (string key, string value)[]
					{
						(TwoDashes + CommonResStrings.ControlDetails, controlDetail.SequenceNumber),
						(ThreeDashes + CommonResStrings.TypeOfDiscrepancies, controlDetail.TypeOfDiscrepancies),
						(ThreeDashes + CommonResStrings.AttributePointer, controlDetail.AttributePointer),
						(ThreeDashes + CommonResStrings.CorrectedValue, controlDetail.CorrectedValue),
						(ThreeDashes + CommonResStrings.Remarks, controlDetail.Remarks),
					});
				}
			}

			return result;
		}

		internal static IEnumerable<(string key, string value)> GetMessageDetails(this IDocumentRequestMessageProvider provider)
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.LocalReferenceNumber, provider.LRN);
			yield return (CommonResStrings.RequestDate, provider.RequestDate.ToShortDateString());
			yield return (CommonResStrings.DateLimit, provider.DateLimit.ToShortDateString());
			foreach (var document in provider.AdditionalInformations)
			{
				yield return (CommonResStrings.DocumentType, document.DocumentType);
				yield return (Res.GetString("6A723891-AB6A-4138-A241-7943BCCF171D", "Document reference"), document.RequestInformation);
			}
		}

		internal static List<string[]> GetGeneralRemarks(GeneralRemarkProvider[] generalRemarks)
		{
			var result = new List<string[]>
			{
				new string[]
				{
					CommonResStrings.Sequence,
					Res.GetString("4D250368-636D-45BB-8B10-04F825C7AB76", "General Remarks"),
				}
			};
			for (var i = 0; i < generalRemarks.Length; i++)
			{
				result.Add(new string[] { (i + 1).ToString(), generalRemarks[i].GeneralRemarks });
			}
			return result;
		}

		const string OneDash = "-";
		const string TwoDashes = "--";
		const string ThreeDashes = "---";
	}
}
