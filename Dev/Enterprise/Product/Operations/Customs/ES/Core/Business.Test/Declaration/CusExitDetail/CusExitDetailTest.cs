using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(CusExitDetail))]
	class CusExitDetailTest : EnterpriseBusinessObjectTestCase
	{
		public void TestFormattedCircuit()
		{
			var exitDetail = Factory.New<CusExitDetail>();

			exitDetail.ZG_Circuit = "4";
			AssertEquals("GREEN", exitDetail.FormattedCircuit);
		}

		public void TestIESMessageInfoProvider_Broker()
		{
			CombineAssertions(() =>
			{
				var exitHeader = Factory.New<CusExitControlHeader>();
				var exitDetail = Factory.New<CusExitDetail>();
				exitDetail.CED_CEH = exitHeader.PK;
				var esMessageInfoProvider = exitDetail as IESMessageInfoProvider;
				exitHeader.CEH_GS_NKCustomsAgent = ZString.Empty;
				AssertNull("Broker is null", esMessageInfoProvider.Broker);

				var staff = Factory.New<GlbStaff>();
				staff.GS_Code = "AZM";
				exitHeader.CEH_GS_NKCustomsAgent = staff.GS_Code;
				AssertEquals("Broker is not null", staff, esMessageInfoProvider.Broker);
			});
		}

		public void TestIESMessageInfoProvider_EntryReference()
		{
			var exitDetail = Factory.New<CusExitDetail>();
			var esMessageInfoProvider = exitDetail as IESMessageInfoProvider;
			exitDetail.CED_MovementReferenceNumber = "Reference";
			AssertEquals("EntryReference has the correct value", "Reference", esMessageInfoProvider.EntryReference);
		}

		public void TestIESMessageInfoProvider_MRN()
		{
			var exitDetail = Factory.New<CusExitDetail>();
			var esMessageInfoProvider = exitDetail as IESMessageInfoProvider;
			CombineAssertions(() =>
			{
				AssertEquals("MRN has the correct value (empty when exitDetail has no mrn)", ZString.Empty, esMessageInfoProvider.MRN);

				exitDetail.CED_MovementReferenceNumber = "20ES00999830001277";
				AssertEquals("MRN has the correct value", "20ES00999830001277", esMessageInfoProvider.MRN);
			});
		}

		public void TestIESResponseBusinessObject_BranchPK()
		{
			var exitDetail = Factory.New<CusExitDetail>();
			var esResponseBusinessObject = exitDetail as IESResponseBusinessObject;

			CombineAssertions(() =>
			{
				AssertEquals("BranchPK has the correct value, empty when detail not associated to parent", ZGuid.Empty, esResponseBusinessObject.BranchPK);

				var declaration = Factory.New<JobDeclaration>();
				var exitHeader = Factory.New<CusExitControlHeader>();
				exitHeader.CEH_Parent = declaration;
				exitDetail.CED_CEH = exitHeader.PK;
				AssertEquals("BranchPK has the correct value, empty when detail not associated to parent", declaration.Branch.PK, esResponseBusinessObject.BranchPK);
			});
		}

		public void TestIESResponseBusinessObject_MessageCollection()
		{
			var exitDetail = Factory.New<CusExitDetail>();
			var esResponseBusinessObject = exitDetail as IESResponseBusinessObject;
			exitDetail.Messages.AddNew();
			AssertEquals("MessageCollection has the correct value", exitDetail.Messages, esResponseBusinessObject.MessageCollection);
		}

		public void TestReadOnlyWhenStatusChanges()
		{
			var exitDetail = Factory.New<CusExitDetail>();

			CombineAssertions(() =>
			{
				exitDetail.CED_Status = "AWR";
				AssertEquals("With status AWR exitDetail is readonly", true, exitDetail.ReadOnly);
				AssertEquals("With status AWR exitDetail's children are readonly", true, exitDetail.CusExitItems.ReadOnly);

				exitDetail.CED_Status = ZString.Empty;
				AssertEquals("With status empty exitDetail is not readonly", false, exitDetail.ReadOnly);
				AssertEquals("With status empty exitDetail's children are not readonly", false, exitDetail.CusExitItems.ReadOnly);

				exitDetail.CED_Status = "CLR";
				AssertEquals("With status CLR exitDetail is readonly", true, exitDetail.ReadOnly);
				AssertEquals("With status CLR exitDetail's children are readonly", true, exitDetail.CusExitItems.ReadOnly);

				exitDetail.CED_Status = "CLP";
				AssertEquals("With status CLP exitDetail is not readonly", false, exitDetail.ReadOnly);
				AssertEquals("With status CLP exitDetail's children are not readonly", false, exitDetail.CusExitItems.ReadOnly);

				exitDetail.CED_Status = "CDA";
				AssertEquals("With status CDA exitDetail is readonly", true, exitDetail.ReadOnly);
				AssertEquals("With status CDA exitDetail's children are readonly", true, exitDetail.CusExitItems.ReadOnly);
			});
		}

		public void TestUpdateCSVClearance()
		{
			var declaration = Factory.New<JobDeclaration>();

			var exitHeader = Factory.New<CusExitControlHeader>();
			exitHeader.CEH_Parent = declaration;
			exitHeader.CEH_ReferenceNumber = "ExitHeaderRef";
			var exitDetail = exitHeader.CusExitDetails.AddNew();
			exitDetail.CED_MovementReferenceNumber = "MRNCode";

			CombineAssertions(() =>
			{
				exitDetail.UpdateCSVClearance("+--**__aa/#€&@");
				AssertEquals("CSV clearance has not been changed when the pop up was accepted but code is not valid", ZString.Empty, exitDetail.ZG_CSVClearance);

				exitDetail.UpdateCSVClearance("CLEARANCE1234567");
				AssertEquals("CSV clearance has been changed when the pop up was accepted", "CLEARANCE1234567", exitDetail.ZG_CSVClearance);
				AssertEquals("New event in logs", "|NEW=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Exit Movement MRNCode|TYP=CSV", exitDetail.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

				exitDetail.UpdateCSVClearance("AAAAAAAAAAAAAAAA");

				//Changed to Save and Load in new Factory. Sleep(5) has intermittent failures. This new approach gives 0 failures and average time of 0.8s of execution.
				Factory.Save();
				var newFactory = Factory.CreateNewFactory();
				var newLoadedExitDetail = newFactory.Load<CusExitDetail>(exitDetail.PK);

				AssertEquals("CSV clearance has been changed when the pop up was accepted a second time", "AAAAAAAAAAAAAAAA", newLoadedExitDetail.ZG_CSVClearance);
				AssertEquals("New event in logs for second change", "|NEW=AAAAAAAAAAAAAAAA|OLD=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Exit Movement MRNCode|TYP=CSV", newLoadedExitDetail.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);
			});
		}

		public void TestIsSentOrAccepted()
		{
			var exitDetail = Factory.New<CusExitDetail>();

			CombineAssertions(() =>
			{
				exitDetail.CED_Status = "AWR";
				AssertEquals("With status AWR exitDetail is sent", true, exitDetail.IsSentOrAccepted);

				exitDetail.CED_Status = ZString.Empty;
				AssertEquals("With status empty exitDetail is not sent or accepted", false, exitDetail.IsSentOrAccepted);

				exitDetail.CED_Status = "CLR";
				AssertEquals("With status CLR exitDetail is accepted", true, exitDetail.IsSentOrAccepted);

				exitDetail.CED_Status = "CLP";
				AssertEquals("With status CLP exitDetail is not sent or accepted", false, exitDetail.IsSentOrAccepted);

				exitDetail.CED_Status = "CDA";
				AssertEquals("With status CDA exitDetail is accepted", true, exitDetail.IsSentOrAccepted);

				exitDetail.CED_Status = "ERR";
				AssertEquals("With status ERR exitDetail is not sent or accepted", false, exitDetail.IsSentOrAccepted);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var exitHeader = Factory.New<CusExitControlHeader>();
			return exitHeader.CusExitDetails.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var exitHeader = factory.NewWithValidTestData<CusExitControlHeader>();
			return exitHeader.CusExitDetails.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
	}
}
