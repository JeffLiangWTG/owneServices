using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(IMDSupervisorOverrides))]
	sealed class IMDSupervisorOverridesTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetMessageErrorCollector()
		{
			var declaration = Factory.New<JobDeclaration>();
			var imdSupervisorOverrides = new IMDSupervisorOverridesForTest(declaration, Customs.Business.SupervisorOverridesContext.SendingMessages);
			AssertType<IMDMultiMessageManager.ZNotificationCollectorMinusCPDecQuestionsAndDuplicates>(imdSupervisorOverrides.MessageErrorCollectorExposed);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new IMDSupervisorOverrides(declaration, Customs.Business.SupervisorOverridesContext.SendingMessages);
		}

		class IMDSupervisorOverridesForTest : IMDSupervisorOverrides
		{
			public IMDSupervisorOverridesForTest(IBusiness businessEntity, string context)
				: base(businessEntity, context)
			{
			}

			public Customs.Business.CustomsNotificationCollector MessageErrorCollectorExposed => GetMessageErrorCollector(this.businessEntity);
		}
	}
}
