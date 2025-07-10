using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Enterprise.ActivityLogger
{
	[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	[StructLayout(LayoutKind.Sequential)]
	internal struct HookMessage
	{
		[SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		public Int32 dwProcessId;

		[SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		public Int32 hookId;

		[SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		public Int32 nCode;

		[SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		[SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
		public IntPtr wParam; // 8 byte on x64, 4 byte on x86

		[SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		[SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
		public IntPtr lParam; // 8 byte on x64, 4 byte on x86
	}
}