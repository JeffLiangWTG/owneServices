using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeaderAssemblyData),
	Enterprise.Core.Constants.DocManagerCodes.TempStorageHeaderUCC6)]
namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageHeaderAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType => new TemporaryStorageHeaderTypeDecider().GetTypeForCountryCode(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		public override string ReferenceType => Enterprise.Core.Constants.ReferenceTypes.SupplyChainLogistics;

		protected override Type CollectionType => null;

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("7F2A8BA6-F2B1-41E7-B758-FB0C7A7D3974", "Temporary Storage UCC6");
	}
}
