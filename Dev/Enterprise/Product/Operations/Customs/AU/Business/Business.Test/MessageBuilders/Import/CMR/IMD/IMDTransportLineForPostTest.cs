using CargoWise.Types;
using Enterprise.Edifact.D99B.Messages.CUSDEC;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class IMDTransportLineForPostTest : IMDTransportLineAbstractTest
	{
		public override void TestPopulate()
		{
			ZString result = GetPopulatedMessage();
			AssertEquals("Details For Packs", true, result.Contains("LIN+1+I'PAC+302+1'PAC+2+2'PAC+3+3'"));
			AssertEquals("Does not have cargo type", false, result.Contains("PAC+++MAI:67:95'"));
		}

		public void TestPostGroup10WithNoteFields()
		{
			package1.CW_MarksAndNos = "Container Marks and Numbers";
			package2.CW_MarksAndNos = "Warehouse Marks and Numbers";
			package3.CW_MarksAndNos = "Container Marks and Numbers for Container 3";

			ZString result = GetPopulatedMessage();
			AssertEquals("Details For Packs", true, result.Contains("LIN+1+I'PAC+302+1'PAC+2+2'PAC+3+3'PCI+28+CONTAINER MARKS AND NUMBERS WAREHOU:SE MARKS AND NUMBERS CONTAINER MARK:S AND NUMBERS FOR CONTAINER 3'"));
			AssertEquals("Does not have cargo type", false, result.Contains("PAC+++MAI:67:95'"));
		}

		protected override IMDTransportLine GetTransportLineToTest => new IMDTransportLineForPost(testDec, new SegmentGroup10().Group21.InstantiateAChildAndAddItToChildrenCollection());

		protected override string DeclarationTransportMode => Core.Constants.TransportModes.Mail;

		protected override void SetupDeclarationWithMarksAndNumbers()
		{
			testDec.Packages[0].CW_MarksAndNos = MarksAndNumbers;
		}
	}
}
