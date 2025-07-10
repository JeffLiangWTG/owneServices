using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.MasterFiles.Testing
{
	public class RefUNLOCOExtension : TestCaseWithFactory
	{
		public void TestIsInEUSpecialTerritory()
		{
			var riga = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "LVRIX"));
			var sydney = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			var barcelona = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "ESBCN"));
			var heligoland = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "DEHGL"));
			var esPro = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "ESPRO"));  // 	Puerto del Rosario on Fuerteventura 
			var esLpa = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "ESLPA"));
			var douglas = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "IMDGS"));
			var stPeterPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GGSPT"));
			var stHellier = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "JESTH"));
			var grandCase = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GPCCE"));
			var saintPierre = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "MQSPI"));
			var reunion = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "REREU"));
			var aland = Factory.New<RefUNLOCO>();
			aland.RL_Code = "AXYYY";
			var genericCanaryIsland = Factory.New<RefUNLOCO>();
			genericCanaryIsland.RL_Code = "ICYYY";
			Assert(!riga.IsInEUSpecialTerritory());
			Assert(!sydney.IsInEUSpecialTerritory());
			Assert(!barcelona.IsInEUSpecialTerritory());
			Assert(esPro.IsInEUSpecialTerritory());
			Assert(esLpa.IsInEUSpecialTerritory());
			Assert(douglas.IsInEUSpecialTerritory());
			Assert(stPeterPort.IsInEUSpecialTerritory());
			Assert(stHellier.IsInEUSpecialTerritory());
			Assert(grandCase.IsInEUSpecialTerritory());
			Assert(reunion.IsInEUSpecialTerritory());
			Assert(aland.IsInEUSpecialTerritory());
			Assert(heligoland.IsInEUSpecialTerritory());
			Assert(genericCanaryIsland.IsInEUSpecialTerritory());
		}
	}
}
