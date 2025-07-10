using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocERA : DocBaseWrapper
	{
		DocERA(IPRAMessagingData data, BusinessObjectFactory factoryToWrap)
			: base(data, factoryToWrap)
		{
		}

		public static DocERA New(CommonContainer container, BusinessObjectFactory factoryToWrap)
		{
			return new DocERA(new FreightDataLayer(container), factoryToWrap);
		}

		#region ZString Properties

		public ZString VesselName
		{
			get { return Data.VesselName; }
		}

		public ZString VoyageNo
		{
			get { return Data.Voyage; }
		}

		#endregion

		#region Wrapper Properties

		public DocUNLOCO DischargePort
		{
			get { return DocUNLOCO.New(Factory, Data.PortOfDischarge); }
		}

		public DocUNLOCO FinalDestination
		{
			get { return DocUNLOCO.New(Factory, Data.PortOfFinalDischarge); }
		}

		#endregion

		#region Implementation

		IPRAMessagingData Data
		{
			get { return (IPRAMessagingData)WrappedObject; }
		}

		#endregion
	}
}
