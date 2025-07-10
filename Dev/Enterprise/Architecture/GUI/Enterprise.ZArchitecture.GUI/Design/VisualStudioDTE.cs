using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using EnvDTE;
using Process = System.Diagnostics.Process;

namespace Enterprise.ZArchitecture.Design
{
	static class VisualStudioDTE
	{
		const uint S_OK = 0;
		[ThreadStatic]
		static _DTE DTE;

		/// <summary>
		/// Gets the running object table
		/// </summary>
		/// <param name="res"></param>
		/// <param name="rOT"></param>
		/// <returns></returns>
		[DllImport("ole32.dll", EntryPoint = "GetRunningObjectTable")]
		public static extern uint GetRunningObjectTable(
			uint res, out IRunningObjectTable rOT);

		/// <summary>
		/// Creates Bind Ctx
		/// </summary>
		/// <param name="res"></param>
		/// <param name="ctx"></param>
		/// <returns></returns>
		[DllImport("ole32.dll", EntryPoint = "CreateBindCtx")]
		public static extern uint CreateBindCtx(uint res, out IBindCtx ctx);

		/// <summary>
		/// Returns a reference to the Development Tools Extensibility (DTE) object
		/// </summary>
		/// <returns>DTE object</returns>
		public static _DTE GetDTE()
		{
			if (DTE != null)
			{
				return DTE;
			}

			// Get the DTE
			// The following method works fine IF you only have one instance of VS .NET open...
			// It returns the first created instance of VS .NET...not necessarily the current instance!
			//    return (EnvDTE._DTE)System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE");

			// This code was adapted from Girish Hegde's on-line article
			// "Adding or Changing Code using FileCodeModel in Visual Studio .NET

			DTE dte = null;
			IntPtr fetched = Marshal.AllocCoTaskMem(sizeof(int));

			try
			{
				// Get the ROT

				IRunningObjectTable rot;
				var uret = GetRunningObjectTable(0, out rot);

				if (uret == S_OK)
				{
					// Get an enumerator to access the registered objects

					IEnumMoniker enumMon;
					rot.EnumRunning(out enumMon);

					if (enumMon != null)
					{
						// Just grab a bunch of them at once, 100 should be
						// plenty for a test

						const int NumMons = 100;
						var aMons = new IMoniker[NumMons];

						enumMon.Next(NumMons, aMons, fetched);

						// Set up a binding context so we can access the monikers
						string name;

						IBindCtx ctx;
						uret = CreateBindCtx(0, out ctx);

						// Create the ROT name of the _DTE object using the process id
						var currentProcess =
							Process.GetCurrentProcess();

						var dteName = "VisualStudio.DTE.";
						var processID = ":" + currentProcess.Id;

						// for each moniker retrieved
						var fetchedCount = Marshal.ReadInt32(fetched);

						for (var i = 0; i < fetchedCount; i++)
						{
							// Get the display string
							aMons[i].GetDisplayName(ctx, null, out name);

							// If this is the one we are interested in...
							if (name.IndexOf(dteName) != -1 && name.IndexOf(processID) != -1)
							{
								object temp;
								rot.GetObject(aMons[i], out temp);
								dte = (DTE)temp;
								break;
							}
						}
					}
				}

				// *** RAS: Added Cache the reference if found
				DTE = (_DTE)dte;

				return (_DTE)dte;
			}
			finally
			{
				Marshal.FreeCoTaskMem(fetched);
			}
		}
	}
}
