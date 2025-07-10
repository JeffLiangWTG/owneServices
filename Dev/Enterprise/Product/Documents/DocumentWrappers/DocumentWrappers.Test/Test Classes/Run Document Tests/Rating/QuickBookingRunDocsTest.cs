using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class QuickBookingRunDocsTest : BaseRunDocumentsTest
	{
		public QuickBookingRunDocsTest() { }

		public override BusinessObject GetBusinessObject
		{
			get
			{
				ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
				return QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			}
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.QuotedBooking; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}

		[ExpectNoExceptions]
		public void TestBookingConfirmation()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Booking Confirmation");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestRequestForService()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Request For Service");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestAuthorisationForService()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Authorization for Service");
			RunDocument();
		}

		#region Implementation

		ZQuery fFilterForMenuItem;

		#endregion
	}
}
