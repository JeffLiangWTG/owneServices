using System;
using System.Collections;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public class GdmConfiguration
	{
		public static GdmConfiguration GetConfiguration(BusinessObjectFactory factory, string countryOrGrouping)
		{
			return factory.GetCachedValue(FormattableString.Invariant($"GdmConfiguration_{countryOrGrouping}"), () =>
			{
				object supporter = null;
				var builders = ObjectFactory.Get<Hashtable>("GdmConfiguration");
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
				return (GdmConfiguration)supporter;
			});
		}

		public ZBool IsAWaiver(ZString conditionCode) => !WhatIsAWaiver.IsEmpty && Regex.IsMatch(conditionCode, WhatIsAWaiver);

		public ZString WhatIsAWaiver => WhatIsAWaiverCore;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		protected virtual ZString WhatIsAWaiverCore => @"^(?i)y.{3}$";
	}
}
