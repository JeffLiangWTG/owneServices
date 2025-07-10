using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class PGAProgramRequirement : NonPersistentBusinessObject, IObsoleteValidation
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public PGAProgramRequirement(PGARequirement pgaRequirement, ZString programCodePassed, ZString programDescriptionPassed, Func<IPGAProgramRequirementProvider> getProgramRequirementProvider)
			: base(pgaRequirement.Factory)
		{
			this.pgaRequirement = Argument.NotNull(pgaRequirement, nameof(pgaRequirement));
			this.programCodePassed = Argument.NotNull(programCodePassed, nameof(programCodePassed));
			this.programDescriptionPassed = Argument.NotNull(programDescriptionPassed, nameof(programDescriptionPassed));
			this.getProgramRequirementProvider = Argument.NotNull(getProgramRequirementProvider, nameof(getProgramRequirementProvider));
		}

		readonly PGARequirement pgaRequirement;
		readonly ZString programCodePassed;
		readonly ZString programDescriptionPassed;
		readonly Func<IPGAProgramRequirementProvider> getProgramRequirementProvider;

		#region Schema

		public sealed class Schema
		{
			Schema() { }
			public const string Indicator = "Indicator";
			public const string DeclareYes = "DeclareYes";
			public const string DeclareNo = "DeclareNo";
			public const string DeclareNotApplicable = "DeclareNotApplicable";
		}

		#endregion

		#region Indicator

		[MaxLength(1)]
		public ZString Indicator
		{
			get
			{
				var indicatorInfoPassed = ProgramRequirementProvider?.GetProgramIndicatorInfo(programCodePassed);
				return indicatorInfoPassed != null ? (ZString)indicatorInfoPassed.Value : ZString.Empty;
			}
			set
			{
				EnsureActivePGAHeaderIfNecessary(value);
				var indicatorInfoPassed = ProgramRequirementProvider?.GetProgramIndicatorInfo(programCodePassed);
				var oldValue = indicatorInfoPassed != null ? (ZString)indicatorInfoPassed.Value : ZString.Empty;
				CheckMaximumLength(IndicatorInfo, value);

				if (!IsCopying
					&& oldValue != value
					&& indicatorInfoPassed != null)
				{
					if (!YesNoList.IsYes(oldValue)
						|| (!(indicatorInfoPassed.BizObj as IPurgeValueParent)?.PurgeHelper.HasValueNeededToBePurged() ?? true)
						|| (ShouldUpdateIndicatorEvent?.Invoke(oldValue, value, ProgramCodeDescription) ?? true))
					{
						var setterSuspender = ProgramRequirementProvider.SetterSuspender;
						if (!setterSuspender.IsSetterSuspended(indicatorInfoPassed.Name))
						{
							indicatorInfoPassed.Value = value;
						}
						UpdatePGAIndicator();
						pgaRequirement.ProgramInfo.RefreshBinding();
						ValidateDeclareYes();
						ValidateDeclareNo();
						ValidateDeclareNotApplicable();
					}

					AfterShouldUpdateIndicatorEvent?.Invoke(this, EventArgs.Empty);
				}

				IndicatorInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo IndicatorInfo => GetZPropertyInfo(nameof(Indicator));

		public delegate bool ShouldUpdateIndicatorEventHandler(ZString oldValue, ZString newValue, string programDescription);

		public ShouldUpdateIndicatorEventHandler ShouldUpdateIndicatorEvent;
		public EventHandler AfterShouldUpdateIndicatorEvent;

		void EnsureActivePGAHeaderIfNecessary(string programIndicator)
		{
			if (!YesNoList.IsYesOrNo(pgaRequirement.Indicator) && YesNoList.IsYesOrNo(programIndicator))
			{
				pgaRequirement.Indicator = programIndicator;
			}
		}

		void UpdatePGAIndicator()
		{
			if (YesNoList.IsYes(Indicator))
			{
				pgaRequirement.Indicator = YesNoList.Codes.Yes;
			}
			else if (YesNoList.IsNo(Indicator))
			{
				if (!pgaRequirement.ProgramCodeRequirements.Cast<PGAProgramRequirement>().Any(x => x.Indicator == YesNoList.Codes.Yes))
				{
					pgaRequirement.Indicator = YesNoList.Codes.No;
				}
			}
			else if (pgaRequirement.ProgramCodeRequirements.Cast<PGAProgramRequirement>().All(p => p.DeclareNotApplicable))
			{
				pgaRequirement.Indicator = ZString.Empty;
			}
		}

		#endregion

		#region DeclareYes
		[ResourceStringData("{800FEF14-103B-4F4B-A8A0-523536DC0033}", Caption = "Declare PGA Program Yes", ShortCaption = "Yes")]
		public ZBool DeclareYes
		{
			get => Indicator == YesNoList.Codes.Yes;
			set
			{
				var oldValue = DeclareYes;
				if (value)
				{
					Indicator = YesNoList.Codes.Yes;
				}
				else
				{
					Indicator = oldValue && IsProgramRequired ? (ZString)YesNoList.Codes.No : ZString.Empty;
				}

				DeclareYesInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo DeclareYesInfo => GetZPropertyInfo(nameof(DeclareYes));

		#endregion

		#region DeclareNo

		[ResourceStringData("{6CA3681B-F60D-4613-A3D7-91DA656AA18A}", Caption = "Declare PGA Program No", ShortCaption = "No")]
		public ZBool DeclareNo
		{
			get => Indicator == YesNoList.Codes.No;
			set
			{
				var oldValue = DeclareYes;
				if (value)
				{
					Indicator = YesNoList.Codes.No;
				}
				else
				{
					Indicator = ZString.Empty;
				}
				DeclareNoInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo DeclareNoInfo => GetZPropertyInfo(nameof(DeclareNo));

		#endregion

		#region DeclareNotApplicable

		[ResourceStringData("{A184E153-E0F3-4A0F-9D67-6380F077932B}", Caption = "Not Applicable", ShortCaption = "N/A")]
		public ZBool DeclareNotApplicable => Indicator != YesNoList.Codes.No && Indicator != YesNoList.Codes.Yes;

		public ZPropertyInfo DeclareNotApplicableInfo => GetZPropertyInfo(nameof(DeclareNotApplicable));

		#endregion

		#region Implementation

		public PGARequirement ParentRequirement => pgaRequirement;
		public ZString ProgramCodeDescription => programDescriptionPassed;
		public ZString PGAType => pgaRequirement.AgencyCode;
		public ZString ProgramCode => programCodePassed;
		public IPGAProgramRequirementProvider ProgramRequirementProvider => getProgramRequirementProvider();
		public bool IsProgramRequired => CARefTariffDataLoader.DoesTariffPGATypeHasProgram(ParentRequirement.Factory, ParentRequirement.Provider.TariffNo, PGAType, ProgramCode, ParentRequirement.Provider.TariffEffectiveDate);

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateDeclareYes();
			ValidateDeclareNo();
			ValidateDeclareNotApplicable();
		}

		public PGAProgramRequirementValidation Validation => validation ?? (validation = new PGAProgramRequirementValidation(this));
		PGAProgramRequirementValidation validation;

		public void ValidateDeclareNotApplicable()
		{
			if (!IsValidationSuspended)
			{
				Validation.CheckDeclareNotApplicable(DeclareNotApplicableInfo);
			}
		}

		public void ValidateDeclareNo()
		{
			if (!IsValidationSuspended)
			{
				Validation.CheckDeclareNo(DeclareNoInfo);
			}
		}

		public void ValidateDeclareYes()
		{
			if (!IsValidationSuspended)
			{
				Validation.CheckDeclareYes(DeclareYesInfo);
			}
		}

		#endregion
	}
}
