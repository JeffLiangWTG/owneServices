using System;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeaderAssemblyData),
	Enterprise.Core.Constants.DocManagerCodes.TempStorageRegHeader,
	Country = Enterprise.Core.Constants.CountryCodes.Spain)]

namespace Enterprise.Customs.ES.Business.CusTempStorage
{
	public class CusTempStorageRegHeaderAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader);

		protected override Type CollectionType => null;

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;
	}
}
