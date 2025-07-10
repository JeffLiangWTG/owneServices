using NLog;
using NLog.Config;
using NLog.Targets;

namespace ServiceManager.Logging.CW
{
	class NLogColoredConsoleTargetFactory : INLogTargetFactory
	{
		public Target GetOrCreateTarget()
		{
			var x = LogManager.Configuration;

			LogManager.Configuration ??= new LoggingConfiguration();

			var target = LogManager.Configuration.FindTargetByName<ColoredConsoleTarget>(TargetName);
			if (target != null)
			{
				return target;
			}

			target = new ColoredConsoleTarget();
			target.Name = TargetName;
			target.Layout = target
				.Layout
				.ToString()!
				.Replace(
					"${message:withexception=true}",
					"${replace:searchFor=\u2192:replaceWith=\t:${replace:searchFor=\u21B5:replaceWith=\r\n:${message:withexception=true}}}");

			return target;
		}

		public const string TargetName = nameof(ColoredConsoleTarget);
	}
}
