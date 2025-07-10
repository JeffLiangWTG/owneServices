using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public class ExitControlAdditionalInfoLookups : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoLookups
	{
		public ExitControlAdditionalInfoLookups(ExitControlAdditionalInfo parent)
			: base(parent)
		{
		}

		public override ICollection CodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory
					, Parent.ExitConsignment.Header.CountryCode
					, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_AI44X
					, ZDateTime.Today);

		new ExitControlAdditionalInfo Parent => (ExitControlAdditionalInfo)base.Parent;
	}
}
