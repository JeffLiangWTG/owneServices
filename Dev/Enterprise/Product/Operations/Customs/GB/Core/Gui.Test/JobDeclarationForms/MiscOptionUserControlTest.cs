using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.JobDeclarationForms.Testing
{
	class MiscOptionUserControlTest : TestCaseWithFactory
	{
		public void TestCaptionForChief()
		{
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			using (var form = new ZForm(declaration))
			using (var control = new MiscOptionsUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				AssertEquals("[14] Rep. Type", control.FindSingle<ZDropEdit>(x => x.Name == "RepresentationDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("[44] Supporting Documents", control.FindSingle<ZTabPage>(x => x.Name == "SupportingDocumentTabPage").GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("[40] Previous Documents", control.FindSingle<ZTabPage>(x => x.Name == "PreviousDocumentTabPage").GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("[44] Additional Info", control.FindSingle<ZTabPage>(x => x.Name == "AdditionalInfoTabPage").GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("[52] Guarantees", control.FindSingle<ZTabPage>(x => x.Name == "GuaranteesTabPage").GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestCaptionForCDS()
		{
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			using (var form = new ZForm(declaration))
			using (var control = new MiscOptionsUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				AssertEquals("[UCC 3/21] Rep. Type", control.FindSingle<ZDropEdit>(x => x.Name == "RepresentationDropEdit").GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("[UCC 2/3 && 8/7] Supporting Documents", control.FindSingle<ZTabPage>(x => x.Name == "SupportingDocumentTabPage").GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("[UCC 2/1] Previous Documents", control.FindSingle<ZTabPage>(x => x.Name == "PreviousDocumentTabPage").GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("[UCC 2/2] Additional Info", control.FindSingle<ZTabPage>(x => x.Name == "AdditionalInfoTabPage").GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("[UCC 8/3] Guarantees", control.FindSingle<ZTabPage>(x => x.Name == "GuaranteesTabPage").GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestNIProtocolControl()
		{
			using (ZForm form = new ZForm(declaration))
			using (var misc = new MiscOptionsUserControl())
			{
				misc.JobDeclaration = declaration;
				form.Controls.Add(misc);
				form.Show();
				var control = misc.FindSingle<NIProtocolUserControl>();
				AssertNotNull(control);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;
	}
}
