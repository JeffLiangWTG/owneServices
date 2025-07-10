using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(CusTempStorageJobHeader), "CusTempStorageDec")]
	public class ISTCusTempStorageDec : CusTempStorageDec, ITemporaryStorageRegisterTransactionDataProvider
	{
		public ISTCusTempStorageDec(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			STH_DeclarationType = FRConstants.TemporaryStorage.AppCodeIST;
		}

		public new ISTCusTempStorageDecValidation Validation => (ISTCusTempStorageDecValidation)base.Validation;

		protected override EU.Business.CusTempStorage.CusTempStorageDecValidation GetNewValidation()
		{
			return new ISTCusTempStorageDecValidation(this);
		}

		#region Properties

		#endregion

		#region CusTempStorageLines

		public new ISTCusTempStorageLineCollection CusTempStorageLines => (ISTCusTempStorageLineCollection)base.CusTempStorageLines;

		protected override CusTempStorageLineCollection CreateNewCusTempStorageLines() => new ISTCusTempStorageLineCollection(this);

		#endregion

		public static ISTCusTempStorageDec New(CusTempStorageJobHeader parent)
		{
			var result = parent.Factory.New<ISTCusTempStorageDec>();
			using (result.SuspendSettingHasChanges())
			{
				result.STH_SJH = parent.PK;
			}
			return result;
		}

		public static ISTCusTempStorageDec Load(CusTempStorageJobHeader parent)
		{
			return CusTempStorageDec.Load<ISTCusTempStorageDec>(parent);
		}

		public IEnumerable<TemporaryStorageRegisterTransactionData> GetTemporaryStorageRegisterTransactionData()
		{
			foreach (ISTCusTempStorageLine line in CusTempStorageLines)
			{
				yield return new TemporaryStorageRegisterTransactionData(Factory)
				{
					PreviousRegisterHeader = StorageHeader.PreviousISTHeader?.RegisterHeader,
					CustomsReferenceNumber = StorageHeader.DDTNumber,
					InternalReferenceNumber = StorageHeader.SJH_JobReference,
					GrossMass = line.EffectiveGrossWeight.InKilograms,
					PackageQuantity = line.TSL_PackageQty,
					ReferenceType = TempStorageTransactionRefTypeList.Codes.IstHeader,
					Comments = ZString.Empty,
					RegisterLineNo = line.TSL_ReferenceNumberLine
				};
			}
		}
	}
}
