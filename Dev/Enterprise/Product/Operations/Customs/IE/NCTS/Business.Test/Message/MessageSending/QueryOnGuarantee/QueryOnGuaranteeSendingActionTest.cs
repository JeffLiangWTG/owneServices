using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(QueryOnGuaranteeSendingAction))]
	sealed class QueryOnGuaranteeSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHeader() => AssertType<NctsHeader>(GetNewQueryOnGuaranteeSendingAction().Header);

		public void TestLookups()
		{
			var sendingObj = GetNewQueryOnGuaranteeSendingAction();
			AssertType("Should have created a correct Lookups.", typeof(QueryOnGuaranteeSendingActionLookups), sendingObj.Lookups);
		}

		public void TestSenderType()
		{
			var sendingObj = GetNewQueryOnGuaranteeSendingAction();
			AssertType("Should have defined a correct SenderType.", typeof(NctsMessageSender), sendingObj.CreateSender());
		}

		public void TestQueryIdentifier_Caption()
		{
			var sendingObj = GetNewQueryOnGuaranteeSendingAction();
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingObj.QueryIdentifierInfo);
			AssertEquals("QueryIdentifierInfo caption", "Query Identifier", resData.Caption);
		}

		public void TestPeriodFrom_Caption()
		{
			var sendingObj = GetNewQueryOnGuaranteeSendingAction();
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingObj.PeriodFromInfo);
			AssertEquals("PeriodFromInfo caption", "Period From", resData.Caption);
		}

		public void TestPeriodTo_Caption()
		{
			var sendingObj = GetNewQueryOnGuaranteeSendingAction();
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingObj.PeriodToInfo);
			AssertEquals("PeriodToInfo caption", "Period To", resData.Caption);
		}

		public void TestValidationType()
		{
			var sendingObj = GetNewQueryOnGuaranteeSendingAction();
			AssertType("Should have created a correct Validation.", typeof(QueryOnGuaranteeSendingActionValidation), sendingObj.Validation);
		}

		public void TestMessageAttachee()
		{
			var sendingObj = GetNewQueryOnGuaranteeSendingAction();
			AssertType<NctsDepartureMovementHeader>("Departure MessageAttachee", sendingObj.MessageAttachee);

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			_ = header.Guarantees.AddNew();
			_ = header.Guarantees.AddNew();
			AssertType<NctsHeader>("Arrival MessageAttachee", new QueryOnGuaranteeSendingAction(header).MessageAttachee);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			_ = header.MovementHeader.Guarantees.AddNew();
			_ = header.MovementHeader.Guarantees.AddNew();
			return new QueryOnGuaranteeSendingAction(header);
		}

		QueryOnGuaranteeSendingAction GetNewQueryOnGuaranteeSendingAction() => (QueryOnGuaranteeSendingAction)GetNewBusinessObject();
	}
}
