using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Usage", "CA1816:SCall GC.SuppressFinalize correctly", Scope = "module")]

[assembly: SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Scope = "module")]
[assembly: SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Scope = "module")]
[assembly: SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity", Scope = "module")]
[assembly: SuppressMessage("Globalization", "CA1309:Use ordinal string comparison", Scope = "module")]
[assembly: SuppressMessage("Performance", "CA1840:Use 'Environment.CurrentManagedThreadId' instead of 'Thread.CurrentThread.ManagedThreadId'", Scope = "module")]
[assembly: SuppressMessage("Performance", "CA1841:Use 'Environment.CurrentManagedThreadId' instead of 'Thread.CurrentThread.ManagedThreadId'", Scope = "module")]
[assembly: SuppressMessage("Performance", "CA1843:Do not use 'WaitAll' with a single task", Scope = "module")]
[assembly: SuppressMessage("Performance", "CA1845:Use span-based 'string.Concat' and 'AsSpan' instead of 'Substring'", Scope = "module")]
[assembly: SuppressMessage("Performance", "CA1846:Prefer 'AsSpan' over 'Substring' when span-based overloads are available", Scope = "module")]
[assembly: SuppressMessage("Performance", "CA1847:Use 'string.Contains(char)' instead of 'string.Contains(string)' when searching for a single character", Scope = "module")]
[assembly: SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Scope = "module")]
[assembly: SuppressMessage("Usage", "CA2251:Use 'string.Equals' instead of comparing the result of 'string.Compare' to 0", Scope = "module")]
