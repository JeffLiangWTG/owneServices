using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JobDocAddressSynchroniserWithFallback : JobDocAddressSynchroniser
	{
		public JobDocAddressSynchroniserWithFallback(JobDocAddress destination, JobDocAddress source, JobDocAddress fallbackSource)
			: base(destination, source)
		{
			Argument.NotNull(fallbackSource, "fallbackSource");

			this.FallbackSource = fallbackSource;
		}
		internal protected JobDocAddress FallbackSource { get; protected set; }

		JobDocAddress EligibleSource
		{
			get
			{
				if (!Source.IsDeleted && !Source.IsEmpty)
				{
					return Source;
				}
				else
				{
					return FallbackSource;
				}
			}
		}

		protected override void ForceSynchroniseCore()
		{
			if (!Destination.IsDeleted && !EligibleSource.IsDeleted)
			{
				Destination.SynchroniseWithParent(EligibleSource);
			}
			Destination.ReadOnly = true;
		}

		protected override void HookSynchronisers()
		{
			if (!Destination.IsDeleted && !EligibleSource.IsDeleted)
			{
				Destination.SynchroniseWithParent(EligibleSource, false);
				if (!Source.IsDeleted)
				{
					Source.DocAddressChanged -= ReHookDestination;
					Source.DocAddressChanged += ReHookDestination;
				}
				if (!FallbackSource.IsDeleted)
				{
					FallbackSource.DocAddressChanged -= ReHookDestination;
					FallbackSource.DocAddressChanged += ReHookDestination;
				}
			}
			Destination.ReadOnly = true;
		}

		protected override void UnHookSynchronisers()
		{
			if (!Destination.IsDeleted)
			{
				Destination.DeSynchroniseWithParent();
				if (!Source.IsDeleted)
				{
					Source.DocAddressChanged -= ReHookDestination;
				}
				if (!FallbackSource.IsDeleted)
				{
					FallbackSource.DocAddressChanged -= ReHookDestination;
				}
			}
			Destination.ReadOnly = false;
		}

		void ReHookDestination(object sender, System.EventArgs e)
		{
			SwitchSource();
		}

		void SwitchSource()
		{
			if (!Destination.IsDeleted)
			{
				Destination.DeSynchroniseWithParent();
				if (!EligibleSource.IsDeleted)
				{
					Destination.SynchroniseWithParent(EligibleSource);
				}
			}
		}
	}
}
