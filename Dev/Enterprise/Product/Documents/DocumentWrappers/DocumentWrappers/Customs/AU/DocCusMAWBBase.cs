using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocCusMAWBBase : DocumentWrapper
	{
		protected DocCusMAWBBase(CusMAWBBase cusMAWB, BusinessObjectFactory factoryToWrap)
			: base(cusMAWB, factoryToWrap)
		{
		}

		public static DocCusMAWBBase New(CusMAWBBase cusMAWB, BusinessObjectFactory factoryToWrap)
		{
			if (cusMAWB == null)
			{
				return null;
			}
			else
			{
				return new DocCusMAWBBase(cusMAWB, factoryToWrap);
			}
		}

		CusMAWBBase CusMAWB
		{
			get { return (CusMAWBBase)WrappedObject; }
		}

		public override string ToString()
		{
			return ZString.Empty;
		}

		public ZString MAWB
		{
			get { return CusMAWB.CM_MAWB; }
		}

		public ZDateTime ArrivalDate
		{
			get { return CusMAWB.CM_ArrivalDate; }
		}

		public ZString FlightNo
		{
			get { return CusMAWB.CM_FlightNo; }
		}

		public DocUNLOCO NKDischargePort
		{
			get { return CusMAWB.CM_RL_NKDischargePort.IsValid ? DocUNLOCO.New(CusMAWB.Factory, CusMAWB.CM_RL_NKDischargePort) : null; }
		}

		public DocUNLOCO NKLoadPort
		{
			get { return CusMAWB.CM_RL_NKLoadPort.IsValid ? DocUNLOCO.New(CusMAWB.Factory, CusMAWB.CM_RL_NKLoadPort) : null; }
		}

		public ZString MasterHouseBill
		{
			get { return CusMAWB.CM_MasterHouseBill; }
		}

		public ZString ApplicationCode
		{
			get { return CusMAWB.CM_ApplicationCode; }
		}

		public ZDateTime DateOfFirstArrival
		{
			get { return CusMAWB.CM_DateOfFirstArrival; }
		}

		public ZString Folio
		{
			get { return CusMAWB.CM_Folio; }
		}

		public DocBranch Branch
		{
			get { return CusMAWB.Branch != null ? DocBranch.New(CusMAWB.Branch, Factory) : null; }
		}

		public ZBool HouseMessageIsSent
		{
			get { return CusMAWB.CM_HouseMessageIsSent; }
		}

		public ZBool IsCTOMAWB
		{
			get { return CusMAWB.CM_IsCTOMAWB; }
		}

		public DocForwardingConsol Consol
		{
			get { return CusMAWB.CM_JK.IsValid ? DocForwardingConsol.New(CusMAWB.Factory, CusMAWB.CM_JK) : null; }
		}

		public DocUNLOCO NKFirstArrivalPort
		{
			get { return CusMAWB.CM_RL_NKFirstArrivalPort.IsValid ? DocUNLOCO.New(CusMAWB.Factory, CusMAWB.CM_RL_NKFirstArrivalPort) : null; }
		}
	}
}
