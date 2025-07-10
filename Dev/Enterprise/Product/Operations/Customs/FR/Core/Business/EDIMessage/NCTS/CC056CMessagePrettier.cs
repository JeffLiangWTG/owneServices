using System.Collections.Specialized;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC056C;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC056CMessagePrettier : NCTSMessagePrettier<Cc056CType>
	{
		public CC056CMessagePrettier(CC056CMessageDataObject messageDataObject)
			: base(messageDataObject)
		{
		}

		protected override ZString GetMessageInterpretationCore(Cc056CType messageObject)
		{
			var transitOperation = messageObject.TransitOperation;

			var businessRejectionType = transitOperation?.BusinessRejectionType ?? ZString.Empty;
			var businessRejectionTypeDescription = GetEUNCodeDescription(businessRejectionType, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL560);

			var rejectionCode = transitOperation?.RejectionCode ?? ZString.Empty;
			var rejectionCodeDescription = GetEUNCodeDescription(rejectionCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL226);

			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				((NoResString)"Status", TP5ResponseMessageSubTypeList.Descriptions.RejectionFromOfficeOfDeparture.ToUpper()),
				("LRN", transitOperation?.Lrn ?? ZString.Empty),
				("MRN", transitOperation?.Mrn ?? ZString.Empty),
				((NoResString)"Business Rejection Type", businessRejectionType.IsEmpty() ? ZString.Empty : businessRejectionType + " - " + businessRejectionTypeDescription),
				((NoResString)"Rejection Date and Time", transitOperation?.RejectionDateAndTime.ToString() ?? ZString.Empty),
				((NoResString)"Rejection Code", transitOperation?.RejectionCode ?? ZString.Empty),
				((NoResString)"Rejection Reason", rejectionCode.IsEmpty() ? ZString.Empty : rejectionCode + " - " + rejectionCodeDescription),
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Cell strings")]
		protected override ZString GetErrorsTableIfNeeded(Cc056CType messageObject)
		{
			HtmlTableCreator tableCreator = null;
			var functionalErrors = messageObject.FunctionalError;
			if (functionalErrors.Count > 0)
			{
				tableCreator = GetHtmlTableCreator();
				var cell1 = new CellWithFormatting("Level", "width", "100px");
				var cell2 = new CellWithFormatting("Error Code", "width", "100px");
				var cell3 = new CellWithFormatting("Field Code", "width", "75px");
				var cell4 = new CellWithFormatting("Error Reason", "width", "100px");
				var cell5 = new CellWithFormatting("Original Value", "width", "100px");
				var cell6 = new CellWithFormatting("Remarks", "width", "150px");
				tableCreator.WriteRowWithFormatting(new NameValueCollection { { "align", "center" } }, new[] { cell1, cell2, cell3, cell4, cell5, cell6 });

				foreach (var functionalError in functionalErrors)
				{
					tableCreator.WriteRow(ZString.Empty
						, functionalError.ErrorCode
						, GetFieldCode(functionalError.ErrorPointer)
						, functionalError.ErrorReason
						, functionalError.OriginalAttributeValue
						, GetRemark(functionalError.ErrorCode.ToString())
					);
				}
			}

			return ToTableSection("Functional Errors", tableCreator);
		}

		ZString GetEUNCodeDescription(string code, string codeType)
		{
			return string.IsNullOrEmpty(code) ? ZString.Empty : (ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(MessageDataObject.Factory, code, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, codeType, ZDateTime.Today)?.ZZD_Description ?? ZString.Empty);
		}
	}
}
