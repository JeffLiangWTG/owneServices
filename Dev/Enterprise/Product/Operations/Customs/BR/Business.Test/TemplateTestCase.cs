using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class TemplateTestCase : TestCaseWithFactory
	{
		protected void AssertTemplate()
		{
			Assert("Please implement a test before checkin", IsTemplate);
		}

		protected bool IsTemplate
		{
			get
			{
				return GetType().FullName.IndexOf("BR") > -1;
			}
		}
	}
}
