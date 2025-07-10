using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(GbEDIMessage))]
	public class GbEdiMessageTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<GbEDIMessage>();
		}

		public void TestMessageDescriptionForEdocs()
		{
			GBCustomsDataRegistry.Instance.EDocsParentNameMacro.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "a <CODE> b <NUMBER> c <REFERENCE> d <TYPE> e <SUBTYPE>");
			var message = Factory.New<GbEDIMessage>();
			message.EM_ApplicationCode = "AAA";
			message.EM_MessageNum = "BBB";
			message.EM_ApplicationReference = "CCC";
			message.EM_MessageType = "DDD";
			message.EM_MessageSubType = "EEE";
			AssertEquals("a AAA b BBB c CCC d DDD e EEE", message.MessageDescriptionForEdocs);
		}

		public void TestSettingOfGbMessageNumberStrategyIfNotSet()
		{
			AssertNoExceptionThrown(() =>
			{
				var gbEDIMessage = (GbEDIMessage)GetNewBusinessObject();
				gbEDIMessage.EM_ApplicationCode = "CUK";
				Factory.Save();
			});
		}

		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
			GbEDIMessage gbEDIMessage = (GbEDIMessage)GetNewBusinessObject();
			AssertEquals(typeof(EdiMessageDocumentSupporter), gbEDIMessage.DocumentSupporter.GetType());
			AssertEquals(BusinessContext.EDIMessage, gbEDIMessage.DocumentSupporter.BusinessContext);
		}
	}
}
