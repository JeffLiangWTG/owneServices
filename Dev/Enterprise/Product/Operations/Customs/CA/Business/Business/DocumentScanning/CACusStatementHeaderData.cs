using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CACusStatementHeaderData),
	Enterprise.Core.Constants.DocManagerCodes.CusStatementHeader,
	Country = "CA")]

namespace Enterprise.Customs.CA.Business
{
	using System;

	class CACusStatementHeaderData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(Customs.Business.BaseCusStatementHeader);
		protected override Type CollectionType => null;
		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;
		public override bool IsAllowedForUnallocatedeDocs => true;
		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("1c7b47bf-5276-4ee6-8fee-a7ac2715b334", "DN/SOA Statements");
	}
}
