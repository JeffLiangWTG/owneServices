using Enterprise.Customs.EU.Integration.SadH
;

namespace Enterprise.Customs.GB.Business.Messaging
{
	class Seal : ISeal
	{
		public Seal(string sealId)
		{
			this.sealId = sealId;
		}
		readonly string sealId;

		#region ISeal Members

		CargoWise.Types.ZString ISeal.SealId
		{
			get { return sealId; }
		}

		#endregion
	}
}
