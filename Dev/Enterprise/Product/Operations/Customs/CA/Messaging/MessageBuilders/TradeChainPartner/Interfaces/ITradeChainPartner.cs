using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CA.Messaging
{
	public interface ITradeChainPartner : ICAEDIFACTMessageAttachee
	{
		ZString MessageNumber { get; }

		#region Fields for CST
		bool IsVendor { get; }
		ZString ApplicationImporterNumber { get; }
		ZString DivisionImporterNumber { get; }
		#endregion

		#region Fields for DTM
		ZDateTime TransactionDate { get; }
		#endregion

		#region Group7

		#region RFF
		ZString OrgCodeType { get; }
		ZString ReferenceIdentifier { get; }
		#endregion

		#region Group 10
		#region NAD
		IDocAddress VendorOrConsigneeAddress { get; }
		#endregion
		#endregion

		#endregion
	}
}
