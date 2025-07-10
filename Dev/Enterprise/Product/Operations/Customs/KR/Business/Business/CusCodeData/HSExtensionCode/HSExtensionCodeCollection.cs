using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.KR.Business
{
	public class HSExtensionCodeCollection : CusCodeDataCollection<HSExtensionCode>
	{
		public HSExtensionCodeCollection(ILineOrProduct lineOrProduct)
		: base((BusinessObject)lineOrProduct, CusCodeDataTypeList.Codes.HsExtensionCode)
		{
		}

		public void UpdateMainAdditionalCode(TariffView universalTariff)
		{
			RemoveAndDeleteAll();
			if (universalTariff != null)
			{
				if (universalTariff.AdditionalCodes.Any(x => x.ZY2_ZY3_NKCategory == Messaging.Constants.HsExtensionCodes.Code.Category))
				{
					var mainHSExtensionCode = AddNew();
					mainHSExtensionCode.CY_Order = 0;
				}
			}
		}

		public void UpdateSubAdditionalCode(TariffView universalTariff, ZString mainAdditionalCode)
		{
			this.Where(x => x.Category == Messaging.Constants.HsExtensionCodes.Code.SubCategory).DeleteAll();
			if (universalTariff != null)
			{
				var subAdditionalCodes = universalTariff.AdditionalCodes.Where(x => x.ZY2_ZY3_NKCategory == Messaging.Constants.HsExtensionCodes.Code.SubCategory
																				&& x.ZY2_ParentAdditionalCode == mainAdditionalCode
																				&& x.ZY2_ZY3_NKParentCategory == Messaging.Constants.HsExtensionCodes.Code.Category
																				).DistinctBy(x => x.ZY2_AdditionalCode.Left(1)).OrderBy(x => x.ZY2_AdditionalCode);
				foreach (var subAdditionalCode in subAdditionalCodes)
				{
					var subHSExtensionCode = AddNew();
					subHSExtensionCode.CY_Order = ZShort.Parse(subAdditionalCode.ZY2_AdditionalCode.Left(1));
				}
			}
		}

		protected override bool AllowNewCore => false;
	}
}
