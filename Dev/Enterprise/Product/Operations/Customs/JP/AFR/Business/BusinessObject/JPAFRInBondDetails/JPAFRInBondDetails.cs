using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.JP.AFR.Business
{
	[SingleObjectAroundARow]
	public class JPAFRInBondDetails : AutoJPAFRInBondDetails
	{
		public JPAFRInBondDetails(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Override Properties
		[RelatedBusinessObject("Header")]
		public override ZGuid JPI_JPH_Header
		{
			get { return base.JPI_JPH_Header; }
			set { base.JPI_JPH_Header = value; }
		}

		public JPAFRHeader Header
		{
			get { return Factory.Load<JPAFRHeader>(JPI_JPH_Header); }
		}

		[List(nameof(Lookups) + "." + nameof(JPAFRInBondDetailsLookups.TemporaryLandingReasonCodeList))]
		public override ZString JPI_TemporaryLandingReason
		{
			get { return base.JPI_TemporaryLandingReason; }
			set { base.JPI_TemporaryLandingReason = value; }
		}

		#endregion
	}
}
