using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitConsignment))]
	sealed class CusExitConsignmentTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidation()
		{
			AssertType<CusExitConsignmentValidation>(consignment.Validation);
		}

		public void TestLookups()
		{
			AssertType<CusExitConsignmentLookups>(consignment.Lookups);
		}

		public void TestCusExitConsignmentItems()
		{
			AssertType<ExitControlBase.Business.CusExitConsignmentItemCollection<CusExitConsignmentItem>>(consignment.CusExitConsignmentItems);
		}

		public void TestHeader()
		{
			AssertType<CusExitHeader>(consignment.Header);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("HumanReadableName", "Consignment", consignment.HumanReadableName);
		}

		public void TestTypeDecider()
		{
			AssertType<CusExitConsignmentTypeDecider>(CusExitConsignment.TypeDecider);
		}

		public void TestMessages()
		{
			var message1 = Factory.New<EDIMessage>();
			message1.EM_LinkedObject = consignment;
			var message2 = Factory.New<EDIMessage>();
			message2.EM_LinkedObject = consignment;
			AssertContainsExactElementsInAnyOrder(new[] { message1, message2 }, consignment.Messages.Cast<EDIMessage>());
		}

		public void TestCXC_MovementReference_Caption()
		{
			AssertEquals("MRN", DataBoundResourceStrings.GetDataForProperty(consignment.CXC_MovementReferenceInfo).Caption);
		}

		public void TestCXC_LocalReference_Caption()
		{
			AssertEquals("LRN", DataBoundResourceStrings.GetDataForProperty(consignment.CXC_LocalReferenceInfo).Caption);
		}

		public void TestCXC_Status_Caption()
		{
			AssertEquals("Status", DataBoundResourceStrings.GetDataForProperty(consignment.CXC_StatusInfo).Caption);
		}

		public void TestCXC_Status_List()
		{
			AssertEquals("Lookups.StatusList", consignment.CXC_StatusInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestCXC_Status_ReadOnly()
		{
			AssertEquals(expected: true, consignment.CXC_StatusInfo.ReadOnly);
		}

		public void TestStatusDescription_Caption()
		{
			CombineAssertions(() =>
			{
				var resourceStringData = DataBoundResourceStrings.GetDataForProperty(typeof(CusExitConsignment), nameof(CusExitConsignment.StatusDescription));
				AssertEquals("Caption", "Status Description", resourceStringData.Caption);
				AssertEquals("MediumCaption", "Status Desc.", resourceStringData.MediumCaption);
				AssertEquals("ShortCaption", "Desc.", resourceStringData.ShortCaption);
			});
		}

		public void TestStatusDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "CSTEX");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "123", "123 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("CXC_Status is empty", ZString.Empty, consignment.StatusDescription);

				consignment.CXC_Status = "456";
				AssertEquals("CXC_Status is invalid", ZString.Empty, consignment.StatusDescription);

				consignment.CXC_Status = "123";
				AssertEquals("CXC_Status is valid", "123 DESC", consignment.StatusDescription);
			});
		}

		public void TestCXC_UniqueConsignmentReference_Caption()
		{
			CombineAssertions(() =>
			{
				var resourceStringData = DataBoundResourceStrings.GetDataForProperty(consignment.CXC_UniqueConsignmentReferenceInfo);
				AssertEquals("Caption", "Reference Number UCR", resourceStringData.Caption);
				AssertEquals("MediumCaption", "Ref. No. UCR", resourceStringData.MediumCaption);
			});
		}

		public void TestMessageCodeForEdocs_LRN()
		{
			consignment.CXC_MovementReference = ZString.Empty;
			consignment.CXC_LocalReference = "LocalReferenceABC123";
			AssertEquals("LocalReferenceABC123", consignment.MessageCodeForEdocs);

			consignment.CXC_MovementReference = "MRNABC123";
			AssertEquals("MRNABC123", consignment.MessageCodeForEdocs);
		}

		public void TestMessageCodeForEdocs_MRN()
		{
			consignment.CXC_LocalReference = "LocalReferenceABC123";
			consignment.CXC_MovementReference = "MovementReferenceXYZ987";
			AssertEquals("MovementReferenceXYZ987", consignment.MessageCodeForEdocs);
		}

		public void TestMessageDescriptionForEdocs_LRN()
		{
			consignment.CXC_MovementReference = ZString.Empty;
			consignment.CXC_LocalReference = "LocalReferenceABC123";
			AssertEquals("LRN:LocalReferenceABC123", consignment.MessageDescriptionForEdocs);

			consignment.CXC_MovementReference = "MRNABC123";
			AssertEquals("MRN:MRNABC123", consignment.MessageDescriptionForEdocs);
		}

		public void TestMessageDescriptionForEdocs_MRN()
		{
			consignment.CXC_LocalReference = "LocalReferenceABC123";
			consignment.CXC_MovementReference = "MovementReferenceXYZ987";
			AssertEquals("MRN:MovementReferenceXYZ987", consignment.MessageDescriptionForEdocs);
		}

		public void TestCanDelete()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Can delete consignment when not attached to an exit report", true, consignment.CanDelete);
				var exitReport = consignment.Header.CusExitReports.AddNew();
				exitReport.CER_CXC_Consignment = consignment.PK;
				AssertEquals("Cannot delete consignment attached to an exit report", false, consignment.CanDelete);
				var consignment2 = consignment.Header.CusExitConsignments.AddNew();
				var item = consignment2.CusExitConsignmentItems.AddNew();
				AssertEquals("Can delete consignment when not attached to an existing exit report", true, consignment2.CanDelete);
			});
		}

		public void TestReasonForNotAbleToDelete()
		{
			AssertEquals("Entry cannot be deleted as it is referenced in an Exit Report.", consignment.ReasonForNotAbleToDelete);
		}

		public void TestConsignmentItemsRequiredToCreateCusExitReport()
		{
			AssertEquals(true, consignment.ConsignmentItemsRequiredToCreateCusExitReport);
		}

		public void TestIsUCC6() => CombineAssertions(() =>
		{
			var objectForTest = GetNewBusinessObject() as CusExitConsignment;
			AssertEquals("Not UCC6", objectForTest.IsUCC6, false);

			AssertEquals("No Parent = not UCC6", Factory.New<CusExitConsignment>().IsUCC6, false);

			var ucc6ObjectForTest = Factory.GetUcc6ExitHeader().CusExitConsignments.AddNew();
			AssertEquals("UCC6 taken from parent", ucc6ObjectForTest.IsUCC6, true);
		});

		public void TestMaxItemCountCore()
		{
			AssertEquals("Max Item Count", 9999, consignment.MaxItemCount);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(Factory).consignment;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => consignment;

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory).consignment;

		protected override void SetUp()
		{
			base.SetUp();
			consignment = GetNewBusinessObject(Factory).consignment;
		}
		CusExitConsignment consignment;

		public static (CusExitConsignment consignment, CusExitHeader header) GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = CusExitHeaderAbstractTest<CusExitHeader>.GetNewBusinessObject(factory);
			var consignment = header.CusExitConsignments.AddNew();
			consignment.CXC_Status = "REJ";
			return (consignment, header);
		}
	}
}
