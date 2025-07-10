using System;
using System.Data;
using System.IO;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.DataTransfer.DataInterface
{
	public class ChinaStandardExporter
	{
		public ChinaStandardExporter(ChinaStandardWrapper bizObj, NotificationBuffer notification)
		{
			this.BizObj = bizObj;
			this.Notification = notification;
		}

		public ChinaStandardExporter(ChinaStandardWrapper bizObj, NotificationBuffer notification, bool isAutomaticExport)
			: this(bizObj, notification)
		{
			IsAutomaticExport = isAutomaticExport;
		}

		public bool ExportData(IValueObjectDataAdapter dataAdapter)
		{
			bool isExportSomething;
			try
			{
				if (!IsAutomaticExport)
				{
					string message = Res.GetString("5a7de9c6-f9ce-414d-8824-9919d250af47",
												   "Export in progress - please be patient, this may take some time.{0}",
												   System.Environment.NewLine);
					Notification.Notify(new InfoNotification(message));
				}

				DataTable table = DataUtils.GetDataTableFromQuery(Db.Connection,
																  string.Format(
																	@"select * from dbo.GLAccountsMissingLanguageMapping('{0}','ZH-CN','CN')", // May be a some code abbreviature.
																	GlbCompany.CurrentCompany.PK));

				table.PrimaryKey = new[] { table.Columns["GLAccount"] };
				if (table.Rows.Count > 0)
				{
					Notification.Notify(new InfoNotification(Res.GetString("DBBB90FC-7D18-4DFD-8BE9-12C16D3F1B8C",
																		   "THE XML CANNOT BE PROPERLY GENERATED AS THERE ARE SOME PARENT GL ACCOUNTS OF WHICH TRANSACTIONS HAVE BEEN POSTED TO BUT DO NOT HAVE CORRESPONDING CN ZH-CN MAPPING ACCOUNTS.\r\nPLEASE CREATE OR MAP THESE PARENT GL ACOUNTS BEFORE EXPORTING THE XML.\r\n---")));
					foreach (DataRow dr in table.Rows)
					{
						Notification.Notify(
							new InfoNotification(dr["GLAccount"] + "  -  " + dr["GLDescription"] + System.Environment.NewLine));
					}

					return false;
				}

				TempFile = Env.GetTempFileName(BizObj.ExportDirectory);
				Document = File.Open(TempFile, FileMode.Open, FileAccess.ReadWrite);
				isExportSomething = ExportDataCore(dataAdapter);
			}
			catch (System.Data.Common.DbException ex)
			{
				isExportSomething = false;
				Globals.Message.ShowDeveloperException(ex);
				Notification.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("528e22c9-9db3-42a3-8b21-03ffb7fd533a", "A problem happened while getting data from the database.") + ex.Message));
			}
			catch (UnauthorizedAccessException ex)
			{
				isExportSomething = false;
				AddIOError(ex);
			}
			catch (IOException ex)
			{
				isExportSomething = false;
				AddIOError(ex);
			}
			finally
			{
				if (Document != null)
				{
					Document.Close();
				}

				BizObj.Factory.SubscribeForDispose(new DisposableAction(() => AccountingUtils.DeleteFileSafe(TempFile)));
			}
			return isExportSomething;
		}

		void AddIOError(Exception ex)
		{
			Notification.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("4646646d-dd9c-4aaa-9f3d-fc4dcb614674", "A problem happened while writing the export data into the file. More info: {0}", ex.Message)));
		}

		#region Implementation

		protected ChinaStandardWrapper BizObj;
		protected NotificationBuffer Notification;
		protected bool IsAutomaticExport;
		protected XmlTextWriter XmlDocumentWriter;
		protected Stream Document;
		protected string TempFile;

		#region ExportDataCore

		protected virtual bool ExportDataCore(IValueObjectDataAdapter dataAdapter)
		{
			return false;
		}

		protected virtual void AfterDataExport(bool isExportSomething)
		{
			if (isExportSomething)
			{
				using (var sourceStream = File.OpenRead(TempFile))
				using (var targetStream = ObjectFactory.Get<IFileMapper>().OpenWrite(FileName))
				{
					sourceStream.CopyTo(targetStream);
				}
				Notification.Notify(
					new InfoNotification(Res.GetString("6fca4968-9b52-4213-bc12-e16aabd98966",
													   "Data exported successfully to the file '{0}'.", FileName)));
			}
			else
			{
				Notification.Notify(
					new InfoNotification(Res.GetString("aadd18fa-54a0-4b62-9259-98c8d82e95c3", "There are no data to export.")));
			}
		}

		protected virtual ZString FileName { get; set; }

		protected ZDateTime FromDate
		{
			get { return BizObj.Period.IsValid && BizObj.Period > 0 ? (new AccountingPeriodCalculator(Factory)).GetFirstDayForPeriod(BizObj.Period) : ZDateTime.Today; }
		}

		protected ZDateTime ToDate
		{
			get { return BizObj.Period.IsValid && BizObj.Period > 0 ? (new AccountingPeriodCalculator(Factory)).GetLastDayForPeriod(BizObj.Period).Date.AddDays(1) : ZDateTime.Today; }
		}

		#endregion

		#region Factory

		protected BusinessObjectFactory Factory
		{
			get { return fFactory ?? (fFactory = new BusinessObjectFactory()); }
		}

		BusinessObjectFactory fFactory;

		#endregion

		#endregion

	}
}
