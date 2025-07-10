using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class PGARequirement : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Constructor

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public PGARequirement(BusinessObjectFactory factory, ZString agencyCodePassed, PGARequirementProvider provider)
		: base(factory)
		{
			this.provider = Argument.NotNull(provider, nameof(provider));
			this.agencyCodePassed = agencyCodePassed;
			this.getIndicatorInfoPassed = () => provider.GetIndicatorInfo(agencyCodePassed);
			this.getProgramRequirementProvider = () => provider.GetProgramRequirementProvider(agencyCodePassed);
		}

		readonly Func<ZPropertyInfo> getIndicatorInfoPassed;
		readonly Func<IPGAProgramRequirementProvider> getProgramRequirementProvider;
		readonly ZString agencyCodePassed;
		readonly PGARequirementProvider provider;

		#endregion

		#region Properties
		public PGARequirementProvider Provider => provider;

		public ZBool IsEffective => provider.IsPGARequirementEffective;

		public ZString AgencyCode => agencyCodePassed;

		ZString AgencyDescription => Factory.GetCachedValue<PGACodes>().GetDescriptionFromCode(agencyCodePassed);

		[ReadOnly(true)]
		public ZString AgencyCodeWithDescription => AgencyCode + " - " + AgencyDescription;

		public ZString Program
		{
			get
			{
				var result = ZString.Empty;
				var enabledPrograms = ProgramCodeRequirements.Cast<PGAProgramRequirement>().Where(p => p.DeclareYes).ToArray();
				if (enabledPrograms.Length == 1)
				{
					result = enabledPrograms.First().ProgramCodeDescription;
				}
				else if (enabledPrograms.Length > 1)
				{
					result = Res.GetString("7c97cb2e-9774-4a22-91ab-7f6d1887bf4c", "MULTIPLE");
				}

				return result;
			}
		}

		public ZPropertyInfo ProgramInfo => GetZPropertyInfo(nameof(Program));

		[MaxLength(1)]
		public ZString Indicator
		{
			get
			{
				var indicatorInfoPassed = getIndicatorInfoPassed();
				return indicatorInfoPassed != null ? (ZString)indicatorInfoPassed.Value : ZString.Empty;
			}
			set
			{
				var indicatorInfoPassed = getIndicatorInfoPassed();
				var oldValue = indicatorInfoPassed != null ? (ZString)indicatorInfoPassed.Value : ZString.Empty;
				if (!IsCopying && oldValue != value && indicatorInfoPassed != null)
				{
					indicatorInfoPassed.Value = value;
					IndicatorInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo IndicatorInfo => GetZPropertyInfo(nameof(Indicator));

		protected override ZString HumanReadableNameCore => AgencyDescription;

		#endregion

		#region Validation

		public PGARequirementValidation Validation => validation ?? (validation = new PGARequirementValidation(this));
		PGARequirementValidation validation;

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidatePGARequired();
		}

		#endregion

		#region ProgramCodeRequirements

		[ChildEditable]
		public PGAProgramRequirementCollection ProgramCodeRequirements
		{
			get
			{
				if (programCodeRequirements == null)
				{
					programCodeRequirements = new PGAProgramRequirementCollection(this, getProgramRequirementProvider);
					programCodeRequirements.Populate();
					RegisterEditableChildObject(programCodeRequirements);
				}

				return programCodeRequirements;
			}
		}
		PGAProgramRequirementCollection programCodeRequirements;

		#endregion

		#region SetDefaultValue

		public void SetDefaultValueForIndicatorWhenTariffChanged()
		{
			var programmCodesFromTariff = CARefTariffDataLoader.LoadAllProgramsOfPGAType(Factory, provider.TariffNo, AgencyCode, provider.TariffEffectiveDate);
			var programCodesFromPGAHeader = provider.GetProgramRequirementProvider(AgencyCode).GetProgramCodesList().GetAllCodesZString();
			var codesToSet = new List<ZString>();
			var codesToUnset = new List<ZString>();
			foreach (var programCode in programCodesFromPGAHeader)
			{
				if (programmCodesFromTariff.Contains(programCode))
				{
					codesToSet.Add(programCode);
				}
				else
				{
					codesToUnset.Add(programCode);
				}
			}

			Indicator = codesToSet.Any() ? (ZString)YesNoList.Codes.Yes : ZString.Empty;

			var programRequirementProvider = getProgramRequirementProvider?.Invoke();
			if (programRequirementProvider != null)
			{
				using (programRequirementProvider.SuspendSettingDefaultValues() ?? DisposableAction.NoAction)
				{
					// We must set indicators to 'Y' for matching program codes before setting '' to non-matching codes.
					// Otherwise, we can accidentally delete PGA header.
					foreach (var programCode in codesToSet)
					{
						var programRequirement = ProgramCodeRequirements.OfType<PGAProgramRequirement>().FirstOrDefault(x => x.ProgramCode == programCode);
						if (programRequirement != null)
						{
							programRequirement.Indicator = YesNoList.Codes.Yes;
						}
					}
					foreach (var programCode in codesToUnset)
					{
						var programRequirement = ProgramCodeRequirements.OfType<PGAProgramRequirement>().FirstOrDefault(x => x.ProgramCode == programCode);
						if (programRequirement != null)
						{
							programRequirement.Indicator = ZString.Empty;
						}
					}
				}
			}
		}

		#endregion

		public IPGAHeader PGAHeader => getProgramRequirementProvider?.Invoke();

		public void CopyPersistentValuesFrom(PGARequirement source)
		{
			// setting indicator to remove all existing values
			Indicator = ZString.Empty;

			Indicator = source.Indicator;

			if (YesNoList.IsYesOrNo(source.Indicator))
			{
				PGAHeader.CopyPersistentValuesFrom(source.PGAHeader);
				foreach (PGAProgramRequirement requirement in ProgramCodeRequirements)
				{
					requirement.IndicatorInfo.RefreshBinding();
				}
			}
		}
	}
}
