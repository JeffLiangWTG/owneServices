using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.ActivityLogger
{
	internal class Hooker : IDisposable
	{
		public static Hooker Instance => instance;
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static Hooker instance;

		bool disposed;

		#region Lifetime

		~Hooker()
		{
			Dispose(false);
		}

		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		void ReleaseUnmanagedResources()
		{
			try
			{
				SafeNativeMethods.DetachHooks();
			}
			catch (Win32Exception)
			{
				// If we can't detach the hooks there is literally nothing we
				// can do except to send some info back to WTG.
				//ExceptionReporter.Instance.ReportDeveloperException("DetachHooks", e);
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
		void Dispose(bool disposing)
		{
			disposed = true;
			ReleaseUnmanagedResources();
		}

		#endregion

		public HookerEvent WaitForEvent(Int32 milliseconds)
		{
			if (disposed)
			{
				throw new ObjectDisposedException("Can't call WaitForEvent - Hooker was already disposed!");
			}

			return SafeNativeMethods.WaitForEvent(milliseconds);
		}

		// Returns true if we attached the Hooker or it was already attached, false if we did not
		public static bool Attach()
		{
			if (Instance != null)
			{
				return true;
			}

			try
			{
				SafeNativeMethods.AttachHooks();
			}
			catch (Win32Exception)
			{
				//ExceptionReporter.Instance.ReportDeveloperException("AttachHooks", e);
				return false;
			}

			instance = new Hooker();
			return true;
		}
	}
}