using System.Collections;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI;

public class SumARegisterFormLayoutProvider : ISumARegisterFormLayoutProvider
{
	public static ISumARegisterFormLayoutProvider GetLayoutProvider()
	{
		ISumARegisterFormLayoutProvider layoutProvider = null;
		var providers = ObjectFactory.Get<Hashtable>("SumARegisterFormLayoutProviders");

		if (providers is not null && !string.IsNullOrWhiteSpace(CountryCode) && CountryCode != Core.Constants.CountryCodes._TemplateCountryName_)
		{
			var objectHandle = (ObjectHandle)providers?[CountryCode];
			layoutProvider = (ISumARegisterFormLayoutProvider)objectHandle?.GetObject();
		}

		return layoutProvider ?? new SumARegisterFormLayoutProvider();
	}

	IPanelLayoutProvider ISumARegisterFormLayoutProvider.GetDetailsHeaderLayout() => GetDetailsHeaderLayoutCore();
	protected virtual IPanelLayoutProvider GetDetailsHeaderLayoutCore() => new DetailsHeaderLayout();

	IPanelLayoutProvider ISumARegisterFormLayoutProvider.GetLinesDetailsLayout() => GetLinesDetailsLayoutCore();
	protected virtual IPanelLayoutProvider GetLinesDetailsLayoutCore() => new LinesDetailsLayout();

	static string CountryCode => GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
}
