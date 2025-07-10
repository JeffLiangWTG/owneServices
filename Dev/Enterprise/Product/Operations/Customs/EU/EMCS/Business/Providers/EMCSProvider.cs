using System.Collections;
using CargoWise.Application;
using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSProvider
	{
		protected EMCSProvider(ZString countryCode)
		{
			CountryCode = countryCode;
		}

		public readonly ZString CountryCode;

		public static EMCSProvider GetByCountryCode(string countryCode)
		{
			EMCSProvider result = null;
			if (!string.IsNullOrEmpty(countryCode))
			{
				var types = ObjectFactory.Get<Hashtable>("EMCSProviders");
				var objectHandle = (ObjectHandle)types[countryCode];
				result = (EMCSProvider)objectHandle?.GetObject(countryCode);
			}
			if (result == null)
			{
				result = new EMCSProvider(countryCode);
			}
			return result;
		}

		public EMCSJobComInvoiceLineValidation GetNewValidation(EMCSJobComInvoiceLine invLine) => GetNewValidationCore(invLine);
		protected virtual EMCSJobComInvoiceLineValidation GetNewValidationCore(EMCSJobComInvoiceLine invLine) => new EMCSJobComInvoiceLineValidation(invLine);

		public EMCSJobComInvoiceLineLookups GetNewLookups(EMCSJobComInvoiceLine invLine) => GetNewLookupsCore(invLine);
		protected virtual EMCSJobComInvoiceLineLookups GetNewLookupsCore(EMCSJobComInvoiceLine invLine) => new EMCSJobComInvoiceLineLookups(invLine);

		public EMCSAddInfoJobComInvoiceLineValidation GetNewAddInfoValidation(EMCSAddInfoJobComInvoiceLine invLine) => GetNewAddInfoValidationCore(invLine);
		protected virtual EMCSAddInfoJobComInvoiceLineValidation GetNewAddInfoValidationCore(EMCSAddInfoJobComInvoiceLine invLine) => new EMCSAddInfoJobComInvoiceLineValidation(invLine);

		public EMCSAddInfoJobComInvoiceLineLookups GetNewAddInfoLookups(EMCSAddInfoJobComInvoiceLine invLine) => GetNewAddInfoLookupsCore(invLine);
		protected virtual EMCSAddInfoJobComInvoiceLineLookups GetNewAddInfoLookupsCore(EMCSAddInfoJobComInvoiceLine invLine) => new EMCSAddInfoJobComInvoiceLineLookups(invLine);

		public EMCSJobComInvoiceHeaderValidation GetNewInvoiceHeaderValidation(EMCSJobComInvoiceHeader invHeader) => GetNewInvoiceHeaderValidationCore(invHeader);
		protected virtual EMCSJobComInvoiceHeaderValidation GetNewInvoiceHeaderValidationCore(EMCSJobComInvoiceHeader invHeader) => new EMCSJobComInvoiceHeaderValidation(invHeader);
	}
}
