using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Edifact.D96B;
using Enterprise.Edifact.D96B.Elements;
using Enterprise.Edifact.D96B.Messages.CUSRES;
using Enterprise.Edifact.D96B.Segments;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageProcessors
{
	public class CUSRESD96BMessageHelper : NonPersistentBusinessObject, INctsArrivalResponseMessageProvider
	{
		protected CUSRESD96BMessageHelper(BusinessObjectFactory factory, CUSRESMessage message)
			: base(factory)
		{
			cusresMessage = Argument.NotNull(message, "message");
		}

		readonly CUSRESMessage cusresMessage;

		public static CUSRESD96BMessageHelper New(EDIMessage message)
		{
			CUSRESD96BMessageHelper result = null;
			if (message != null)
			{
				var oldMessageText = AddERPSegment(message);
				var d96BMessageFactory = new EdifactD96BMessageFactory();
				var esCharSet = new UNOAESCharacterSet();
				var cusresMessage = message.GetAutoEdifactMessageUsingNamedFactory(d96BMessageFactory, esCharSet) as CUSRESMessage;
				if (cusresMessage != null)
				{
					result = new CUSRESD96BMessageHelper(message.Factory, cusresMessage);
				}
				message.EM_MessageText = oldMessageText;
			}
			return result;
		}

		static ZString AddERPSegment(EDIMessage message)
		{
			var messageText = message.EM_MessageText;
			var i = messageText.IndexOf("FTX");
			if (messageText.Contains("FTX") && !messageText.Contains("ERP"))
			{
				message.EM_MessageText = messageText.SubstringSafe(0, i) + (NoResString)"ERP'" + messageText.SubstringSafe(i);
			}
			return messageText;
		}

		public ZString DocumentMessageName => cusresMessage.BGM[0].DocumentMessageName.DocumentMessageNameCoded.ToString() ?? ZString.Empty;

		public ZDateTime AdmissionDate
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

		public ZString MessageFunction
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

		public ZString PreviousSummaryDiscrepancy
		{
			get
			{
				if (!previousSummaryDiscrepancy.HasValue)
				{
					previousSummaryDiscrepancy = GetPreviousSummaryDiscrepancy();
				}
				return previousSummaryDiscrepancy.Value;
			}
		}
		ZString? previousSummaryDiscrepancy;

		ZString GetPreviousSummaryDiscrepancy()
		{
			var result = ZString.Empty;
			foreach (GISSegment gis in cusresMessage.GIS)
			{
				if (gis.ProcessingIndicator.CodeListQualifier == CodeListQualifierList.CustomsStatusOfGoods)
				{
					result = gis.ProcessingIndicator.ProcessingIndicatorCoded.ToString();
					break;
				}
			}
			return result;
		}

		public List<ErrorMessage> FreeTextErrors
		{
			get
			{
				if (freeTextErrorList == null)
				{
					freeTextErrorList = new List<ErrorMessage>();
					foreach (FTXSegment ftx in cusresMessage.Group4[0].FTX)
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

		public ZString TransitReferenceNumber
		{
			get
			{
				if (!transitReferenceNumber.HasValue)
				{
					transitReferenceNumber = GetTransitReferenceNumber();
				}
				return transitReferenceNumber.Value;
			}
		}
		ZString? transitReferenceNumber;

		ZString GetTransitReferenceNumber()
		{
			var result = ZString.Empty;
			foreach (SegmentGroup3 group3 in cusresMessage.Group3)
			{
				var rff = group3.RFF[0];
				if (rff.Reference.ReferenceQualifier == ReferenceQualifierList.CustomsDeclarationNumber)
				{
					result = rff.Reference.ReferenceNumber;
					break;
				}
			}
			return result;
		}

		public ZString SummaryReferenceNumber
		{
			get
			{
				if (!summaryReferenceNumber.HasValue)
				{
					summaryReferenceNumber = GetSummaryReferenceNumber();
				}
				return summaryReferenceNumber.Value;
			}
		}
		ZString? summaryReferenceNumber;

		ZString GetSummaryReferenceNumber()
		{
			var result = ZString.Empty;
			foreach (SegmentGroup3 group3 in cusresMessage.Group3)
			{
				var rff = group3.RFF[0];
				if (rff.Reference.ReferenceQualifier == ReferenceQualifierList.CargoManifestNumber)
				{
					result = rff.Reference.ReferenceNumber;
					break;
				}
			}
			return result;
		}
	}
}
