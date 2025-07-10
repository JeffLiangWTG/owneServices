namespace Enterprise.Builder.GenerateDbUpgraderResources
{
	class VersionInfo
	{
		public VersionInfo(int number, bool isMajor)
		{
			this.number = number;
			this.isMajor = isMajor;
			this.resxTag = (isMajor) ? MajorVersionTagName : MinorVersionTagName;
		}

		public string ResxTag
		{
			get { return resxTag; }
		}

		readonly string resxTag;

		public int Number
		{
			get { return number; }
		}

		readonly int number;

		public bool IsMajor
		{
			get { return isMajor; }
		}

		readonly bool isMajor;

		internal const string MajorVersionTagName = "MAJOR_VERSION";
		internal const string MinorVersionTagName = "MINOR_VERSION";
	}

	class VersionInfoPair
	{
		public VersionInfoPair(int majorNumber, int minorNumber)
		{
			major = new VersionInfo(majorNumber, true);
			minor = new VersionInfo(minorNumber, false);
		}

		public VersionInfo Major
		{
			get { return major; }
		}

		readonly VersionInfo major;

		public VersionInfo Minor
		{
			get { return minor; }
		}

		readonly VersionInfo minor;
	}
}
