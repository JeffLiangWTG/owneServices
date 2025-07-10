using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocCTOCusMAWB : DocCusMAWBBase
	{
		protected DocCTOCusMAWB(CTOCusMAWB mawb, BusinessObjectFactory factory)
			: base(mawb, factory)
		{
		}

		public static DocCTOCusMAWB New(CTOCusMAWB mawb, BusinessObjectFactory factory)
		{
			return mawb == null ? null : new DocCTOCusMAWB(mawb, factory);
		}

		public ZString AirlineName
		{
			get
			{
				ZString result = "";

				ZString flightprefix = Mawb.CM_FlightNo.SubstringSafe(0, 2);

				if (!flightprefix.IsEmpty)
				{
					RefAirline airline = RefAirline.LoadFromAirline2LetterCode(Factory, flightprefix);
					if (airline != null)
					{
						result = airline.RM_AirlineName1;
					}
				}

				return result;
			}
		}

		public DocCTOCusHAWBCollection ChildrenBills
		{
			get { return new DocCTOCusHAWBCollection(Mawb.ChildBills); }
		}

		public ZDecimal TotalWeight
		{
			get
			{
				ZDecimal total = 0m;
				var weightUnit = TotalWeightUnit;

				foreach (CTOCusHAWB hawb in Mawb.ChildBills)
				{
					total += Core.Constants.Weight.Convert(hawb.CS_Weight, hawb.CS_WeightUQ, weightUnit);
				}

				return Utilities.Round(total, 3);
			}
		}

		public ZString TotalWeightUnit
		{
			get { return Env.Registry.FreightWeightUnit; }
		}

		public ZInt TotalPiecesManifested
		{
			get
			{
				ZInt total = 0;

				foreach (CTOCusHAWB hawb in Mawb.ChildBills)
				{
					total += hawb.CS_PiecesManifested;
				}

				return total;
			}
		}

		CTOCusMAWB Mawb
		{
			get { return (CTOCusMAWB)WrappedObject; }
		}
	}
}
