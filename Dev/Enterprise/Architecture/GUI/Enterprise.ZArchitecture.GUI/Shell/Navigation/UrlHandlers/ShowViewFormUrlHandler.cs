using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture
{
	[ImmutableObject(true)]
	public class ShowViewFormUrlHandler : ShowFormUrlHandler, IShowViewFormUrlCreator
	{
		protected ShowViewFormUrlHandler()
		{
		}

		public static ShowViewFormUrlHandler Instance
		{
			get { return instance ?? (instance = new ShowViewFormUrlHandler()); }
		}
		static ShowViewFormUrlHandler instance;

		protected override string ExpectedCommandText
		{
			get { return "ShowViewForm"; }
		}

		protected override Form PerformFormAction(ControllerID controllerID, ZGuid pk, IEnumerable<string> args)
		{
			var result = ZControllerFactory.GetCorrectControllerAndBusinessObject(controllerID, pk, IsReportError);
			var controller = result.Controller;
			var bizO = result.BusinessObject;

			using (controller.SetArgsForNewForm(args))
			{
				return (Form)controller.ShowViewForm(bizO);
			}
		}

		string IShowViewFormUrlCreator.Create(ControllerID controllerID, Guid pk) => Create(controllerID, pk);
	}
}
