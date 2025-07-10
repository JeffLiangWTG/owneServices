using System;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CusTempStorageRegPremisesAssemblyData),
	Enterprise.Core.Constants.DocManagerCodes.TempStorageRegPremises)]

namespace Enterprise.Customs.EU.TemporaryStorage.Business
{
	public class CusTempStorageRegPremisesAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(CusTempStorageRegPremises);

		protected override Type CollectionType => null;

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("E4652C1D-AA27-41F1-9D42-E5ECAF2E054F", "Temporary Storage Premises");
	}
}
