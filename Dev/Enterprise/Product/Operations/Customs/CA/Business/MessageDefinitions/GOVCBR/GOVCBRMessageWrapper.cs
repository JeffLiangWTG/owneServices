using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class GOVCBRMessageWrapper
	{
		public GOVCBRMessageWrapper(EDIMessage message)
		{
			Message = Argument.NotNull(message, nameof(Message));
		}
		protected EDIMessage Message { get; }

		#region Has GOVCBR

		public virtual ZBool HasGOVCBRMessage => false;

		#endregion

		#region Document Name

		public ZString DocumentName
		{
			get
			{
				if (!documentName.HasValue)
				{
					documentName = GetDocumentName;
				}

				return documentName.Value;
			}
		}
		ZString? documentName;

		protected virtual ZString GetDocumentName => ZString.Empty;

		#endregion

		#region Document Reference

		public ZString DocumentReference
		{
			get
			{
				if (!documentReference.HasValue)
				{
					documentReference = GetDocumentReference;
				}

				return documentReference.Value;
			}
		}
		ZString? documentReference;

		protected virtual ZString GetDocumentReference => ZString.Empty;

		#endregion

		#region Processing Date

		public ZDateTime ProcessingDate
		{
			get
			{
				if (!processingDate.HasValue)
				{
					processingDate = GetProcessingDate;
				}

				return processingDate.Value;
			}
		}
		ZDateTime? processingDate;

		protected virtual ZDateTime GetProcessingDate => ZDateTime.Empty;

		#endregion

		#region Processing Indicators

		public virtual ZBool IsMessageContentAccepted => false;

		public virtual ZBool IsMessageContentAcceptedWithComments => false;

		public virtual ZBool IsMessageContentRejectedWithComment => false;

		public virtual ZBool IsMessageReceived => false;

		public virtual ZBool IsErrorMessage => false;

		#endregion

		#region Original Message Reference

		public ZString OriginalMessageReference
		{
			get
			{
				if (!originalMessageReference.HasValue)
				{
					originalMessageReference = GetOriginalMessageReference;
				}
				return originalMessageReference.Value;
			}
		}
		ZString? originalMessageReference;

		protected virtual ZString GetOriginalMessageReference => ZString.Empty;

		#endregion

		#region Error Comments

		public IEnumerable<string> ErrorComments
		{
			get
			{
				if (errorComments == null)
				{
					errorComments = GetErrorComments;
				}

				return errorComments;
			}
		}
		IEnumerable<string> errorComments;

		protected virtual IEnumerable<string> GetErrorComments => Array.Empty<string>();

		protected string ErrorCommentText(string textLiteral)
		{
			if (textLiteral.Length == 2)
			{
				switch (textLiteral)
				{
					case "20":
						return Res.GetString("245dfa72-14e1-4533-b261-225111152d33", "20-Administration");
					case "21":
						return Res.GetString("e40d05fd-a09c-41f7-a9f5-aa5d3a53fe33", "21-Enforcement");
					case "22":
						return Res.GetString("5ed9431c-c422-4471-a22e-3c4eaf638c4e", "22-Conformance");
					case "28":
						return Res.GetString("bb8c4c08-6f1f-4e6e-8cc4-1ba13a03c70c", "28-Batch Error");
					case "29":
						return Res.GetString("921f0557-75c4-4d98-9be7-a38dc327e5c1", "29-Data Error");
				}
			}
			return textLiteral;
		}

		#endregion

		#region Notifications

		public IEnumerable<Notification> Notifications
		{
			get
			{
				if (notifications == null)
				{
					notifications = GetNotifications;
				}

				return notifications;
			}
		}
		IEnumerable<Notification> notifications;

		protected virtual IEnumerable<Notification> GetNotifications => Array.Empty<Notification>();

		public class Notification : ITableInterpretation
		{
			internal Notification(EDIMessage message, string code)
			{
				this.code = code;
				this.message = message;
			}
			readonly EDIMessage message;

			#region Implementation of ITableInterpretation

			string ITableInterpretation.Caption
			{
				get
				{
					return Res.GetString("EDFB8D05-1041-4999-92B8-E6DC034459F6", "Error Messages");
				}
			}

			IEnumerable<string> ITableInterpretation.Titles
			{
				get
				{
					yield return Res.GetString("4d5be95b-450e-4103-91d7-774f2f032539", "Code");
					yield return Res.GetString("AC6C5D22-F6F0-4183-9CAC-C38DA930F8BE", "Error");
				}
			}

			IEnumerable<object> ITableValues.Values
			{
				get { return new[] { code, message.GetErrorDescription(code) }; }
			}

			#endregion

			internal readonly string code;
		}

		#endregion

		#region Notice Status Code

		public ZString NoticeStatusCode
		{
			get
			{
				if (!noticeStatusCode.HasValue)
				{
					noticeStatusCode = GetNoticeStatusCode;
				}
				return noticeStatusCode.Value;
			}
		}
		ZString? noticeStatusCode;

		protected virtual ZString GetNoticeStatusCode => ZString.Empty;

		#endregion
	}
}
