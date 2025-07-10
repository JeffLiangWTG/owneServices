using System;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class CargoReportHelper
	{
		public static ZString GetConsigneeABN(ZString businessNumber)
		{
			var abn = businessNumber.Replace(" ", "");
			if (abn.Contains("/", StringComparison.OrdinalIgnoreCase))
			{
				var arr = abn.Split('/');

				if (arr.Length == 2)
				{
					return arr[0].Left(11);
				}
			}
			return abn.Left(11);
		}

		public static ZString GetConsigneeCAC(ZString businessNumber)
		{
			var abn = businessNumber.Replace(" ", "");
			if (abn.Contains("/", StringComparison.OrdinalIgnoreCase))
			{
				var arr = abn.Split('/');

				if (arr.Length == 2)
				{
					return arr[1].Left(3);
				}
			}
			return ZString.Empty;
		}

		public static void DoICSRelease(Action iCSReleaseAction)
		{
			if (ZDateTime.Today >= AUCustomsDataRegistry.Instance.ICSReleaseEffectiveDate.Value)
			{
				iCSReleaseAction?.Invoke();
			}
		}
	}
}
