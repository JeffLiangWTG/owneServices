using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(InvoiceTaxMessageData),
	Enterprise.Core.Constants.DocManagerCodes.InvoiceTaxMessage)]

namespace Enterprise.Accounting.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	public class InvoiceTaxMessageData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(AccInvMsg); } }
		protected override Type CollectionType
		{
			get { return typeof(AccInvMsgCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new AccInvMsgCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.AccInvMsg; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("a697ddb2-431f-43dd-9227-d3bf1d35d2e8", "Invoice Tax Message"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
