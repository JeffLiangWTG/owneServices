using System.Collections.Generic;

namespace Enterprise.ArchiveManager.Engine.Test.TestDoubles
{
	public class ArchiveImage
	{
		public ArchiveImage(byte[] image)
		{
			Image = image;
			MetaDataList = new List<KeyValuePair<string, string>>();
		}

		public List<KeyValuePair<string, string>> MetaDataList { get; private set; }

		#region IArchiveImage Members

		public IEnumerable<KeyValuePair<string, string>> MetaData
			=> MetaDataList;

		public byte[] Image { get; private set; }

		#endregion
	}
}
