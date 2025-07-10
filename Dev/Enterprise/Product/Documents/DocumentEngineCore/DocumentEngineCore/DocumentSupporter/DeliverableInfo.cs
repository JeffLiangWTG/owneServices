using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentEngineCore.DocumentSupport
{
	public class DeliverableInfo
	{
		public DeliverableInfo(ZString deliverableName, ZString documentType = default, IStmMenuItem menuItem = default, IOrgHeader orgHeader = default)
		{
			DeliverableName = deliverableName;
			DocumentType = documentType;
			MenuItem = menuItem;
			OrgHeader = orgHeader;
		}

		/// <summary>
		/// A description of this Deliverable object e.g. Report Name/title
		/// </summary>
		public ZString DeliverableName { get; }

		public ZString DocumentType { get; }

		public IStmMenuItem MenuItem { get; }

		public IOrgHeader OrgHeader { get; }
	}
}
