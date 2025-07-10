using System.Collections;
using CargoWise.Application;
using CargoWise.Types;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class ABLEntryNumProvider
	{
		public ABLEntryNumProvider(ZString countryCode)
		{
			CountryCode = countryCode;
		}

		public ZString CountryCode { get; }

		public static ABLEntryNumProvider GetByCountryCode(ZString countryCode)
		{
			ABLEntryNumProvider result = null;
			if (!countryCode.IsEmpty)
			{
				var types = ObjectFactory.Get<Hashtable>("ABLEntryNumProviders");
				var objectHandle = (ObjectHandle)types[countryCode.ToString()];
				result = (ABLEntryNumProvider)objectHandle?.GetObject(countryCode);
			}
			if (result == null)
			{
				result = new ABLEntryNumProvider(countryCode);
			}
			return result;
		}

		public virtual ABLEntryNumValidation GetNewValidation(ABLEntryNum entryNumber) => new ABLEntryNumValidation(entryNumber);
	}
}
