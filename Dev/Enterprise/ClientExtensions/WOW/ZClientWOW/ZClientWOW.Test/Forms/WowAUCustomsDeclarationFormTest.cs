using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using Bill = Enterprise.Customs.Business.Bill;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WowAUCustomsDeclarationForm))]
	public class WowAUCustomsDeclarationFormTest : ZFormBasherTest
	{
		[ExpectNoExceptions]
		public void TestLoadForm()
		{
			JobDeclaration bO = Factory.New<JobDeclaration>();
			bO.ApportionmentDirty = false;
			ZController controller = ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);
			controller.ShowNewForm();
			Application.DoEvents();
			using (controller.LastShownForm)
			{
				AssertEquals("Correct form type created", typeof(WowAUCustomsDeclarationForm), controller.LastShownForm.GetType());
			}
		}

		#region Implementation
		protected override Form GetFormToBashCore()
		{
			WoolworthsJobDeclaration declaration = GetPopulatedDeclaration();
			Factory.Save();
			WowAUCustomsDeclarationForm result = new WowAUCustomsDeclarationForm(declaration);
			result.ControllerID = ControllerIDs.Customs.JobDeclaration;
			return result;
		}

		WoolworthsJobDeclaration GetPopulatedDeclaration()
		{
			WoolworthsJobDeclaration declaration = Factory.New<WoolworthsJobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "CMR";
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			JobComInvoiceGroupHeader header = declaration.JobComInvoiceGroupHeaders[0];
			JobComInvoiceHeader invoice = header.JobComInvoiceHeaders.AddNew();
			CusContainer container = declaration.CusContainers.AddNew();
			Bill bill = declaration.Bills.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			return declaration;
		}
		#endregion
	}
}
