using System;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(EDIMessageData),
	Enterprise.Core.Constants.DocManagerCodes.EDIMessage)]

namespace Enterprise.Messaging.Business
{
	public class EDIMessageData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(EDIMessage); } }
		protected override Type CollectionType
		{
			get { return null; }
		}
		public override string ReferenceType { get { return Enterprise.Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("ab3bb282-e3e0-4553-af76-76188134b83a", "EDI Message"); } }
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.Messaging.EDIMessage; } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}