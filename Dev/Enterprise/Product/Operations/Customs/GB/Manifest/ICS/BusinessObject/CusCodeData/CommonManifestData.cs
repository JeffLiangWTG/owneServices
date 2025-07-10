using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Manifest.Business;

namespace Enterprise.Customs.GB.ICS
{
	public class CommonManifestData
	{
		public CommonManifestData(ZString code, ZString data, ZDateTime date, ZString type)
		{
			Code = code;
			Data = data;
			Date = date;
			Type = type;
		}

		public ZString Code { get; }
		public ZString Data { get; }
		public ZDateTime Date { get; }
		public ZString Type { get; }
	}

	public static class CommonManifestDataMapper
	{
		public static CommonManifestData[] ToCommonManifestData(this IcsOfficeCode[] data) =>
			data?.Select(x => new CommonManifestData(x.CY_Code, x.CY_Data, x.CY_Date, x.CY_Type)).ToArray() ?? Array.Empty<CommonManifestData>();
	}
}
