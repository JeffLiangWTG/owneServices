
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	static class IDutyAndTaxDataExtensions
	{
		public static CACClassHeader GetClassHeader(this IDutyAndTaxData data)
		{
			CACClassHeader result = null;
			if (data != null && !data.ClassificationNumber.IsEmpty)
			{
				result = CACClassHeader.Load(data.Factory, data.EffectiveDutyDate, data.ClassificationNumber);
			}
			return result;
		}

		public static CACTaxRefNumHeader GetTaxRefNumHeader(this IDutyAndTaxData data)
		{
			CACTaxRefNumHeader result = null;
			if (data != null)
			{
				var classHeader = data.GetClassHeader();
				if (classHeader != null)
				{
					if (classHeader.ZA_AreaCode == UniversalReferenceConstants.TempAreaCode)
					{
						classHeader = CACClassHeader.Load(data.Factory, UniversalReferenceConstants.SyncCAReferenceCutOffDate.AddHours(-25), classHeader.ZA_ClassificationNumber);
					}

					if (classHeader != null)
					{
						result = CACTaxRefNumHeader.Load(classHeader, data.EffectiveDutyDate);
					}
				}
			}
			return result;
		}

		public static bool IsCigars(this IDutyAndTaxData data)
		{
			var result = false;
			if (data != null)
			{
				var refNumHeader = data.GetTaxRefNumHeader();
				result = refNumHeader != null && refNumHeader.IsCigars;
			}
			return result;
		}

		public static bool IsSimaAmountPayable(ZString simaCode)
		{
			return simaCode.EndsWith("1"); // last digit of SIMA code = "1" = CASH PAYMENT
		}
	}
}
