using System;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.Malaysia
{
	public class MalaysiaEInvoiceDocType_RegistryDescriptor : CountrySpecificDefaultRegistryDescriptor<string>
	{
		protected override Func<IDefaultValuesForCountrySpecificRegistryItems, string> DefaultValueGetter => (IDefaultValuesForCountrySpecificRegistryItems defaultRegistryValuesForCountry) => Core.Constants.RefDocTypes.Invoice;

		protected override MultilingualString GetDefaultTypeCaptionCore()
		{
			return ResString.GetMultilingualString("FB4CABD0-0987-415B-BA19-CD1DFE7785EA", "Electronic Invoice Document Type");
		}
	}
}
