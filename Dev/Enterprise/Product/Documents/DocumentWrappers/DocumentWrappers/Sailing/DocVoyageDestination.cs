using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocVoyageDestination : DocumentWrapper
	{
		DocVoyageDestination(VoyageDestination voyageDestination, BusinessObjectFactory factoryToWrap)
			: base(voyageDestination, factoryToWrap)
		{
		}

		public static DocVoyageDestination New(BusinessObjectFactory factory, ZGuid pK)
		{
			return New(factory.Load<VoyageDestination>(pK), factory);
		}

		public static DocVoyageDestination New(VoyageDestination voyageDestination, BusinessObjectFactory factoryToWrap)
		{
			if (voyageDestination == null)
			{
				return null;
			}
			else
			{
				return new DocVoyageDestination(voyageDestination, factoryToWrap);
			}
		}

		VoyageDestination VoyageDestination
		{
			get { return (VoyageDestination)WrappedObject; }
		}

		public override string ToString()
		{
			return "";
		}

		public ZDateTime ActualARV
		{
			get { return VoyageDestination.JB_A_ARV; }
		}

		public ZDateTime EstimatedARV
		{
			get { return VoyageDestination.JB_E_ARV; }
		}

		public ZBool IsTranshipment
		{
			get { return VoyageDestination.JB_IsTranshipment; }
		}

		public DocVoyage Voyage
		{
			get { return DocVoyage.New(VoyageDestination.Voyage, Factory); }
		}

		public DocUNLOCO NKPortOfDischarge
		{
			get { return VoyageDestination.JB_RL_NKPortOfDischarge.IsValid ? DocUNLOCO.New(VoyageDestination.Factory, VoyageDestination.JB_RL_NKPortOfDischarge) : null; }
		}

		public ZString Berth
		{
			get { return VoyageDestination.JB_Berth; }
		}
	}
}
