using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public class GLControlAccounts
	{
		GLControlAccounts()
		{
			this.Factory = new ReadOnlyBusinessObjectFactory();
		}
		readonly ReadOnlyBusinessObjectFactory Factory;

		public static GLControlAccounts Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new GLControlAccounts();
				}

				return instance;
			}
		}

		[ThreadStatic]
		static GLControlAccounts instance;

		public AccGLHeader APControlAccount
		{
			get
			{
				return Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.APControlAccount.Value);
			}
		}

		public AccGLHeader ARControlAccount
		{
			get
			{
				return Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.ARControlAccount.Value);
			}
		}

		public AccGLHeader ARSuspenseControlAccount
		{
			get
			{
				return Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.Value);
			}
		}

		public AccGLHeader APSuspenseControlAccount
		{
			get
			{
				return Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.Value);
			}
		}

		public AccGLHeader WIPControlAccount
		{
			get
			{
				return Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value);
			}
		}

		public AccGLHeader ACRControlAccount
		{
			get
			{
				return Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value);
			}
		}

		public AccGLHeader GSTOutputControlAccount
		{
			get
			{
				return Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value);
			}
		}

		public AccGLHeader PendingGSTOutputControlAccount
		{
			get
			{
				return Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.Value);
			}
		}

		public AccGLHeader GSTInputControlAccount
		{
			get
			{
				return Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value);
			}
		}

		public AccGLHeader PendingGSTInputControlAccount
		{
			get
			{
				return Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.Value);
			}
		}

		public AccGLHeader JobRevenueJournalControlAccount
		{
			get
			{
				return Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.Value);
			}
		}

		public AccGLHeader CFXAccount
		{
			get
			{
				return Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.CFXAccount.Value);
			}
		}
	}
}
