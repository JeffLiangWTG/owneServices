using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SeaCargoPluginTestHelper
	{
		internal static ForwardingConsol GetConsolForTest(BusinessObjectFactory factory)
		{
			var consol = factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUSYD";
			return consol;
		}

		internal static ZSaveException GetZSaveExceptionForTest(BusinessObjectFactory factory, DataRow row)
		{
			var dataException = new ZDataException(new Exception(), row, Db.Connection);
			dataException.SetFriendlyMessageForTest("SaveException For Test");
			return new ZSaveErrorAfterCommitInDbException(dataException, factory);
		}
	}
}
