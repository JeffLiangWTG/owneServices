using System.Collections.Specialized;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC057C;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC057CMessagePrettier : NCTSMessagePrettier<Cc057CType>
	{
		public CC057CMessagePrettier(NCTSMessageDataObject<Cc057CType> messageDataObject) : base(messageDataObject)
		{
		}

		protected override ZString GetMessageInterpretationCore(Cc057CType messageObject)
		{
			var transitOperation = messageObject.TransitOperation;

			var businessRejectionType = transitOperation?.BusinessRejectionType ?? ZString.Empty;
			var businessRejectionTypeDescription = GetEUNCodeDescription(businessRejectionType, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL570);

			var rejectionCode = transitOperation?.RejectionCode ?? ZString.Empty;
			var rejectionCodeDescription = GetEUNCodeDescription(rejectionCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL227);

			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				((NoResString)"Status", TP5ResponseMessageSubTypeList.Descriptions.RejectionFromOfficeOfDestination ?? ZString.Empty),
				("MRN", transitOperation?.Mrn ?? ZString.Empty),
				((NoResString)"Business Rejection Type", businessRejectionType.IsEmpty() ? ZString.Empty : $"{businessRejectionType} - {businessRejectionTypeDescription}"),
				((NoResString)"Rejection Date and Time", transitOperation?.RejectionDateAndTime.ToString() ?? ZString.Empty),
				((NoResString)"Rejection Code", rejectionCode.IsEmpty() ? ZString.Empty : $"{rejectionCode} - {rejectionCodeDescription}"),
				((NoResString)"Rejection Reason", transitOperation?.RejectionReason ?? ZString.Empty),
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Cell strings")]
		protected override ZString GetErrorsTableIfNeeded(Cc057CType messageObject)
		{
			var functionalErrors = messageObject.FunctionalError;
			if (functionalErrors.Count == 0)
			{
				return ZString.Empty;
			}

			var tableCreator = GetHtmlTableCreator();
			var headers = new[]
			{
				new CellWithFormatting("Level", "width", "75px"),
				new CellWithFormatting("Error Code", "width", "100px"),
				new CellWithFormatting("Field Code", "width", "100px"),
				new CellWithFormatting("Error Reason", "width", "150px"),
				new CellWithFormatting("Original Value", "width", "125px"),
				new CellWithFormatting("Remarks", "width", "150px")
			};
			tableCreator.WriteRowWithFormatting(new NameValueCollection { { "align", "center" } }, headers);

			foreach (var functionalError in functionalErrors)
			{
				tableCreator.WriteRow(ZString.Empty,
					GetErrorCodeNumber(functionalError.ErrorCode.ToString()),
					GetFieldCode(functionalError.ErrorPointer),
					functionalError.ErrorReason,
					functionalError.OriginalAttributeValue is string attributeValue ? AppendWordBreaksToPath(functionalError.OriginalAttributeValue) : string.Empty,
					GetRemark(functionalError.ErrorCode.ToString()));
			}

			return ToTableSection("Functional Errors", tableCreator);
		}

		ZString GetEUNCodeDescription(string code, string codeType)
		{
			return string.IsNullOrEmpty(code) ? ZString.Empty : (ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(MessageDataObject.Factory, code, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, codeType, ZDateTime.Today)?.ZZD_Description ?? ZString.Empty);
		}
	}
}
