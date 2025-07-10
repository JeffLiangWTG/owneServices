using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Usage", "CA1816:SCall GC.SuppressFinalize correctly", Scope = "module")]

[assembly: SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Scope = "module")]
[assembly: SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Scope = "module")]
[assembly: SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity", Scope = "module")]
[assembly: SuppressMessage("Globalization", "CA1309:Use ordinal string comparison", Scope = "module")]
[assembly: SuppressMessage("Performance", "CA1847:Use 'string.Contains(char)' instead of 'string.Contains(string)' when searching for a single character", Scope = "module")]
[assembly: SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Scope = "module")]
