using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[CodeAlive("Please delete this dummy form after the first real form has been added to the project. This dummy form prevents TestNoUnusedWinformsDirectives from failing.")]
[TestExcludeZWinFormsAllHaveFormBashers]
public partial class Form1 : ZChildForm
{
	// Please delete this dummy form after the first real form has been added to the project.
	// This dummy form prevents TestNoUnusedWinformsDirectives from failing.
	public Form1()
	{
		InitializeComponent();
	}
}
