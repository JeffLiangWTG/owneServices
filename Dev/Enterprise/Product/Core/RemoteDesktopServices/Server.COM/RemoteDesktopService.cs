using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;

namespace Enterprise.RemoteDesktopServices.Server.COM
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	[Guid("2D7F2392-363E-4134-8D30-9C313330CB0F")]
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class RemoteDesktopService : IRemoteDesktopService
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Baseline")]
		static RemoteDesktopService()
		{
			try
			{
				CargoWise.Common.ErrorReporter.Instance = new ErrorReporter();
				AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
				InitializeChannel();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				MessageBox.Show(ex.Message);
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static void InitializeChannel()
		{
			EnterpriseChannel.Initialize(new InitializationMessageHandler());
			MessageHandlers.Register(EnterpriseChannelMessageTypes.DragDrop, new DragDropHandler());
		}

		static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			int index = args.Name.IndexOf(",");
			string fileName = index >= 0 ? args.Name.Substring(0, index) : args.Name;
			string assemblyFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName + ".dll");
			if (File.Exists(assemblyFile))
			{
				return Assembly.LoadFile(assemblyFile);
			}
			else
			{
				return null;
			}
		}

		public bool IsConnected()
		{
			return EnterpriseChannel.Instance != null && EnterpriseChannel.Instance.IsConnected;
		}

		public void OpenFile(string file, bool readOnly)
		{
			using (var remoteFile = new RemoteFile(file, File.ReadAllBytes(file), readOnly))
			{
				remoteFile.Open();
			}
		}

		public void OpenWebUrl(string url)
		{
			EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.WebUrl, Encoding.UTF8.GetBytes(url));
		}

		public void RegisterRemoteDropHandler(IRemoteFileDropHandler dropHandler)
		{
			DragDropHandler.RegisterRemoteDropHandler(dropHandler);
		}
	}
}
