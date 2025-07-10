using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(ChequeBookData),
	Enterprise.Core.Constants.DocManagerCodes.ChequeBook)]

namespace Enterprise.Accounting.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	public class ChequeBookData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(AccChequeBook); } }
		protected override Type CollectionType
		{
			get { return typeof(AccChequeBookCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new AccChequeBookCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.AccChequeBook; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("1610dcff-4cee-4ea8-bf17-862d2387b19b", "Cheque Book"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
