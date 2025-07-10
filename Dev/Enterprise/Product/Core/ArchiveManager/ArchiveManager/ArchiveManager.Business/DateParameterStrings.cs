using System;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business
{
	public static class DateParameterStrings
	{
		public static ZString GetCode(DateParameterType dateParameterType)
		{
			switch (dateParameterType)
			{
				case DateParameterType.JCL:
					return "JCL";
				case DateParameterType.JOP:
					return "JOP";
				default:
					throw new ArgumentOutOfRangeException(nameof(dateParameterType));
			}
		}

		public static ZString GetMultilingualName(SchemaDateTimeColumn column)
			=> column.Name switch
			{
				JobHeaderSchema.Constants.JH_A_JCL => Res.GetString("2d324942-adf0-4585-9c56-e330205ddd0a", "Job Close Date"),
				JobHeaderSchema.Constants.JH_A_JOP => Res.GetString("865452b2-912f-41eb-98b0-1aa273671988", "Job Open Date"),
				JobHeaderSchema.Constants.JH_SystemCreateTimeUtc => Res.GetString("d0ea94e2-df50-448f-8e6d-4c71105ff4ff", "Job Create Time (UTC)"),
				_ => throw new ArgumentOutOfRangeException(nameof(column), $"No registered name for {column.Name}"),
			};

		public static ZString GetName(bool isFilteringByJobOpenDate)
		{
			return isFilteringByJobOpenDate ? (NoResString)"Job Open Date" : (NoResString)"Job Close Date";
		}
	}
}
