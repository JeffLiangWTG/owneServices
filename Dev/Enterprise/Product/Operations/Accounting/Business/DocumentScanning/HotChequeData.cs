using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(HotChequeData),
	Enterprise.Core.Constants.DocManagerCodes.HotCheque)]

namespace Enterprise.Accounting.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.Accounting.Business.ARAP.HotCheque;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	public class HotChequeData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(AccHotCheque); } }
		protected override Type CollectionType
		{
			get { return typeof(AccHotChequeCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new AccHotChequeCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.AccHotCheque; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("8835e147-7a2e-443f-a1f7-0e177feb7bc5", "Hot Cheque"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
