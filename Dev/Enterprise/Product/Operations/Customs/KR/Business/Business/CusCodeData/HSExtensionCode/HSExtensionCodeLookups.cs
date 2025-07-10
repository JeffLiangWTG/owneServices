using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class HSExtensionCodeLookups : CusCodeDataLookups
	{
		public HSExtensionCodeLookups(HSExtensionCode parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList CY_CodeList
		{
			get
			{
				var tariff = Parent.Parent.Tariff;
				var categoryCode = Parent.Parent.HSExtensionCodeCollection.Cast<HSExtensionCode>().FirstOrDefault(x => x.Category == Messaging.Constants.HsExtensionCodes.Code.Category)?.CY_Code ?? ZString.Empty;
				return Factory.GetCachedValue($"HSExtensionCode.CY_CodeList_{tariff}_{Parent.CY_Order}_{categoryCode}", () =>
				{
					var result = new CodeDescriptionPairList();
					var tariffView = Parent.TariffView;
					if (tariffView != null)
					{
						var tariffAdditionalCodeViewCollection = tariffView.AdditionalCodes.Where(x => x.ZY2_ZY3_NKCategory == Parent.Category);
						if (Parent.Category == Messaging.Constants.HsExtensionCodes.Code.SubCategory)
						{
							tariffAdditionalCodeViewCollection = tariffAdditionalCodeViewCollection.Where(x => x.ZY2_ParentAdditionalCode == categoryCode && x.ZY2_AdditionalCode.Left(1) == Parent.CY_Order.ToString());
						}
						var orderedCollection = tariffAdditionalCodeViewCollection.Distinct().OrderBy(x => x.ZY2_AdditionalCode);
						foreach (var item in orderedCollection)
						{
							result.AddPair(item.ZY2_AdditionalCode, item.ZY2_Description);
						}
					}
					return result;
				});
			}
		}
		protected new HSExtensionCode Parent => base.Parent as HSExtensionCode;
	}
}
