using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Client.EDI.MasterFiles.GUI.Deduplication
{
	public class EDIDuplicateAlertControlHelper : DuplicateAlertControlHelper
	{
		protected override Form GetForm(DuplicationEventArgs e)
		{
			return new EDIDeduplicationResultsViewerForm(e.Master as BusinessObject, e.TargetObjects as IEnumerable<object>, e.Results, e.ResultsModels, e.SelectedMasterPK);
		}
	}
}
