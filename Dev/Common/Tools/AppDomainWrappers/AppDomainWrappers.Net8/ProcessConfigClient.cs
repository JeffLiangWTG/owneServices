public class ProcessConfigClient
{
	public List<string> AssemblyDependancies { get; set; }
	public string AssemblyFile { get; set; }
	public string NamespacePath { get; set; }
	public string ClassName { get; set; }
	public string MethodName { get; set; }
	public string[] MethodParameters { get; set; }
	public string BinFolder { get; set; }
	public bool DoNotUseTempDirectory { get; set; }
	public string TempWorkingDirectoryPath { get; set; }
	public string TempConfigDirectoryPath { get; set; }
	public string ClientLockFileName { get; set; }
	public string HostLockFileName { get; set; }
	public AppDomainRunMode RunMode { get; set; }
	public List<string> UsingImports { get; set; }
}
