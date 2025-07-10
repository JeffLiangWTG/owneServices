using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.V921ES;
using Enterprise.Edifact.V921ES.Elements;
using Enterprise.Edifact.V921ES.Messages.CUSRES;
using Enterprise.Edifact.V921ES.Segments;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.Messaging.MessageProcessors
{
	public class CUSRESV921ESMessageHelper : NonPersistentBusinessObject, ICUSRESV921ESMessageProvider, IExportResponseMessageProvider
	{
		protected CUSRESV921ESMessageHelper(BusinessObjectFactory factory, CUSRESMessage message)
			: base(factory)
		{
			cusresMessage = Argument.NotNull(message, "message");
		}

		protected readonly CUSRESMessage cusresMessage;

		public static CUSRESV921ESMessageHelper New(EDIMessage message)
		{
			CUSRESV921ESMessageHelper result = null;
			if (message != null)
			{
				var d921MessageFactory = new D921ESMessageFactory();
				var esCharSet = new UNOAESCharacterSet();
				var cusresMessage = message.GetAutoEdifactMessageUsingNamedFactory(d921MessageFactory, esCharSet) as CUSRESMessage;
				if (cusresMessage != null)
				{
					result = new CUSRESV921ESMessageHelper(message.Factory, cusresMessage);
				}
			}
			return result;
		}

		public static class Constants
		{
			public static class Identifiers
			{
				public const string CSVRelease = "LEVA";
				public const string CSVT2LF = "T2LF";
			}
		}

		ZString ICUSRESMessageProvider.DocumentMessageName => cusresMessage.BGM[0].DocumentMessageName.DocumentMessageNameCoded.ToString() ?? ZString.Empty;

		ZString IExportResponseMessageProvider.UniqueReferenceNumber => cusresMessage.BGM[0].DocumentMessageNumber ?? ZString.Empty;

		ZDateTime ICUSRESMessageProvider.AdmissionDate
		{
			get
			{
				if (!admissionDate.HasValue)
				{
					admissionDate = GetAdmissionDate();
				}
				return admissionDate.Value;
			}
		}
		ZDateTime? admissionDate;

		protected ZDateTime GetAdmissionDate()
		{
			var result = ZDateTime.Invalid;
			foreach (DTMSegment dtm in cusresMessage.DTM)
			{
				if (dtm.DateTimePeriod.DateTimePeriodQualifier == DateTimePeriodQualifierList.AcceptanceDateOfGoodsDeclarationCustoms)
				{
					ZDateTime.TryParseExact(dtm.DateTimePeriod.DateTimePeriod, out result, CustomsDateTimeExtension.DateTimeFormatShort);
					break;
				}
			}
			return result;
		}

		ZDateTime ICUSRESV921ESMessageProvider.TransitMaxDate
		{
			get
			{
				if (!transitMaxDate.HasValue)
				{
					transitMaxDate = GetTransitMaxDate();
				}
				return transitMaxDate.Value;
			}
		}
		ZDateTime? transitMaxDate;

		ZDateTime GetTransitMaxDate()
		{
			var result = ZDateTime.Invalid;
			foreach (DTMSegment dtm in cusresMessage.DTM)
			{
				if (dtm.DateTimePeriod.DateTimePeriodQualifier == DateTimePeriodQualifierList.TransitTimeLimits)
				{
					ZDateTime.TryParseExact(dtm.DateTimePeriod.DateTimePeriod, out result, CustomsDateTimeExtension.DateFormat);
					break;
				}
			}
			return result;
		}

		ZString ICUSRESMessageProvider.MessageFunction
		{
			get
			{
				if (!messageFunction.HasValue)
				{
					messageFunction = GetMessageFunction();
				}
				return messageFunction.Value;
			}
		}
		ZString? messageFunction;

		protected ZString GetMessageFunction()
		{
			var result = ZString.Empty;
			foreach (GISSegment gis in cusresMessage.GIS)
			{
				if (gis.ProcessingIndicator.CodeListQualifier == CodeListQualifierList.CustomsProcedure)
				{
					result = gis.ProcessingIndicator.ProcessingIndicatorCoded.ToString();
					break;
				}
			}
			return result;
		}

		ZString IExportResponseMessageProvider.MessageFunctionCAN
		{
			get
			{
				if (!messageFunctionCAN.HasValue)
				{
					messageFunctionCAN = GetMessageFunctionCAN();
				}
				return messageFunctionCAN.Value;
			}
		}
		ZString? messageFunctionCAN;

		ZString GetMessageFunctionCAN()
		{
			var result = ZString.Empty;
			foreach (GISSegment gis in cusresMessage.GIS)
			{
				if (gis.ProcessingIndicator.CodeListQualifier == CodeListQualifierList.GovernmentAgencyProcedure)
				{
					result = gis.ProcessingIndicator.ProcessingIndicatorCoded.ToString();
					break;
				}
			}
			return result;
		}

		ZString IExportResponseMessageProvider.CustomsClearanceStatus
		{
			get
			{
				if (!customsClearanceStatus.HasValue)
				{
					customsClearanceStatus = GetCustomsClearanceStatus();
				}
				return customsClearanceStatus.Value;
			}
		}
		ZString? customsClearanceStatus;

		ZString GetCustomsClearanceStatus()
		{
			var result = ZString.Empty;
			foreach (GISSegment gis in cusresMessage.GIS)
			{
				if (gis.ProcessingIndicator.CodeListQualifier == CodeListQualifierList.CustomsSimplifiedProcedure)
				{
					result = gis.ProcessingIndicator.CodeListResponsibleAgencyCoded.ToString();
					break;
				}
			}
			return result;
		}

		ZString ICUSRESV921ESMessageProvider.PrintActionRequired
		{
			get
			{
				if (!printActionRequired.HasValue)
				{
					printActionRequired = GetPrintActionRequired();
				}
				return printActionRequired.Value;
			}
		}
		ZString? printActionRequired;

		ZString GetPrintActionRequired()
		{
			var result = ZString.Empty;
			foreach (GISSegment gis in cusresMessage.GIS)
			{
				if (gis.ProcessingIndicator.CodeListQualifier == CodeListQualifierList.CustomsSimplifiedProcedure)
				{
					result = gis.ProcessingIndicator.ProcessTypeIdentification.ToString();
					break;
				}
			}
			return result;
		}

		List<ErrorMessage> ICUSRESMessageProvider.FreeTextErrors
		{
			get
			{
				if (freeTextErrorList == null)
				{
					freeTextErrorList = new List<ErrorMessage>();
					foreach (FTXSegment ftx in cusresMessage.FTX)
					{
						var freeText3 = ftx.TextLiteral.FreeText3;
						var freeText5 = ftx.TextLiteral.FreeText5;
						var error = new ErrorMessage
						{
							Code = ftx.TextLiteral.FreeText1,
							Location = ftx.TextLiteral.FreeText2 + (!freeText3.IsNullOrEmpty() ? "." + freeText3 : string.Empty),
							Description = ftx.TextLiteral.FreeText4 + (!freeText5.IsNullOrEmpty() ? "." + freeText5 : string.Empty)
						};
						freeTextErrorList.Add(error);
					}
				}
				return freeTextErrorList;
			}
		}
		List<ErrorMessage> freeTextErrorList;

		ZString ICUSRESV921ESMessageProvider.RegistrationNumber => cusresMessage?.Group5[0].RFF[0].Reference.ReferenceNumber ?? ZString.Empty;

		ZString ICUSRESV921ESMessageProvider.CSVReleaseCode
		{
			get
			{
				if (!cSVReleaseCode.HasValue)
				{
					cSVReleaseCode = GetCSVReleaseCode();
				}
				return cSVReleaseCode.Value;
			}
		}
		ZString? cSVReleaseCode;

		ZString GetCSVReleaseCode()
		{
			var result = ZString.Empty;
			if (cusresMessage.Group9.Count > 0)
			{
				var aut = cusresMessage.Group9[0].AUT[0];
				if (aut.ValidationKeyIdentification == Constants.Identifiers.CSVRelease || aut.ValidationKeyIdentification.IsNullOrEmpty())
				{
					result = aut.ValidationResult;
				}
			}
			return result;
		}

		ZString IExportResponseMessageProvider.CSVT2LFCode
		{
			get
			{
				if (!cSVT2LFCode.HasValue)
				{
					cSVT2LFCode = GetDCSVT2LFCode();
				}
				return cSVT2LFCode.Value;
			}
		}
		ZString? cSVT2LFCode;

		ZString GetDCSVT2LFCode()
		{
			var result = ZString.Empty;
			if (cusresMessage.Group9.Count > 1)
			{
				var aut = cusresMessage.Group9[1].AUT[0];
				if (aut.ValidationKeyIdentification == Constants.Identifiers.CSVT2LF)
				{
					result = aut.ValidationResult;
				}
			}
			return result;
		}

		ZDateTime ICUSRESV921ESMessageProvider.CSVReleaseCreationDate
		{
			get
			{
				if (!cSVReleaseCreationDate.HasValue)
				{
					cSVReleaseCreationDate = GetCSVReleaseCreationDate();
				}
				return cSVReleaseCreationDate.Value;
			}
		}
		ZDateTime? cSVReleaseCreationDate;

		ZDateTime GetCSVReleaseCreationDate()
		{
			var result = ZDateTime.Invalid;
			if (cusresMessage.Group9.Count > 0)
			{
				var aut = cusresMessage.Group9[0].AUT[0];
				var dtm = cusresMessage.Group9[0].DTM[0];
				if (aut.ValidationKeyIdentification == Constants.Identifiers.CSVRelease || aut.ValidationKeyIdentification.IsNullOrEmpty())
				{
					ZDateTime.TryParseExact(dtm.DateTimePeriod.DateTimePeriod, out result, CustomsDateTimeExtension.DateTimeFormatShort);
				}
			}
			return result;
		}
	}
}
