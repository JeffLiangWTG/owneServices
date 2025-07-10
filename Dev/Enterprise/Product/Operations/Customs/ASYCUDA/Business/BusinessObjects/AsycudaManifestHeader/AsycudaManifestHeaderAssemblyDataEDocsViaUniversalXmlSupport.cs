using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaManifestHeaderAssemblyDataEDocsViaUniversalXmlSupport : IEDocsViaUniversalXmlSupport
	{
		public BusinessObject LoadBusinessObjectFromCode(BusinessObjectFactory factory, ZString code)
		{
			return factory.LoadTop1<AsycudaManifestHeader>(new ZQuery(AsycudaManifestHeaderSchema.AMA_JobReference, code));
		}

		public ZString ExpectedCodeFormat => "JobReference";
		public ZString ExampleCodeFormat => "ABC";
	}
}
