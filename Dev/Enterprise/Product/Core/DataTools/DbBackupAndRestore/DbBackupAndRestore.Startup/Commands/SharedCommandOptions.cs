using System.CommandLine;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DataTools.DbBackupAndRestore.Commands;

public static class SharedCommandOptions
{
	[ThreadSafe]
	public static readonly Option<string> DatabaseOption = new (
		"--Database",
		"Target database name") { IsRequired = true };

	[ThreadSafe]
	public static readonly Option<string> ServerOption = new (
		"--Server",
		"Destination SQL Instance") { IsRequired = true };
}
