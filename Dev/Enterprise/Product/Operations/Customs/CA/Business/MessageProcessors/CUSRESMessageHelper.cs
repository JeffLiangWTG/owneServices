using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Messaging.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.CA.Business
{
	[CodeAlive("To be used by CA Customs")]
	public class CUSRESMessageHelper : NonPersistentBusinessObject
	{
		public CUSRESMessageHelper(EDIMessage ediMessage) : base(ediMessage.Factory)
		{
			this.EdiMessage = ediMessage;
		}

		public CUSRESMessageHelper(EDIMessage ediMessage, CUSRESMessage message)
			: this(ediMessage)
		{
			cusresMessage = Argument.NotNull(message, "message");
		}

		readonly CUSRESMessage cusresMessage;

		public EDIInterchange Interchange => EdiMessage.Interchange;
		public readonly EDIMessage EdiMessage;

		public static CUSRESMessageHelper New(EDIMessage message)
		{
			CUSRESMessageHelper result = null;
			if (message != null)
			{
				var cusresMessage = message.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet()) as CUSRESMessage;
				if (cusresMessage != null)
				{
					result = new CUSRESMessageHelper(message, cusresMessage);
				}
			}
			return result;
		}

		public ZString DocumentMessageName => cusresMessage?.BGM[0].DocumentMessageName.DocumentName ?? ZString.Empty;

		public ZString UniqueReferenceNumber => cusresMessage?.BGM[0].DocumentMessageIdentification.DocumentMessageNumber ?? ZString.Empty;

		public ZDateTime DocumentMessageDateTime
		{
			get
			{
				ZDateTime result = ZDateTime.Invalid;
				var postingDateString = cusresMessage.DTM.Cast<DTMSegment>().FirstOrDefault(dtm => dtm.DateTimePeriod.DateTimePeriodFunctionCodeQualifier == DateTimePeriodFunctionCodeQualifierList.DocumentMessageDateTime)?.DateTimePeriod.DateTimePeriodValue;
				ZDateTime.TryParseExact(postingDateString, out result, "yyyyMMddHHmm");
				return result;
			}
		}

		public ZDateTime DocumentMessageDate
		{
			get
			{
				ZDateTime result = ZDateTime.Invalid;
				var postingDateString = cusresMessage.DTM.Cast<DTMSegment>().FirstOrDefault(dtm => dtm.DateTimePeriod.DateTimePeriodFunctionCodeQualifier == DateTimePeriodFunctionCodeQualifierList.DocumentMessageDateTime)?.DateTimePeriod.DateTimePeriodValue;
				ZDateTime.TryParseExact(postingDateString, out result, "yyyyMMdd");
				return result;
			}
		}

		public List<FTXSegment> FreeTextErrors
		{
			get
			{
				if (ftxSegmentList == null)
				{
					ftxSegmentList = new List<FTXSegment>();
					foreach (SegmentGroup4 group4Item in cusresMessage.Group4)
					{
						ftxSegmentList.AddRange(group4Item.FTX.Cast<FTXSegment>());
					}
				}
				return ftxSegmentList;
			}
		}
		List<FTXSegment> ftxSegmentList;

		public List<ERPSegment> DetailSectionMessages
		{
			get
			{
				if (erpSegmentList == null)
				{
					erpSegmentList = new List<ERPSegment>();
					foreach (SegmentGroup4 group4Item in cusresMessage.Group4)
					{
						erpSegmentList.AddRange(group4Item.ERP.Cast<ERPSegment>());
					}
				}
				return erpSegmentList;
			}
		}
		List<ERPSegment> erpSegmentList;

		public List<ZString> AccountSecurityNumbers
		{
			get
			{
				if (accountSecurityNumbers == null)
				{
					accountSecurityNumbers = new List<ZString>();
					foreach (SegmentGroup3 group3 in cusresMessage.Group3)
					{
						foreach (RFFSegment rFF in group3.RFF)
						{
							if (rFF.Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.DeclarantsCustomsIdentityNumber)
							{
								accountSecurityNumbers.Add(rFF.Reference.ReferenceIdentifier);
							}
						}
					}
				}
				return accountSecurityNumbers;
			}
		}
		List<ZString> accountSecurityNumbers;

		public List<ZString> ApplicableReferenceNumbers
		{
			get
			{
				if (applicableReferenceNumbers == null)
				{
					applicableReferenceNumbers = new List<ZString>();
					foreach (SegmentGroup4 group4 in cusresMessage.Group4)
					{
						foreach (RFFSegment rFF in group4.RFF)
						{
							if (rFF.Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.OriginatorsReference)
							{
								applicableReferenceNumbers.Add(rFF.Reference.ReferenceIdentifier);
							}
						}
					}
				}
				return applicableReferenceNumbers;
			}
		}
		List<ZString> applicableReferenceNumbers;

		public List<ERCSegment> ApplicationErrorInformations
		{
			get
			{
				if (ercSegmentList == null)
				{
					ercSegmentList = new List<ERCSegment>();
					foreach (SegmentGroup4 group4Item in cusresMessage.Group4)
					{
						ercSegmentList.AddRange(group4Item.ERC.Cast<ERCSegment>());
					}
				}
				return ercSegmentList;
			}
		}
		List<ERCSegment> ercSegmentList;

		public ZInt TotalNumberOfTransactions
		{
			get
			{
				var resultString = cusresMessage.Group6[0].Group11[0].CST.Cast<CSTSegment>().FirstOrDefault()?.CustomsIdentityCodes1.CustomsCodeIdentification ?? ZString.Empty;
				ZInt result;
				return ZInt.TryParse(resultString, out result) ? result : ZInt.Zero;
			}
		}

		public ZInt NumberOfValidTransactions
		{
			get
			{
				var resultString = cusresMessage.Group6[0].Group11[0].CST.Cast<CSTSegment>().FirstOrDefault()?.CustomsIdentityCodes2.CustomsCodeIdentification ?? ZString.Empty;
				ZInt result;
				return ZInt.TryParse(resultString, out result) ? result : ZInt.Zero;
			}
		}

		public ZInt NumberOfInValidTransactions
		{
			get
			{
				var resultString = cusresMessage.Group6[0].Group11[0].CST.Cast<CSTSegment>().FirstOrDefault()?.CustomsIdentityCodes3.CustomsCodeIdentification ?? ZString.Empty;
				ZInt result;
				return ZInt.TryParse(resultString, out result) ? result : ZInt.Zero;
			}
		}
	}
}
