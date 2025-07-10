using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	internal sealed partial class ControllerAction : HyperlinkAction<LogControllerLink>
	{
		public ControllerAction(LogControllerLink hyperlink, string key)
			: base(hyperlink, key) { }

		public override void DoAction()
		{
			var controller = ZControllerFactory.Create(Hyperlink.Controller);
			LogController(controller);

			var bizObj = controller.Factory.Load(controller.TypeOfTopLevelBusinessObject, Hyperlink.PK);

			if (bizObj == null)
			{
				var message = Res.GetString("d0c79a9b-b249-44b3-b43c-363199b2b1ef", "This record could not be found in the database, it was most likely deleted.");
				var caption = Res.GetString("18955d72-499b-47e9-afd2-9a0eedf933f7", "Record not found");

				Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, DialogResult.OK);
			}
			else
			{
				controller.ShowEditForm(bizObj);
			}
		}

		partial void LogController(ZController controller);
	}
}

#region Test
#if DEBUG

#region Test Members

namespace Enterprise.ZArchitecture.GUI
{
	using Enterprise.ZArchitecture.Environment;

	partial class ControllerAction
	{
		partial void LogController(ZController controller)
		{
			if (Globals.IsTest)
			{
				lastUsedControllerForTesting = controller;
			}
		}

		public ZController lastUsedControllerForTesting;
	}
}

#endregion

#endif
#endregion
