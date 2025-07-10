using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public abstract class JXCMessageProcessor
	{
		public JXCMessageProcessor(JXCRecord[] records)
		{
			this.Records = records;
			if (records.Length > 2)
			{
				BodyRecords = new JXCRecord[records.Length - 2];
				Array.Copy(records, 1, BodyRecords, 0, BodyRecords.Length);
			}
			else
			{
				BodyRecords = Array.Empty<JXCRecord>();
			}
		}

		public ZString MessageType
		{
			get { return (Records.Length > 1) ? Records[1].LineType : ZString.Empty; }
		}

		public bool ProcessRecords(BusinessObjectFactoryProvider factoryProvider, INotifications notificationSubscriber)
		{
			bool result = false;

			if (Records.Length > 2)
			{
				bool isFirstLineHeader = (HEADRecord != null);
				bool isLastLineTrailer = (TRLRRecord != null);
				bool isValidFirstLine = BodyRecords[0].GetType().IsAssignableFrom(FirstLineType);

				if (!isFirstLineHeader || !isLastLineTrailer)
				{
					NotifyStructureError(notificationSubscriber, "HEAD and/or TRLR does not exist.");
				}
				else if (!isValidFirstLine)
				{
					NotifyStructureError(notificationSubscriber, "First body line (" + BodyRecords[0].LineType + ") is invalid");
				}
				else
				{
					result = ProcessRecordsCore(factoryProvider, notificationSubscriber);
				}
			}
			else
			{
				NotifyStructureError(notificationSubscriber, "Invalid JXC file");
			}

			return result;
		}

		void NotifyStructureError(INotifications notificationSubscriber, string errorMessage)
		{
			ErrorNotification errorNotification = new ErrorNotification(ErrorType.InvalidFileFormat, errorMessage);
			notificationSubscriber.Notify(errorNotification);
		}

		protected abstract Type FirstLineType { get; }
		protected abstract bool ProcessRecordsCore(BusinessObjectFactoryProvider factoryProvider, INotifications notificationSubscriber);

		protected int LastChildrenRecordIndex
		{
			get { return Records.Length - 2; }
		}

		protected HEADRecord HEADRecord
		{
			get { return (Records.Length > 0) ? Records[0] as HEADRecord : null; }
		}

		protected TRLRRecord TRLRRecord
		{
			get { return (Records.Length > 2) ? Records[Records.Length - 1] as TRLRRecord : null; }
		}

		public readonly JXCRecord[] Records;
		public readonly JXCRecord[] BodyRecords;
	}
}

#region Implementation
#endregion
