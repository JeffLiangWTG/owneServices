using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(K84Message))]
	public class K84MessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAccountingAndStatementDates()
		{
			var message = Factory.New<K84Message>();
			message.EM_MessageSubType = K84ReportTypes.Codes.Daily;
			AssertEquals("K84Statement date", ZDateTime.Empty, message.K84StatementDate);
			AssertEquals("K84Accounting date", ZDateTime.Empty, message.K84AccountingDate);

			message = Factory.New<K84Message>();
			message.EM_MessageSubType = K84ReportTypes.Codes.Daily;
			message.SetSystemDefinedValue(EDIMessage.Schema.K84StatementDate, new ZDateTime(2011, 4, 11));
			message.SetSystemDefinedValue(EDIMessage.Schema.K84AccountingDate, new ZDateTime(2011, 4, 08));
			AssertEquals("K84Statement date", new ZDateTime(2011, 4, 11), message.K84StatementDate);
			AssertEquals("K84Accounting date", new ZDateTime(2011, 4, 08), message.K84AccountingDate);

			message = Factory.New<K84Message>();
			message.EM_MessageSubType = K84ReportTypes.Codes.Monthly;
			message.SetSystemDefinedValue(EDIMessage.Schema.K84StatementDate, new ZDateTime(2011, 3, 30));
			AssertEquals("K84Statement date", new ZDateTime(2011, 3, 30), message.K84StatementDate);
		}

		public void TestIControllerIDProviderMembers()
		{
			var message = Factory.New<K84Message>();
			IControllerIDProvider provider = message;
			AssertEquals("ControllerID", ControllerIDs.Customs.CA.K84Reports, provider.ControllerID);
			AssertEquals("BusinessObjectPK", message.PK.ToGuid(), provider.BusinessObjectPK);
		}

		public void TestMessageSubTypeDescription()
		{
			message.EM_MessageSubType = K84ReportTypes.Codes.Overdue;
			AssertEquals("EM_MessageSubTypeDescription", K84ReportTypes.Descriptions.Overdue, message.EM_MessageSubTypeDescription);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = (K84Message)GetNewBusinessObject();
			result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			return result;
		}

		protected override bool CanPersistedObjectBeDeleted => false;

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<K84Message>();
		}

		public virtual void TestDefaultValues()
		{
			AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.CAIMP, message.EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.K84Report, message.EM_MessageType);
			AssertEquals("ShouldShowInterpretation", true, message.ShouldShowInterpretation);
		}

		public void TestMessageNumberFilledIn()
		{
			var number = Env.NumberFountains.EDIFACTNumberFountain("M", "IMP", EDIMessage.ApplicationCodes.CAIMP).PeekPreliminaryFormatted(Factory);
			Factory.Save();
			AssertEquals("MessageNumberFilledIn", "Message Number = " + number, message.EM_MessageText);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			message = (EDIMessage)GetNewBusinessObject();
			message.EM_MessageText = "Message Number = " + EDIMessage.MessageNumberPlaceHolder;
		}

		EDIMessage message;

		#endregion
	}
}
