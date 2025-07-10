using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.MasterFiles;

namespace Enterprise.Customs.EU.Business
{
	public class CusClassPartPivotConfiguration
	{
		public static CusClassPartPivotConfiguration GetConfiguration(BusinessObjectFactory factory, string countryOrGrouping)
		{
			return factory.GetCachedValue(FormattableString.Invariant($"CusClassPartPivotConfiguration_{countryOrGrouping}"), () =>
			{
				object supporter = null;
				var builders = ObjectFactory.Get<Hashtable>("CusClassPartPivotConfiguration");
				if (!string.IsNullOrEmpty(countryOrGrouping))
				{
					var objectHandle = (ObjectHandle)builders[countryOrGrouping];
					supporter = objectHandle?.GetObject();
				}
				if (supporter == null)
				{
					var objectHandle = (ObjectHandle)builders[Core.Constants.CountryCodes.EuropeanUnion];
					supporter = objectHandle.GetObject();
				}
				return (CusClassPartPivotConfiguration)supporter;
			});
		}

		public ZBool UCCAdditionalInfosSupport(CusClassPartPivot pivot) => UCCAdditionalInfosSupportCore(pivot);
		protected virtual ZBool UCCAdditionalInfosSupportCore(CusClassPartPivot businessObject) => false;
	}
}


