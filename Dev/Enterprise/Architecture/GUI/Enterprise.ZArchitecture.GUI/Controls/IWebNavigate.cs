namespace Enterprise.ZArchitecture.GUI
{
	public interface IWebNavigate
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
		void Navigate(string urlString);
	}
}