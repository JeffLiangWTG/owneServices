
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.UPE.Business.CMR
{
	public class UPESACDecider : SACDecider
	{
		public UPESACDecider(UPECusHAWB cusHAWB)
			: base(cusHAWB.Factory, cusHAWB.GoodsValueInAUD, cusHAWB.CS_GoodsDescription)
		{
			this.CusHAWB = cusHAWB;
		}

		public override ZBool IsValidForSAC
		{
			get
			{
				return !Screening.UPSStopPhrasesFoundInGoodsDescription.Any() &&
					!Screening.QuarantineStopPhrasesFoundInGoodsDescription.Any() &&
					base.IsValidForSAC;
			}
		}

		#region Implementation

		readonly UPECusHAWB CusHAWB;

		UPEScreening Screening
		{
			get
			{
				if (fScreening == null)
				{
					fScreening = new UPEScreening(CusHAWB);
				}
				return fScreening;
			}
		}
		UPEScreening fScreening;

		#endregion
	}
}
