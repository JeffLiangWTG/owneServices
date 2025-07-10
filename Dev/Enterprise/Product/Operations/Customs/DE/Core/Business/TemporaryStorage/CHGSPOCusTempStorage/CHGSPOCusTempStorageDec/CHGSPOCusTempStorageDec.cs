using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(CusTempStorageJobHeader), "CHGSPOCusTempStorageDecs")]
	public class CHGSPOCusTempStorageDec : CusTempStorageDec
	{
		public CHGSPOCusTempStorageDec(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		public override ZString ReferenceNumber => FormattedOwnerReferenceNumber;

		#endregion

		#region CusTempStorageLines

		public new CusTempStorageLineCollection<CHGSPOCusTempStorageLine, CHGSPOCusTempStorageDec> CusTempStorageLines => (CusTempStorageLineCollection<CHGSPOCusTempStorageLine, CHGSPOCusTempStorageDec>)base.CusTempStorageLines;

		protected override CusTempStorageLineCollection CreateNewCusTempStorageLines() => new CusTempStorageLineCollection<CHGSPOCusTempStorageLine, CHGSPOCusTempStorageDec>(this);

		#endregion

		#region Lookups

		public new CHGSPOCusTempStorageDecLookups Lookups => (CHGSPOCusTempStorageDecLookups)base.Lookups;

		protected override EU.Business.CusTempStorage.CusTempStorageDecLookups GetNewLookups() => new CHGSPOCusTempStorageDecLookups(this);

		#endregion

		#region Validation

		public new CHGSPOCusTempStorageDecValidation Validation => (CHGSPOCusTempStorageDecValidation)base.Validation;

		protected override EU.Business.CusTempStorage.CusTempStorageDecValidation GetNewValidation() => new CHGSPOCusTempStorageDecValidation(this);

		#endregion

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.ChangeOfSpecificOrderTerm;
		}

		#endregion
	}
}
