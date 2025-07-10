using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Mail.Business
{
	public class EDIMailDBItemTemplateLookups : MailDBItemTemplateLookups
	{
		protected EDIMailDBItemTemplateLookups()
			: base()
		{
		}

		#region Register/Unregister SubType Override

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		#endregion

		public static new MailDBItemTemplateLookups New()
		{
			return new EDIMailDBItemTemplateLookups();
		}

		protected override CodeDescriptionPairList GetNewMailTemplateCategoriesCore()
		{
			var list = base.GetNewMailTemplateCategoriesCore();
			list.AddRange(new EDIMailTemplateCategoryList());
			return list;
		}
	}
}
