using System.Collections;
using System.Collections.Generic;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Services.OperationalActions.Support.Testing
{
	public static class TestHelper
	{
		public static void SelectExact(ZGrid grid, params BusinessObject[] objects)
		{
			IList toSelect = objects;
			IList managerList = grid.ListManager.List;
			for (int i = 0; i < managerList.Count; i++)
			{
				if (toSelect.Contains(managerList[i]))
				{
					grid.Select(i);
				}
				else
				{
					grid.UnSelect(i);
				}
			}
		}

		public static string RenderSelection(IDictionary<ZGuid, string> map, ISelectedRecords records)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine();
			builder.Append("AutoSelectedAllKeys: ");
			builder.Append(records.AutoSelectedAllKeys);
			builder.AppendLine();
			ZGuid[] targets = records.PrimaryKeys;
			for (int i = 0; i < targets.Length; i++)
			{
				builder.Append("  ");
				string label;
				if (map.TryGetValue(targets[i], out label))
				{
					builder.AppendLine(label);
				}
				else
				{
					builder.AppendLine(targets[i].ToString());
				}
			}

			return builder.ToString();
		}
	}
}
