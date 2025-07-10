using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsPreviousProcedureMaster))]
	sealed class NctsPreviousProcedureMasterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAuthorizationNumberMaxLength()
		{
			AssertEquals(35, previousProcedureMaster.AuthorizationNumberInfo.MaxLength);
		}

		public void TestAuthorizationNumber()
		{
			previousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			var prevProcedure1 = goodsItem.PreviousProcedures.AddNew();
			var prevProcedure2 = goodsItem.PreviousProcedures.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Authorization Number 1 empty", ZString.Empty, prevProcedure1.AuthorizationNumber);
				AssertEquals("Authorization Number 2 empty", ZString.Empty, prevProcedure2.AuthorizationNumber);

				previousProcedureMaster.AuthorizationNumber = "DOCAUTHNU";
				AssertEquals("Authorization Number 1 update from Master", "DOCAUTHNU", prevProcedure1.AuthorizationNumber);
				AssertEquals("Authorization Number 2 update from Master", "DOCAUTHNU", prevProcedure2.AuthorizationNumber);
			});
		}

		public void TestAuthorizationNumberReadOnlyAndBlankWhenSubTypeIsTrue_9DEY()
		{
			previousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;
			previousProcedureMaster.SimplifiedGrantAuthorizationFlag = false;
			previousProcedureMaster.AuthorizationNumber = "Test";
			previousProcedureMaster.SimplifiedGrantAuthorizationFlag = true;
			CombineAssertions(() =>
			{
				AssertEquals("Cleared", ZString.Empty, previousProcedureMaster.AuthorizationNumber);
				AssertEquals("ReadOnly", true, previousProcedureMaster.AuthorizationNumberInfo.ReadOnly);
			});
		}

		public void TestCSI_ProcedureMaxLength()
		{
			AssertEquals(7, previousProcedureMaster.CSI_ProcedureInfo.MaxLength);
		}

		public void TestCSI_Procedure_ThatOnlyRequiresACode()
		{
			previousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;
			AssertEquals(NctsPreviousProcedureList.Codes._9DEY, goodsItem.PreviousProcedures[0].CSI_Procedure);
		}

		public void TestCSI_Procedure_FromMultipleToSingleToBlank()
		{
			previousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			goodsItem.PreviousProcedures.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Multiple Previous Procedures", 2, goodsItem.PreviousProcedures.Count);
				previousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;
				AssertEquals("Single Procedure Code", 1, goodsItem.PreviousProcedures.Count);
				previousProcedureMaster.CSI_Procedure = ZString.Empty;
				AssertEquals("No code collection empty", 0, goodsItem.PreviousProcedures.Count);
			});
		}

		public void TestCSI_Procedure_Cancellation()
		{
			previousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;
			header.OnPreviousProcedureMasterCSI_ProcedureAboutToChange += CanceledFunction;
			previousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;

			CombineAssertions(() =>
			{
				AssertEquals("Should not have set the value, as the change was canceled.", NctsPreviousProcedureList.Codes._9DEY, previousProcedureMaster.CSI_Procedure);
				header.OnPreviousProcedureMasterCSI_ProcedureAboutToChange -= CanceledFunction;
				header.OnPreviousProcedureMasterCSI_ProcedureAboutToChange += NotCanceledFunction;
				previousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
				AssertEquals("Should have set the value, as the change was not canceled.", NctsPreviousProcedureList.Codes._9DEZ, previousProcedureMaster.CSI_Procedure);
			});
			header.OnPreviousProcedureMasterCSI_ProcedureAboutToChange -= NotCanceledFunction;

			void CanceledFunction(object sender, CancelEventArgs args) => args.Cancel = true;
			void NotCanceledFunction(object sender, CancelEventArgs args) => args.Cancel = false;
		}

		public void TestCSI_Procedure_InvalidCode()
		{
			previousProcedureMaster.CSI_Procedure = "AAA";
			AssertEquals("Add PreviousProcedure for invalid CSI_Procedure", "AAA", goodsItem.PreviousProcedures[0].CSI_Procedure);
		}

		public void TestCSI_Procedure_AddPreviousDocumentToParent()
		{
			CombineAssertions(() =>
			{
				previousProcedureMaster.CSI_Procedure = "AAA";
				AssertEquals("Doesn't add PreviousDocument for invalid CSI_Procedure", 0, goodsItem.PreviousDocuments.Count);

				previousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;
				AssertEquals("Add PreviousDocument for valid CSI_Procedure", NctsPreviousProcedureList.Codes._9DEY, goodsItem.PreviousDocuments[0].CSI_Code);

				var previousDocument = goodsItem.PreviousDocuments.AddNew();
				previousDocument.CSI_Code = NctsPreviousProcedureList.Codes._N337;

				previousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
				var newPreviousDocument = goodsItem.PreviousDocuments.Cast<NctsPreviousDocument>().Single();
				AssertNotEquals("Readd PreviousDocument", previousDocument.PK, newPreviousDocument.PK);
			});
		}

		public void TestCSI_Procedure_DeletePreviousDocumentFromParent()
		{
			var previousDocument = goodsItem.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = NctsPreviousProcedureList.Codes._N337;
			var previousDocument2 = goodsItem.PreviousDocuments.AddNew();
			previousDocument2.CSI_Code = NctsPreviousProcedureList.Codes._9DEY;
			var previousDocument3 = goodsItem.PreviousDocuments.AddNew();
			previousDocument3.CSI_Code = "AAA";

			CombineAssertions(() =>
			{
				previousProcedureMaster.CSI_Procedure = "BBB";
				AssertContainsExactElementsInAnyOrder("Doesn't delete PreviousProcedures for invalid CSI_Procedure", new string[] { NctsPreviousProcedureList.Codes._N337, NctsPreviousProcedureList.Codes._9DEY, "AAA" },
					goodsItem.PreviousDocuments.Cast<NctsPreviousDocument>().Select(x => x.CSI_Code));

				previousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
				AssertContainsExactElementsInAnyOrder("Delete all PreviousProcedures with valid CSI_Procedure codes", new string[] { NctsPreviousProcedureList.Codes._9DEZ, "AAA" }, goodsItem.PreviousDocuments.Cast<NctsPreviousDocument>().Select(x => x.CSI_Code));
			});
		}

		public void TestCSI_ReferenceNumber2_MaxLength()
		{
			AssertEquals(35, previousProcedureMaster.CSI_ReferenceNumber2Info.MaxLength);
		}

		public void TestCSI_ReferenceNumber2()
		{
			previousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			var prevProcedure1 = goodsItem.PreviousProcedures.AddNew();
			var prevProcedure2 = goodsItem.PreviousProcedures.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Reference Number 1 empty", ZString.Empty, prevProcedure1.CSI_ReferenceNumber2);
				AssertEquals("Reference Number 2 empty", ZString.Empty, prevProcedure2.CSI_ReferenceNumber2);

				previousProcedureMaster.CSI_ReferenceNumber2 = "DOCLOCREF";
				AssertEquals("Reference Number 1 Updated", "DOCLOCREF", prevProcedure1.CSI_ReferenceNumber2);
				AssertEquals("Reference Number 2 Updated", "DOCLOCREF", prevProcedure2.CSI_ReferenceNumber2);
			});
		}

		public void TestCSI_CustomsOffice()
		{
			previousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			var prevProcedure1 = goodsItem.PreviousProcedures.AddNew();
			var prevProcedure2 = goodsItem.PreviousProcedures.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals(10, previousProcedureMaster.CSI_CustomsOfficeInfo.MaxLength);
				AssertEquals("Customs Office 1 empty", ZString.Empty, prevProcedure1.CSI_CustomsOffice);
				AssertEquals("Customs Office 2 empty", ZString.Empty, prevProcedure2.CSI_CustomsOffice);

				previousProcedureMaster.CSI_CustomsOffice = "DE00567";
				AssertEquals("Customs Office 1 Updated", "DE00567", prevProcedure1.CSI_CustomsOffice);
				AssertEquals("Customs Office 2 Updated", "DE00567", prevProcedure2.CSI_CustomsOffice);
			});
		}

		public void TestSimplifiedGrantAuthorizationFlag()
		{
			previousProcedureMaster.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			var prevProcedure1 = goodsItem.PreviousProcedures.AddNew();
			var prevProcedure2 = goodsItem.PreviousProcedures.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Simplified Grant Authorization Flag 1 is false", false, prevProcedure1.SimplifiedGrantAuthorizationFlag);
				AssertEquals("Simplified Grant Authorization Flag 2 is false", false, prevProcedure2.SimplifiedGrantAuthorizationFlag);

				previousProcedureMaster.SimplifiedGrantAuthorizationFlag = true;
				AssertEquals("Simplified Grant Authorization Flag 1 Updated", true, prevProcedure1.SimplifiedGrantAuthorizationFlag);
				AssertEquals("Simplified Grant Authorization Flag 2 Updated", true, prevProcedure2.SimplifiedGrantAuthorizationFlag);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => previousProcedureMaster;

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
			previousProcedureMaster = goodsItem.PreviousProcedureMaster;
		}
		NctsHeader header;
		NctsDepartureCargoDesc goodsItem;
		NctsPreviousProcedureMaster previousProcedureMaster;
	}
}
