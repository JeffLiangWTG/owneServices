using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(CUSPCSCusTempStorageDec), "CusTempStorageLines")]
	public class CUSPCSSplitCusTempStorageLine : CusTempStorageLine
	{
		public CUSPCSSplitCusTempStorageLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Dec

		public new CUSPCSCusTempStorageDec Dec => Factory.Load<CUSPCSCusTempStorageDec>(TSL_STH);

		#endregion

		#region Validation

		protected override EU.Business.CusTempStorage.CusTempStorageLineValidation GetNewValidation() => new CUSPCSSplitCusTempStorageLineValidation(this);

		#endregion

		#region Lookups

		public new CUSPCSSplitCusTempStorageLineLookups Lookups => (CUSPCSSplitCusTempStorageLineLookups)base.Lookups;
		protected override EU.Business.CusTempStorage.CusTempStorageLineLookups GetNewLookups() => new CUSPCSSplitCusTempStorageLineLookups(this);

		#endregion

	}
}
