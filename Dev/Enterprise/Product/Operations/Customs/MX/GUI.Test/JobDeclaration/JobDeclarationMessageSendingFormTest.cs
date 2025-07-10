using System.Windows.Forms;
using Enterprise.Customs.MX.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.GUI.Testing
{
	[TestedType(typeof(JobDeclarationMessageSendingForm))]
	public class JobDeclarationMessageSendingFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var sendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);
			return new JobDeclarationMessageSendingForm(sendingObjectParent);
		}
	}
}
