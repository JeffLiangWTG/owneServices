using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	abstract class LucalGenralResponseMaker_Success : LucalGenralResponseMaker
	{
		public static LucalGenralResponseMaker New(string requestText, IBusiness mainBusinessObject, IBusiness[] childrenBusinessObjects,
												LucasGenralEnquiryHandler.EnquiryObjectTypes objectRequestType, EDIMessage inboundEdiMessage,
												ZString commonAccessReference)
		{
			switch (objectRequestType)
			{
				case LucasGenralEnquiryHandler.EnquiryObjectTypes.MAWB:
					return new LucalGenralResponseMaker_Mawb(requestText, mainBusinessObject, childrenBusinessObjects, inboundEdiMessage, commonAccessReference);
				case LucasGenralEnquiryHandler.EnquiryObjectTypes.DUCR:
					return new LucalGenralResponseMaker_DUCR(requestText, mainBusinessObject, childrenBusinessObjects, inboundEdiMessage, commonAccessReference);
				case LucasGenralEnquiryHandler.EnquiryObjectTypes.HAWB:
					return new LucalGenralResponseMaker_Hawb(requestText, mainBusinessObject, childrenBusinessObjects, inboundEdiMessage, commonAccessReference);
				case LucasGenralEnquiryHandler.EnquiryObjectTypes.SHPR:
				case LucasGenralEnquiryHandler.EnquiryObjectTypes.CNSE:
					return new LucalGenralResponseMaker_OrgHeader(requestText, mainBusinessObject, childrenBusinessObjects, inboundEdiMessage, objectRequestType, commonAccessReference);
				default:
					return new LucalGenralResponseMaker_Failure("NOT IMPLEMENTED", inboundEdiMessage, requestText, commonAccessReference);
			}
		}

		protected bool needsFooter = true;

		protected void SendGenralsFromOneLargePayload(ZString payload)
		{
			if (!payload.IsEmpty)
			{
				var pages = SplitPayloadIntoPages(payload);
				foreach (var page in pages)
				{
					var pageWithHeaderAndFooter = AddHeaderAndMaybeFooter(page, pages.Count, needsFooter);
					var generalEdiMessage = GenralEdiMessage.MakeNewOutboundFromPayload(pageWithHeaderAndFooter, GenralEdiMessage.ConvertLucasGenralPima(inboundEdiMessage), inboundEdiMessage.Factory, inboundEdiMessage.Interchange.EI_To, true, commonAccessReference);
					if (inboundEdiMessage.EM_LinkedObject != null)
					{
						generalEdiMessage.EM_LinkedObject = inboundEdiMessage.EM_LinkedObject;
					}
				}
			}
		}

		string AddHeaderAndMaybeFooter(PageOfGenralResponse page, int totalPages, bool needsFooter)
		{
			var maybeFooter = string.Format("\r\nPAGE {0}/{1}", page.oneBasedPageNumber.ToString("0#"), page.oneBasedPageNumber == totalPages ? "END" : "MORE");
			if (!needsFooter)
			{
				maybeFooter = "";
			}

			string responseTypeForGenralHeader = inboundEdiMessage.EM_MessageSubType == CcsukTransmissionMessageFunction.CIM.FSR.SubCode ? "FSR" : "DEP";
			return string.Format(@"{5} response for: {0}
From:{1}                                     sent: {2}

{3}{4}",
						enquiryString,
						inboundEdiMessage.Interchange.EI_To.Replace("/", "").Right(6), ZDateTime.Now.ToString("dd/MM/yyyy HH:mm"),
						page.pageOfData,
						maybeFooter,
						responseTypeForGenralHeader);
		}

		List<PageOfGenralResponse> SplitPayloadIntoPages(string payload)
		{
			var lines = Regex.Split(payload, System.Environment.NewLine);
			var result = new List<PageOfGenralResponse>();
			var sb = new ZStringBuilder();
			var pageIndex = 0;
			int maxLinesBeforePageBreak = needsFooter ? 16 : 17;
			for (int i = 1; i <= lines.Length; i++)
			{
				sb.Append(lines[i - 1]);
				if ((i % maxLinesBeforePageBreak) == 0 || i == lines.Length)
				{
					var page = new PageOfGenralResponse(++pageIndex, sb.ToStringWithNewLineBetweenAppends());
					result.Add(page);
					sb = new ZStringBuilder();
				}
			}
			return result;
		}

		protected LucalGenralResponseMaker_Success(string requestText, IBusiness mainBusinessObject, IBusiness[] childrenBusinessObjects, EDIMessage inboundEdiMessage, ZString commonAccessReference)
			: base(inboundEdiMessage, requestText, commonAccessReference)
		{
			this.mainBusinessObject = mainBusinessObject;
			this.childrenBusinessObjects = childrenBusinessObjects;
			this.inboundEdiMessage = inboundEdiMessage;
		}

		class PageOfGenralResponse
		{
			public PageOfGenralResponse(int oneBasedPageNumber, ZString pageOfData)
			{
				this.pageOfData = pageOfData;
				this.oneBasedPageNumber = oneBasedPageNumber;
			}

			public int oneBasedPageNumber { get; private set; }
			public ZString pageOfData { get; private set; }
		}

		protected IBusiness mainBusinessObject;
		protected IBusiness[] childrenBusinessObjects;
	}
}
