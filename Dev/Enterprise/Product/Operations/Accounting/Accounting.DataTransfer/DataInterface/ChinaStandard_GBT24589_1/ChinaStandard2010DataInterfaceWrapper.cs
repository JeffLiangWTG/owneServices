using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1;
using Enterprise.DataTransfer.Integration;

namespace Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT24589_1
{
	public class ChinaStandard2010DataInterfaceWrapper : ChinaStandardWrapper
	{
		public ChinaStandard2010DataInterfaceWrapper()
		{ }

		public ChinaStandard2010DataInterfaceWrapper(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (ExportFiles.Select(valueObject => Exporter != null && Exporter.ExportData(valueObject)).Any(dataExportSucceeded => !dataExportSucceeded))
			{
				throw new ZCannotSaveException("Export failed.", "Export files failed!\r\n文件输出失败!");
			}
			HasChanges = false;
		}

		#region Public Properties

		#region Period

		[ReadOnlyMember(nameof(Period_ReadOnly))]
		public override ZInt Period
		{
			get { return base.Period; }
			set { base.Period = value; }
		}

		public ZBool Period_ReadOnly
		{
			get { return PeriodReadOnly; }
		}

		#endregion

		#region Export files

#if DEBUG

		public
#endif
 ZBool PeriodReadOnly
		{
			get
			{
				return ExportFiles.Count == 1 && ExportFiles[0].GetType() == typeof(ReferenceFilesDataAdapter);
			}
		}

		public ZBool ExportARAP
		{
			get { return fExportARAP; }
			set
			{
				SetNonPersistentPropertyValue(ExportARAPInfo, ref fExportARAP, value);
				if (fExportARAP)
				{
					if (!ExportFiles.Any(valueObject => valueObject.GetType() == typeof(ARAPDataAdapter)))
					{
						ExportFiles.Add(new ARAPDataAdapter());
					}
				}
				else
				{
					foreach (var valueObject in ExportFiles.Where(valueObject => valueObject.GetType() == typeof(ARAPDataAdapter)))
					{
						ExportFiles.Remove(valueObject);
						break;
					}
				}
				if (!IsValidationSuspended)
				{
					ValidatExportFiles();
				}
			}
		}

		public ZPropertyInfo ExportARAPInfo
		{
			get { return GetZPropertyInfo(nameof(ExportARAP)); }
		}

		public ZBool ExportReferenceFiles
		{
			get { return fExportReferenceFiles; }
			set
			{
				SetNonPersistentPropertyValue(ExportReferenceFilesInfo, ref fExportReferenceFiles, value);
				if (fExportReferenceFiles)
				{
					if (!ExportFiles.Any(valueObject => valueObject.GetType() == typeof(ReferenceFilesDataAdapter)))
					{
						ExportFiles.Add(new ReferenceFilesDataAdapter());
					}
				}
				else
				{
					foreach (var valueObject in ExportFiles.Where(valueObject => valueObject.GetType() == typeof(ReferenceFilesDataAdapter)))
					{
						ExportFiles.Remove(valueObject);
						break;
					}
				}
				if (!IsValidationSuspended)
				{
					ValidatExportFiles();
				}
			}
		}

		public ZPropertyInfo ExportReferenceFilesInfo
		{
			get { return GetZPropertyInfo(nameof(ExportReferenceFiles)); }
		}

		public ZBool ExportGeneralLedger
		{
			get { return fExportGeneralLedger; }
			set
			{
				SetNonPersistentPropertyValue(ExportGeneralLedgerInfo, ref fExportGeneralLedger, value);
				if (fExportGeneralLedger)
				{
					if (!ExportFiles.Any(valueObject => valueObject.GetType() == typeof(GeneralLedgerDataAdapter)))
					{
						ExportFiles.Add(new GeneralLedgerDataAdapter());
					}
				}
				else
				{
					foreach (var valueObject in ExportFiles.Where(valueObject => valueObject.GetType() == typeof(GeneralLedgerDataAdapter)))
					{
						ExportFiles.Remove(valueObject);
						break;
					}
				}

				if (!IsValidationSuspended)
				{
					ValidatExportFiles();
				}
			}
		}

		public ZPropertyInfo ExportGeneralLedgerInfo
		{
			get { return GetZPropertyInfo(nameof(ExportGeneralLedger)); }
		}

		public ZBool ExportFixedAssets
		{
			get { return fExportFixedAssets; }
			set
			{
				SetNonPersistentPropertyValue(ExportFixedAssetsInfo, ref fExportFixedAssets, value);
				if (fExportFixedAssets)
				{
					if (!ExportFiles.Any(valueObject => valueObject.GetType() == typeof(FixedAssetsDataAdapter)))
					{
						ExportFiles.Add(new FixedAssetsDataAdapter());
					}
				}
				else
				{
					foreach (var valueObject in ExportFiles.Where(valueObject => valueObject.GetType() == typeof(FixedAssetsDataAdapter)))
					{
						ExportFiles.Remove(valueObject);
						break;
					}
				}

				if (!IsValidationSuspended)
				{
					ValidatExportFiles();
				}
			}
		}

		public ZPropertyInfo ExportFixedAssetsInfo
		{
			get { return GetZPropertyInfo(nameof(ExportFixedAssets)); }
		}

		public ZBool ExportPayrolls
		{
			get { return fExportPayrolls; }
			set
			{
				SetNonPersistentPropertyValue(ExportPayrollsInfo, ref fExportPayrolls, value);
				if (fExportPayrolls)
				{
					if (!ExportFiles.Any(valueObject => valueObject.GetType() == typeof(PayrollsDataAdapter)))
					{
						ExportFiles.Add(new PayrollsDataAdapter());
					}
				}
				else
				{
					foreach (var valueObject in ExportFiles.Where(valueObject => valueObject.GetType() == typeof(PayrollsDataAdapter)))
					{
						ExportFiles.Remove(valueObject);
						break;
					}
				}

				if (!IsValidationSuspended)
				{
					ValidatExportFiles();
				}
			}
		}

		public ZPropertyInfo ExportPayrollsInfo
		{
			get { return GetZPropertyInfo(nameof(ExportPayrolls)); }
		}

		#endregion

		ZBool fExportARAP;
		ZBool fExportReferenceFiles;
		ZBool fExportGeneralLedger;
		ZBool fExportFixedAssets;
		ZBool fExportPayrolls;

#if DEBUG

		public
#endif
 List<IValueObjectDataAdapter> ExportFiles = new List<IValueObjectDataAdapter>();

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			ValidateFinanceYear();
			ValidateExportDirectory();
			ValidatExportFiles();
		}

		protected void ValidatExportFiles()
		{
			ExportReferenceFilesInfo.ClearAllNotifications();
			ExportPayrollsInfo.ClearAllNotifications();
			ExportFixedAssetsInfo.ClearAllNotifications();
			ExportGeneralLedgerInfo.ClearAllNotifications();
			ExportARAPInfo.ClearAllNotifications();
			if (ExportFiles.Count == 0)
			{
				string errorMessage = (Res.GetString("64CE0B38-9E2C-44B1-B469-DFF2E10AEC1D", "At least one of the 'Export Files' should have checked."));
				ExportReferenceFilesInfo.AddError(errorMessage);
				ExportPayrollsInfo.AddError(errorMessage);
				ExportFixedAssetsInfo.AddError(errorMessage);
				ExportGeneralLedgerInfo.AddError(errorMessage);
				ExportARAPInfo.AddError(errorMessage);
			}
			ValidatePeriod();
		}

		protected override void ValidatePeriod()
		{
			PeriodInfo.ClearAllNotifications();

			if (!PeriodReadOnly)
			{
				if (Period.IsEmpty || Period == 0)
				{
					string errorMessage = Res.GetString("0C3BF8A7-5072-48B4-B926-33E888551010", "The 'Period' should have data.");
					PeriodInfo.AddError(errorMessage);
				}
				else
					if (ExportGeneralLedger && (!PeriodCalculator.IsPeriodGLClosed(Period) || !PeriodCalculator.IsPeriodSubLedgerClosed(Period)))
					{
						string errorMessage = Res.GetString("02FFA54D-6304-4178-B262-FF5F3CE336DF", "This Period '{0}' is not closed. Please close both sub ledger and general ledger for this period before running the export function.", Period);
						PeriodInfo.AddError(errorMessage);
					}
			}
		}

		protected override void ValidateBranch()
		{
			base.ValidateBranch();
			if (ExportGeneralLedger && !Branch.IsEmpty)
			{
				BranchInfo.AddWarning(ResString.GetMultilingualString("4453B60E-F571-49F2-A026-37E5C95914E8", "You cannot select Branch Filter if you are printing Job Costing Voucher."));
			}
		}

		#endregion
	}
}
