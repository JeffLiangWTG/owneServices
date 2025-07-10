using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	class RIDMilitaryConsignmentComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => false;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			var dataItem = wrapper?.DGData;
			if (dataItem == null)
			{
				return ZString.Empty;
			}

			if ((dataItem?.Subs?.DG_Class.ToString() ?? ZString.Empty) == "1")
			{
				if (((dataItem?.Subs?.StandardSubstance as UNDGSubstanceRID)?.RID_SpecialProvisions ?? ZString.Empty) == "W2")
				{
					return (NoResString)"MILITARY CONSIGNMENT";
				}
			}

			return ZString.Empty;
		}
	}
}
