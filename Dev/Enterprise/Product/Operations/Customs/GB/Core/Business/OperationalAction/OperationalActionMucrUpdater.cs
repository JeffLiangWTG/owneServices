using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Business.OperationalAction
{
	class OperationalActionMucrUpdater
	{
		readonly JobDeclaration dec;

		public OperationalActionMucrUpdater(JobDeclaration dec)
		{
			this.dec = dec;
		}

		public void OperationaActionUpdateMucr(ZString mucrManualOverride, ZString newMawp, ZString newMawn, ZString newAirp, ZString newShed)
		{
			if (!mucrManualOverride.IsEmpty)
			{
				dec.JE_MasterUCR = mucrManualOverride;
			}
			else
			{
				// Setting these three will fire the Mucr generation accoring to the badge's generation style.
				if (!newMawp.IsEmpty && !newMawn.IsEmpty)
				{
					dec.JE_MasterBill = newMawp + newMawn;
				}
				if (!newAirp.IsEmpty)
				{
					dec.JE_LocationOfGoods = newAirp.PadRight(3).Right(3);
				}
				if (!newShed.IsEmpty)
				{
					dec.SubLocation = newShed;
				}
			}
		}
	}
}
