using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Module.Testing
{
	[TestedType(typeof(ExitControlController))]
	sealed class ExitControlControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.ExitControl;

		public void TestOpenFormForDeclaration()
		{
			var controller = new ExitControlController();
			var exitHeader = GetExitForTesting(Factory);
			var declaration = Factory.New<JobDeclaration>();
			exitHeader.Parent = declaration;
			Factory.Save();

			using (var form = controller.ShowEditForm(exitHeader) as ZForm)
			{
				form.Show();
				Application.DoEvents();
				AssertType<EU.GUI.JobDeclarationForm>("Should open a Declaration form for exit header ", form);
			}
		}

		public void TestOpenFormForShipment()
		{
			var controller = new ExitControlController();
			var exitHeader = GetExitForTesting(Factory);
			exitHeader.Parent = Factory.New<ForwardingShipment>();

			Factory.Save();

			using (var form = controller.ShowEditForm(exitHeader) as ZForm)
			{
				form.Show();
				Application.DoEvents();
				AssertType<ShipmentForm>("Should open a Shipment Form for exit header ", form);
			}
		}

		public void TestOpenFormForConsol()
		{
			var controller = new ExitControlController();
			var exitHeader = GetExitForTesting(Factory);
			exitHeader.Parent = Factory.New<ForwardingConsol>();

			Factory.Save();

			using (var form = controller.ShowEditForm(exitHeader) as ZForm)
			{
				form.Show();
				Application.DoEvents();
				AssertType<ConsolForm>("Should open a Consol Form for exit header ", form);
			}
		}

		internal static CusExitHeader GetExitForTesting(BusinessObjectFactory factory)
		{
			var header1 = factory.NewWithValidTestData<CusExitHeader>();
			header1.CXH_JobReference = header1.PK.ToString().Substring(0, 35);
			var consignment1 = header1.CusExitConsignments.AddNew();

			var report1 = factory.NewWithValidTestData<CusExitReport>();
			report1.CER_CXC_Consignment = consignment1.PK;
			report1.CER_OfficeOfExit = "DE001";
			report1.CER_CXH_Header = header1.PK;

			return header1;
		}
	}
}
