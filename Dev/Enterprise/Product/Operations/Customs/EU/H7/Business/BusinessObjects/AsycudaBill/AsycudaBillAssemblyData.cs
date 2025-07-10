using System;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using static Enterprise.Core.Constants;

[assembly: AssemblyDataProvider(
	typeof(Enterprise.Customs.EU.H7.Business.AsycudaBillAssemblyData),
	Enterprise.Core.Constants.DocManagerCodes.AsycudaBill)]

namespace Enterprise.Customs.EU.H7.Business
{
	public class AsycudaBillAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(AsycudaBill);

		public override string ReferenceType => ReferenceTypes.SupplyChainLogistics;

		protected override Type CollectionType => null;

		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new AsycudaBillEDocsViaUniversalXmlSupport();
	}
}
