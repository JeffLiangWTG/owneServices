using System;
using CargoWise.Types;

namespace Enterprise.Integration.Freight
{
	public interface ITransport
	{
		ZString JW_Status { get; set; }
		ZBool JW_IsLinked { get; set; }
		Type ParentType { get; set; }
		ZString JW_RL_NKLoadPort { get; set; }
		ZString JW_RL_NKDiscPort { get; set; }
		ZString JW_VoyageFlight { get; set; }
		ZString JW_Vessel { get; set; }
		ZDateTime JW_ETA { get; set; }
		ZDateTime JW_ATA { get; set; }
		ZDateTime JW_ETD { get; set; }
		ZGuid JW_ParentGUID { get; set; }
		ZString JW_TransportMode { get; }
		ZByte JW_LegOrder { get; }
		ZString JW_VesselScreeningStatus { get; set; }
		ZGuid JW_OA_CarrierAddress { get; set; }
		ZString JW_LegNotes { get; set; }
		ZDateTime JW_DocumentaryCutOff { get; set; }
		ZDateTime JW_TerminalCutOff { get; set; }
		ZDateTime JW_VGMCutOff { get; set; }
		ZString CarrierName { get; }
		ZString CarrierCode { get; }
	}
}
