using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocVoyageOrigin : DocumentWrapper
	{
		DocVoyageOrigin(VoyageOrigin voyageOrigin, BusinessObjectFactory factoryToWrap)
			: base(voyageOrigin, factoryToWrap)
		{
		}

		public static DocVoyageOrigin New(VoyageOrigin voyageOrigin, BusinessObjectFactory factoryToWrap)
		{
			if (voyageOrigin == null)
			{
				return null;
			}
			else
			{
				return new DocVoyageOrigin(voyageOrigin, factoryToWrap);
			}
		}

		public override string ToString()
		{
			return "";
		}

		VoyageOrigin VoyageOrigin
		{
			get { return (VoyageOrigin)WrappedObject; }
		}

		public ZDateTime ActualDEP
		{
			get { return VoyageOrigin.JA_A_DEP; }
		}

		public ZDateTime EstimatedDEP
		{
			get { return VoyageOrigin.JA_E_DEP; }
		}

		public ZString Berth
		{
			get { return VoyageOrigin.JA_Berth; }
		}

		public DocVoyage Voyage
		{
			get { return DocVoyage.New(VoyageOrigin.Voyage, Factory); }
		}

		public DocUNLOCO NKPortOfLoading
		{
			get { return VoyageOrigin.JA_RL_NKPortOfLoading.IsValid ? DocUNLOCO.New(VoyageOrigin.Factory, VoyageOrigin.JA_RL_NKPortOfLoading) : null; }
		}
	}
}
