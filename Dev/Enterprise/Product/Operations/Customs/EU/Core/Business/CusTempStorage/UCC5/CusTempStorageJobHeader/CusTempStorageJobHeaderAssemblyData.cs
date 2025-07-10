using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageJobHeaderAssemblyData),
	Enterprise.Core.Constants.DocManagerCodes.TempStorageHeader)]

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class CusTempStorageJobHeaderAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType => new CusTempStorageJobHeaderTypeDecider().GetTypeForCountryCode(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		protected override Type CollectionType => null;

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("7F2A8BA6-F2B1-41E7-BF58-FB0C7A7D3974", "Temporary Storage");
	}
}
