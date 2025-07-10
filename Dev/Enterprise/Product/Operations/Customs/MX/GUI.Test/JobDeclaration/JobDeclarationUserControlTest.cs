using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.MX.Business;
using NUnit.Framework;

namespace Enterprise.Customs.MX.GUI.Testing
{
	[TestedType(typeof(JobDeclarationUserControl))]
	class JobDeclarationUserControlTest : BaseCustomsDeclarationUserControlAbstractTest<JobDeclarationUserControl, JobDeclaration>
	{
		public void TestCustomsAreaUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			using (var declarationForm = new JobDeclarationForm(declaration))
			{
				using (var declarationUserControl = declarationForm.CustomsBrokerageUserControl.DeclarationUserControlForTesting as JobDeclarationUserControl)
				{
					declarationForm.Show();
					using (IDisposable control = declarationUserControl.CustomsAreaUserControl)
					{
						AssertType<CustomsAreaUserControl>(control);
					}
				}
			}
		}

		public void TestCustomsControlCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			using (var declarationForm = new JobDeclarationForm(declaration))
			{
				using (var declarationUserControl = declarationForm.CustomsBrokerageUserControl.DeclarationUserControlForTesting as JobDeclarationUserControl)
				{
					var control = declarationUserControl.CustomsAreaUserControl;
					declarationForm.Show();
					AssertEquals("Exit Area", control.EntryOrExitAreaFindBox.GetExtension<LabelCaptionRenderer>().Caption);

					declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
					AssertEquals("Entry Area", control.EntryOrExitAreaFindBox.GetExtension<LabelCaptionRenderer>().Caption);

					AssertEquals("Clearance Area", control.ClearanceAreaFindBox.GetExtension<LabelCaptionRenderer>().Caption);
				}
			}
		}
	}
}
