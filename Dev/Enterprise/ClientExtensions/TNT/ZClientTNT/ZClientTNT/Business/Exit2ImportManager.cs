using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.TNT
{
	public class Exit2ImportManager : NonPersistentBusinessObject, INotifications, IObsoleteValidation
	{
		public Exit2ImportManager(BusinessObjectFactory factory, string exit2FilePath)
			: base(factory)
		{
			fExit2FileInfo = new FileInfo(exit2FilePath);
		}

		#region Collections

		QuantumMawbCollection fExit2Mawbs;
		public QuantumMawbCollection Exit2Mawbs
		{
			get { return QuantumMawbCollectionCore; }
		}

		protected virtual QuantumMawbCollection QuantumMawbCollectionCore
		{
			get
			{
				if (fExit2Mawbs == null)
				{
					fExit2Mawbs = new QuantumMawbCollection(Factory, fExit2FileInfo.FullName);
					fExit2Mawbs.LoadFromFile();
				}
				return fExit2Mawbs;
			}
		}

		#endregion

		#region Collection Lists for FindBoxes

		#region BindingLists

		protected BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		#endregion

		#region RefUNLOCO_List

		public RefUNLOCOCollection RefUNLOCO_List
		{
			get { return BindingLists.RefUNLOCO_List; }
		}

		#endregion

		#endregion

		#region Properties

		#region FileName

		public virtual ZString FileFullName
		{
			get { return fExit2FileInfo.FullName; }
		}

		public ZPropertyInfo FileFullNameInfo
		{
			get { return GetZPropertyInfo(nameof(FileFullName)); }
		}

		#endregion

		#endregion

		#region SaveFactories

		protected ITransactionParticipant[] fSaveFactories;
		public IReadOnlyList<ITransactionParticipant> SaveFactories
		{
			get
			{
				if (fSaveFactories == null)
				{
					fSaveFactories =
						new ITransactionParticipant[]
						{
							new MawbInterfaceDetailSaver(Exit2Mawbs),
							new SaveInTransactionActionProcessedFileMover(fExit2FileInfo, QuantumFileProcessedDirectory)
						};
				}

				return fSaveFactories;
			}
		}

		#endregion

		#region INotifications

		#region NotificationBuffer

		protected StringBuilder NotificationBuffer
		{
			get
			{
				if (fNotificationBuffer == null)
				{
					fNotificationBuffer = new StringBuilder();
				}
				return fNotificationBuffer;
			}
		}

		StringBuilder fNotificationBuffer;

		#endregion

		public ZString NotificationText
		{
			get { return NotificationBuffer.ToString(); }
		}

		#region ErrorBuffer

		protected StringBuilder ErrorBuffer
		{
			get
			{
				if (fErrorBuffer == null)
				{
					fErrorBuffer = new StringBuilder();
				}
				return fErrorBuffer;
			}
		}

		StringBuilder fErrorBuffer;

		#endregion

		public ZString ErrorText
		{
			get { return ErrorBuffer.ToString(); }
		}

		#region INotifications Members

		void INotifications.Add(INotification @event)
		{
			Notify(@event);
		}

		void Notify(INotification @event)
		{
			NotificationBuffer.Append(@event.Message + System.Environment.NewLine);
			if (@event.Type is ErrorType)
			{
				ErrorBuffer.Append(@event.Message + System.Environment.NewLine);
			}
		}

		#endregion

		#endregion

		#region Exit2 Processing

		public bool IsAnyMawbLinkedToAnEnterpriseConsol
		{
			get
			{
				bool result = false;

				foreach (QuantumMawb mawb in Exit2Mawbs)
				{
					if (mawb.IsLinkedToConsol)
					{
						result = true;
						break;
					}
				}

				return result;
			}
		}

		public void ProcessAllMatches(bool createDeclaration)
		{
			HousebillsInInterfaceFileList.Instance.Clear();
			Exit2Mawbs.Factory.SuspendValidation();

			int noOfRecordsToBeProcessed = Exit2Mawbs.Sum(rec => ((QuantumMawb)rec).fQuantumSegment.Shipments.Length);
			int processedRecords = 0;

			foreach (QuantumMawb mawb in Exit2Mawbs)
			{
				mawb.OnProgress += new TNTProgressEventHandler(Exit2ImportManager_OnProgress);
				mawb.LinkMawbShipmentsToConsolCreatingNonExisting(this, createDeclaration, processedRecords, noOfRecordsToBeProcessed);
				processedRecords += mawb.fQuantumSegment.Shipments.Length;
			}
		}

		public event TNTProgressEventHandler OnProgress;

		public string GetExit2ProcessReport()
		{
			StringBuilder result = new StringBuilder();
			result.Append("Consol ID\t\t- MasterBill\t- Shipments Linked" + System.Environment.NewLine + System.Environment.NewLine);
			List<ZGuid> processedConsolPKs = new List<ZGuid>();
			foreach (QuantumMawb mawb in Exit2Mawbs)
			{
				if (mawb.LinkedConsol != null && !processedConsolPKs.Contains(mawb.LinkedConsol.PK))
				{
					result.Append(mawb.LinkedConsolUniqueConsignRef + "\t- ");
					if (mawb.MasterBillNum.IsEmpty)
					{
						result.Append("\t\t- ");
					}
					else
					{
						result.Append(mawb.MasterBillNum + "\t- ");
					}
					result.Append(mawb.LinkedConsol.Shipments.Count.ToString());
					result.Append(System.Environment.NewLine);
					processedConsolPKs.Add(mawb.LinkedConsol.PK);
				}
			}

			return result.ToString().Trim();
		}

		#endregion

		protected FileInfo fExit2FileInfo;

		void Exit2ImportManager_OnProgress(object sender, TNTProgressEventArgs e)
		{
			if (OnProgress != null)
			{
				OnProgress(this, e);
			}
		}

		protected virtual ZString QuantumFileProcessedDirectory
		{
			get { return TNTDataRegistry.Instance.QuantumFileProcessedDirectoryForManualImport; }
		}
	}
}
