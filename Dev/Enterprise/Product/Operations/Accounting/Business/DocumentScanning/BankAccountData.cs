using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(BankAccountData),
	Enterprise.Core.Constants.DocManagerCodes.BankAccount)]

namespace Enterprise.Accounting.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	public class BankAccountData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(AccBankAccount); } }
		protected override Type CollectionType
		{
			get { return typeof(AccBankAccountCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new AccBankAccountCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.AccBankAccount; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("a07b4c26-27cc-451a-94b1-85b2d6637f5a", "Bank Account"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
