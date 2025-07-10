using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Schema.Tests
{
	public class CusAddInfoTypeCodeTest : TestCase
	{
		public void TestMakeSureEveryTypeCodeIsCusAddInfoColumnSchemaResolver()
		{
			var typeCodesList = new List<string>
			{
				"AML",
				"AMS",
				"APH",
				"API",
				"APL",
				"APN",
				"APP",
				"APR",
				"APS",
				"ARL",
				"ATF",
				"CAC",
				"CSM",
				"CDT",
				"CLR",
				"CON",
				"CPC",
				"CPR",
				"CPS",
				"DEA",
				"DEC",
				"DOF",
				"DOG",
				"DTI",
				"ERE",
				"ERI",
				"FDA",
				"FDH",
				"FLS",
				"FSH",
				"FSL",
				"FWH",
				"FWL",
				"GBA",
				"GBM",
				"GSH",
				"GTX",
				"IPK",
				"ITN",
				"LAC",
				"LOT",
				"LSN",
				"NFH",
				"NFL",
				"NFV",
				"NMD",
				"NMF",
				"NTA",
				"NTC",
				"NTD",
				"NTH",
				"NTP",
				"OMC",
				"OMD",
				"PGA",
				"PSL",
				"PST",
				"REG",
				"RFP",
				"ROC",
				"ROL",
				"RQD",
				"SCI",
				"SID",
				"TBC",
				"TBL",
				"TBP",
				"TCC",
				"TCD",
				"TCI",
				"TCP",
				"UCN",
				"UDC",
				"UDH",
				"UDL",
				"UDP",
				"UDZ",
				"UFH",
				"UFL",
				"ULE",
				"ULR",
				"UOD",
				"UPQ",
				"US7",
				"USA",
				"USC",
				"USD",
				"USE",
				"USI",
				"USP",
				"UST",
				"USV",
				"UWD",
				"VDE",
				"VID",
				"VV1",
				"VEH",
				"WCA",
				"WPK",
				"WPL",
				"ALI"
			};

			var failureMessage =
				"Make sure every type code from CusAddInfoTypeAttribute.cs is added to the resolver. ";

			var schemaResolver = new CusAddInfoColumnSchemaResolver();
			foreach (var typeCode in typeCodesList)
			{
				var addInfoSchema = schemaResolver.GetCusAddInfoSchemaSchema(typeCode);
				AssertNotNull(failureMessage + "\r\nTypeCode: " + typeCode, addInfoSchema);
			}
		}
	}
}
