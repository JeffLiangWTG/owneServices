using System.Globalization;
using System.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Business
{
	public class LogFileRecord : AutoLogFileRecord
	{
		public LogFileRecord(ZString name, ZString host)
			: base(name, host)
		{ }

		public ILogViewerDataProvider LogViewerDataProvider { get; set; }

		public EventRecordCollection EventList
		{
			get
			{
				if (_eventList == null)
				{
					_eventList = new EventRecordCollection();
					RegisterEditableChildObject(_eventList);
				}

				if (reload && LogViewerDataProvider != null)
				{
					UnRegisterEditableChildObject(_eventList);
					var sorting = _eventList.SortInformation;
					_eventList = new EventRecordCollection();

					var buffer = LogViewerDataProvider.GetBytes(Name);
					if (buffer != null)
					{
						using (var stream = new MemoryStream(buffer))
						{
							_eventList.Load(stream);
						}
						reload = false;
					}

					if (sorting != null)
					{
						_eventList.Sort(sorting);
					}
				}

				return _eventList;
			}
		}

		EventRecordCollection _eventList;

		public void ReloadEvents()
		{
			reload = true;
			EventList.RefreshBinding();
		}

		public void Send(params string[] recipients)
		{
			if (recipients.Length == 0)
			{
				return;
			}

			var mail = new EmailDef();
			mail.Subject = string.Format(CultureInfo.InvariantCulture, "{0} on {1}", Name, Host);
			foreach (var recipient in recipients)
			{
				mail.AddRecipientForUserCommunication(recipient);
			}
			using (var ms = new MemoryStream())
			{
				var zipCreator = new ZipCreator();
				var buffer = LogViewerDataProvider.GetBytes(Name);
				if (buffer != null)
				{
					using (var stream = new MemoryStream(buffer))
					{
						zipCreator.ZipStream(Name, stream, ms);
					}
				}
				var attachment = new AttachmentDef(Name + ".zip", ms.ToArray());
				mail.Attachments.Add(attachment);
			}
			Env.OutgoingMailManager.CreateAndSave(mail);
		}

		bool reload = true;
	}
}

