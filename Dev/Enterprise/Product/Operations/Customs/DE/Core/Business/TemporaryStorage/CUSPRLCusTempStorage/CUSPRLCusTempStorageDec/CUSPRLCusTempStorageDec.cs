using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(CusTempStorageJobHeader), "CUSPRLCusTempStorageDec")]
	public class CUSPRLCusTempStorageDec : CusTempStorageDec
	{
		public CUSPRLCusTempStorageDec(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Construction / Loading

		public static CUSPRLCusTempStorageDec LoadOrCreate(CusTempStorageJobHeader parent)
		{
			return Load(parent) ?? New(parent);
		}

		public static CUSPRLCusTempStorageDec Load(CusTempStorageJobHeader parent)
		{
			return (CUSPRLCusTempStorageDec)Load(parent, TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationLedger);
		}

		public static CUSPRLCusTempStorageDec New(CusTempStorageJobHeader parent)
		{
			var result = parent.Factory.New<CUSPRLCusTempStorageDec>();
			using (result.SuspendSettingHasChanges())
			{
				result.STH_SJH = parent.PK;
			}
			return result;
		}

		#endregion

		#region Properties

		public override ZString ReferenceNumber
		{
			get => base.ReferenceNumber;
			set
			{
				var oldValue = ReferenceNumber;
				base.ReferenceNumber = value;
				if (!IsCopying && ReferenceNumber != oldValue && ReferenceNumber.IsEmpty && STH_MessageStatus.IsEmpty)
				{
					LineNumberGenerator.ReCalculateAll();
				}
			}
		}

		protected override ZBool ReferenceNumber_ReadOnly => ReferenceNumberIssueDatePopulated;

		#endregion

		#region Validation

		protected override EU.Business.CusTempStorage.CusTempStorageDecValidation GetNewValidation() => new CUSPRLCusTempStorageDecValidation(this);

		#endregion

		#region CusTempStorageLines

		public new CusTempStorageLineCollection<CUSPRLCusTempStorageLine, CUSPRLCusTempStorageDec> CusTempStorageLines => (CusTempStorageLineCollection<CUSPRLCusTempStorageLine, CUSPRLCusTempStorageDec>)base.CusTempStorageLines;

		protected override CusTempStorageLineCollection CreateNewCusTempStorageLines() => new CusTempStorageLineCollection<CUSPRLCusTempStorageLine, CUSPRLCusTempStorageDec>(this);

		#endregion

		#region Overrides 

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationLedger;
			STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
		}

		protected override ZString EntryNumberType => CusEntryNumberTypes.Germany.SumAEntryNumber;

		#endregion

		#region Implementation

		public override bool IsDataEmpty => ReferenceNumber.IsEmpty && STH_MessageStatus.IsEmpty && CusTempStorageLines.Count == 0;

		#endregion
	}
}
