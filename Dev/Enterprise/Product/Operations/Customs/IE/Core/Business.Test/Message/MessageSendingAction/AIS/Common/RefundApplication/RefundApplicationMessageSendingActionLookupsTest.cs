using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Testing.Message.MessageSendingAction.AIS.RefundApplications
{
	class RefundApplicationMessageSendingActionLookupsTest : TestCaseWithFactory
	{
		public void TestRefundTypeList()
		{
			AssertEquals("RefundTypeList.CodesAsString", new AISRefundTypeList().CodesAsString, lookups.RefundTypeList.CodesAsString);

			var entryHeader = (CusEntryHeader)Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			var anotherSendingAction = new RefundApplicationMessageSendingAction(entryHeader);
			AssertSame("Should be cached.", lookups.RefundTypeList, anotherSendingAction.Lookups.RefundTypeList);
		}

		public void TestCustomsOfficesList()
		{
			AssertContainsExactElementsInAnyOrder(lookups.CustomsOfficesList, Factory.New<JobDeclaration>().Lookups.CustomsOffices);
		}

		public void TestLegalBasisList()
		{
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, UniversalReferenceConstants.RefCusCodeListTypes.Codes.LegalBasisCode, "ABC", "ABC Desc");
			TestHelper.CreateNewOrGetExistingCusCodeList(Factory, UniversalReferenceConstants.RefCusCodeListTypes.Codes.LegalBasisCode, "DEF", "DEF Desc");
			Factory.Save();

			var entryHeader = (CusEntryHeader)Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			var sendingAction = new RefundApplicationMessageSendingAction(entryHeader);
			var legalBasisListLookups = (CodeDescriptionPairList)sendingAction.Lookups.LegalBasisList;

			var entryHeader2 = (CusEntryHeader)Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			var sendingAction2 = new RefundApplicationMessageSendingAction(entryHeader2);
			var legalBasisListLookups2 = (CodeDescriptionPairList)sendingAction2.Lookups.LegalBasisList;

			CombineAssertions(() =>
			{
				AssertSame("Cached", legalBasisListLookups, legalBasisListLookups2);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "ABC", "DEF" }, legalBasisListLookups.Cast<ICodeDescription>().Select(x => x.Code));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var entryHeader = (CusEntryHeader)Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			sendingAction = new RefundApplicationMessageSendingAction(entryHeader);
			lookups = sendingAction.Lookups;
		}
		RefundApplicationMessageSendingAction sendingAction;
		RefundApplicationMessageSendingActionLookups lookups;
	}
}
