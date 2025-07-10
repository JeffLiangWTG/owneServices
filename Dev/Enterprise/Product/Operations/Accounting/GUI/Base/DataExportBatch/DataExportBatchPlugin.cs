#define CODE_ANALYSIS

using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business.DataExportBatch;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Accounting.GUI.DataExportBatch
{
	public class DataExportBatchPlugin : ZPlugIn
	{
		public DataExportBatchPlugin(IDataExportBatchSource source)
			: base(source)
		{
		}

		public override string Name
		{
			get { return (NoResString)"Data Export Batch"; } // Plugin's hard-coded name
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		#region User Control

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override Control GetNewUserControl()
		{
			var control = new DataExportBatchUserControl();
			control.Dock = DockStyle.Fill;
			return control;
		}

		#endregion
	}
}
