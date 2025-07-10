using System;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeaderAssemblyData),
	Enterprise.Core.Constants.DocManagerCodes.AsycudaManifest)]
namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaManifestHeaderAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType
		{
			get { return typeof(AsycudaManifestHeader); }
		}

		protected override Type CollectionType
		{
			get { return null; }
		}

		public override string ReferenceType
		{
			get { return Enterprise.Core.Constants.ReferenceTypes.SupplyChainLogistics; }
		}

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("09974092-9872-400E-BF08-F8864A59F65F", "Manifest");

		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new AsycudaManifestHeaderAssemblyDataEDocsViaUniversalXmlSupport();
	}
}
