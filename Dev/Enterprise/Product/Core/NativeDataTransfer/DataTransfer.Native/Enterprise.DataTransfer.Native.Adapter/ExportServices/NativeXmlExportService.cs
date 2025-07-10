using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DataTransfer.Native.Adapter
{
	public class NativeXmlExportService : IExportService
	{
		public IBusinessSerializer Serializer { get; set; }
		public IExportValidator Validator { get; set; }
		public ISaveFileLocator FileLocator
		{
			get
			{
				if (fileLocator == null)
				{
					fileLocator = new PromptDialogFileLocator();
				}
				return fileLocator;
			}
			set { fileLocator = value; }
		}
		ISaveFileLocator fileLocator;

		public IUserNotification Logger
		{
			get
			{
				if (logger == null)
				{
					logger = Globals.Message;
				}
				return logger;
			}
			set { logger = value; }
		}
		IUserNotification logger;

		#region IExportService Members

		public void Export(IEnumerable<IBusiness> businessObjects)
		{
			Export(businessObjects, false, null);
		}

		public void ExportWithSave(IEnumerable<IBusiness> businessObjects)
		{
			Export(businessObjects, true, null);
		}

		public void ExportWithSave(IEnumerable<IBusiness> businessObjects, Func<DataTable, DataRow> filterOnMultiRowResult)
		{
			Export(businessObjects, true, filterOnMultiRowResult);
		}

		public void Export(IEnumerable<IBusiness> businessObjects, Stream targetStream)
		{
			Export(businessObjects, targetStream, null, null);
		}

		public void Export(IEnumerable<IBusiness> businessObjects, Stream targetStream, BusinessObjectFactory factory)
		{
			Export(businessObjects, targetStream, factory, null);
		}

		void Export(IEnumerable<IBusiness> businessObjects, Stream targetStream, BusinessObjectFactory factory, Func<DataTable, DataRow> filterOnMultiRowResult)
		{
			using (var sourceStream = Serializer.Export(businessObjects, factory, filterOnMultiRowResult))
			{
				sourceStream.Position = 0;
				sourceStream.CopyTo(targetStream);
			}
		}

		public bool CanBeExported(Type type)
		{
			return Validator.CanBeExported(type);
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log information")]
		void Export(IEnumerable<IBusiness> businessObjects, bool saveFactories, Func<DataTable, DataRow> filterOnMultiRowResult)
		{
			try
			{
				Validator.Validate(businessObjects);

				string displayFileName;
				var fileStream = FileLocator.GetFileStream(businessObjects, out displayFileName);
				if (fileStream == null)
				{
					return;
				}

				exportProgressForm = new ProgressForm();
				ExportAndReportProgress(businessObjects, fileStream, filterOnMultiRowResult);

				Logger.ShowInformation(string.Format(CultureInfo.InvariantCulture, "File saved at {0}", displayFileName));
				AddStmLogs(businessObjects);

				if (saveFactories)
				{
					bool exceptionOccuredDuringSavingAFactory = false;

					foreach (var factory in businessObjects.Select(bo => bo.Factory).Distinct())
					{
						try
						{
							factory.Save();
						}
						catch (ZSaveException)
						{
							exceptionOccuredDuringSavingAFactory = true;
						}
					}
					if (exceptionOccuredDuringSavingAFactory)
					{
						Logger.ShowError(string.Format(CultureInfo.InvariantCulture, "There was a problem saving the logs for this data export. This means that workflow automation configured on entities exported may not have fired."));
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Logger.ShowError(ex.Message);
			}
			finally
			{
				Serializer.ProgressChanged -= OnProgressChanged;
				exportProgressForm?.Close();
				exportProgressForm?.Dispose();
				exportProgressForm = null;
			}
		}

		internal void AddStmLogs(IEnumerable<IBusiness> businessObjects)
		{
			foreach (BusinessObject bizObj in businessObjects)
			{
				AddStmLog(bizObj);
			}
		}

		internal void AddStmLog(BusinessObject businessObject)
		{
			businessObject.GetLogs().AddNew(Events.DataExport);
		}

		#region Exporting Progress

		void ExportAndReportProgress(IEnumerable<IBusiness> businessObjects, Stream fileStream, Func<DataTable, DataRow> filterOnMultiRowResult)
		{
			using (fileStream)
			{
				InitializeExporting(businessObjects);

				Export(businessObjects, fileStream, null, filterOnMultiRowResult);

				FinishExporting();
			}
		}

		void OnProgressChanged(object sender, EventArgs eventArgs)
		{
			var msg = Res.GetString("113e0dcb-c15a-4fd2-93e6-93e6b5466659", "Exporting Native XML...{0}/{1}", processedCount, totalCount);
			exportProgressForm.SetStatusAndPercentComplete(msg, (int)(processedCount / (double)totalCount * 100));
			processedCount++;
		}

		void InitializeExporting(IEnumerable<IBusiness> businessObjects)
		{
			processedCount = 0;
			totalCount = businessObjects.Count();
			Serializer.ProgressChanged += OnProgressChanged;

			exportProgressForm.ShowCancelButton = false;
			exportProgressForm.Show(exportProgressForm.ParentForm);
			exportProgressForm.Update();
		}

		void FinishExporting()
		{
			exportProgressForm.SetStatusAndPercentComplete(Res.GetString("163a6394-828c-3fd1-a520-9dad59f5c194", "Exporting finished."), 100);
		}

		ProgressForm exportProgressForm;
		int processedCount;
		int totalCount;

		#endregion
	}
}
