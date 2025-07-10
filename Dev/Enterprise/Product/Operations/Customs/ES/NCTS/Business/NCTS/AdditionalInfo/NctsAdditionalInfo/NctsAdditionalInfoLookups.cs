using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsAdditionalInfoLookups : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoLookups
	{
		public NctsAdditionalInfoLookups(NctsAdditionalInfo parent) : base(parent)
		{
		}

		const string NctsAdditionalInfoCodeType = "AINN";

		protected new NctsAdditionalInfo Parent => (NctsAdditionalInfo)base.Parent;

		public override ICollection CodeList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Spain, NctsAdditionalInfoCodeType, ZDateTime.Now);
	}
}
