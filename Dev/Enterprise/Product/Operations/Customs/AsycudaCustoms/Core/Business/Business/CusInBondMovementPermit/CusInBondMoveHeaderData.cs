using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(Enterprise.Customs.AsycudaCustoms.Business.CusInBondMoveHeaderData),
	Enterprise.Core.Constants.DocManagerCodes.AsycudaCusInBondMoveHeader)]

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusInBondMoveHeaderData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(CusInBondMoveHeader);

		protected override Type CollectionType => typeof(CusInBondMoveHeaderCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory) => new CusInBondMoveHeaderCollection(factory);

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.EntryHeader;

		public override string ReferenceType => Core.Constants.ReferenceTypes.ClientSupplierRelationship;

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("1b38faeb-8bef-43de-bc48-7b95d9ed962d", "Transit Permits");

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
