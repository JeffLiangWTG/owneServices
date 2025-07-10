using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocSalesCall))]
	public class DocSalesCallTest : DocumentWrapperTestCase
	{
		public void TestStaffCoordinator()
		{
			var salesCall = Factory.New<OrgSalesCall>();
			var doc = DocSalesCall.New(salesCall, Factory);
			salesCall.OQ_GS_NKSalesRep = "";
			AssertEquals("", doc.StaffCoordinator);

			var salesRep = Factory.New<GlbStaff>();
			salesRep.GS_Code = "ADL";
			salesRep.GS_FullName = "Andrew";

			salesCall.OQ_GS_NKSalesRep = "ADL";
			AssertEquals("Andrew", doc.StaffCoordinator);
		}

		public void TestAttendees()
		{
			var salesCall = Factory.New<OrgSalesCall>();
			var doc = DocSalesCall.New(salesCall, Factory);
			AssertEquals("", doc.OtherAttendee);

			var contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "Andrew";
			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "Samuel";

			salesCall.AdditionalAttendeesContact.AddNew().O6_AttendeeID = contact.PK;
			salesCall.AdditionalAttendeesStaff.AddNew().O6_AttendeeID = staff.PK;
			salesCall.AdditionalAttendeesOther.AddNew().O6_AttendeeName = "Richard";

			AssertMultilineASCIIEquals("Should be list of staff and other attendees",
@"Andrew
Samuel
Richard",
				doc.Attendees);
		}

		public void TestPurposeCode()
		{
			var salesCall = Factory.New<OrgSalesCall>();
			var doc = DocSalesCall.New(salesCall, Factory);
			salesCall.OQ_Category = "cg1";
			AssertEquals("cg1", doc.PurposeCode);
		}

		public void TestMethodCode()
		{
			var salesCall = Factory.New<OrgSalesCall>();
			var doc = DocSalesCall.New(salesCall, Factory);
			salesCall.OQ_TypeOfCall = "tc1";
			AssertEquals("tc1", doc.MethodCode);
		}

		public void TestSubject()
		{
			var salesCall = Factory.New<OrgSalesCall>();
			var doc = DocSalesCall.New(salesCall, Factory);
			salesCall.OQ_CallSummary = "callSummary1";
			AssertEquals("callSummary1", doc.CommunicationSubject);
		}

		public void TestClientOrg()
		{
			var salesCall = Factory.New<OrgSalesCall>();
			var doc = DocSalesCall.New(salesCall, Factory);
			AssertNull(doc.ClientOrg);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "A1B2C3ZY2Z1X";
			salesCall.OQ_OH = org.PK;
			AssertType(typeof(DocOrganisation), doc.ClientOrg);
			AssertEquals(org.OH_Code, doc.ClientOrg.Code);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocSalesCall.New(Factory.New<OrgSalesCall>(), Factory)
			};
		}

		#region Discussed Trade Lanes Is Correct

		public void TestDiscussedTradeLanes()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "XXXXXX";

			var comm = Factory.NewWithValidTestData<OrgSalesCall>();
			comm.OQ_OH = org.PK;

			var tradeLane1 = CreateTradeLane(org, Core.Constants.Sales.Mode.Import, "GBLON");
			var tradeLane2 = CreateTradeLane(org, Core.Constants.Sales.Mode.Export, "USLAX");

			_ = CreateSalesValueAssociationPivot(comm, tradeLane1);
			_ = CreateSalesValueAssociationPivot(comm, tradeLane2);

			var testWrapper = DocSalesCall.New(comm, Factory);
			var discussedTradeLanes = testWrapper.DiscussedTradeLanes.Split('\n');
			Array.Sort(discussedTradeLanes);
			AssertEquals(2, discussedTradeLanes.Length);
			AssertEquals("GBLON (Import)", discussedTradeLanes[0]);
			AssertEquals("USLAX (Export)", discussedTradeLanes[1]);
		}

		#region Helpers

		OrgSales CreateTradeLane(OrgHeader org, ZString mode, string location)
		{
			var newTradeLane = Factory.NewWithValidTestData<OrgSales>();
			newTradeLane.OW_OH_Primary = org.PK;

			if (mode == Core.Constants.Sales.Mode.Import)
			{
				newTradeLane.OW_OH_Buyer = org.PK;
				newTradeLane.OW_OriginID = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, location).PK;
				newTradeLane.OW_OriginTableCode = RefUNLOCOSchema.Constants.Prefix;
			}
			else
			{
				newTradeLane.OW_OH_Supplier = org.PK;
				newTradeLane.OW_DestinationID = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, location).PK;
				newTradeLane.OW_DestinationTableCode = RefUNLOCOSchema.Constants.Prefix;
			}
			return newTradeLane;
		}

		OrgSalesValueAssociationPivot CreateSalesValueAssociationPivot(BusinessObject activity, BusinessObject trade)
		{
			var pivot = Factory.NewWithValidTestData<OrgSalesValueAssociationPivot>();
			pivot.SVP_ActivityTableCode = activity.TablePrefix;
			pivot.SVP_ActivityId = activity.PK;
			pivot.SVP_TradeTableCode = trade.TablePrefix;
			pivot.SVP_TradeId = trade.PK;
			return pivot;
		}

		#endregion

		#endregion
	}
}
