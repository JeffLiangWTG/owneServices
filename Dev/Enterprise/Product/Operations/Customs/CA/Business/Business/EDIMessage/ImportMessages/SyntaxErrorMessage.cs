using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	class SyntaxErrorMessage : EDIMessageWithBatchNumber
	{
		public SyntaxErrorMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static (ZString sourceMessageTextWithErrorMarks, IEnumerable<SyntaxError> syntaxErrors) GetSyntaxErrorData(EDIMessage syntaxMessage, ZString originalMessageText)
		{
			if (syntaxMessage == null)
			{
				return (ZString.Empty, Enumerable.Empty<SyntaxError>());
			}
			var syntaxErrors = syntaxMessage.EdifactMessage.GetSyntaxErrors(originalMessageText);
			var sourceMessageTextWithErrorMarks = GetErrorMarks(originalMessageText, syntaxErrors);
			return (sourceMessageTextWithErrorMarks, syntaxErrors);
		}

		#region SourceMessageTextWithErrorMarks

		internal ZString SourceMessageTextWithErrorMarks
		{
			get
			{
				if (sourceMessageTextWithErrorMarks == null)
				{
					sourceMessageTextWithErrorMarks = GetErrorMarks(SourceMessageText, SyntaxErrors);
				}
				return sourceMessageTextWithErrorMarks;
			}
		}
		string sourceMessageTextWithErrorMarks;

		static ZString GetErrorMarks(ZString messageText, IEnumerable<SyntaxError> syntaxErrors)
		{
			var builder = new ZStringBuilder();
			if (!messageText.IsEmpty)
			{
				var lineNumber = 0;
				foreach (var line in messageText.TrimEnd(SyntaxError.Delimiter).Split(SyntaxError.Delimiter))
				{
					var lineNum = lineNumber.ToString();
					var lineErrors = syntaxErrors.Where(err => err.LineNumber == lineNum);
					if (lineErrors.Any())
					{
						builder.Append(string.Format("\r\n## {0} - {1}'", lineNumber, line));
						lineErrors.Aggregate(builder, (b, error) => b.Append("## " + error));
						builder.Append(string.Empty);
					}
					else
					{
						builder.Append(string.Format("{0} - {1}'", lineNumber, line));
					}
					lineNumber++;
				}

				syntaxErrors.Where(err => err.LineNumber.IsEmpty).Aggregate(builder, (b, error) => b.Append("\r\n## " + error));
			}
			return builder.ToStringWithNewLineBetweenAppends();
		}

		#region SyntaxErrors

		internal IEnumerable<SyntaxError> SyntaxErrors
		{
			get { return syntaxErrors ?? (syntaxErrors = EdifactMessage.GetSyntaxErrors(SourceMessageText)); }
		}

		IEnumerable<SyntaxError> syntaxErrors;

		#endregion

		#region SourceMessageText

		ZString SourceMessageText
		{
			get { return sourceMessageText ?? (sourceMessageText = OriginalMessage?.EM_MessageText ?? ZString.Empty); }
		}
		string sourceMessageText;

		public override EDIMessage OriginalMessage
		{
			get
			{
				if (fOriginalMessage == null)
				{
					fOriginalMessage = GetOriginalMessage(this);
				}
				return fOriginalMessage;
			}
		}
		EDIMessage fOriginalMessage;

		public static EDIMessage GetOriginalMessage(EDIMessage receiveMessage)
		{
			if (receiveMessage.IsTransmitMessage)
			{
				throw new InvalidOperationException("Original Message is only available for response messages. This message is a transmit");
			}
			var originalMessageNo = receiveMessage.EdifactMessage.GetOriginalMessageNo();
			var linkUniqueID = receiveMessage.EM_LinkUniqueID;
			var batchNumber = receiveMessage.BatchNumber;
			EDIMessage result = null;
			if (!originalMessageNo.IsEmpty || !linkUniqueID.IsEmpty || !batchNumber.IsEmpty)
			{
				var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, receiveMessage.EM_ApplicationCode);
				query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, Direction.Transmit);
				query.AddToFilter(EDIMessageSchema.EM_Status, Status.Sent);
				var systemCreateTimeUtc = receiveMessage.EM_SystemCreateTimeUtc;
				query.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, systemCreateTimeUtc);
				query.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, systemCreateTimeUtc.AddYears(-1));
				query.AddToFilter(EDIMessageSchema.EM_MessageType, receiveMessage.EM_MessageSubType);
				if (!originalMessageNo.IsEmpty)
				{
					query.AddToFilter(EDIMessageSchema.EM_MessageNum, originalMessageNo);
				}
				if (!linkUniqueID.IsEmpty)
				{
					query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, linkUniqueID);
					query.AddToFilter(EDIMessageSchema.EM_LinkTable, receiveMessage.EM_LinkTable);
				}
				else
				{
					query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, null);
				}
				query.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + OrderByClause.Descending;
				var factory = receiveMessage.Factory;
				result = batchNumber.IsEmpty ? factory.LoadTop1<EDIMessage>(query)
										: factory.Load<EDIMessage>(query).FirstOrDefault(x => x.BatchNumber == batchNumber);
			}
			return result;
		}
		#endregion

		#endregion

		#region Overrides

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", "IMP", ApplicationCodes.CAIMP).GetNextFormatted(Factory);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.CAIMP;
			EM_MessageType = MessageTypeList.Codes.SyntaxError;
		}

		#region Properties

		protected override IStreamFormatter MessageStreamFormatter
		{
			get
			{
				return SourceMessageTextWithErrorMarks.IsEmpty ? new EDIMessageStreamFormatter()
						: new SyntaxErrorMessageStreamFormatter(
							Res.GetString("07c1da43-5c51-4a30-9d41-15eec8668b34",
										  "Source Message Interpretation:\r\n\r\n{0}\r\n\r\n\r\nMessage Text:",
										  SourceMessageTextWithErrorMarks), "\r\n\r\n");
			}
		}

		protected override bool ShouldUseUnformattedMessageText
		{
			get { return false; }
		}

		public override bool ShouldShowInterpretation
		{
			get { return true; }
		}

		protected override CodeDescriptionPairList MessageSubTypeList
		{
			get { return new MessageTypeList(); }
		}

		#endregion

		#endregion
	}
}
