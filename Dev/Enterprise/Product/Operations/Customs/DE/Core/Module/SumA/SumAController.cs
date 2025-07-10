using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.GUI;
using Enterprise.Customs.EU.TemporaryStorage.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.Module
{
	public class SumAController : TemporaryStorageController
	{
		public SumAController()
			: this(TemporaryStorageApplicationCodeList.Codes.SumA)
		{
		}

		public SumAController(ZString applicationCode)
		{
			this.applicationCode = applicationCode;
		}

		readonly ZString applicationCode;

		public override System.Type TypeOfTopLevelBusinessObject => typeof(CusTempStorageJobHeader);

		protected override IZForm GetForm(IBusiness businessEntity) => new TemporaryStorageForm((CusTempStorageJobHeader)businessEntity);

		#region New

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var result = Factory.New<CusTempStorageJobHeader>();
			result.SJH_AppCode = applicationCode;
			switch (applicationCode)
			{
				case TemporaryStorageApplicationCodeList.Codes.REX:
					REXDISCusTempStorageDec.New(result);
					break;
				case TemporaryStorageApplicationCodeList.Codes.SumA:
					result.SetDefaultValuesForSumA();
					break;
			}
			return result;
		}

		#endregion
	}
}
