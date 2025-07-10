using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CusStatementHeaderAssemblyData),
	Enterprise.Core.Constants.DocManagerCodes.CusStatementHeader,
	Country = "FR")]

namespace Enterprise.Customs.FR.Business.CusStatement
{
	class CusStatementHeaderAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(Customs.Business.BaseCusStatementHeader);
		protected override Type CollectionType => typeof(CusStatementHeaderCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new CusStatementHeaderCollection(factory);
		}

		public override ModuleIdentifier ModuleID { get { return ModuleIDs.Customs.EU.FR.CustomsStatement; } }

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;
		public override bool IsAllowedForUnallocatedeDocs => true;
		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("C0283C4A-06DB-4D86-818B-455C2072AC45", "Statement Header");
	}
}
