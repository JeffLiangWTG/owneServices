using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(CusTempStorageJobHeader), "PRLCONCusTempStorageDecs")]
	public class PRLCONCusTempStorageDec : CusTempStorageDec
	{
		public PRLCONCusTempStorageDec(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString STH_IdentificationIndicator
		{
			get { return base.STH_IdentificationIndicator; }
			set
			{
				if (value != STH_IdentificationIndicator)
				{
					base.STH_IdentificationIndicator = value;
					if (IsAWBDeclaration)
					{
						foreach (PRLCONCusTempStorageLineToConsolidate storageLine in CusTempStorageLines)
						{
							storageLine.TSL_LineNo = Math.Max(storageLine.TSL_LineNo, ZInt.Zero);
						}
					}
					else
					{
						LineNumberGenerator.ReCalculateAll();
					}
				}
			}
		}

		#region Lookups

		public new PRLCONCusTempStorageDecLookups Lookups => (PRLCONCusTempStorageDecLookups)base.Lookups;
		protected override EU.Business.CusTempStorage.CusTempStorageDecLookups GetNewLookups() => new PRLCONCusTempStorageDecLookups(this);

		#endregion

		#region Validation

		public new PRLCONCusTempStorageDecValidation Validation => (PRLCONCusTempStorageDecValidation)base.Validation;

		protected override EU.Business.CusTempStorage.CusTempStorageDecValidation GetNewValidation() => new PRLCONCusTempStorageDecValidation(this);

		#endregion

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.PresentationLedgerConsolidation;
		}

		#endregion

		#region CusTempStorageLines

		public new PRLCONCusTempStorageLineToConsolidateCollection CusTempStorageLines => (PRLCONCusTempStorageLineToConsolidateCollection)base.CusTempStorageLines;
		protected override EU.Business.CusTempStorage.CusTempStorageLineCollection CreateNewCusTempStorageLines() => new PRLCONCusTempStorageLineToConsolidateCollection(this);

		#endregion

		#region ConsolidatedLine

		public PRLCONConsolidatedCusTempStorageLine ConsolidatedLine
		{
			get
			{
				if (!IsDeleted && (consolidatedLine == null || consolidatedLine.IsDeleted))
				{
					consolidatedLine = consolidatedLine = PRLCONConsolidatedCusTempStorageLine.LoadOrCreate(this);
					RegisterEditableChildObject(consolidatedLine);
				}
				return consolidatedLine;
			}
		}
		PRLCONConsolidatedCusTempStorageLine consolidatedLine;

		#endregion
	}
}
