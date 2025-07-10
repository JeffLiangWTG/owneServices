using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.SWL.Business
{
	public abstract class ShipnetProcessor : INotifications
	{
		public void Process()
		{
			if (CanContinue && IsEnvironmentValid())
			{
				ProcessShipnetData();
			}
		}

		public event ShipnetProcessProgressEventHandler OnProgress;

		#region Implementation

		protected virtual bool IsEnvironmentValid()
		{
			bool result = (StartDateTimeUTC < EndDateTimeUTC);
			if (ShipnetCarriers.Count == 0)
			{
				Notify(new ErrorNotification(ErrorType.Error, "No Shipnet Carriers found"));
				result = false;
			}
			if (string.IsNullOrEmpty(SWLDataRegistry.Instance.ShipnetBackupDirectoryItem.Value))
			{
				Notify(new ErrorNotification(ErrorType.Error, "No Shipnet Backup Directory has been setup"));
				result = false;
			}
			return result;
		}

		void ProcessShipnetData()
		{
			if (Globals.IsTest)
			{
				InternalLogsLastExecuteTest.RemoveAll();
			}

			Notify(new InfoNotification(string.Format(CultureInfo.CurrentCulture, "Processing {0} company...", GlbCompany.CurrentCompany.GC_Code)));

			ZDateTime endDateTimeUTC = EndDateTimeUTC;
			Notify(new InfoNotification(string.Format(CultureInfo.CurrentCulture, "Current Batch Interval UTC: '{0}' - '{1}'", StartDateTimeUTC, endDateTimeUTC)));
			FilteredBusinessObjectReader reader = new FilteredBusinessObjectReader(FactoryProvider, CreateFilter(StartDateTimeUTC, endDateTimeUTC), typeof(StmALog));
			reader.BatchSize = 300;
			int count = 0;
			ShipnetCarrierObjectCollection shipnetCarriers = new ShipnetCarrierObjectCollection(this);
			int totalNumberOfLogs = reader.ApproximateCount;
			if (!CanContinue)
			{
				return;
			}
			foreach (StmALog log in reader)
			{
				if (!CanContinue)
				{
					return;
				}
				count++;
				ShowProgress(count, totalNumberOfLogs, log);

				if (Globals.IsTest)
				{
					InternalLogsLastExecuteTest.Add(log);
				}

				ShipnetARInvoice header = log.Factory.Load<ShipnetARInvoice>(log.SL_Parent);

				if (header != null && header.IsValidShipnetType && IsOneOfShipnetCarrier(header.Carrier))
				{
					shipnetCarriers.Add(header);
				}
			}

			shipnetCarriers.Export();
			StartDateTimeUTC = endDateTimeUTC;
		}

		static ZQuery CreateFilter(ZDateTime startDateTimeUTC, ZDateTime endDateTimeUTC)
		{
			ZQuery result = new ZQuery(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThan, startDateTimeUTC);
			result.AddToFilter(JoinCondition.And, StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, endDateTimeUTC);
			result.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.AddedARecordToTheSystem.Code);
			result.AddToFilter(StmALogSchema.SL_Table, AccTransactionHeaderSchema.Constants.TableName);

			const string AH_LedgerName = "@AH_Ledger";
			const string InvoiceTypeName = "@InvoiceTypeName";
			const string CreditNoteTypeName = "@CreditNoteTypeName";
			const string GB_GCName = "@GB_GC";

			result.AddFilterAndZSQLParameterCollection(GetShipnetARInvoiceFilter(AH_LedgerName, InvoiceTypeName, CreditNoteTypeName, GB_GCName), GetShipnetARInvoiceFilterParameters(AH_LedgerName, InvoiceTypeName, CreditNoteTypeName, GB_GCName));
			result.OrderBy = StmALogSchema.SL_PostedTimeUtc.Name;
			result.IsNoLock = false;
			return result;
		}

		static string GetShipnetARInvoiceFilter(string aH_LedgerName, string invoiceTypeName, string creditNoteTypeName, string gB_GCName)
		{
			return StmALogSchema.SL_Parent.Name + " in (select " + AccTransactionHeaderSchema.PK.Name
				+ " from " + AccTransactionHeaderSchema.Constants.SqlSchemaName + "." + AccTransactionHeaderSchema.Constants.TableName + " "
				+ " where " + AccTransactionHeaderSchema.AH_Ledger.Name + " = " + aH_LedgerName
				+ " and (" + AccTransactionHeaderSchema.AH_TransactionType.Name + " = " + invoiceTypeName
				+ " or " + AccTransactionHeaderSchema.AH_TransactionType.Name + " = " + creditNoteTypeName
				+ " ) and " + AccTransactionHeaderSchema.AH_GB.Name
				+ " in (select " + GlbBranchSchema.PK.Name + " from " + GlbBranchSchema.Constants.SqlSchemaName + "." + GlbBranchSchema.Constants.TableName + " "
				+ " where " + GlbBranchSchema.GB_GC.Name + " = " + gB_GCName + "))";
		}

		static ZSqlParameterCollection GetShipnetARInvoiceFilterParameters(string aH_LedgerName, string invoiceTypeName, string creditNoteTypeName, string gB_GCName)
		{
			ZSqlParameterCollection result = new ZSqlParameterCollection();
			result.Add(aH_LedgerName, ZArchitecture.Core.LedgerTypes.AccountsReceivable, AccTransactionHeaderSchema.AH_Ledger);
			result.Add(invoiceTypeName, ZArchitecture.Core.TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_TransactionType);
			result.Add(creditNoteTypeName, ZArchitecture.Core.TransactionTypes.CreditNote, AccTransactionHeaderSchema.AH_TransactionType);
			result.Add(gB_GCName, GlbCompany.CurrentCompany.PK, GlbBranchSchema.GB_GC);
			return result;
		}

		protected abstract ZDateTime StartDateTimeUTC { get; set; }

		protected abstract ZDateTime EndDateTimeUTC { get; }

		protected BusinessObjectFactoryProvider FactoryProvider
		{
			get
			{
				if (fFactoryProvider == null)
				{
					BusinessObjectFactory initialFactory = new BusinessObjectFactory();
					initialFactory.RefreshEnabled = false;
					fFactoryProvider = new BusinessObjectFactoryProvider(initialFactory);
				}
				return fFactoryProvider;
			}
		}

		BusinessObjectFactoryProvider fFactoryProvider;

		protected bool IsOneOfShipnetCarrier(OrgHeader carrier)
		{
			bool result = false;
			if (carrier != null)
			{
				result = ShipnetCarriers.Contains(carrier.PK);
			}
			return result;
		}

		protected abstract OrgHeaderCollection ShipnetCarriers
		{
			get;
		}

		internal StmALogCollection InternalLogsLastExecuteTest
		{
			get
			{
				if (fInternalLogsLastExecuteTest == null)
				{
					fInternalLogsLastExecuteTest = new StmALogCollection(new BusinessObjectFactory());
				}
				return fInternalLogsLastExecuteTest;
			}
		}
		StmALogCollection fInternalLogsLastExecuteTest;

		#region INotifications Members

		void INotifications.Add(INotification notification)
		{
			Notify(notification);
		}

		protected abstract void Notify(INotification notification);

		#endregion

		void ShowProgress(int count, int totalNumberOfLogs, StmALog log)
		{
			if (OnProgress != null)
			{
				OnProgress(this, new ShipnetProcessProgressChangedEventArgs(count, totalNumberOfLogs, log));
			}
		}

		public bool CanContinue { get; set; }

		#endregion
	}
}
