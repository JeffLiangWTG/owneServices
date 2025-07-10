using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class PGARequirementCollection : NonPersistentBusinessObjectCollection<PGARequirement>
	{
		public PGARequirementCollection(PGARequirementProvider pgaProvider)
			: base(pgaProvider.Factory)
		{
			this.PGAProvider = pgaProvider;
		}

		internal PGARequirementProvider PGAProvider;

		public bool PGATabCollectionNeedToBeRefreshed;

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new InvalidOperationException();
		}

		internal void Populate()
		{
			RemoveAndDeleteAll();

			var agencyCodesList = PGAProvider.GetGovernmentAgencyCodesList();
			foreach (CodeDescriptionPair agency in agencyCodesList)
			{
				var agencyCode = agency.Code;
				this.Add(new PGARequirement(Factory, agencyCode, PGAProvider));
			}
		}

		public void SetDefaultValueForIndicatorWhenTariffChanged()
		{
			this.Cast<PGARequirement>().ForEach(x => x.SetDefaultValueForIndicatorWhenTariffChanged());
		}

		public ZBool HasPGAProgramCodesDeclared(string agencyCode)
		{
			foreach (BusinessObject bo in this)
			{
				PGARequirement requirement = (PGARequirement)bo;
				if (requirement.AgencyCode == agencyCode)
				{
					foreach (PGAProgramRequirement programRequirement in requirement.ProgramCodeRequirements)
					{
						if (!PGAProvider.Supporter.IsDeleted && programRequirement.DeclareYes)
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		public PGARequirement PGARequirement(string agencyCode)
		{
			return this.OfType<PGARequirement>().FirstOrDefault(x => x.AgencyCode == agencyCode);
		}

		public void CopyPersistentValuesFrom(PGARequirementCollection source)
		{
			foreach (PGARequirement sourceRequirement in source)
			{
				var targetRequirement = PGARequirement(sourceRequirement.AgencyCode);
				if (targetRequirement != null)
				{
					targetRequirement.CopyPersistentValuesFrom(sourceRequirement);
				}
			}
		}

		public void RefreshBindingForDeclaredPGAHeaders()
		{
			var pgaRequirementsDeclared = this.Cast<PGARequirement>().Where(pgaRequirement => pgaRequirement.Indicator == YesNoList.Codes.Yes);

			foreach (var pgaRequirement in pgaRequirementsDeclared)
			{
				var pgaHeader = pgaRequirement.PGAHeader as BusinessObject;
				if (pgaHeader != null && !pgaHeader.IsDeleted)
				{
					pgaHeader.RefreshBinding();
				}
			}
		}
	}
}
