using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocERA))]
	sealed class DocERABOTest : DocumentWrapperTestCase
	{
		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "USLAX";

			Transport transport1 = consol.Transports[0];
			transport1.JW_RL_NKDiscPort = "SGSIN";
			transport1.JW_Vessel = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_Code;
			transport1.JW_VoyageFlight = "1234";

			Transport transport2 = consol.Transports[0];
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "USLAX";

			CommonContainer container = consol.Containers.AddNew();

			return new DocumentWrapper[]
			{
				DocERA.New(container, Factory)
			};
		}

		#endregion
	}
}
