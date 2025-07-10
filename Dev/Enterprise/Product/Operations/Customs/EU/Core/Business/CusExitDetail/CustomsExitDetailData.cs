using System;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CustomsExitDetailData),
	Enterprise.Core.Constants.DocManagerCodes.CustomsExitDetail)]

namespace Enterprise.Customs.EU.Business
{
	public class CustomsExitDetailData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(CusExitDetail);
		protected override Type CollectionType => null;
		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;
		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("E04A3463-4BC4-42C9-A675-CCC96774C84C", "Exit Movement");
	}
}
