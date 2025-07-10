using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(CusTempStorageJobHeader), "CUSPCSCusTempStorageDecs")]
	public class CUSPCSCusTempStorageDec : CusTempStorageDec
	{
		public CUSPCSCusTempStorageDec(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties
		public override ZString STH_IdentificationIndicator
		{
			get => base.STH_IdentificationIndicator;
			set
			{
				if (base.STH_IdentificationIndicator != value)
				{
					base.STH_IdentificationIndicator = value;

					if (IsAWBDeclaration)
					{
						ConsolidatedCusTempStorageLine.TSL_LineNo = 1;
					}
					else if (IsREGDeclaration)
					{
						ConsolidatedCusTempStorageLine.TSL_OwnerReferenceType = TemporaryStorageIdentificationIndicatorList.Codes.REG;
						ConsolidatedCusTempStorageLine.TSL_OA_Custodian_ZAddress.SetOrgWithoutSettingDefaultAddress(ZGuid.Empty);
						ConsolidatedCusTempStorageLine.TSL_OA_Custodian = ZGuid.Empty;
					}
				}
			}
		}

		[ResourceStringData("D5E5A554-B64A-4914-88A2-935BF566E2F7", Caption = "New Reference")]
		public override ZString ReferenceNumber { get => base.ReferenceNumber; set => base.ReferenceNumber = value; }

		#endregion

		#region Lookups

		public new CUSPCSCusTempStorageDecLookups Lookups => (CUSPCSCusTempStorageDecLookups)base.Lookups;

		protected override EU.Business.CusTempStorage.CusTempStorageDecLookups GetNewLookups() => new CUSPCSCusTempStorageDecLookups(this);

		#endregion

		#region Validation

		protected override EU.Business.CusTempStorage.CusTempStorageDecValidation GetNewValidation() => new CUSPCSCusTempStorageDecValidation(this);

		#endregion

		#region Implement

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationCargo;
			STH_IdentificationIndicator = ZString.Empty;
		}

		protected override IEnumerable<ISequenceNumberLine> GetLinesCore()
		{
			return new TypedEnumerable<ISequenceNumberLine>(ConsolidatedCusTempStorageLine.CusTempStorageLinesTo);
		}

		#endregion

		#region CusTempStorageLines

		public new CUSPCSConsolidatedCusTempStorageLineCollection CusTempStorageLines => (CUSPCSConsolidatedCusTempStorageLineCollection)base.CusTempStorageLines;

		protected override EU.Business.CusTempStorage.CusTempStorageLineCollection CreateNewCusTempStorageLines() => new CUSPCSConsolidatedCusTempStorageLineCollection(this);

		#endregion

		#region ConsolidatedCusTempStorageLine

		public CUSPCSConsolidatedCusTempStorageLine ConsolidatedCusTempStorageLine
		{
			get
			{
				if (consolidatedCusTempStorageLine == null)
				{
					consolidatedCusTempStorageLine = CUSPCSConsolidatedCusTempStorageLine.LoadOrCreate(this);
					RegisterEditableChildObject(consolidatedCusTempStorageLine);
				}
				return consolidatedCusTempStorageLine;
			}
		}
		CUSPCSConsolidatedCusTempStorageLine consolidatedCusTempStorageLine;

		#endregion
	}
}
