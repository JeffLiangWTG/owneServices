using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment.OverrideLevels;

namespace Enterprise.ZArchitecture.Environment.Registry
{
	public class RegistryItemSaveCsvHandler : IRegistryItemSaveHandler
	{
		#region Saving
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "CSV Header")]
		public void SaveItems(IEnumerable<IRegistryItem> itemsToSaveToStream, IOverrideLevel levelToSaveFor, Stream stream)
		{
			using (var writer = new StreamWriter(stream))
			{
				writer.WriteLine("Category,Caption");
				foreach (var item in itemsToSaveToStream)
				{
					var csvLine = new OCsvLine(new string[] { item.Category, item.Caption });
					writer.WriteLine(csvLine.ToString());
				}
			}
		}
		#endregion

		public LoadedRegistryItems LoadItems(IEnumerable<IRegistryItem> allRegistryItems, Stream stream)
		{
			throw new NotImplementedException();
		}
	}
}
