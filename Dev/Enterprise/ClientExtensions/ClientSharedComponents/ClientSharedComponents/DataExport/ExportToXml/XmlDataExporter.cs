using System;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ClientSharedComponents.DataExport
{
	public abstract class XmlDataExporter
	{
		public void Export(BusinessObject bizOToExport, INotifications notifications)
		{
			ExportCore(bizOToExport, notifications);
		}

		protected virtual void ExportCore(BusinessObject bizOToExport, INotifications notifications)
		{
			string objectReference = GetBizOReference(bizOToExport);
			string fileName = Path.Combine(ExportDirectory, objectReference + FileNamePostfix + ".xml");
			NotificationBuffer notify = new NotificationBuffer(notifications);
			try
			{
				using (Stream toFile = File.Create(fileName))
				{
					DoExport(toFile, bizOToExport, notify);
					bizOToExport.Factory.Save();
				}
				if (notify.HasErrors)
				{
					SendEmailToNotificationGroup(notify.AsString, objectReference, fileName);
				}
				OnAfterExport(notify);
			}
			catch (IOException ex)
			{
				SendEmailToNotificationGroup(ex.Message, objectReference);
				OnIOException(ex.Message);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				notify.Notify(new ErrorNotification(ErrorType.Error, ex.Message));
			}
		}

		protected virtual void DoExport(Stream fileName, BusinessObject bizOToExport, INotifications notify)
		{
			XmlValueObjectSerializer serialiser = new XmlValueObjectSerializer(Adapter.ValueObjectType);
			serialiser.ExportXmlData(fileName, Adapter, new BusinessObject[] { bizOToExport }, new ValueObjectExportContext(notify));
		}

		#region Adapter

		protected IValueObjectDataAdapter Adapter
		{
			get
			{
				if (fAdapter == null)
				{
					fAdapter = GetValueObjectDataAdapter();
				}
				return fAdapter;
			}
		}
		IValueObjectDataAdapter fAdapter;

		protected void ResetAdapter()
		{
			fAdapter = null;
		}

		#endregion

		protected void SendEmailToNotificationGroup(ZString bodyContents, ZString objectReference)
		{
			SendEmailToNotificationGroup(bodyContents, objectReference, ZString.Empty);
		}

		protected void SendEmailToNotificationGroup(ZString bodyContents, ZString objectReference, ZString pathToFile)
		{
			EmailDef email = new EmailDef();
			email.Body = bodyContents;
			email.Subject = objectReference + ": " + EmailSubject;
			if (!pathToFile.IsEmpty)
			{
				email.Attachments.Add(new AttachmentDef(pathToFile));
			}
			Env.OutgoingMailManager.CreateAndSave(email, NotificationGroupGuid, GroupSourceLocator.GetFromRegistryItem(NotificationGroup));
		}

		protected virtual void OnIOException(ZString errorMessage)
		{
		}

		protected virtual void OnAfterExport(NotificationBuffer notify)
		{
		}

		protected virtual ZString FileNamePostfix
		{
			get { return "_" + ZDateTime.Now.ToString("yyyyMMddhhmmss"); }
		}

		protected virtual ZString EmailSubject
		{
			get { return "Error exporting to Xml"; }
		}

		protected abstract IValueObjectDataAdapter GetValueObjectDataAdapter();
		protected abstract ZString GetBizOReference(BusinessObject bizOToExport);
		protected abstract ZString ExportDirectory { get; }
		protected abstract IRegistryItem NotificationGroup { get; }
		protected abstract Guid NotificationGroupGuid { get; }
	}
}
