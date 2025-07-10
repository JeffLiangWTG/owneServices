using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.MasterFiles.Organisations.EDIAccountVerificationStatus
{
	public class EdiAccountVerificationStatus : NonPersistentBusinessObject
	{
		public EdiAccountVerificationStatus(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public ZString ContactRelationshipStatus { get; set; }

		public ZString Product { get; set; }

		public ZString LicenceType { get; set; }

		public ZString ServerCode { get; set; }

		public ZBool IsActive { get; set; }

		public ZString UserID { get; set; }

		public ZString FullName { get; set; }

		public ZString Email { get; set; }
	}
}
