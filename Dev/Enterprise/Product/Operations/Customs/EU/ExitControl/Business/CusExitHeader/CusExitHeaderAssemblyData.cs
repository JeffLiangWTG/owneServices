using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CusExitHeaderAssemblyData),
	Enterprise.Core.Constants.DocManagerCodes.CustomsExitHeader)]

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitHeaderAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(CusExitHeader);

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		protected override Type CollectionType => typeof(CusExitHeaderCollection<CusExitHeader>);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory) => new CusExitHeaderCollection<CusExitHeader>(factory);

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("E6889DFD-E925-440C-82A2-BD35DD9DA01D", "Customs Exit Header");
	}
}
