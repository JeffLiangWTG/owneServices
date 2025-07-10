using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class CustomsArchiveableBusinessObjectProviderAssistant : Customs.Business.ArchiveManager.CustomsArchiveableBusinessObjectProviderAssistant
	{
		public override string CountryCode
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}

		public override IArchiveableBusinessObject LoadArchiveableBusinessObject(BusinessObject businessObjectLoadedByProvider)
		{
			Argument.NotNull(businessObjectLoadedByProvider, "businessObjectLoadedByProvider");

			if (businessObjectLoadedByProvider is Integration.Customs.AU.IJobDeclaration)
			{
				return new ArchiveableJobDeclaration((Integration.Customs.AU.IJobDeclaration)businessObjectLoadedByProvider);
			}
			else
			{
				throw new ArgumentException("businessObjectLoadedByProvider type expected to be Enterprise.Integration.Customs.AU.IJobDeclaration, but was " + businessObjectLoadedByProvider.GetType().ToString());
			}
		}
	}
}
