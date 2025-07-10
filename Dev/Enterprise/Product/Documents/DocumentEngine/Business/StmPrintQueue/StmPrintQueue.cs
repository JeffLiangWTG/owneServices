using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.RemotePrinting.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	[CodeProperty(AutoStmPrintQueue.Schema.SQ_DisplayName), DescriptionProperty(AutoStmPrintQueue.Schema.SQ_QueueName)]
	public class StmPrintQueue : AutoStmPrintQueue, Enterprise.Integration.DocumentEngine.IStmPrintQueue, ICodeDescription
	{
		public StmPrintQueue(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SQ_LastUsedDateTimeUtc), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SQ_PrintQueueStateChanged), ConcurrencyPolicy.Ignore);
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public override void OnSaving()
		{
			SQ_PrintQueueStateChanged = ZGuid.NewZGuid();
			if (!SQ_SPS_Server.IsValid)
			{
				SQ_ServerName = System.Environment.MachineName;
			}
			base.OnSaving();
		}

		public bool IsOnline => SQ_AllowPrinting && !SQ_QueueDeleted.IsValid;

		public override void Delete()
		{
			DeleteUnusedStmData();
			DeleteUnusedPrintQueueSecurity();
			ClearPlainPaperPrinterFKs();
			base.Delete();
		}

		void DeleteUnusedStmData()
		{
			var unusedStmData = new StmDataCollection(Factory, new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.StartsWith, FormattableString.Invariant($"ShowHelpForPrinter{PK.ToGuid():N}")));
			unusedStmData.DeleteAll();
		}

		void DeleteUnusedPrintQueueSecurity()
		{
			var unusedPrintQueueSecurity = new GlbSecurityCollection(Factory);
			var filter1 = new ZQuery(GlbSecuritySchema.GU_SecurityRight, "StmPrintQueue");
			var filter2 = new ZQuery(GlbSecuritySchema.GU_ItemGUID, PK.ToGuid());
			var filter = new ZQuery(filter1, filter2);
			unusedPrintQueueSecurity.Load(filter);
			unusedPrintQueueSecurity.DeleteAll();
		}

		void ClearPlainPaperPrinterFKs()
		{
			var plainPaperPrinters = Factory.Load<StmPrintQueue>(new ZQuery(StmPrintQueueSchema.SQ_SQ_PlainPaperPrinter, PK));
			foreach (var printer in plainPaperPrinters)
			{
				printer.SQ_SQ_PlainPaperPrinter = ZGuid.Empty;
			}
		}

		#region Security

		public bool IsPrintAllowed
		{
			get { return Env.Security.GetPrintQueueCheckPoint(PK.ToGuid(), SQ_DisplayName).IsAllowed; }
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList SQ_PrintLanguage_List => new PrintLanguagePairList();

		class PrintLanguagePairList : CodeDescriptionPairList
		{
			public PrintLanguagePairList()
			{
				AddPair(PrintLanguageTypes.None, ResString.GetMultilingualString("4f5296bc-2200-47b3-a910-75bc7e7ca6a0", "Not applicable"));
				AddPair(PrintLanguageTypes.Epson, ResString.GetMultilingualString("0d9637a6-bc69-451b-b6a7-49c6b2ec5de7", "Epson ESC/P"));
				AddPair(PrintLanguageTypes.IBM, ResString.GetMultilingualString("e08c8769-6c68-4ba5-877c-16c219fd309e", "IBM ProPrinter"));
				AddPair(PrintLanguageTypes.OKI, ResString.GetMultilingualString("8635b773-0a1c-441f-ad4c-c997f482fb22", "OKI MicroLine"));
			}
		}

		public static class PrintLanguageTypes
		{
			public const string None = "N/A";
			public const string Epson = "ESP";
			public const string IBM = "IBM";
			public const string OKI = "OML";
		}

		#region ICodeDescription Members

		string ICodeDescription.Code
		{
			get { return base.SQ_DisplayName; }
		}

		string ICodeDescription.Description
		{
			get { return base.SQ_QueueName; }
		}

		object ICodeDescription.PK
		{
			get { return base.PK; }
		}

		#endregion

		#endregion

		#region Properties

		[ActionField(CollectionType = typeof(PrintLanguagePairList))]
		public override ZString SQ_PrintLanguage { get => base.SQ_PrintLanguage; set => base.SQ_PrintLanguage = value; }

		#region ActionFieldHidden

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZDateTime SQ_QueueDeleted
		{
			get => base.SQ_QueueDeleted;
			set
			{
				if (value != SQ_QueueDeleted)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, string.Format("{0}, Service Task: {1}", value == ZDateTime.Empty ? (NoResString)"Set it to online" : (NoResString)"Set it to offline", Env.Instance.ServiceTaskCode));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
				base.SQ_QueueDeleted = value;
			}
		}

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZBool SQ_HasAvailableJob { get => base.SQ_HasAvailableJob; set => base.SQ_HasAvailableJob = value; }

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZBool SQ_IsPrivate { get => base.SQ_IsPrivate; set => base.SQ_IsPrivate = value; }

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZInt SQ_PaperHeight { get => base.SQ_PaperHeight; set => base.SQ_PaperHeight = value; }

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZString SQ_PaperName { get => base.SQ_PaperName; set => base.SQ_PaperName = value; }

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZInt SQ_PaperWidth { get => base.SQ_PaperWidth; set => base.SQ_PaperWidth = value; }

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZString SQ_PrinterCategory { get => base.SQ_PrinterCategory; set => base.SQ_PrinterCategory = value; }

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZBool SQ_PrinterIsColor { get => base.SQ_PrinterIsColor; set => base.SQ_PrinterIsColor = value; }

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZString SQ_WebPrintServiceAddress { get => base.SQ_WebPrintServiceAddress; set => base.SQ_WebPrintServiceAddress = value; }

		[ReadOnly(true)]
		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZDateTime SQ_LastUsedDateTimeUtc { get => base.SQ_LastUsedDateTimeUtc; set => base.SQ_LastUsedDateTimeUtc = value; }

		#endregion

		public ZDateTime SQ_LastUsedDateTimeLocal => base.SQ_LastUsedDateTimeUtc.IsValid ? Env.Time.GetLocalTimeFromUtc(base.SQ_LastUsedDateTimeUtc.ToDateTime()) : ZDateTime.Empty;

		protected override ZString HumanReadableNameCore
		{
			get
			{
				if (!String.IsNullOrEmpty(this.SQ_DisplayName))
				{
					return this.SQ_DisplayName;
				}
				else
				{
					return base.HumanReadableNameCore;
				}
			}
		}

		[CargoWise.ComponentModel.MaxLength(30)]
		public ZString IsXLSTemplateForPrintSettingsLoaded
		{
			get { return SQ_XLSTemplateForPrintSettings.IsEmpty ? Res.GetString("StmPrintQueue|IsXLSTemplateForPrintSettingsLoaded|Empty", "Printer driver settings are not loaded.") : Res.GetString("StmPrintQueue|IsXLSTemplateForPrintSettingsLoaded|Loaded", "Printer driver settings are loaded."); }
		}

		public ZPropertyInfo IsXLSTemplateForPrintSettingsLoadedInfo
		{
			get { return GetZPropertyInfo(nameof(IsXLSTemplateForPrintSettingsLoaded)); }
		}

		[ReadOnly(true)]
		public ZString SQ_ServerName
		{
			get
			{
				return PrintServer?.SPS_ServerName ?? ZString.Empty;
			}
			set
			{
				if (serverName != value)
				{
					serverName = value;
					printServer = GetPrintServerByName() ?? CreatePrintServer();
					SQ_SPS_Server = printServer.PK;
				}
			}
		}
		ZString serverName;

		[RelatedBusinessObject("PrintServer")]
		public override ZGuid SQ_SPS_Server
		{
			get => base.SQ_SPS_Server;
			set
			{
				base.SQ_SPS_Server = value;
				printServer = null;
			}
		}

		#region PrintServer

		[ActionFieldFollow(false)]
		public StmPrintServer PrintServer => printServer ?? (printServer = GetPrintServer());
		StmPrintServer printServer;

		StmPrintServer GetPrintServer()
		{
			return Factory.Load<StmPrintServer>(SQ_SPS_Server);
		}

		StmPrintServer GetPrintServerByName(bool reloadRow = false)
		{
			var query = new ZQuery(StmPrintServerSchema.SPS_ServerName, serverName);
			query.ReLoadExistingRows = reloadRow;
			return Factory.LoadTop1<StmPrintServer>(query);
		}

		StmPrintServer CreatePrintServer(int retries = 3)
		{
			SqlApplicationLock mutex;
			if (Db.Connection.TryGetLock("CreatingNewPrintServer" + serverName, out mutex))
			{
				using (mutex)
				{
					var result = GetPrintServerByName(true);

					if (result == null)
					{
						var pk = CreateAndSaveNewPrintServerOnNewFactory();
						result = Factory.Load<StmPrintServer>(pk);
					}

					return result;
				}
			}
			else if (retries > 0)
			{
				Thread.Sleep(100);
				return CreatePrintServer(retries - 1);
			}
			else
			{
				throw new InvalidOperationException(SqlLockExceptionMessage);
			}
		}

		string SqlLockExceptionMessage => (NoResString)"SQL Lock for new StmPrintServer could not be created";

		ZGuid CreateAndSaveNewPrintServerOnNewFactory(string defaultMessage = null)
		{
			var factory = new BusinessObjectFactory();
			var printServer = factory.New<StmPrintServer>();
			printServer.SPS_ServerName = serverName;

			using (FactorySaveAlerterOverride.TemporarilyOverride(printServer))
			{
				factory.Save();
			}

			return printServer.PK;
		}

		#endregion

		[ReadOnly(true)]
		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZString SQ_QueueName
		{
			get { return base.SQ_QueueName; }
			set { base.SQ_QueueName = value; }
		}

		#endregion
		#region HasDefaultValues

		public bool HasDefaultValues()
		{
			bool result = true;

			// We have to use the schema columns bacause because the BO properties do not have default values
			// They are available in the schema columns only
			foreach (SchemaColumn column in StmPrintQueueSchema.All)
			{
				object current = ((IBusinessObjectInternals)this).Row[column.Name];
				object @default = column.SqlDbDefault;

				if (column == StmPrintQueueSchema.SQ_DisplayName)
				{
					result = (current as string).Equals(GetDefaultDisplayName());
				}
				else if (!ColumnsToIgnore.Contains(column))
				{
					result = (current == @default) || (current.Equals(@default) && @default.Equals(current));

					if (!result)
					{
						// We need to recheck if it is a Blob column and it is BDNull by default
						if ((column.ColumnType == SchemaColumnType.Blob) &&
							(column.SqlDbDefault == DBNull.Value))
						{
							// because it could be presented as an empty array instead of DBNull
							result = (current as Array).Length == 0;
						}
					}
				}

				if (!result)
				{
					return result;
				}
			}
			return result;
		}

		protected ArrayList ColumnsToIgnore
		{
			get
			{
				if (fColumnsToIgnore == null)
				{
					fColumnsToIgnore = new ArrayList();
					fColumnsToIgnore.Add(StmPrintQueueSchema.PK);
					fColumnsToIgnore.Add(StmPrintQueueSchema.SQ_QueueName);
					fColumnsToIgnore.Add(StmPrintQueueSchema.SQ_SPS_Server);
					fColumnsToIgnore.Add(StmPrintQueueSchema.SQ_QueueDeleted);
					fColumnsToIgnore.Add(StmPrintQueueSchema.SQ_PrintQueueStateChanged);
					fColumnsToIgnore.Add(StmPrintQueueSchema.SQ_SystemLastEditUser);
					fColumnsToIgnore.Add(StmPrintQueueSchema.SQ_SystemCreateUser);
					fColumnsToIgnore.Add(StmPrintQueueSchema.SQ_SystemCreateTimeUtc);
					fColumnsToIgnore.Add(StmPrintQueueSchema.SQ_SystemLastEditTimeUtc);
				}
				return fColumnsToIgnore;
			}
		}

		protected ArrayList fColumnsToIgnore;

		#endregion

		#region GetDefaultDisplayName

		public string GetDefaultDisplayName()
		{
			string fResult = SQ_QueueName;
			if (fResult.IndexOf("\\") > -1)
			{
				fResult = fResult.Substring(fResult.LastIndexOf("\\") + 1);
			}
			return fResult;
		}

		#endregion

		public SerialisablePrintQueue GetSerialisablePrintQueue()
		{
			SerialisablePrintQueue queue = new SerialisablePrintQueue();
			queue.Name = SQ_QueueName;
			queue.DisplayName = SQ_DisplayName;
			queue.ColumnScale = SQ_ColumnScale;
			queue.RowScale = SQ_RowScale;
			queue.Scale = SQ_Scale;
			queue.LeftMargin = SQ_LeftMargin;
			queue.TopMargin = SQ_TopMargin;
			queue.PrintLanguage = SQ_PrintLanguage;
			queue.QueuePk = PK.ToGuid();
			queue.StateChangedStamp = SQ_PrintQueueStateChanged.IsEmpty ? Guid.Empty : SQ_PrintQueueStateChanged.ToGuid();
			queue.SuppressLetterhead = SQ_SupressLetterhead;
			queue.XlsTemplate = SQ_XLSTemplateForPrintSettings;
			return queue;
		}

		#region IStmPrintQueue Members

		ZGuid Enterprise.Integration.DocumentEngine.IStmPrintQueue.PK
		{
			get { return PK; }
		}

		ZString Enterprise.Integration.DocumentEngine.IStmPrintQueue.QueueName
		{
			get { return SQ_QueueName; }
			set { SQ_QueueName = value; }
		}

		ZString Enterprise.Integration.DocumentEngine.IStmPrintQueue.SQ_ServerName
		{
			get { return SQ_ServerName; }
			set { SQ_ServerName = value; }
		}

		ZGuid Enterprise.Integration.DocumentEngine.IStmPrintQueue.SQ_SPS_Server
		{
			get { return SQ_SPS_Server; }
			set { SQ_SPS_Server = value; }
		}

		#endregion
	}
}
