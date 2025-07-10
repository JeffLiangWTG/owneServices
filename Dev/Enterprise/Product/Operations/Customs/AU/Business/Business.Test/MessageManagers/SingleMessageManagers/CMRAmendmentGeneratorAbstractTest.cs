using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class CMRAmendmentGeneratorAbstractTest : TestCaseWithFactory
	{
		public void TestGenerateOriginalMessageCore()
		{
			var result = Generator.GenerateOriginalMessageCore();
			AssertCommon(result);
			AssertEquals("EM_MEssageSubType", CMRMessage.MessageSubTypes.Original, result.EM_MessageSubType);
		}

		public void TestGenerateAmendmentMessageCore()
		{
			var result = Generator.GenerateAmendmentMessageCore();
			AssertCommon(result);
			AssertEquals("EM_MEssageSubType", CMRMessage.MessageSubTypes.Change, result.EM_MessageSubType);
		}

		public void TestGenerateWithdrawalMessageCore()
		{
			var result = Generator.GenerateWithdrawalMessageCore(Bizo, DBBizo);
			AssertCommon(result);
			AssertEquals("EM_MEssageSubType", CMRMessage.MessageSubTypes.Withdraw, result.EM_MessageSubType);
		}

		protected abstract Type ExpectedMessageType { get; }

		protected abstract ICMRAmendmentGeneratorForTest GetGenerator(BusinessObject bizo);

		ICMRAmendmentGeneratorForTest generator;
		protected ICMRAmendmentGeneratorForTest Generator => generator ?? (generator = GetGenerator(Bizo));

		protected abstract BusinessObject GetSavedBizo();

		BusinessObject bizo;
		protected BusinessObject Bizo => bizo ?? (bizo = GetSavedBizo());

		protected virtual void AssertCommon(EDIMessage message)
		{
			AssertEquals("MessageType", ExpectedMessageType, message.GetType());
			AssertEquals("EM_LinkedObject", Bizo, message.EM_LinkedObject);
		}

		protected virtual BusinessObject DBBizo => new BusinessObjectFactory().Load(Bizo.GetType(), Bizo.PK);

		protected interface ICMRAmendmentGeneratorForTest
		{
			ZPropertyInfo[] UniqueIdentifierInfos { get; }
			EDIMessage GenerateOriginalMessageCore();
			EDIMessage GenerateAmendmentMessageCore();
			EDIMessage GenerateWithdrawalMessageCore(BusinessObject bizo, BusinessObject bizoInNewFactory);
		}
	}
}
