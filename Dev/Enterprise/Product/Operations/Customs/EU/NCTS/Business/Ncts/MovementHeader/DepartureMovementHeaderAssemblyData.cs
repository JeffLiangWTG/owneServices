using System;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(Enterprise.Customs.EU.NCTS.Business.DepartureMovementHeaderAssemblyData),
	Enterprise.Core.Constants.DocManagerCodes.NctsMoveHeader
	)]

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class DepartureMovementHeaderAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(NctsDepartureMovementHeader);

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		protected override Type CollectionType => typeof(NctsDepartureMovementHeaderCollection<>);

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("28E3AF4D-D81F-47FF-91AD-2298B9C89DEC", "Movement");
	}
}
