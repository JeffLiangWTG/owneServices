using System;
using CargoWise.Types;

namespace Enterprise.ZArchitecture
{
	public delegate void FilenameEventHandler(object sender, FilenameEventArgs ea);

	public class FilenameEventArgs : EventArgs
	{
		public ZString UnmappedFilename
		{
			get;
			set;
		}
	}
}
