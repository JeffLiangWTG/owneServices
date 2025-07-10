using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.Testing
{
	[TestedType(typeof(UPEAUCustomsDeclarationForm))]
	internal class UPEAUCustomsDeclarationFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			UPEJobDeclaration declaration = Factory.New<UPEJobDeclaration>();
			declaration.ApportionmentDirty = false;
			UPEAUCustomsDeclarationForm result = new UPEAUCustomsDeclarationForm(declaration);
			result.ControllerID = ControllerIDs.Customs.JobDeclaration;
			return result;
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
