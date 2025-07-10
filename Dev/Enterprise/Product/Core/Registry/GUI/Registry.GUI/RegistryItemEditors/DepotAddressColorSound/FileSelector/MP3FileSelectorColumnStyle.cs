using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class MP3FileSelectorColumnStyle : ZCodeFindBoxColumnStyle
	{
		public MP3FileSelectorColumnStyle(MP3FileSelectorColumnStyleInfo info)
			: base(() => new FileSelectorFindBox(".mp3"), info)
		{
		}
	}

	public class MP3FileSelectorColumnStyleInfo : ZCodeFindBoxColumnStyleInfo
	{
		public override Type ColumnStyleType
		{
			get
			{
				return typeof(MP3FileSelectorColumnStyle);
			}
		}
	}
}
