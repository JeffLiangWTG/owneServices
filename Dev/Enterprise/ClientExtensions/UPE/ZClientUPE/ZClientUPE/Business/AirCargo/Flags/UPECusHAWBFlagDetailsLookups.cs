
using CargoWise.EntityFramework;

namespace Enterprise.Client.UPE.Business
{
	public class UPECusHAWBFlagDetailsLookups : ZLookups
	{
		public UPECusHAWBFlagDetailsLookups(UPECusHAWBFlagDetails flagDetails)
			: base(flagDetails)
		{
		}

		public AuthReceivedByCodeDescriptionPairList AuthReceivedByList
		{
			get
			{
				if (fAuthReceivedByList == null)
				{
					fAuthReceivedByList = new AuthReceivedByCodeDescriptionPairList();
				}
				return fAuthReceivedByList;
			}
		}

		AuthReceivedByCodeDescriptionPairList fAuthReceivedByList;
	}
}
