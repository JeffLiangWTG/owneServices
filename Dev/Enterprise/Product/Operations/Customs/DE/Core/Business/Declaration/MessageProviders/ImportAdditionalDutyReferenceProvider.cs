using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business
{
	public sealed class ImportAdditionalDutyReferenceProvider : IImportAdditionalDutyReference
	{
		public ImportAdditionalDutyReferenceProvider(CusFiscalReference fiscalReference)
		{
			this.fiscalReference = fiscalReference;
		}
		readonly CusFiscalReference fiscalReference;

		public string ReferenceNumber => fiscalReference.CFR_Code + fiscalReference.CFR_Reference;

		public IImportParty DutyInterestedParty => CachedValueHelper.GetValue(ref dutyInterestedParty, () => ImportPartyProvider.NewOrNull(Owner));
		CachedValue<IImportParty> dutyInterestedParty;

		public Guid DutyInterestedPartyPK
		{
			get
			{
				var result = Guid.Empty;
				var owner = Owner;
				if (owner != null)
				{
					result = owner.PK.ToGuid();
				}
				return result;
			}
		}

		OrgAddress Owner => fiscalReference.Owner;
	}
}
