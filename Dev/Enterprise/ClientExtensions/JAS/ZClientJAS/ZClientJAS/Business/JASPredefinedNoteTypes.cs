using CargoWise.Definitions;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.JAS.Business
{
	public class JASPredefinedNoteTypes : PredefinedNoteTypes
	{
		protected JASPredefinedNoteTypes()
		{
		}

		#region Instance

		public new static JASPredefinedNoteTypes Instance
		{
			get { return (JASPredefinedNoteTypes)PredefinedNoteTypes.Instance; }
		}

		public static void RegisterThisSubTypeOverride()
		{
			PredefinedNoteTypes.OverrideNewDelegate(New);
		}

		static PredefinedNoteTypes New()
		{
			return new JASPredefinedNoteTypes();
		}

		#endregion

		public JASPredefinedNoteType JXCExportLog
		{
			get
			{
				if (fJXCExportLog == null)
				{
					fJXCExportLog = new JASPredefinedNoteType("JXC Export Log", StmNoteVisibility.PRV, true, true, true);
				}
				return fJXCExportLog;
			}
		}

		JASPredefinedNoteType fJXCExportLog;
	}
}
