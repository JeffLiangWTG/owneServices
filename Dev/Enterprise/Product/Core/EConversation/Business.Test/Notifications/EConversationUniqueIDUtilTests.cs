using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.EConversation.Business.Testing
{
	public class EConversationUniqueIDUtilTests : TestCaseWithFactory
	{
		public void TestTryLoadFromEmailBody()
		{
			var dummy = Factory.NewWithValidTestData<DummyBaseBusinessObject>();
			var element = EConversationUniqueIDUtil.GenerateElement(dummy);

			var success = EConversationUniqueIDUtil.TryLoadFromEmailBody<DummyBaseBusinessObject>(Factory, element, out var result);

			Assert(success.Value);
			AssertEquals(dummy, result);
		}

		public void TestTryLoadFromEmailBody_3CharTablePrefix()
		{
			var dummy = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			AssertEquals("Precondition", 3, dummy.TablePrefix.Length);
			var element = EConversationUniqueIDUtil.GenerateElement(dummy);

			var success = EConversationUniqueIDUtil.TryLoadFromEmailBody<DummyDependantBusinessObject>(Factory, element, out var result);

			Assert(success.Value);
			AssertEquals(dummy, result);
		}

		public void TestTryLoadFromEmailBody_NameModifiedByClient()
		{
			var dummy = Factory.NewWithValidTestData<DummyBaseBusinessObject>();
			var element = EConversationUniqueIDUtil.GenerateElement(dummy);
			element = element.Replace(EConversationUniqueIDUtil.ElementName, $"x_{EConversationUniqueIDUtil.ElementName}abc");

			var success = EConversationUniqueIDUtil.TryLoadFromEmailBody<DummyBaseBusinessObject>(Factory, element, out var result);

			Assert(success.Value);
			AssertEquals(dummy, result);
		}

		public void TestTryLoadFromEmailBody_NameAndIDModifiedByClient()
		{
			var dummy = Factory.NewWithValidTestData<DummyBaseBusinessObject>();
			var element = EConversationUniqueIDUtil.GenerateElement(dummy);
			element = element.Replace(EConversationUniqueIDUtil.ElementName, $"x_{EConversationUniqueIDUtil.ElementName}abc");

			var success = EConversationUniqueIDUtil.TryLoadFromEmailBody<DummyBaseBusinessObject>(Factory, element, out var result);

			Assert(success.Value);
			AssertEquals(dummy, result);
		}

		public void TestTryLoadFromEmailBody_IdModifiedByClient()
		{
			var dummy = Factory.NewWithValidTestData<DummyBaseBusinessObject>();
			var element = EConversationUniqueIDUtil.GenerateElement(dummy);
			element = element.Replace($"{dummy.TablePrefix}|{dummy.PK}", $"x_abc{dummy.TablePrefix}|{dummy.PK}abc");

			var success = EConversationUniqueIDUtil.TryLoadFromEmailBody<DummyBaseBusinessObject>(Factory, element, out var result);

			Assert(success.Value);
			AssertEquals(dummy, result);
		}

		public void TestTryLoadFromEmailBody_IdModifiedByClient_3CharTablePrefix()
		{
			var dummy = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			AssertEquals("Precondition", 3, dummy.TablePrefix.Length);
			var element = EConversationUniqueIDUtil.GenerateElement(dummy);
			element = element.Replace($"{dummy.TablePrefix}|{dummy.PK}", $"x_abc{dummy.TablePrefix}|{dummy.PK}abc");

			var success = EConversationUniqueIDUtil.TryLoadFromEmailBody<DummyDependantBusinessObject>(Factory, element, out var result);

			Assert(success.Value);
			AssertEquals(dummy, result);
		}

		public void TestTryLoadFromEmailBody_IdModifiedByClient_MutatedPK()
		{
			var dummy = Factory.NewWithValidTestData<DummyBaseBusinessObject>();
			var element = EConversationUniqueIDUtil.GenerateElement(dummy);
			element = element.Replace($"{dummy.TablePrefix}|{dummy.PK}", $"x_abc{dummy.TablePrefix}|{dummy.PK.ToString().Replace("-", "")}abc");

			var success = EConversationUniqueIDUtil.TryLoadFromEmailBody<DummyBaseBusinessObject>(Factory, element, out var result);

			Assert(success.Value);
			AssertEquals(dummy, result);
		}

		public void TestTryLoadFromEmailBody_NoElement()
		{
			var success = EConversationUniqueIDUtil.TryLoadFromEmailBody<DummyBaseBusinessObject>(Factory, "some email body", out var result);

			Assert(!success.HasValue);
			AssertNull(result);
		}

		public void TestTryLoadFromEmailBody_NoBizo()
		{
			var dummy = Factory.NewWithValidTestData<DummyBaseBusinessObject>();
			var element = EConversationUniqueIDUtil.GenerateElement(dummy);
			element = element.Replace($"{dummy.TablePrefix}|{dummy.PK}", $"{dummy.TablePrefix}|{Guid.NewGuid()}");

			var success = EConversationUniqueIDUtil.TryLoadFromEmailBody<DummyBaseBusinessObject>(Factory, element, out var result);

			Assert(!success.Value);
			AssertNull(result);
		}
	}
}
