using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.BuildTools;
using CargoWise.Common;

namespace Enterprise.ResourceStrings.Business
{
	public class ResourceStringsMetaData
	{
		public static ResourceStringsMetaData GetInstance()
		{
			ResourceStringsMetaData metaData;
			if (instance.Value != null && (metaData = instance.Value.Target as ResourceStringsMetaData) != null)
			{
				return metaData;
			}
			metaData = new ResourceStringsMetaData();
			instance.Value = new WeakReference(metaData);
			return metaData;
		}

		readonly static Overridable<WeakReference> instance = new Overridable<WeakReference>(null);

		ResourceStringsMetaData()
		{ }

		public void Attach(HelpDataString data)
		{
			Entry entry;
			List.TryGetValue(data.HD_Code, out entry);
			if (entry != null)
			{
				data.HD_ControlPath = entry.HD_ControlPath;
				data.HD_ControlIndexInParent = entry.HD_ControlIndexInParent;
			}
		}

		void LoadData()
		{
			list = new Dictionary<string, Entry>(20000, StringComparer.InvariantCultureIgnoreCase);
#if DEBUG
			using (var reader = new StreamReader(File))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					string[] parsed = line.Split('\t');
					list.Add(string.Intern(parsed[0]), new Entry(parsed));
				}
			}
#endif
		}

#if DEBUG
		public void Save(HelpDataString[] update)
		{
			bool changed = false;
			foreach (var item in update)
			{
				if (item.HasChanges)
				{
					List[item.HD_Code] = new Entry(item.HD_ControlPath, item.HD_ControlIndexInParent);
					changed = true;
				}
			}
			if (changed)
			{
				SourceControl.EnterpriseDatabase.CheckOut(File, false);
				using (var writer = new StreamWriter(File))
				{
					foreach (var entry in list)
					{
						writer.Write(entry.Key);
						writer.Write('\t');
						writer.Write(entry.Value.HD_ControlPath);
						writer.Write('\t');
						writer.Write(entry.Value.HD_ControlIndexInParent);
						writer.WriteLine();
					}
				}
			}
		}

		public string File
		{
			get { return Path.Combine(UseSourceFile ? ResourcesDeltaSource.GetSourceControlDirectory(Res.DefaultLanguage) : AssemblyLoader.GetBinPath(), "ResourceStringsMetaData.txt"); }
		}

		readonly static Overridable<bool> useSourceFileOverride = new Overridable<bool>(false);

		public static bool UseSourceFile
		{
			get => useSourceFileOverride.Value;
			set => useSourceFileOverride.Value = value;
		}

#endif
		public IEnumerable<Entry> Entries
		{
			get { return List.Values; }
		}

		Dictionary<string, Entry> List
		{
			get
			{
				if (list == null)
				{
					LoadData();
				}
				return list;
			}
		}

		Dictionary<string, Entry> list;

		public class Entry
		{
			public Entry(string hD_ControlPath, int hD_ControlIndexInParent)
			{
				this.HD_ControlPath = hD_ControlPath;
				this.HD_ControlIndexInParent = hD_ControlIndexInParent;
			}

			public Entry(string[] parsedLine)
			{
				HD_ControlPath = string.Intern(parsedLine[1]);
				HD_ControlIndexInParent = string.IsNullOrEmpty(parsedLine[2]) ? 0 : int.Parse(parsedLine[2]);
			}

			public readonly string HD_ControlPath;
			public readonly int HD_ControlIndexInParent;
		}
	}
}
