using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.EConversation.Business
{
	[DebuggerDisplay("Msg: {SenderCode} - {JCM_Body}")]
	public class JobConversationMessage : AutoJobConversationMessage, IConversationMessage
	{
		/* MaxChunkSize is used to crop the value of JCM_Body
		 * Because when we create a new JobConversationMessage where the JCM_Body has a value bigger than 42000 caracters (ZLargeColumnSaver.MaxChunkSize)
		 * the system will try to save the record first with an empty JCM_Body and then update the value of JCM_Body in a later sql UPDATE.
		 * 
		 * If we try to save a JobConversationMessage with empty JCM_Body we get a constraint violation because Constraint_JCM_Body is [JCM_Body]<>''
		 * 
		 * ZSqlCommandBuilder.AddDataParameterAndBlobSaverIfLargeBlobOrText #81
		 * Value from ZLargeColumnSaver.MaxChunkSize
		 */
		const int MaxChunkSize = 42000;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public JobConversationMessage(BusinessObjectFactory factory, DataRow dataRow)
			: base(factory, dataRow)
		{ }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			JCM_IsLocal = true;
			JCM_PostedTimeUtc = ZDateTime.UtcNow;
			JCM_Language = Core.SharedConstants.Languages.English;
		}

		public JobConversation Conversation => Factory.Load<JobConversation>(JCM_JCC_Conversation);
		public JobConversationParticipant Sender => Factory.Load<JobConversationParticipant>(JCM_JCP_Participant);

		[RelatedBusinessObject("Conversation")]
		public override ZGuid JCM_JCC_Conversation
		{
			get { return base.JCM_JCC_Conversation; }
			set { base.JCM_JCC_Conversation = value; }
		}

		[RelatedBusinessObject("Sender")]
		public override ZGuid JCM_JCP_Participant
		{
			get { return base.JCM_JCP_Participant; }
			set { base.JCM_JCP_Participant = value; }
		}

		public static string TrimBody(string body)
		{
			// Trim body to avoid Constraint_JCM_Body issue when saving a new message
			if (!string.IsNullOrEmpty(body) && body.Length > MaxChunkSize)
			{
				return body.Substring(0, MaxChunkSize);
			}
			return body;
		}

		#region IConversationMessage

		public ZString AdditionalNoteForDisplay
		{
			get
			{
				if (JCM_IsInternal && JCM_IsSystem)
				{
					return Res.GetString("76da020f-fb32-4c26-ba1a-c5aecec294a5", "[Internal System Message]");
				}
				else if (JCM_IsInternal)
				{
					return Res.GetString("02D4FCA8-5869-49FC-B83C-B1C90DA1CA97", "[Internal Only]");
				}
				else if (JCM_IsSystem)
				{
					return Res.GetString("49623c4b-b04d-462c-a10e-89cb46dd0ae9", "[System Message]");
				}
				else if (JCM_IsBroadcast)
				{
					return Res.GetString("96d9443d-0c55-4781-a684-aee9f6316455", "[Broadcast]");
				}
				else
				{
					return string.Empty;
				}
			}
		}

		public ZString Body => JCM_Body;
		public bool HasRatingChanged => false;
		public ZGuid Id => PK;
		public int Rating => JCM_Score;
		public bool IsSystemMessage
		{
			get
			{
				var sender = Sender;
				if (sender == null)
				{
					return true;
				}
				else if (sender.JCP_ParticipantTableCode == "GS")
				{
					var staffCode = sender.Parent.Code;
					return staffCode == User.ServiceUserCode || staffCode == User.WebUserCode;
				}

				return false;
			}
		}

		public MessageType MessageType
		{
			get
			{
				if (JCM_IsLocal)
				{
					return JCM_IsInternal ? MessageType.LocalInternal : MessageType.LocalPublished;
				}
				else
				{
					return MessageType.Remote;
				}
			}
		}
		public MessageSubType MessageSubType => JCM_IsSystem ? MessageSubType.SystemLog : MessageSubType.UserMessage;
		public ZString SenderCode => Sender?.Parent.Code ?? string.Empty;
		public ZBool HideStaffName { get; set; } = false;
		public ZString SenderDisplayName
		{
			get
			{
				if (HideStaffName && !IsSystemMessage && MessageType == MessageType.LocalPublished)
				{
					return Res.GetString("BEDB811D-FFE5-481F-9FB1-708D4F276C30", "Customer Support");
				}

				return !IsSystemMessage ? (string)Sender.Parent.Name : Res.GetString("8364601c-8cb5-413a-b614-099d6ed5a1e7", "System");
			}
		} 
		public ZDateTime SendLocalDateTime => AsLocalDateTime(JCM_PostedTimeUtc);
		public ZDateTime SystemCreateTimeInUtc => JCM_PostedTimeUtc;

		#region Rating

		public const int MaxRating = 3;
		public const int MinRating = -3;

		public bool IsRatingEnabled => false; // rating is only editable from the web portal

		public void Dislike()
		{
			throw new NotImplementedException();
		}

		public void Like()
		{
			throw new NotImplementedException();
		}

		#endregion

		#endregion

		static ZDateTime AsLocalDateTime(ZDateTime utc)
		{
			var local = Env.Time.GetLocalTimeFromUtc(utc.ToDateTime());
			return new ZDateTime(local);
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			JCM_Body = "If Im left blank than the base tries to fill me to my maximum length - which is very very high";

			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif
	}
}
