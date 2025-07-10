using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.CustomsLists;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public partial class ImportUCC6JobDeclarationValidation : ImportJobDeclarationValidation
	{
		public ImportUCC6JobDeclarationValidation(JobDeclaration parent) : base(parent)
		{
		}

		protected override void CheckJE_TransportMode()
		{
			base.CheckJE_TransportMode();
			CheckRuleBR3403(Parent.JE_TransportModeInfo);
		}

		protected override void CheckJE_TransportModeInland()
		{
			base.CheckJE_TransportModeInland();
			CheckRuleBR3403(Parent.JE_TransportModeInlandInfo);
		}

		#region Validation BR3403

		void CheckRuleBR3403(ZPropertyInfo info)
		{
			if (HasEntryInstructionThatMeetsRuleBR3403())
			{
				var transportMeans = Parent.JE_TransportMeans.ToUpperInvariant();
				if (GetBR3403ValidTransportValues(transportMeans) is string[] validTransportValues
					&& !validTransportValues.Contains(Parent.JE_TransportMode.ToUpperInvariant().ToString())
					&& !validTransportValues.Contains(Parent.JE_TransportModeInland.ToUpperInvariant().ToString()))
				{
					info.AddMessageError(GetBR3404ErrorMessage(transportMeans, info));
				}
			}
		}

		string GetBR3404ErrorMessage(string transportMeans, ZPropertyInfo info)
		{
			switch (transportMeans)
			{
				case TransportMeansList.Codes.ImoShipIdentificationNumber:
				case TransportMeansList.Codes.NameOfTheSeaGoingVessel:
				case TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode:
				case TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel:
					return Res.GetString("FEAE1D82-22CA-482E-B5EB-16743BA8C32C", "[BR3403] {0} must be 'SEA'.", info.Description);
				case TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle:
					return Res.GetString("79C6E72E-2810-4DC0-A85F-D5747E469040", "[BR3403] {0} must be 'ROA'.", info.Description);
				case TransportMeansList.Codes.IataFlightNumber:
				case TransportMeansList.Codes.RegistrationNumberOfTheAircraft:
					return Res.GetString("43EBB135-2C03-4C2E-A9B2-F0EE1C1DBCAF", "[BR3403] {0} must be 'AIR'.", info.Description);
				default:
					return Res.GetString("123CF65F-66B3-4B8F-B43C-FEDB679D0BFF", "[BR3403] {0} must be 'MAI' or 'FIX'.", info.Description);
			}
		}

		string[] GetBR3403ValidTransportValues(string transportMeans)
		{
			return BR3403CombinationDictionary.TryGetValue(transportMeans, out var keyValuePair) ? keyValuePair : null;
		}

		bool HasEntryInstructionThatMeetsRuleBR3403()
		{
			return Parent.CustomsEntryInstructions
				.Any(x => x.CEI_Style.EqualsIgnoringCase(ImportDeclarationTypeList.Codes.H5)
					|| (IsBR3403Style(x.CEI_Style) && !x.CEI_SubStyle.IsEmpty && !x.CEI_SubStyle.EqualsIgnoringCase(EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic)));
		}

		bool IsBR3403Style(ZString style)
		{
			switch (style.ToUpperInvariant())
			{
				case ImportDeclarationTypeList.Codes.H1:
				case ImportDeclarationTypeList.Codes.H2:
				case ImportDeclarationTypeList.Codes.H3:
				case ImportDeclarationTypeList.Codes.H4:
					return true;
				default:
					return false;
			}
		}

		Dictionary<string, string[]> BR3403CombinationDictionary
		{
			get
			{
				if (br3403CombinationDictionary == null)
				{
					var seaCodes = new[] { TransportTypeGenericList.Codes.Sea };
					var airCodes = new[] { TransportTypeGenericList.Codes.Air };
					br3403CombinationDictionary = new Dictionary<string, string[]>
					{
						{ string.Empty, new[] { TransportTypeGenericList.Codes.PostMail, TransportTypeGenericList.Codes.FixedTransportInstallations } },
						{ TransportMeansList.Codes.ImoShipIdentificationNumber, seaCodes },
						{ TransportMeansList.Codes.NameOfTheSeaGoingVessel, seaCodes },
						{ TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode, seaCodes },
						{ TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel, seaCodes },
						{ TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle, new[] { TransportTypeGenericList.Codes.Road } },
						{ TransportMeansList.Codes.IataFlightNumber, airCodes },
						{ TransportMeansList.Codes.RegistrationNumberOfTheAircraft, airCodes }
					};
				}

				return br3403CombinationDictionary;
			}
		}
		Dictionary<string, string[]> br3403CombinationDictionary;

		#endregion

		protected override void CheckJE_VesselName()
		{
			base.CheckJE_VesselName();

			var entryStyleIsH1345 = Parent.CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(x => ImportDeclarationTypeList.h1H3H4H5.Contains(x.CEI_Style));
			if (entryStyleIsH1345 && Parent.TransportIDRequiredWhenImport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_VesselNameInfo);
			}
		}

		protected override void CheckJE_RN_NKTransportNationalityInland()
		{
			base.CheckJE_RN_NKTransportNationalityInland();
			var parent = Parent;
			var targetInfo = parent.JE_RN_NKTransportNationalityInlandInfo;

			if (parent.CustomsEntryInstructions.Any(x =>
			{
				var declarationType = x.CEI_Style;
				return declarationType == ImportDeclarationTypeList.Codes.H1 || declarationType == ImportDeclarationTypeList.Codes.H3 || declarationType == ImportDeclarationTypeList.Codes.H4 || declarationType == ImportDeclarationTypeList.Codes.H5;
			}))
			{
				var ruleTransportType = parent.JE_TransportMode == Core.Constants.TransportModes.Rail || parent.JE_TransportMode == Core.Constants.TransportModes.Mail || parent.JE_TransportMode == Core.Constants.TransportModes.FixedTransportInstallations;
				if (ruleTransportType && !parent.JE_RN_NKTransportNationalityInland.IsEmpty)
				{
					targetInfo.AddMessageError(Res.GetString("858ED7F9-CE25-481D-8D1C-C7CC947F6B99", "[C0010] Nationality cannot be used when Transport Mode is 'RAI' or 'MAI' or 'FIX'."));
				}
				if (!ruleTransportType && parent.JE_RN_NKTransportNationalityInland.IsEmpty)
				{
					targetInfo.AddMessageError(Res.GetString("CD2E807D-D3D5-4B3B-B4CB-7DA9D2BFCD6F", "[C0010] Please enter a Nationality."));
				}
			}
		}
	}
}
