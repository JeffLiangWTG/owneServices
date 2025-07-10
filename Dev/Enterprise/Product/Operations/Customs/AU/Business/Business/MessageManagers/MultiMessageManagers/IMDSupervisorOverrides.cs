using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class IMDSupervisorOverrides : Customs.Business.SupervisorOverrides
	{
		public IMDSupervisorOverrides(IBusiness businessEntity, string context)
			: base(businessEntity, context)
		{
		}

		protected override Customs.Business.CustomsNotificationCollector GetMessageErrorCollector(IBusiness entity)
		{
			return new IMDMultiMessageManager.ZNotificationCollectorMinusCPDecQuestionsAndDuplicates(entity);
		}
	}
}
