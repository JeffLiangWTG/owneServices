using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;

namespace CargoWise.Loader.Common
{
	public abstract class InstallationItem
	{
		protected InstallationItem(Installation installation)
		{
			if (installation != null)
			{
				this.Installation = installation;
			}
		}

		public void AddDependency(InstallationItem dependency)
		{
			Argument.NotNull(dependency, nameof(dependency));
			if (IsRemoteExclusive || !dependency.IsRemoteExclusive)
			{
				Dependencies.Add(dependency);
			}
			else
			{
				throw new InvalidOperationException("Should not include remote exclusive installation item.");
			}
		}

		public bool NeedsToInstall()
		{
			return NeedsToInstallCore();
		}

		protected abstract bool NeedsToInstallCore();

		public InstallationItemCollection Dependencies
		{
			get
			{
				if (fDependencies == null)
				{
					fDependencies = new InstallationItemCollection();
				}
				return fDependencies;
			}
		}

		public void ChangeCurrentTaskDescription(string taskDescription)
		{
			Installation.OnCurrentTaskDescriptionChanged(taskDescription);
		}

#if DEBUG
		virtual // for mocks
#endif
		public void CheckAvailability(InstallationResultCollection results)
		{
			Argument.NotNull(results, nameof(results));
			DoRecursiveAvailabilityCheck(results);
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is essentially a top level exception handler")]
#if DEBUG
		virtual // for mocks
#endif
		public void Install(InstallationResultCollection results)
		{
			Argument.NotNull(results, nameof(results));
			try
			{
				if (NeedsToInstall())
				{
					DoRecursiveInstallation(results);
				}
				else
				{
					MoveProgressForwardWhenInstallationIsNotRequired();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				results.Add(InstallationResult.Error("Installation failed with an unhandled exception:\r\n" + ex.ToString()));
			}
		}

		public virtual int TotalProgressCount
		{
			get
			{
				int count = 0;
				IEnumerator<InstallationItem> enumerator = GetDepthFirstEnumerable().GetEnumerator();
				while (enumerator.MoveNext())
				{
					count++;
				}
				return count * 2; // Once for checking availability, once for installing.
			}
		}

		Installation installation;
		public Installation Installation
		{
			get
			{
				return installation ?? (installation = new Installation(new Configuration()));
			}
			protected set
			{
				installation = value;
			}
		}

		public IEnumerable<InstallationItem> GetDepthFirstEnumerable()
		{
			foreach (InstallationItem item in Dependencies.GetDepthFirstEnumerable())
			{
				yield return item;
			}

			yield return this;

			yield break;
		}

		public InstallationItemCollection FindItems(InstallationItemFilter filter)
		{
			Argument.NotNull(filter, nameof(filter));
			InstallationItemCollection result = new InstallationItemCollection();
			foreach (InstallationItem item in GetDepthFirstEnumerable())
			{
				if (filter(item))
				{
					result.Add(item);
				}
			}
			return result;
		}

		#region Implementation

		InstallationItemCollection fDependencies;

		protected virtual InstallationResult CheckAvailabilityExcludingDependencies()
		{
			return InstallationResult.OK();
		}

		protected virtual InstallationResult InstallExcludingDependencies()
		{
			return InstallationResult.OK();
		}

		public void AddTerminalServerInstallModeDependency()
		{
			AddDependency(new TerminalServerInstallMode(Installation));
		}

		void OnProgress()
		{
			Installation.OnProgress();
		}

		void DoRecursiveInstallation(InstallationResultCollection results)
		{
			Argument.NotNull(results, nameof(results));
			int previousErrorCount = results.ErrorCount;

			foreach (InstallationItem dependency in Dependencies)
			{
				dependency.Install(results);
				if (results.ErrorCount > previousErrorCount)
				{
					return;
				}
			}

			var result = InstallExcludingDependencies();
			results.Add(result);
			if (results.ErrorCount == 0)
			{
				OnProgress();
			}
		}

		void DoRecursiveAvailabilityCheck(InstallationResultCollection results)
		{
			Argument.NotNull(results, nameof(results));
			foreach (InstallationItem dependency in Dependencies)
			{
				dependency.CheckAvailability(results);
			}

			results.Add(CheckAvailabilityExcludingDependencies());
			OnProgress();
		}

		void MoveProgressForwardWhenInstallationIsNotRequired()
		{
			foreach (object dummy in GetDepthFirstEnumerable())
			{
				GC.KeepAlive(dummy); // stop the unused variable 'Dummy' warning
				OnProgress();
			}
		}

		#endregion

		protected virtual bool IsRemoteExclusive
		{
			get { return false; }
		}
	}
}
