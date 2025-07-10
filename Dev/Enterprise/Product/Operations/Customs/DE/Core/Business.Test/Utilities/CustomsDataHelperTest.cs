using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class CustomsDataHelperTest : TestCaseWithFactory
	{
		public void TestInvalidResponseMessageName()
		{
			message.EM_ApplicationReference = "SCCANC";
			CombineAssertions(() =>
			{
				AssertEquals("Data Provider null", null, message.DataProvider<ICUSCAN>());
				AssertEquals("Message in Error", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Error, message.EM_Status);
				var failureNote = message.Notes.FindByDescription("Processing Log").Single();
				AssertMultilineASCIIEquals("Dictionary Try Get Error", "Unable to find Message Details for Application Reference: SCCANC", failureNote.ST_NoteText);
			});
		}

		public void TestXSDInvalidMessage()
		{
			message.EM_MessageText = $"<DECustomsData><LogbookTime>2019-11-20T15:29:00.2347048+02:00</LogbookTime><CustomsData><{ValidApplicationReference}><MetaData><Preparation><Date>2019-05-24</Date></Preparation></MetaData></{ValidApplicationReference}></CustomsData></DECustomsData>";
			message.EM_ApplicationReference = ValidApplicationReference;
			CombineAssertions(() =>
			{
				AssertEquals("Data Provider null", null, message.DataProvider<ICUSCAN>());
				AssertEquals("Message in Error", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Error, message.EM_Status);
				var failureNote = message.Notes.FindByDescription("Processing Log").Single();
				AssertMultilineASCIIEquals("Validation Error", "The element 'Preparation' has incomplete content. List of possible elements expected: 'Time'.", failureNote.ST_NoteText);
			});
		}

		public void TestDeSerializationInvalidMessage()
		{
			message.EM_MessageText = $"<DECustomsData><LogbookTime>2019-11-20T15:29:00.2347048+02:00</LogbookTime><CustomsData><{ValidApplicationReference}><MetaData><Preparation><Date /></Preparation></MetaData></{ValidApplicationReference}></CustomsData></DECustomsData>";
			message.EM_ApplicationReference = ValidApplicationReference;
			CombineAssertions(() =>
			{
				AssertEquals("Data Provider null", null, message.DataProvider<ICUSCAN>());
				AssertEquals("Message in Error", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Error, message.EM_Status);
				var failureNote = message.Notes.FindByDescription("Processing Log").Single();
				AssertMultilineASCIIEquals("Deserilization Error", "There is an error in XML document (1, 42).", failureNote.ST_NoteText);
			});
		}

		public void TestValidMessage()
		{
			message.EM_MessageText = $"<DECustomsData><LogbookTime>2019-11-20T15:29:00.2347048+02:00</LogbookTime><CustomsData><{ValidApplicationReference} /></CustomsData></DECustomsData>";
			message.EM_ApplicationReference = ValidApplicationReference;
			var dataProvider = message.DataProvider;
			AssertSame("DataProvider is cached", message.DataProvider, dataProvider);
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = Factory.New<AtlasInboundEDIMessage<ICUSCAN>>();
		}
		AtlasInboundEDIMessage<ICUSCAN> message;

		const string ValidApplicationReference = nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.SCCANE);
	}
}
