using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public static class LogHelper
	{
		public static void BuildLog(ZStringBuilder log, string desc, ZPropertyInfo info)
		{
			if (info.HasChanges && log.Length < StmALogSchema.SL_Reference.MaxLength)
			{
				log.Append(string.Concat(desc, info.OriginalValue, "=>", info.Value));
			}
		}

		public static ZString AddChangeLog(ZString log, string desc, ZPropertyInfo info)
		{
			if (info.HasChanges && log.Length < StmALogSchema.SL_Reference.MaxLength)
			{
				log = AddChangeLog(log, desc, info.Value, info.OriginalValue);
			}

			return log;
		}

		public static ZString AddShortDateChangeLog(ZString log, string desc, ZPropertyInfo info)
		{
			if (info.HasChanges && log.Length < StmALogSchema.SL_Reference.MaxLength)
			{
				log = AddChangeLog(log, desc, ((ZDateTime)info.Value).ToShortDateString(), ((ZDateTime)info.OriginalValue).ToShortDateString());
			}

			return log;
		}

		public static ZString AddChangeLog(ZString log, string desc, object newValue, object oldValue)
		{
			if (log.Length < StmALogSchema.SL_Reference.MaxLength)
			{
				log = string.Concat(log, !log.IsEmpty ? " " : string.Empty, desc, ": ", newValue, "(", oldValue, ")");
			}

			return log;
		}
	}
}

