using System;
using System.Collections.Generic;
using System.IO;
using Enterprise.ClientSharedComponents;
using Enterprise.Environment;

namespace Enterprise.Client.YAS.ServiceTasks.Testing
{
	public class YASTestHelper : SharedTestHelper
	{
		public YASTestHelper() : base() { }

		public void TidyUp()
		{
			foreach (string folderFullName in TempTestFolders)
			{
				foreach (string fileName in Directory.GetFiles(folderFullName))
				{
					if (File.Exists(fileName))
					{
						File.Delete(fileName);
					}
				}
				if (folderFullName != Env.TempPath && Directory.Exists(folderFullName))
				{
					Directory.Delete(folderFullName);
				}
			}
		}

		internal List<String> TempTestFolders
		{
			get { return tempTestFolders ?? (tempTestFolders = new List<string>()); }
		}
		List<String> tempTestFolders;
	}
}
