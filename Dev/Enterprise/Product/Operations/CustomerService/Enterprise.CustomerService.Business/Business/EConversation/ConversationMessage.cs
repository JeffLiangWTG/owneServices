using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.CustomerService.Business
{
	public class ConversationMessage : AutoConversationMessage, IConversationMessage
	{
		public static class MessageTypes
		{
			public const string LocalInternal = "LIN";
			public const string LocalPublished = "LOC";
			public const string Remote = "RMT";
		}

		public static class MessageSubTypes
		{
			public const string UserMessage = "USR";
			public const string SystemLog = "SYS";
		}

		public ConversationMessage(ConversationNote note)
			: base(note.Factory)
		{
			this.note = note;
			if (!note.ST_NoteText.IsEmpty)
			{
				LoadFromXmlString(note.ST_NoteText);
				if (IsRatingSupported)
				{
					rating = GetRatingFromDb();
				}
			}
			if (!note.ST_ParentID.IsValid)
			{
				note.ST_ParentID = this.PK;
				note.ST_Table = this.TableName;
			}
		}

		readonly ConversationNote note;

		#region IConversationMessage

		static readonly MultilingualString SystemMessageDisplayName = ResString.GetMultilingualString("ad0a27fc-6697-4c64-b551-23369ee056cc", "Automatic Message");

		ZString IConversationMessage.SenderDisplayName => IsSystemMessage ? SystemMessageDisplayName : UserName;
		ZString IConversationMessage.SenderCode => UserCode;
		bool IConversationMessage.IsRatingEnabled => CanRate && IsRatingSupported;
		ZString IConversationMessage.AdditionalNoteForDisplay => AddtionalNoteForDisplay; // I am leaving the spelling mistake in this because it'd break all existing XML to rename it now.
		int IConversationMessage.Rating => Rating;

		MessageType IConversationMessage.MessageType
		{
			get
			{
				switch (MessageType)
				{
					case MessageTypes.LocalInternal:
						return Enterprise.EConversation.Business.MessageType.LocalInternal;
					case MessageTypes.LocalPublished:
						return Enterprise.EConversation.Business.MessageType.LocalPublished;
					case MessageTypes.Remote:
						return Enterprise.EConversation.Business.MessageType.Remote;
					default:
						throw new InvalidOperationException("Unrecognised MessageType - '" + MessageType + "'");
				}
			}
		}

		MessageSubType IConversationMessage.MessageSubType
		{
			get
			{
				if (string.IsNullOrEmpty(MessageSubType) || MessageSubType == MessageSubTypes.UserMessage)
				{
					return Enterprise.EConversation.Business.MessageSubType.UserMessage;
				}
				else if (MessageSubType == MessageSubTypes.SystemLog)
				{
					return Enterprise.EConversation.Business.MessageSubType.SystemLog;
				}
				else
				{
					throw new InvalidOperationException("Unrecognised MessageSubType - '" + MessageSubType + "'");
				}
			}
		}

		public bool IsSystemMessage => UserCode == User.ServiceUserCode || UserCode == User.WebUserCode;

		#endregion

		#region Properties

		internal bool IsLocalMessage
		{
			get { return MessageType == MessageTypes.LocalInternal || MessageType == MessageTypes.LocalPublished; }
		}

		internal bool IsLocalPublishedMessage
		{
			get { return MessageType == MessageTypes.LocalPublished; }
		}

		internal bool IsRemoteMessage
		{
			get { return MessageType == MessageTypes.Remote; }
		}

		internal ZGuid NotePk
		{
			get { return note.PK; }
		}

		public ZDateTime SendLocalDateTime
		{
			get { return !SentTimeInUtc.IsEmpty && SentTimeInUtc.IsValid ? EnvProxy.Instance.Time.GetLocalTimeFromUtc(SentTimeInUtc.ToDateTime()) : DateTime.MinValue; }
		}

		public ZString AddtionalNoteForDisplay
		{
			get
			{
				if (MessageType == MessageTypes.LocalInternal)
				{
					return (NoResString)"[Internal Only]";
				}
				else if (!AdditionalNote.IsEmpty)
				{
					return string.Format("[{0}]", AdditionalNote);
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public override string TableName
		{
			get
			{
				return "ConversationMessage[NonPersistent]";
			}
		}

		public override bool IsInDatabase
		{
			get { return note.IsInDatabase; }
		}

		public ZDateTime SystemCreateTimeInUtc
		{
			get { return note.ST_SystemCreateTimeUtc; }
		}

		#endregion

		#region Like / Dislike Rating

		public const int MaxRating = 3;
		public const int MinRating = -3;

		public bool CanRate { get; set; }
		public bool IsRatingSupported
		{
			get { return Id.IsValid && MessageSubType == MessageSubTypes.UserMessage; }
		}

		public ZInt Rating
		{
			get { return rating; }
			private set
			{
				if (value < MinRating || value > MaxRating)
				{
					throw new ArgumentException("Rating must be between min and max value (-3 to 3)");
				}

				SetNonPersistentPropertyValue(RatingInfo, ref rating, value);
				AddOrUpdateRatingLog();
			}
		}

		ZPropertyInfo RatingInfo => GetZPropertyInfo(nameof(Rating));

		public void Like()
		{
			if (Rating < MaxRating)
			{
				Rating++;
			}
		}

		public void Dislike()
		{
			if (Rating > MinRating)
			{
				Rating--;
			}
		}

		public void LogRemoteRatingChange(Xsd.ConversationMessageRating remoteMessageRating)
		{
			AddRatingLog(remoteMessageRating.Rating, remoteMessageRating.UserCode, remoteMessageRating.UserName);
		}

		public bool HasRatingChanged
		{
			get { return ratingLog != null && !ratingLog.IsInDatabase; }
		}

		void AddOrUpdateRatingLog()
		{
			if (ratingLog == null || ratingLog.IsInDatabase)
			{
				ratingLog = AddRatingLog(Rating, GlbStaff.CurrentUser.GS_Code, GlbStaff.CurrentUser.GS_FullName);
			}
			else
			{
				string logText = GetRatingLogText(Rating, GlbStaff.CurrentUser.GS_Code, GlbStaff.CurrentUser.GS_FullName);
				ratingLog.UpdateReference(logText);
			}
		}

		StmALog AddRatingLog(int rating, ZString userCode, ZString userName)
		{
			string logText = GetRatingLogText(rating, userCode, userName);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			return note.Logs.AddNew(AutoEvents.EditedARecord, logText);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
		}

		static string GetRatingLogText(int rating, ZString userCode, ZString userName)
		{
			string logText = string.Format("{0}_{1}_{2}_{3}", RatingLogReference, rating, userCode, userName);
			if (logText.Length > StmALogSchema.SL_Reference.MaxLength)
			{
				logText = logText.Substring(0, StmALogSchema.SL_Reference.MaxLength);
			}
			return logText;
		}

		int GetRatingFromDb()
		{
			int result = 0;

			ZQuery logQuery = new ZQuery();
			logQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, RatingLogReference);
			logQuery.AddToFilter(StmALogSchema.SL_Parent, note.PK);
			logQuery.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;

			StmALog log = note.Factory.LoadTop1<StmALog>(logQuery);
			if (log != null)
			{
				ZString[] words = log.SL_Reference.Split('_');
				if (words.Length > 1)
				{
					Int32.TryParse(words[1], out result);
				}
			}

			return result;
		}

		ZInt rating;
		StmALog ratingLog;
		const string RatingLogReference = "RATING";

		#endregion

		#region XML

		public void SetNoteText()
		{
			note.ST_NoteText = this.ToXmlString();
		}

		internal ZString ToXmlString()
		{
			StringBuilder builder = new StringBuilder();
			using (var xmlWriter = XmlWriter.Create(builder, new XmlWriterSettings() { OmitXmlDeclaration = true }))
			{
				xmlWriter.WriteStartElement("Message");
				((IXmlSerializable)this).WriteXml(xmlWriter);
				xmlWriter.WriteEndElement();
			}

			return builder.ToString();
		}

		void LoadFromXmlString(string xmlString)
		{
			using (StringReader reader = new StringReader(xmlString))
			using (var xmlReader = XmlReader.Create(reader))
			{
				xmlReader.Read();
				xmlReader.MoveToContent();
				((IXmlSerializable)this).ReadXml(xmlReader);
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			base.Delete();
			note.Delete();
		}

		#endregion
	}
}
