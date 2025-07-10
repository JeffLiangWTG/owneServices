using System;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CusExitReportAssemblyData),
	Enterprise.Core.Constants.DocManagerCodes.CustomsExitReport)]

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitReportAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(CusExitReport);

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		protected override Type CollectionType => typeof(CusExitReportCollection<CusExitReport>);

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("8b73a3df-84f9-47c4-ab82-d9a062c3889e", "Customs Exit Report");
	}
}
