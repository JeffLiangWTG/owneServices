using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting
{
	public partial class CalculationXMLUserControl : ZUserControl
	{
		public CalculationXMLUserControl()
		{
			InitializeComponent();
		}

		public bool ShowRevenue { get; set; }

		void auditLogNoteButton_Click(object sender, EventArgs e)
		{
			var loadOption = ShowRevenue ? CalculationLogsLoader.LoadOption.Revenue : CalculationLogsLoader.LoadOption.Costing;

			if (CurrentDataItem != null)
			{
				var bizObj = CurrentDataItem as BusinessObject;
				var note = CalculationLogsLoader.Load(bizObj, loadOption);

				if (note != null)
				{
					using (CalculationXMLTextForm form = new CalculationXMLTextForm(note.ToFormattedXML()))
					{
						ZFormModaliser.ShowDialogAndDispose(form);
						return;
					}
				}
			}

			Globals.Message.Show(Res.GetString("a03a6c08-2612-447b-9bb9-4a48c9395393", @"The {0} does not exist. {1}", AutoRatingRunner.CalculationXML, AutoRatingRunner.CalculationXMLDoesNotExistReason));
		}
	}
}
