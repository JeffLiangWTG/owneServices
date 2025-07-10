using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Common.Import;
using Enterprise.Integration;
using Enterprise.ZArchitecture.GUI;

using IStreamProvider = Enterprise.DataTransfer.Common.Import.IStreamProvider;

namespace Enterprise.DataTransfer.Common.GUI.Import
{
	public partial class DataImportForm<T> : ZChildForm where T : class
	{
		#region Constructor

		public DataImportForm()
			: base(new ImportStatus())
		{
			InitializeComponent();
			Init();
		}

		void Init()
		{
			MinimumSize = Size;
			progressTextBox.ReadOnly = true;
		}

		#endregion

		#region Dependency

		public ILogger Logger
		{
			get { return logger ?? (logger = new ProgressTextBoxLogger(progressTextBox)); }
		}
		ILogger logger;

#if DEBUG
		public void SetLoggerForTesting(ILogger logger)
		{
			this.logger = logger;
		}

		public WhiteTextBox ProgressTextBox { get { return progressTextBox; } }
#endif

		public DataImportService<T> ImportService
		{
			get { return importService; }
			set
			{
				// Logic in setting?
				// Basically, it is a bad practice
				importService = value;
				importService.BeforeProcess += OnBeforeProcess;
				importService.BeforeUnitProcess += OnBeforeUnitProcess;
				importService.AfterUnitProcess += OnAfterUnitProcess;
				importService.AfterProcess += OnAfterProcess;
				importService.UnitProcessSuccess += OnUnitProcessSuccess;
				importService.ErrorOccur += OnErrorOccured;
			}
		}
		DataImportService<T> importService;

		public IStreamProvider StreamProvider { get; set; }

		#endregion

		#region View Model
		// It would be great if ZForm is Generic
		public new ImportStatus BusinessEntity
		{
			get { return (ImportStatus)base.BusinessEntity; }
		}

		#endregion

		#region Import Button

		void Import_Click(object sender, EventArgs e)
		{
			progressTextBox.ResetText();
			importButton.Enabled = false;
			stopButton.Enabled = true;
			BusinessEntity.Reset();

			ImportCore();

			importButton.Enabled = true;
			stopButton.Enabled = false;
		}

		void ImportCore()
		{
			var stream = StreamProvider.Stream();
			if (stream == null) { return; }

			using (stream)
			{
				ImportService.Import(stream);
			}
		}

		#endregion

		#region Close/Cancel Button

		protected override void OnClosing(CancelEventArgs e)
		{
			importService.Cancel();

			base.OnClosing(e);
			if (!closeButton.Enabled)
			{
				e.Cancel = true;
			}
			importService.Cancel();
		}

		void StopButton_Click(object sender, EventArgs e)
		{
			importButton.Enabled = true;
			importService.Cancel();
			stopButton.Enabled = false;
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#region Event Handler for Import Service
		void OnBeforeProcess()
		{
			Logger.Information(Res.GetString("0b151339-3979-4a15-ac61-f88b8886cb88", "Start Import Process"));
			Logger.Information("-----------------------------------------------------------------");
		}

		void OnBeforeUnitProcess(T source)
		{
		}

		void OnAfterUnitProcess(T source)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void OnUnitProcessSuccess(T source)
		{
			if (BusinessEntity != null)
			{
				BusinessEntity.ImportedRecords++;
				Application.DoEvents();
			}
		}

		void OnAfterProcess()
		{
			Logger.Information(Res.GetString("ea8d59d3-a4fe-4ceb-89b8-58bd28cfdd2a", "Import Process Finished"));
			Logger.Information("-----------------------------------------------------------------");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void OnErrorOccured(T source, Exception ex)
		{
			var sourceString = string.Empty;
			var xelement = source as XElement;
			if (xelement != null)
			{
				sourceString = PartialXmlHelper.GeneratePartialXML(xelement);
			}
			else if (source != null)
			{
				sourceString = source.ToString();
				if (sourceString.Length > 1000)
				{
					sourceString = sourceString.Substring(0, 1000) + "...";
				}
			}

			Logger.Error(Res.GetString("b7bc1c4d-5f11-4d96-bb96-0c8bd8ebff91", "Record:") + " ");
			Logger.Error(sourceString);
			Logger.Error(Res.GetString("c1563dbe-b93a-49cb-a891-7d8a6f4fbf3f", "failed to Import:"));

			if (ex is ZDataException dataEx)
			{
				string friendlyMessage = String.Empty;
				try
				{
					friendlyMessage = dataEx.FriendlyMessage;
				}
				catch (Exception errorMessageGenerationException) when (!errorMessageGenerationException.IsCriticalException())
				{
				}

				if (!string.IsNullOrEmpty(friendlyMessage))
				{
					Logger.Error(friendlyMessage);
				}
				else if (ex is ZDataConcurrencyException concurrencyEx)
				{
					Logger.Error(Res.GetString("7486c98b-a4e4-46b7-9f99-ec863492d196", "Concurrency error occurred. Another user has changed a row in the database that you are trying to update. You might like to check their changes before trying to import again.\r\nThe table name is '{0}'.\r\nThe row has PK '{1}'.", concurrencyEx.Row.Table.TableName, ZDataUtils.GetPK(concurrencyEx.Row)));
				}
				else
				{
					SubmitExceptionToWTG(ex);
				}
			}
			else if (ExceptionVisibilityAttribute.GetFirstOccurenceOfUserException(ex) != null)
			{
				Logger.Error(ex.Message);
			}
			else if (ex is XmlException)
			{
				Logger.Error(Res.GetString("4a433060-541a-4fad-95f2-4dae42ea0f5c", "Invalid XML format. Please check your file to solve this problem.\r\n{0}", ex.Message));
			}
			else
			{
				SubmitExceptionToWTG(ex);
			}

			Logger.Information("-----------------------------------------------------------------");

			if (BusinessEntity != null)
			{
				BusinessEntity.ErrorRecords++;
				Application.DoEvents();
			}
		}

		void SubmitExceptionToWTG(Exception ex)
		{
			Logger.Error(Res.GetString("6f9fc8ae-cc29-4d0a-8698-973d0627657a", "{0}\r\nThis error has been submitted to WTG for further investigation.", ex.Message), ex);
			ErrorReporter.ReportOnce("Unknown exception occurred while importing Native XML, If you are a developer looking at the issue (yes you!!) please handle the exception.\r\n\r\nIf this exception occured because of a problem with the incoming XML, please wrap this exception in an exception type that has the ExceptionVisibility.User attribute on it. (eg: NativeXMLUserVisibleException) That will cause the exception to be reported to the User instead of being reported to WTG as an Issue. Make sure that you provide a clear message for the new exception that a User can follow to understand and fix the processing error that has occurred.", ex);
		}
		#endregion

		#region ZForm Overrides

		public override string FormHeading
		{
			get { return Res.GetString("1d734867-99af-4e6c-8082-202c31b05134", "Import Form"); }
		}

		#endregion
	}
}
