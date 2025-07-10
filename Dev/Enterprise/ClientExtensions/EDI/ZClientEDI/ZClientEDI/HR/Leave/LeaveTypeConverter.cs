using System;
using System.Collections.Generic;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.HR
{
	public sealed class LeaveTypeConverter
	{
		public LeaveTypeConverter(ReadOnlyCodeDescriptionPairList leaveTypes)
		{
			leaveMap = new Dictionary<string, string>(leaveTypes.Count, StringComparer.OrdinalIgnoreCase);
			foreach (ICodeDescription item in leaveTypes)
			{
				leaveMap[item.Code] = item.Description;
			}
			DefaultInternalLeaveType = leaveTypes[0].Description;
		}

		readonly Dictionary<string, string> leaveMap;
		public string DefaultInternalLeaveType { get; private set; }

		public string ConvertToInternalLeaveType(string externalLeaveType)
		{
			string result;
			if (!leaveMap.TryGetValue(externalLeaveType, out result))
			{
				result = DefaultInternalLeaveType;
			}

			return result;
		}
	}
}
