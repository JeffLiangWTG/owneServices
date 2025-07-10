// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
using System.Diagnostics.CodeAnalysis;
[assembly: SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Baseline WI00586871", Scope = "member", Target = "~M:ResourceStrings.Cmd.Testing.ProgramTest.TestStdIo")] // Enterprise/Product/Core/ResourceStrings/ResourceStrings.Cmd/Program.cs:77,17
