using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture
{
	[ImmutableObject(true)]
	public class ShowEditFormUrlHandler : ShowFormUrlHandler, IShowEditFormUrlCreator
	{
		protected ShowEditFormUrlHandler()
		{
		}

		public static ShowEditFormUrlHandler Instance
		{
			get { return instance ?? (instance = new ShowEditFormUrlHandler()); }
		}
		static ShowEditFormUrlHandler instance;

		protected override string ExpectedCommandText
		{
			get { return "ShowEditForm"; }
		}

		protected override Form PerformFormAction(ControllerID controllerID, ZGuid pk, IEnumerable<string> args)
		{
			var result = ZControllerFactory.GetCorrectControllerAndBusinessObject(controllerID, pk, IsReportError);
			var controller = result.Controller;
			var bizO = result.BusinessObject;

			using (controller.SetArgsForNewForm(args))
			{
				return (Form)controller.ShowEditForm(bizO);
			}
		}

		#region IShowEditFormUrlCreator Members

		string IShowEditFormUrlCreator.Create(ControllerID controllerID, Guid pk)
		{
			return Create(controllerID, pk);
		}

		string IShowEditFormUrlCreator.Create(IControllerIDProvider provider)
		{
			return provider != null && provider.BusinessObjectPK != Guid.Empty ? ((IShowEditFormUrlCreator)this).Create(provider.ControllerID, provider.BusinessObjectPK) : null;
		}

		#endregion
	}
}
