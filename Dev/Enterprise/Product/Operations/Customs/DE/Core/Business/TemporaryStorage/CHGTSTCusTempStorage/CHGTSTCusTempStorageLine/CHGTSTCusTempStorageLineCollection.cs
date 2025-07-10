using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CHGTSTCusTempStorageLineCollection : CusTempStorageLineCollection<CHGTSTCusTempStorageLine, CHGTSTCusTempStorageDec>
	{
		public CHGTSTCusTempStorageLineCollection(CHGTSTCusTempStorageDec declaration) : base(declaration)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			var line = (CHGTSTCusTempStorageLine)child;
			if (line.IsREGDeclaration)
			{
				using (line.SuspendSettingHasChanges())
				{
					line.TSL_OwnerReferenceType = OwnerReferenceTypeList.Codes.REG;
				}
			}
			base.SetDefaultsForNewChild(child);
		}
	}
}
