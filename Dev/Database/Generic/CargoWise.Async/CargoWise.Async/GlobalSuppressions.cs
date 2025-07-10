using System.Diagnostics.CodeAnalysis;
// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Code Analysis results, point to "Suppress Message", and click 
// "In Suppression File".
// You do not need to add suppressions to this file manually.
[assembly: SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member", Target = "CargoWise.Async.DefaultAsyncStrategy.#RunAndHandleExceptions`1(System.Func`1<!!0>)", Justification = "All exceptions are referred back to main thread exception handler")]
[assembly: SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member", Target = "CargoWise.Async.DefaultAsyncStrategy.#RunAndHandleExceptions(System.Action)", Justification = "All exceptions are referred back to main thread exception handler")]

[assembly: SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Auto generated baseline suppressions - WI00545660", Scope = "member", Target = "~M:CargoWise.Async.WaitHandleExtensions.WaitOneAsync(System.Threading.WaitHandle,System.Int32,System.Threading.CancellationToken)")] // C:\git\wtg\CargoWise\Dev\Database\Generic\CargoWise.Async\CargoWise.Async\WaitHandleExtensions.cs:23:64
[assembly: SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Auto generated baseline suppressions - WI00545660", Scope = "member", Target = "~M:CargoWise.Async.WaitHandleExtensions.WaitOneCoreAsync(System.Threading.WaitHandle,System.Int32,System.Threading.CancellationToken)")] // C:\git\wtg\CargoWise\Dev\Database\Generic\CargoWise.Async\CargoWise.Async\WaitHandleExtensions.cs:34:60
