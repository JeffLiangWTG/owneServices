namespace Enterprise.ZArchitecture.Modules
{
	using Enterprise.ZArchitecture.GUI;

	public abstract class ZSimpleUrlLauncherModule : ZModule
	{
		public abstract System.Uri Url { get; }

		public virtual void Show()
		{
			if (Url != null)
			{
				WebUrlLauncher.Launch(Url.AbsoluteUri);
			}
		}
	}
}
