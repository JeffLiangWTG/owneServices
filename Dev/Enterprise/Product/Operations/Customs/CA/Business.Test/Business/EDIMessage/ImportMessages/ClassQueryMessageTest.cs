using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(QueryMessage))]
	public class ClassQueryMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestMessageSubTypeDescription()
		{
			message.EM_MessageSubType = QueryMessageSubType3CharCodes.Codes.CLASSFILE;
			AssertEquals("EM_MessageSubTypeDescription", QueryMessageSubType3CharCodes.Descriptions.CLASSFILE, message.EM_MessageSubTypeDescription);
		}

		public void TestBatchNumber()
		{
			message.EM_MessageText = "UNH+366+CUSDEC:S:99B:UN'BGM+:::QA+178+9'RFF+ABD:2309903912'UNS+D'UNS+S'UNT+6+366'";
			AssertEquals("BatchNumber", "178", message.BatchNumber);
		}

		public void TestDocumentMessageVersion()
		{
			message.EM_MessageText = "UNH+366+CUSDEC:S:99B:UN'BGM+:::QA+178:1234+9'RFF+ABD:2309903912'UNS+D'UNS+S'UNT+6+366'";
			AssertEquals("DocumentMessageVersion", "1234", message.DocumentMessageVersion);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = (QueryMessage)GetNewBusinessObject();
			result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			return result;
		}

		protected override bool CanPersistedObjectBeDeleted => false;

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<QueryMessage>();
		}

		public virtual void TestDefaultValues()
		{
			AssertEquals(EDIMessage.ApplicationCodes.CAIMP, message.EM_ApplicationCode);
			AssertEquals(MessageTypeList.Codes.Query, message.EM_MessageType);
			AssertEquals("ShouldShowInterpretation", true, message.ShouldShowInterpretation);
		}

		public void TestMessageNumberFilledIn()
		{
			var number = Env.NumberFountains.EDIFACTNumberFountain("M", "IMP", EDIMessage.ApplicationCodes.CAIMP).PeekPreliminaryFormatted(Factory);
			Factory.Save();
			AssertEquals("MessageNumberFilledIn", "Message Number = " + number, message.EM_MessageText);
		}

		public void TestDocumentMessageVersionFilledIn()
		{
			message.EM_MessageText = "Document Message Version = " + EDIMessage.DocumentMessageVersionPlaceHolder;
			message.EM_MessageInterpretation = "Document Message Version = " + EDIMessage.DocumentMessageVersionPlaceHolderHtml;

			var number = Env.NumberFountains.EDIFACTNumberFountain("M", "VER", EDIMessage.ApplicationCodes.CAIMP).PeekPreliminary(Factory).ToString("0000");
			Factory.Save();
			AssertEquals("DocumentMessageVersionFilledIn", "Document Message Version = " + number, message.EM_MessageText);
			AssertEquals("DocumentMessageVersionFilledIn", "Document Message Version = " + number, message.EM_MessageInterpretation);
			AssertEquals("EM_ApplicationReference", number, message.EM_ApplicationReference);
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
