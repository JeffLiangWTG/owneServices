using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentWrappers.Customs;
using NUnit.Framework;
using static Enterprise.Integration.Customs.CA;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(ReleaseStatus))]
	sealed class ReleaseStatusTest : NonPersistentBusinessObjectTestCase
	{
		public void TestContainersForIID()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			declaration.CusContainers.AddNew().CO_ContainerNumber = "CON111";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "CON222";

			var ccn = declaration.CargoControlNumbers.AddNew();
			ccn.CY_CargoControlNumber = "CCN1";

			var helper = new DeclarationTestHelper(Factory, true);
			var releaseStatus = new ReleaseStatus(ccn, (EDIReleaseMessage)helper.GetIIDResponseMessage(new ZDateTime(2018, 01, 16, 11, 49, 00), "1", ZDateTime.Now));

			AssertEquals("CON111, CON222", new ZStringBuilder(releaseStatus.Containers).ToStringWithDelimiterBetweenAppends(", "));
		}

		public void TestReleaseStatusWithoutCCN()
		{
			var helper = new DeclarationTestHelper(Factory, true);
			var releaseStatus = new ReleaseStatus((EDIReleaseMessage)helper.GetEDIReleaseResponseMessage("37132536987", "1"));
			AssertEquals("RL_ServiceOption", "257", releaseStatus.RL_ServiceOption);
			AssertEquals("RL_TransactionNumber", "10207400004068", releaseStatus.RL_TransactionNumber);
			AssertEquals("RL_ProcessingIndicator", "4", releaseStatus.RL_ProcessingIndicator.ToString());
			AssertEquals("RL_ReleaseStatus", "CLR", releaseStatus.RL_ReleaseStatus);
			AssertEquals("RL_ReleaseStatusDescription", "Goods Released", releaseStatus.RL_ReleaseStatusDescription);
			AssertEquals("RL_ReleaseDate", new ZDateTime(2010, 11, 25, 8, 20, 0), releaseStatus.RL_ReleaseDate);
			AssertEquals("RL_ProcessingDate Date", ZDateTime.Empty, releaseStatus.RL_ProcessingDate);
			AssertEquals("RL_CargoControlNumber", "37132536987", releaseStatus.RL_CargoControlNumber);
			AssertEquals("RL_CargoControlNumber read only", true, releaseStatus.RL_CargoControlNumberInfo.ReadOnly);
			Assert("RL_Bill read only", releaseStatus.RL_BillInfo.ReadOnly);
			AssertEquals("RL_DeliveryInstructions", "DELIVERY INSTRUCTIONS LINE 1\r\nLINE 2", releaseStatus.RL_DeliveryInstructions);
			AssertEquals("RL_ReleaseOffice", "0497", releaseStatus.RL_ReleaseOffice);
			AssertEquals("RL_WarehouseCode", "3072", releaseStatus.RL_WarehouseCode);
			AssertEquals("RL_ShouldBePrinted", false, releaseStatus.RL_ShouldBePrinted);
			AssertEquals("CanBePrinted", true, releaseStatus.CanBePrinted);
			AssertEquals("ProcessingIndicatorCodeDescription", "4 - Goods Released", releaseStatus.ProcessingIndicatorCodeDescription.ToString());
			AssertEquals("Containers count", 3, releaseStatus.Containers.Count());
			AssertEquals("CONTAINER1", releaseStatus.Containers.ElementAt(0));
			AssertEquals("CONTAINER2", releaseStatus.Containers.ElementAt(1));
			AssertEquals("CONTAINER3", releaseStatus.Containers.ElementAt(2));

			var bills = releaseStatus.Bills;
			AssertEquals("Type", typeof(BillNonDependentCollection), bills.GetType());
			AssertEquals("Count", 0, bills.Count);
		}

		public void TestReleaseStatusWithMinimalMessageAndWithCCNAsCusEntryNumber()
		{
			var helper = new DeclarationTestHelper(Factory, true);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var ccn1 = declaration.AdditionalReferenceNumbers.AddNew();
			ccn1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			ccn1.CE_EntryNum = "CCN1";

			var releaseStatus = new ReleaseStatus(ccn1, (EDIReleaseMessage)helper.GetEDIReleaseResponseMessage(@"UNH+1+CUSRES:D:96A:UN'UNT+2+1'", ZDateTime.Now, "1"));
			AssertEquals("RL_ServiceOption", ZString.Empty, releaseStatus.RL_ServiceOption);
			AssertEquals("RL_TransactionNumber", ZString.Empty, releaseStatus.RL_TransactionNumber);
			AssertEquals("RL_ProcessingIndicator", Enterprise.Edifact.D96A.Elements.ProcessingIndicatorCodedList.TransactionUnknown.ToString(), releaseStatus.RL_ProcessingIndicator);
			AssertEquals("RL_ReleaseStatus", ZString.Empty, releaseStatus.RL_ReleaseStatus);
			AssertEquals("RL_ReleaseStatusDescription", ZString.Empty, releaseStatus.RL_ReleaseStatusDescription);
			AssertEquals("RL_ReleaseDate", ZDateTime.Empty, releaseStatus.RL_ReleaseDate);
			AssertEquals("RL_ProcessingDate Date", ZDateTime.Empty, releaseStatus.RL_ProcessingDate);
			AssertEquals("RL_CargoControlNumber", "CCN1", releaseStatus.RL_CargoControlNumber);
			AssertEquals("RL_CargoControlNumber read only", false, releaseStatus.RL_CargoControlNumberInfo.ReadOnly);
			Assert("RL_Bill read only", !releaseStatus.RL_BillInfo.ReadOnly);
			AssertEquals("RL_DeliveryInstructions", ZString.Empty, releaseStatus.RL_DeliveryInstructions);
			AssertEquals("RL_ReleaseOffice", ZString.Empty, releaseStatus.RL_ReleaseOffice);
			AssertEquals("RL_WarehouseCode", ZString.Empty, releaseStatus.RL_WarehouseCode);
			AssertEquals("RL_ShouldBePrinted", true, releaseStatus.RL_ShouldBePrinted);
			AssertEquals("CanBePrinted", true, releaseStatus.CanBePrinted);
			AssertEquals("ProcessingIndicatorCodeDescription", ZString.Empty, releaseStatus.ProcessingIndicatorCodeDescription.ToString());
			AssertEquals("Containers count", false, releaseStatus.Containers.Any());

			releaseStatus.RL_CargoControlNumber = "CCN2";
			AssertEquals("CE_EntryNum", "CCN2", ccn1.CE_EntryNum);

			var bills = releaseStatus.Bills;
			AssertEquals("Type", typeof(BillCollection), bills.GetType());
			AssertEquals("Count", 0, bills.Count);
			declaration.Bills.AddNew();
			AssertEquals("Count", 1, bills.Count);
		}

		public void TestReleaseStatusWithNoMessageAndWithCCNAsCargoControlNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var ccn2 = declaration.CargoControlNumbers.AddNew();
			ccn2.CY_CargoControlNumber = "CCN1";

			var releaseStatus = new ReleaseStatus(ccn2);
			AssertEquals("RL_ServiceOption", ZString.Empty, releaseStatus.RL_ServiceOption);
			AssertEquals("RL_TransactionNumber", ZString.Empty, releaseStatus.RL_TransactionNumber);
			AssertEquals("RL_ProcessingIndicator", ZString.Empty, releaseStatus.RL_ProcessingIndicator.ToString());
			AssertEquals("RL_ReleaseStatus", ZString.Empty, releaseStatus.RL_ReleaseStatus);
			AssertEquals("RL_ReleaseStatusDescription", ZString.Empty, releaseStatus.RL_ReleaseStatusDescription);
			AssertEquals("RL_ReleaseDate", ZDateTime.Empty, releaseStatus.RL_ReleaseDate);
			AssertEquals("RL_ProcessingDate Date", ZDateTime.Empty, releaseStatus.RL_ProcessingDate);
			AssertEquals("RL_CargoControlNumber", "CCN1", releaseStatus.RL_CargoControlNumber);
			AssertEquals("RL_CargoControlNumber read only", false, releaseStatus.RL_CargoControlNumberInfo.ReadOnly);
			Assert("RL_Bill read only", !releaseStatus.RL_BillInfo.ReadOnly);
			AssertEquals("RL_DeliveryInstructions", ZString.Empty, releaseStatus.RL_DeliveryInstructions);
			AssertEquals("RL_ReleaseOffice", ZString.Empty, releaseStatus.RL_ReleaseOffice);
			AssertEquals("RL_WarehouseCode", ZString.Empty, releaseStatus.RL_WarehouseCode);
			AssertEquals("RL_ShouldBePrinted", false, releaseStatus.RL_ShouldBePrinted);
			AssertEquals("CanBePrinted", false, releaseStatus.CanBePrinted);
			AssertEquals("ProcessingIndicatorCodeDescription", ZString.Empty, releaseStatus.ProcessingIndicatorCodeDescription.ToString());
			AssertEquals("Containers count", false, releaseStatus.Containers.Any());

			releaseStatus.RL_CargoControlNumber = "CCN2";
			AssertEquals("CY_CargoControlNumber", "CCN2", ccn2.CY_CargoControlNumber);

			var bills = releaseStatus.Bills;
			AssertEquals("Type", typeof(BillCollection), bills.GetType());
			AssertEquals("Count", 0, bills.Count);
			declaration.Bills.AddNew();
			AssertEquals("Count", 1, bills.Count);
		}

		public void TestIsPersistent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var ccn1 = declaration.AdditionalReferenceNumbers.AddNew();
			ccn1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			ccn1.CE_EntryNum = "CCN1";
			Assert("IsPersistent", new ReleaseStatus(ccn1).IsPersistent);
			var ccn2 = declaration.CargoControlNumbers.AddNew();
			ccn2.CY_CargoControlNumber = "CCN2";
			Assert("IsPersistent", new ReleaseStatus(ccn2).IsPersistent);
			var helper = new DeclarationTestHelper(Factory, true);
			var message = (EDIReleaseMessage)helper.GetEDIReleaseResponseMessage("37132536987", "1");
			Assert("IsPersistent", !new ReleaseStatus(message).IsPersistent);
		}

		public void TestWarehouseCodeDescription()
		{
			CACSubLocationTest.CreateSubLocation(Factory, "3072", "ADAMS CARGO LTD");
			var helper = new DeclarationTestHelper(Factory, true);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.CA.CAJobMessageTypeList.Codes.Import;
			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "37132536987";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER1";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER2";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER3";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = "REL";
			entry.Messages.Add(helper.GetEDIReleaseResponseMessage("37132536987", "1"));
			var releaseStatus = declaration.ReleaseStatusesToPrint[0] as IReleaseStatus;
			var wrapper = DocReleaseStatus.New(releaseStatus, Factory);

			AssertEquals("WarehouseCodeDescription", "3072 - ADAMS CARGO LTD", wrapper.WarehouseCodeDescription);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ReleaseStatus(Factory.New<CargoControlNumber>());
		}

		#endregion
	}
}
