using System.Windows.Forms;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MailManager.GUI.Testing
{
	[TestedType(typeof(MailItemTemplateForm))]
	internal sealed class MailItemTemplateFormBasherTest : ZFormBasherTest
	{
#region Implementation
		protected override Form GetFormToBashCore()
		{
			return new MailItemTemplateForm(Factory.New<MailItemTemplate>());
		}

#endregion
		public void TestFormCaption()
		{
			var template = Factory.New<MailItemTemplate>();
			template.MIT_Category = "SHP";
			template.MIT_Name = "T1";
			using (var form = new MailItemTemplateForm(template))
			{
				AssertEquals("Email Template - SHP_T1", form.FormCaption);
			}
		}
	}
}
