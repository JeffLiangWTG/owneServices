namespace Enterprise.ZArchitecture.GUI.WebLauncher
{
	public interface IWebUrlEdientParser
	{
		bool TryParse(string urlString, out string result);
		string Parse(string urlString);
	}
}
