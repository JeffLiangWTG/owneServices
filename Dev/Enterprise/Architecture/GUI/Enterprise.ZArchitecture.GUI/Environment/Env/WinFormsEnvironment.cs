using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Environment.Semaphore;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;

namespace Enterprise.Environment
{
	public class WinFormsEnvironment : BaseEnvironment, IWinFormsEnvironment
	{
		internal WinFormsEnvironment()
			: base(new MultiThreadUserContextManager())
		{
		}

		public override void ExitApplication()
		{
			Application.Exit();
		}

		public FormRegistry FormRegistry
		{
			get
			{
				if (fFormRegistry == null)
				{
					fFormRegistry = new FormRegistry();
				}
				return fFormRegistry;
			}
		}

		public GridLayoutRegistry GridLayoutRegistry
		{
			get
			{
				if (gridLayoutRegistry.Value == null)
				{
					gridLayoutRegistry.Value = new GridLayoutRegistry();
				}

				return gridLayoutRegistry.Value;
			}
		}

		public SplitterLayoutRegistry SplitterLayoutRegistry
		{
			get
			{
				if (fSplitterLayoutRegistry == null)
				{
					fSplitterLayoutRegistry = new SplitterLayoutRegistry();
				}
				return fSplitterLayoutRegistry;
			}
		}

		public override IUserLoginController LoginController
		{
			get { return fLoginController ?? (fLoginController = ObjectFactory.Get<IUserLoginController>()); }
		}

		public string CurrentModule
		{
			get { return fCurrentModule; }
			set { fCurrentModule = value; }
		}

		public override string ApplicationStartupPath
		{
			get { return System.Windows.Forms.Application.StartupPath; }
		}

		public override IDbUpgradeCaptions DbUpgradeCaptions { get; } = new DbUpgradeCaptions();

		protected override void PostUserContextSwitch(
			IUserContext userContext,
			bool setCurrentThreadContext,
			bool isRevert)
		{
			RecentItemManager.Reset();
		}

		#region Enterprise Semaphore Provider

		protected override ISemaphoreProvider EnvironmentSpecificSemaphoreProvider
		{
			get
			{
#if DEBUG
				if (Globals.IsTest && Enterprise.Core.Environment.Semaphores.Testing.TestSemaphoreProviderAttribute.TestProvider != null)
				{
					return Enterprise.Core.Environment.Semaphores.Testing.TestSemaphoreProviderAttribute.TestProvider;
				}
#endif

				return semaphoreProvider;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static readonly ISemaphoreProvider semaphoreProvider = new EnterpriseSemaphoreProvider();

		#endregion

		#region Implementation

		IUserLoginController fLoginController;
		readonly Overridable<GridLayoutRegistry> gridLayoutRegistry = new Overridable<GridLayoutRegistry>();
		FormRegistry fFormRegistry;
		SplitterLayoutRegistry fSplitterLayoutRegistry;
		string fCurrentModule;

#if DEBUG
		public void SetLoginControllerForTesting(IUserLoginController controller)
		{
			fLoginController = controller;
		}
#endif

		#endregion

		#region To Be Moved out of Env

		protected bool fOpenDocumentScanningForm;

		public bool OpenDocumentScanningForm
		{
			get { return fOpenDocumentScanningForm; }
			set { fOpenDocumentScanningForm = value; }
		}

		protected bool fOpenDocumentMaintenanceForm;

		public bool OpenDocumentMaintenanceForm
		{
			get { return fOpenDocumentMaintenanceForm; }
			set { fOpenDocumentMaintenanceForm = value; }
		}

		protected bool fShowInSystemTray;

		public bool ShowInSystemTray
		{
			get { return fShowInSystemTray; }
			set { fShowInSystemTray = value; }
		}

		protected string fOpenDocumentMaintenanceFormJobNo;
		public string OpenDocumentMaintenanceFormJobNo
		{
			get { return fOpenDocumentMaintenanceFormJobNo; }
			set { fOpenDocumentMaintenanceFormJobNo = value; }
		}

		#endregion
	}
}
