using System;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CusExitConsignmentAssemblyData),
	Enterprise.Core.Constants.DocManagerCodes.CustomsExitConsignment)]

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitConsignmentAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(CusExitConsignment);

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		protected override Type CollectionType => typeof(CusExitConsignmentCollection<CusExitConsignment>);

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("CEA47162-562F-49DD-A6C1-744D5540903F", "Customs Exit Consignment");
	}
}
