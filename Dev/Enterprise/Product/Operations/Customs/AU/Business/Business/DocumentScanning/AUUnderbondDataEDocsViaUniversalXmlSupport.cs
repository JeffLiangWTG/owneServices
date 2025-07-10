using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUUnderbondDataEDocsViaUniversalXmlSupport : IEDocsViaUniversalXmlSupport
	{
		public BusinessObject LoadBusinessObjectFromCode(BusinessObjectFactory factory, ZString code)
		{
			return factory.LoadTop1<CusUnderbond>(new ZQuery(CusUnderbondSchema.C4_SendersMessageReference, code));
		}

		public ZString ExpectedCodeFormat => "SendersMessageReference";
		public ZString ExampleCodeFormat => "ABC";
	}
}
