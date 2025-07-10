namespace Enterprise.ResourceStrings.Maintenance
{
	public readonly struct GUIStringContextInfo
	{
		public static GUIStringContextInfo Parse(string applicationContext)
		{
			return new GUIStringContextInfo(applicationContext);
		}

		GUIStringContextInfo(string applicationContext)
		{
			int i = applicationContext.IndexOf(',');
			BasherAssemblyName = applicationContext.Substring(0, i);
			applicationContext = applicationContext.Substring(i + 1);
			i = applicationContext.IndexOf(':');
			BasherClassName = applicationContext.Substring(0, i);
			applicationContext = applicationContext.Substring(i + 1);
			i = applicationContext.IndexOf('@');
			if (i > -1)
			{
				TabContainerPath = applicationContext.Substring(0, i);
				SubContainerPath = applicationContext.Substring(i + 1);
			}
			else
			{
				TabContainerPath = applicationContext;
				SubContainerPath = "";
			}
		}

		public readonly string BasherAssemblyName;
		public readonly string BasherClassName;
		public readonly string TabContainerPath;
		public readonly string SubContainerPath;

		public string GroupingKey
		{
			get { return BasherAssemblyName + "," + BasherClassName + ":" + TabContainerPath; }
		}

		public string BasherNamespace
		{
			get { return BasherClassName.Remove(BasherClassName.LastIndexOf('.')); }
		}
	}
}
