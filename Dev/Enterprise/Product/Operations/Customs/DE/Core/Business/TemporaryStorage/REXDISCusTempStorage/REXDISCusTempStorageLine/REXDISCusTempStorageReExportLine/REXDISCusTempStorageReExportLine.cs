using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(REXDISCusTempStorageDec), "CusTempStorageLines")]
	public class REXDISCusTempStorageReExportLine : REXDISCusTempStorageLine
	{
		public REXDISCusTempStorageReExportLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region CusTempStorageSumALines

		[ChildEditable]
		public REXDISCusTempStorageSumALineCollection CusTempStorageSumALines
		{
			get
			{
				if (cusTempStorageSumALines == null)
				{
					cusTempStorageSumALines = new REXDISCusTempStorageSumALineCollection(this);
					cusTempStorageSumALines.Load();
					RegisterEditableChildObject(cusTempStorageSumALines);
				}
				return cusTempStorageSumALines;
			}
		}
		REXDISCusTempStorageSumALineCollection cusTempStorageSumALines;

		public REXDISCusTempStorageSumALine SumALine
		{
			get
			{
				if (sumALine == null)
				{
					sumALine = REXDISCusTempStorageSumALine.LoadOrCreate(this);
					RegisterEditableChildObject(sumALine);
				}
				return sumALine;
			}
		}
		REXDISCusTempStorageSumALine sumALine;

		#endregion

		#region Overrides

		protected override bool ReadOnlyTSL_LineNo => true;

		protected override bool SequenceNumberEnabledCore => true;

		protected override bool SetLineNumOnSettingTSL_STHEnabled => true;

		#endregion

		#region Validation

		protected override EU.Business.CusTempStorage.CusTempStorageLineValidation GetNewValidation() => new REXDISCusTempStorageReExportLineValidation(this);

		#endregion

		#region Lookups

		protected override EU.Business.CusTempStorage.CusTempStorageLineLookups GetNewLookups() => new REXDISCusTempStorageReExportLineLookups(this);

		public new REXDISCusTempStorageReExportLineLookups Lookups => (REXDISCusTempStorageReExportLineLookups)base.Lookups;

		#endregion
	}
}
