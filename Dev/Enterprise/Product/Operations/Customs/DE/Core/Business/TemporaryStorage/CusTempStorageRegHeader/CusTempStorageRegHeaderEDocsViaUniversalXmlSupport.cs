using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CusTempStorageRegHeaderEDocsViaUniversalXmlSupport : IEDocsViaUniversalXmlSupport
	{
		public ZString ExpectedCodeFormat => "ATB000000000000000001";

		public ZString ExampleCodeFormat => "ATB150008890220205866";

		public BusinessObject LoadBusinessObjectFromCode(BusinessObjectFactory factory, ZString code)
		{
			var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, code);
			query.AddToFilter(CusTempStorageRegHeaderSchema.SRH_AppCode, TemporaryStorageApplicationCodeList.Codes.SumA);
			return factory.Load<CusTempStorageRegHeader>(query).OrderBy(x => x.SRH_SystemCreateTimeUtc).FirstOrDefault();
		}
	}
}
