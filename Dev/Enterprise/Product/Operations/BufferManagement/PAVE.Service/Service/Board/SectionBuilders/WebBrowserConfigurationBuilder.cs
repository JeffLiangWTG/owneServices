using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Business;
using Enterprise.PAVE.MENT.Business;

namespace Enterprise.BufferManagement.Service
{
	internal class WebBrowserConfigurationBuilder : SectionConfigurationBuilder
	{
		WebBrowserSectionConfiguration SectionConfig => (WebBrowserSectionConfiguration)Section.Configuration;

		internal WebBrowserConfigurationBuilder(BMBoardSection section)
			: base(section, SectionType.WebBrowser)
		{
		}

		protected override ISectionConfiguration BuildCore()
		{
			return new WebBrowserSectionConfigurationDTO()
			{
				SectionPK = Section.PK.ToGuid(),
				Name = SectionConfig.SectionName,
				Layout = CreateLayout(),
				URL = SectionConfig.URL
			};
		}
	}
}

