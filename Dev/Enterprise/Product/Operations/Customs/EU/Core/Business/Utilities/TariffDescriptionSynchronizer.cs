using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Registry;

namespace Enterprise.Customs.EU.Business
{
	public interface ITariffDescriptionSynchronizerSupporter
	{
		ZString CurrentTariffDescription { get; set; }
		ZString OfficialCustomsTariffDescription { get; }
		Guid RegistryCompanyPK { get; }
		Guid RegistryBranchPK { get; }
	}

	public class TariffDescriptionSynchronizer
	{
		public TariffDescriptionSynchronizer(ITariffDescriptionSynchronizerSupporter syncSupporter)
		{
			this.syncSupporter = Argument.NotNull(syncSupporter, nameof(syncSupporter));
		}
		readonly ITariffDescriptionSynchronizerSupporter syncSupporter;

		public ZBool ShouldSynchronizeDescription
		{
			get
			{
				var currentTariffDescription = syncSupporter.CurrentTariffDescription;
				return EUCustomsDataRegistry.Instance.EnableAutoTariffDescriptionPopulation.GetFallBackValueAtAllLevels(syncSupporter.RegistryCompanyPK, syncSupporter.RegistryBranchPK, Guid.Empty) && (currentTariffDescription.IsEmpty || currentTariffDescription == syncSupporter.OfficialCustomsTariffDescription);
			}
		}

		public void SynchronizeDescription()
		{
			syncSupporter.CurrentTariffDescription = syncSupporter.OfficialCustomsTariffDescription;
		}
	}
}
