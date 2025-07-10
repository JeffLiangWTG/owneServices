using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture
{
	[ImmutableObject(true)]
	public class ShowDeleteFormUrlHandler : ShowFormUrlHandler
	{
		protected ShowDeleteFormUrlHandler()
		{
		}

		public static ShowDeleteFormUrlHandler Instance
		{
			get { return instance ?? (instance = new ShowDeleteFormUrlHandler()); }
		}
		static ShowDeleteFormUrlHandler instance;

		protected override string ExpectedCommandText
		{
			get { return "ShowDeleteForm"; }
		}

		protected override Form PerformFormAction(ControllerID controllerID, ZGuid pk, IEnumerable<string> args)
		{
			var result = ZControllerFactory.GetCorrectControllerAndBusinessObject(controllerID, pk, IsReportError);
			var controller = result.Controller;
			var bizO = result.BusinessObject;

			using (controller.SetArgsForNewForm(args))
			{
				return (Form)controller.ShowDeleteForm(bizO);
			}
		}

		protected override bool IsReportError
		{
			get
			{
				return true;
			}
		}
	}
}
