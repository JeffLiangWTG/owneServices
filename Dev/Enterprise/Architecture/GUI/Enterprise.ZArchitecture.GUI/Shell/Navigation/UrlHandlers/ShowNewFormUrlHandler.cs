using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture
{
	[ImmutableObject(true)]
	public class ShowNewFormUrlHandler : ShowFormUrlHandler
	{
		protected ShowNewFormUrlHandler()
		{
		}

		public static ShowNewFormUrlHandler Instance
		{
			get { return instance ?? (instance = new ShowNewFormUrlHandler()); }
		}
		static ShowNewFormUrlHandler instance;

		protected override string ExpectedCommandText
		{
			get { return "ShowNewForm"; }
		}

		protected internal override string[] GetQueryNamesSecuredBySecurityHash(QueryString queryString)
		{
			return new string[] { "LicenceCode", "ControllerID" };
		}

		protected override Form PerformFormAction(ControllerID controllerID, ZGuid pk, IEnumerable<string> args)
		{
			var controller = ZControllerFactory.Create(controllerID);

			using (controller.SetArgsForNewForm(args))
			{
				return (Form)controller.ShowNewForm();
			}
		}
	}
}
