using System.Diagnostics.CodeAnalysis;

#if DEBUG
using System;
using CargoWise.BuildTools.Testing;
using CargoWise.Common;
#endif

namespace CargoWise.BuildTools
{
	/// <summary>
	/// This static class gives access to the internal source control databases
	/// </summary>
	public static class SourceControl
	{
		#region Source Control Databases

		#region EnterpriseDatabase SourceControl Instance

		public static ISourceControl EnterpriseDatabase
		{
			[SuppressMessage("Microsoft.Contracts", "EnsuresInMethod-Contract.Result<ISourceControl>() != null")] //Suppress faulty 'ensures unreachable'
			get
			{
				#region MockSourceControlTestDatabase instance
#if DEBUG
				if (WTG.TestHelpers.TestingState.IsTest)
				{
					if (instanceForTesting != null)
					{
						return instanceForTesting;
					}
					return SourceControl.MockSourceControlTestDatabase;
				}
				else
#endif
				#endregion
				{
					if (enterpriseDatabase == null)
					{
						SetEnterpriseDatabase();
					}
					return enterpriseDatabase;
				}
			}
		}

		public static void WithAdditionalRepository(string path)
		{
			ref var field = ref enterpriseDatabase;

#if DEBUG
			if (WTG.TestHelpers.TestingState.IsTest)
			{
				if (instanceForTesting == null)
				{
					throw new InvalidOperationException("'WithAdditionalRepository' not supported on the standard mock SourceControl instance.");
				}

				field = ref instanceForTesting;
			}
			else
#endif
			{
				if (enterpriseDatabase == null)
				{
					SetEnterpriseDatabase();
				}
			}

			field = SourceControlFactory.WithAdditionalRepository(field, path);
		}

		static void SetEnterpriseDatabase()
		{
			enterpriseDatabase = SourceControlFactory.GetSourceControl();
		}

		internal static ISourceControlFactory SourceControlFactory
		{
			get
			{
				return BuildTools.SourceControlFactory.Instance;
			}
		}

		#endregion

		#region SourceControl Instances for Testing
#if DEBUG
		/// <summary>
		/// This is a mock SourceControl Implementation instance for testing outside of BuildTools and can be run on dat
		/// </summary>
		static ISourceControl MockSourceControlTestDatabase
		{
			get
			{
				if (mockSourceControlTestDatabase == null)
				{
					mockSourceControlTestDatabase = new MockSourceControl();
				}
				return mockSourceControlTestDatabase;
			}
		}

		public static void ClearTestDatabaseInstances()
		{
			SourceControl.mockSourceControlTestDatabase = null;
		}

		[SuppressMessage("CargoWiseOne", "CW1021")]
		static ISourceControl mockSourceControlTestDatabase;
#endif
		#endregion

		[SuppressMessage("CargoWiseOne", "CW1021")]
		static ISourceControl enterpriseDatabase;

#if DEBUG

		/// <summary>
		/// Sets a provided ISourceControl implementation in favor of the default MockSourceControl during a Test Run;
		/// </summary>
		/// <param name="sourceControl"></param>
		public static void SetEnterpriseInstanceForTesting(ISourceControl sourceControl)
		{
			Argument.NotNull(sourceControl, nameof(sourceControl));
			instanceForTesting = sourceControl;
		}

		/// <summary>
		/// Removes the previously specified ISourceControl Implementation
		/// </summary>
		public static void RemoveEnterpriseInstanceForTesting()
		{
			if (instanceForTesting != null)
			{
				instanceForTesting = null;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1021")]
		static ISourceControl instanceForTesting;
#endif

		#endregion
	}
}
