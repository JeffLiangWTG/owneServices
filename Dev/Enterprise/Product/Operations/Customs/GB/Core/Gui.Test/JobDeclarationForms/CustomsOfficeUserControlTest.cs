using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.JobDeclarationForms.Testing
{
	public class CustomsOfficeUserControlTest : TestCaseWithFactory
	{
		public void TestCaptionWhenApplicationCodeChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var customsOfficesUserControl = form.FindSingle<CustomsOfficesUserControl>();
				AssertEquals("[29] Office of Exit", customsOfficesUserControl.FindSingle<ZCodeFindBox>(x => x.Name == "CustomsOfficeFindBox").GetExtension<ILabelCaptionRenderer>().Caption);
			}

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var customsOfficesUserControl = form.FindSingle<CustomsOfficesUserControl>();
				AssertEquals("[UCC 5/12] Customs Office of Exit", customsOfficesUserControl.FindSingle<ZCodeFindBox>(x => x.Name == "CustomsOfficeFindBox").GetExtension<ILabelCaptionRenderer>().Caption);

				declaration.JE_MessageType = "IMP";
				AssertEquals("[UCC 5/26] Customs Office of Presentation", customsOfficesUserControl.FindSingle<ZCodeFindBox>(x => x.Name == "CustomsOfficeFindBox").GetExtension<ILabelCaptionRenderer>().Caption);

				declaration.JE_MessageType = "MSC";
				AssertEquals("Customs Office", customsOfficesUserControl.FindSingle<ZCodeFindBox>(x => x.Name == "CustomsOfficeFindBox").GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestIsOverrideLabelByFriendlyName()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (var form = new ZForm(declaration))
			using (var userControl = new JobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var customsOfficesUserControl = (userControl.Controls.Find("CustomsOfficesUserControl", true).First() as ZDynamicControlCreationUserControl).HostedControl as CustomsOfficesUserControl;
				AssertEquals(false, customsOfficesUserControl.IsOverrideLabelByFriendlyName);
			}
		}
	}
}
