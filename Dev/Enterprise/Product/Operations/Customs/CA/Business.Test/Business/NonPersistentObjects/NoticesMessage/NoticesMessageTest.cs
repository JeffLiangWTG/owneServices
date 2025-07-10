using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(NoticesMessage))]
	sealed class NoticesMessageTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new NoticesMessage(Factory.New<UniversalEventMessage>(), ZString.Empty);
		}

		public void TestDocumentSupporter()
		{
			var noticesMessage = new NoticesMessage(message, "StatusDescription");
			AssertType<UniversalEventMessageDocumentSupporter>(((IDocumentSupportable)noticesMessage).DocumentSupporter);
		}

		public void TestReferenceNumber()
		{
			var noticesMessage = new NoticesMessage(message, ZString.Empty);
			AssertEquals("ReferenceNumber", "SECONDARY BUSINESS ID", noticesMessage.ReferenceNumber);
		}

		public void TestStatusDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = Core.Constants.CountryCodes.Canada;
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CANoticeReasonCode;
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;

			helper.CreateNewOrGetExistingCusCodeType(codeType, "CA Notice Reason Code");
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, "S001", "Positive Functional Acknowledgement.", startDate, endDate);
			Factory.Save();

			var noticesMessage = new NoticesMessage(message, "StatusDescription");
			AssertEquals("StatusDescription", "StatusDescription", noticesMessage.StatusDescription);
			noticesMessage = new NoticesMessage(message, ZString.Empty);
			AssertEquals("StatusDescription", "S001 - Positive Functional Acknowledgement.", noticesMessage.StatusDescription);
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = Factory.New<UniversalEventMessage>();
			message.EM_MessageText = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CAeManifestStatusNotice</Type>
					<Key>12345000000012</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>OrganizationReference</Type>
				<Value>SECONDARY BUSINESS ID</Value>
			</Context>
			<Context>
				<Type>Status</Type>
				<Value>S001</Value>
			</Context>
			<Context>
				<Type>Status</Type>
				<Value>S002</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
		}
		UniversalEventMessage message;
	}
}
