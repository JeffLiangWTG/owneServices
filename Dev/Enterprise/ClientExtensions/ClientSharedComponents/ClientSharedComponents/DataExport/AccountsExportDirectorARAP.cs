
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.ClientSharedComponents
{
	/// <summary>
	/// Data Export of All AR and AP Accounting transactions into TWO data files.
	/// </summary>
	/// <remarks>Call from both GUI and Batch Processor classes. When data exporting all AR & AP transactions into separate files, inherit from here.</remarks>
	public abstract class AccountsExportDirectorARAP : AccountsExportDirector
	{
		public AccountsExportDirectorARAP(BusinessObjectFactory factory, INotifications notificationSubscriber)
			: base(factory, notificationSubscriber)
		{
			IsARMode = true;
		}

		protected override void OnBeforeOuterExecute()
		{
			base.OnBeforeOuterExecute();
			SetPhase(Phase.InitialAccountsReceivable);
			existingBatchNumberToExport = enableManualMode ? Exporter.FilterProvider.CurrentBatchNo : ZInt.Zero;
			ARExporter = null;  // Hard reset - required.
			APExporter = null;  // Hard reset - required.
		}

		/// <remarks>This is the primary execution point where the base is divied into 2 separate runs - 1 for AR and 1 for AP.
		/// It then reverts back to AR to accumulate AP counts so the outcome of the entire batch is determined and reported on.</remarks>
		protected override void DoOuterExecute(string tempFileName)
		{
			base.DoOuterExecute(tempFileName);

			SetPhase(Phase.AccountsPayable);
			base.DoOuterExecute(Env.GetTempFileName(Env.TempPath));

			SetPhase(Phase.BatchCompletion);
			ARExporter.AccumulateCounters(APExporter);
		}

		protected override bool AllInnerTransactionsExported
		{
			get { return ((AccountsExporterARAP)Exporter).AllTransactionsExported; }
		}

		protected override string DataTypeName
		{
			get { return base.DataTypeName + (IsARMode ? "Recievable " : "Payable "); }
		}

		public override AccountsExporter Exporter
		{
			get { return IsARMode ? ARExporter : APExporter; }
		}

		protected AccountsExporterARAP ARExporter
		{
			get { return arExporter ?? (arExporter = NewARExporter(existingBatchNumberToExport, factory)); }
			set { arExporter = value; }
		}
		AccountsExporterARAP arExporter;

		protected AccountsExporterARAP APExporter
		{
			get { return apExporter ?? (apExporter = NewAPExporter(existingBatchNumberToExport, factory)); }
			set { apExporter = value; }
		}
		AccountsExporterARAP apExporter;

		/// <remarks>Is used to return either the ARExporter or the APExporter classes depending on the legder type.</remarks>
		void SetPhase(Phase phase)
		{
			switch (phase)
			{
				case Phase.InitialAccountsReceivable:
					IsARMode = true;
					break;

				case Phase.AccountsPayable:
					IsARMode = false;
					break;

				case Phase.BatchCompletion:
					IsARMode = true;
					break;
			}
		}

		protected bool IsARMode
		{
			get { return isARMode; }
			private set { isARMode = value; }
		}
		bool isARMode;

		enum Phase
		{
			InitialAccountsReceivable,
			AccountsPayable,
			BatchCompletion
		}

		protected abstract AccountsExporterARAP NewARExporter(int batchNumber, BusinessObjectFactory factory);
		protected abstract AccountsExporterARAP NewAPExporter(int batchNumber, BusinessObjectFactory factory);
	}
}
