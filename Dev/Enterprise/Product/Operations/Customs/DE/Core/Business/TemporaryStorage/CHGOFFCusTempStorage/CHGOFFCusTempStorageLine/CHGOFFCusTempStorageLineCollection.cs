using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CHGOFFCusTempStorageLineCollection : CusTempStorageLineCollection<CHGOFFCusTempStorageLine, CusTempStorageDec>
	{
		public CHGOFFCusTempStorageLineCollection(CusTempStorageDec declaration) : base(declaration)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			var line = (CHGOFFCusTempStorageLine)child;
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
