using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public struct MessageNotification
	{
		public MessageNotification(ZString line, ZString code, ZString text)
		{
			Line = line;
			Code = code;
			Text = text;
		}
		public ZString Line;
		public ZString Code;
		public ZString Text;
	}

	public abstract class CMRIncomingMessage : CMRMessage
	{
		public CMRIncomingMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly new TypeDecider TypeDecider = new CMRIncomingMessageTypeDecider();

		protected override IStreamFormatter MessageStreamFormatter
		{
			get
			{
				return new CMRMessageStreamFormatter(GetReportForFormattedMessage());
			}
		}

		protected virtual ZString GetReportForFormattedMessage()
		{
			return Report + "\r\n";
		}

		public virtual bool SupportsHTMLResponseEmails
		{
			get { return false; }
		}

		public ZString ReportForHTMLHeaderSection
		{
			get { return reportForHTMLHeaderSection ?? (reportForHTMLHeaderSection = GetReportForHTMLHeaderSection()).Value; }
		}
		ZString? reportForHTMLHeaderSection;

		protected virtual ZString GetReportForHTMLHeaderSection()
		{
			return ZString.Empty;
		}

		public ZString ReportForHTMLFooterSection
		{
			get { return reportForHTMLFooterSection ?? (reportForHTMLFooterSection = GetReportForHTMLFooterSection()).Value; }
		}
		ZString? reportForHTMLFooterSection;

		protected virtual ZString GetReportForHTMLFooterSection()
		{
			return ZString.Empty;
		}

		public List<MessageNotification> ErrorNotifications
		{
			get { return errorNotifications ?? (errorNotifications = GetErrorNotifications()); }
		}
		List<MessageNotification> errorNotifications;

		protected virtual List<MessageNotification> GetErrorNotifications()
		{
			return new List<MessageNotification>();
		}

		public virtual ZString ErrorNotificationsCaption
		{
			get { return ZString.Empty; }
		}

		public List<MessageNotification> LineStatusNotifications
		{
			get { return lineStatusNotifications ?? (lineStatusNotifications = GetStatusNotifications()); }
		}
		List<MessageNotification> lineStatusNotifications;

		protected virtual List<MessageNotification> GetStatusNotifications()
		{
			return new List<MessageNotification>();
		}

		public virtual ZString LineStatusNotificationsCaption
		{
			get { return ZString.Empty; }
		}

		public List<MessageNotification> AdviceNotifications
		{
			get { return adviceNotifications ?? (adviceNotifications = GetAdviceNotifications()); }
		}
		List<MessageNotification> adviceNotifications;

		protected virtual List<MessageNotification> GetAdviceNotifications()
		{
			return new List<MessageNotification>();
		}

		public abstract ZString GetReport();

		protected sealed override ZString GetReportCore()
		{
			ZString result;
			try
			{
				result = GetReport();
			}
			catch (InvalidFormatException ex)
			{
				result = "Invalid format: " + ex.Message;
			}

			return result;
		}

		protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
		{
			//do nothing - we are an incoming message
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		}
	}
}
