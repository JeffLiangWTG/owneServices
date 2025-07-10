using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.GUI;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.Module
{
	public class PrintQueueModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public PrintQueueModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.PrintQueue; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.PrintQueue);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new PrintQueueFilterControl(GridCollection, (PrintQueueFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new StmPrintQueueCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new PrintQueueFilterBusinessObject();
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> menu = new List<MenuItem>(base.GetNewStandardMenuItems());
			menu.Remove(NewMenuItem);
			return menu.ToArray();
		}

		protected override IZForm ShowDeleteForm(BusinessObject selectedBusinessObject)
		{
			IZForm result = null;
			StmPrintQueue queueToDelete = (StmPrintQueue)selectedBusinessObject;

			if (queueToDelete.SQ_QueueDeleted.IsEmpty)
			{
				string message = Res.GetString("362a853f-78eb-4fef-9841-d6a17b67991c", "This printer is still installed on server {0}.\r\n\r\nIf you delete this printer, the service tasks on {1} will create it again and you will lose any custom settings you have made to this printer in {2}.\r\n\r\nAre you sure you want to delete this printer?", queueToDelete.SQ_ServerName, queueToDelete.SQ_ServerName, Core.Constants.ProductName);

				DialogResult dialogResult = Globals.Message.Show(message, Res.GetString("d7f743f6-3509-41fe-a9be-501c36faf5b9", "Warning"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

				if (dialogResult == DialogResult.Yes)
				{
					result = base.ShowDeleteForm(selectedBusinessObject);
				}
			}
			else
			{
				result = base.ShowDeleteForm(selectedBusinessObject);
			}
			return result;
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			result.Add(new ZMenuItem(ResString.GetMultilingualString("20501115-2202-487A-8F65-F8FE8A4E665D", "Replace with another print queue"), ReplacePrintQueue_Click));
			return result.ToArray();
		}

#if DEBUG
		internal
#endif
		void ReplacePrintQueue_Click(object sender, EventArgs args)
		{
			if (Env.Security.PrintQueuesModify.IsAllowed)
			{
				if (SelectedBusinessObjects.Any())
				{
					var bizo = new PrintQueueReplaceBizo((StmPrintQueue)SelectedBusinessObjects[0]);
					using (var printReplace = new PrintQueueReplaceForm(Factory, bizo, false))
					{
						ZFormModaliser.ShowDialogAndDispose(printReplace);
					}
				}
				else
				{
					Globals.Message.Show(Res.GetString("9EF8E6EC-D5F2-4F56-BBD0-31BACAC93A5B", "Please select a print queue to replace."));
				}
			}
			else
			{
				Env.Security.PrintQueuesModify.ShowError();
			}
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.PrintQueues; }
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion

		#region IOperationalActionSupportable

		public OperationalActionSupporter OperationalActionSupporter => new StmPrintQueueActionSupporter();

		#endregion
	}
}
