using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Main.Navigation.ViewModels;

#nullable disable
[CodeAlive("Used in Winzor as a view model for session related data")]
public class SessionContextViewModel
{
	public string UserName { get; set; }
	public string Branch { get; set; }
	public string Company { get; set; }
	public string Department { get; set; }
	public byte[] UserImage { get; set; }
}
