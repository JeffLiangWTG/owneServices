using System.Reflection;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// The default error dialog that appears when no IErrorReporter interface is registered.
	/// </summary>
	/// 
	[CodeAlive("Used Code in tests at DefaultErrorReporterTest via reflection")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1089:DoNotUseAssemblyGetEntryAssemblyAnalyzer", Justification = "Baseline")]
	public partial class DefaultErrorDialog : KForm
	{
		public DefaultErrorDialog()
		{
			InitializeComponent();
			Assembly entryAssembly = Assembly.GetEntryAssembly();
			Text += entryAssembly == null ? (NoResString)"(unknown application)" : Assembly.GetEntryAssembly().GetName().Name;
		}

		public string Detail
		{
			get { return txtDetails.Text; }
			set { txtDetails.Text = value; }
		}
	}
}
