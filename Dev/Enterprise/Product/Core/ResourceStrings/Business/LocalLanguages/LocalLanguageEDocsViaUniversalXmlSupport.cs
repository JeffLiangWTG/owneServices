using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

namespace Enterprise.ResourceStrings.Business
{
	public class LocalLanguageEDocsViaUniversalXmlSupport : IEDocsViaUniversalXmlSupport
	{
		public BusinessObject LoadBusinessObjectFromCode(BusinessObjectFactory factory, ZString code)
		{
			return LanguageHelper.GetCustomLanguageByLanguageCode(code) as BusinessObject;
		}

		public ZString ExpectedCodeFormat { get; } = "Code-CountryCode";
		public ZString ExampleCodeFormat { get; } = "AR-AE";
	}
}
