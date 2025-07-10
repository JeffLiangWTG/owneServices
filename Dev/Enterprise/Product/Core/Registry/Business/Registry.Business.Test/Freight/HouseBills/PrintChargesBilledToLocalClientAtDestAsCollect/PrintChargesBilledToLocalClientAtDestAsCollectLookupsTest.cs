using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Registry.Business.Testing
{
	sealed class PrintChargesBilledToLocalClientAtDestAsCollectLookupsTest : TestCaseWithFactory
	{
		const string countryList = "AD,AE,AF,AG,AI,AL,AM,AN,AO,AQ,AR,AS,AT,AU,AW,AX,AZ,BA,BB,BD,BE,BF,BG,BH,BI,BJ,BL,BM,BN,BO,BQ,BR,BS,BT,BV,BW,BY,BZ,CA,CC,CD,CF,CG,CH,CI,CK,CL,CM,CN,CO,CR,CS,CU,CV,CW,CX,CY,CZ,DE,DJ,DK,DM,DO,DZ,EC,EE,EG,EH,ER,ES,ET,FI,FJ,FK,FM,FO,FR,GA,GB,GD,GE,GF,GG,GH,GI,GL,GM,GN,GP,GQ,GR,GS,GT,GU,GW,GY,HK,HM,HN,HR,HT,HU,ID,IE,IL,IM,IN,IO,IQ,IR,IS,IT,JE,JM,JO,JP,KE,KG,KH,KI,KM,KN,KP,KR,KW,KY,KZ,LA,LB,LC,LI,LK,LR,LS,LT,LU,LV,LY,MA,MC,MD,ME,MF,MG,MH,MK,ML,MM,MN,MO,MP,MQ,MR,MS,MT,MU,MV,MW,MX,MY,MZ,NA,NC,NE,NF,NG,NI,NL,NO,NP,NR,NU,NZ,OM,PA,PE,PF,PG,PH,PK,PL,PM,PN,PR,PS,PT,PW,PY,QA,RE,RO,RS,RU,RW,SA,SB,SC,SD,SE,SG,SH,SI,SJ,SK,SL,SM,SN,SO,SR,SS,ST,SV,SX,SY,SZ,TC,TD,TF,TG,TH,TJ,TK,TL,TM,TN,TO,TR,TT,TV,TW,TZ,UA,UG,UM,US,UY,UZ,VA,VC,VE,VG,VI,VN,VU,WF,WS,XK,XZ,YE,YT,ZA,ZM,ZW";

		public void TestLookups()
		{
			var collection = new PrintChargesBilledToLocalClientAtDestAsCollectCollection();
			var setting = collection.AddNew();
			setting.TransportMode = Core.Constants.TransportModes.Sea;
			setting.ExportCountry = Core.Constants.CountryCodes.Belgium;
			setting.ImportCountry = Core.Constants.CountryCodes.Australia;

			using (FreightDataRegistry.Instance.PrintChargesBilledToLocalClientAtDestAsCollect.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var lookups = new PrintChargesBilledToLocalClientAtDestAsCollectLookups(setting, Factory);

				var result = string.Join(",", lookups.TransportMode_List.ToArray().Select(x => x.Code).OrderBy(s => s));
				AssertEquals(string.Join(",", Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Sea), result);

				result = string.Join(",", lookups.CountryCollection.Cast<IRefCountry>().Select(x => x.RN_Code).OrderBy(s => s));
				AssertEquals(countryList, result);
			}
		}
	}
}
