using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Common
{
	[ModuleID(ModuleId.RefVesselZZ)]
	public class RadioCallSignCodeFindBoxCollection : ActiveBusinessObjectCollection<RefVesselZZForRadioCallSign>
	{
		public RadioCallSignCodeFindBoxCollection(BusinessObjectFactory factory) : base(factory, new ZQuery(RefVesselZZSchema.ZZO_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.Japan))
		{
		}

		public static class FilterConstants
		{
			public const string RadioCallSign = "Radio Call Sign";
		}

		protected override void SetDefaultsForNewElementCore(RefVesselZZForRadioCallSign newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.ZZO_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Japan;
		}

		public static RadioCallSignCodeFindBoxCollection GetCachedCollection(BusinessObjectFactory factory, ZString radioCall)
		{
			var key = string.Format(CultureInfo.InvariantCulture, "RadioCallSignCodeFindBoxCollection_{0}", radioCall);
			return factory.GetCachedValue(key, () =>
			{
				var collection = new RadioCallSignCodeFindBoxCollection(factory);
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterConstants.RadioCallSign, "Property", radioCall));
				return collection;
			});
		}
	}
}
