using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(Enterprise.Customs.DE.Business.CusReconDeclarationData),
	Enterprise.Core.Constants.DocManagerCodes.GermanyMonthlyClosing)]

namespace Enterprise.Customs.DE.Business
{
	public class CusReconDeclarationData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(CusReconDeclaration);

		protected override Type CollectionType => typeof(CusReconDeclarationCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new CusReconDeclarationCollection(factory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.EU.DE.MonthlyClosing;

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("B8AFD79A-1C19-4D5F-A7DD-304C7A767402", "Monthly Closing");

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
