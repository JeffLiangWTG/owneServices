using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	[Flags]
	public enum SuppressFields
	{
		ETA = 1,
		ETD = 2,
		ATA = 4,
		ATD = 8,
		FlightNumber = 16,
		TransportInfo = 64,
		Carrier = 128,
		DeclarationExportDate = 256,
		DeclarationDateAtOrigin = 512,
		DeclarationFolio = 1024,
		MasterBill = 2048,
		//It's important to preserve 1-3 dight codes when changing enum names to avoid writing a transformation
	}

	public static class RegistrySuppressionHelper
	{
		public static CodeDescriptionBoolCollection GetCollection(List<SuppressFields> defaults)
		{
			CodeDescriptionBoolCollection result = new CodeDescriptionBoolCollection();

			foreach (SuppressFields type in Enum.GetValues(typeof(SuppressFields)))
			{
				result.Add(GetCode(type), GetHumanReadableName(type), defaults != null && defaults.Contains(type));
			}

			return result;
		}

		public static bool GetSetting(ICodeDescriptionBoolList list, SuppressFields type)
		{
			foreach (ICodeDescriptionBool codeDescriptionBool in list)
			{
				if (codeDescriptionBool.Code == GetCode(type))
				{
					return codeDescriptionBool.Bool;
				}
			}

			foreach (ICodeDescriptionBool codeDescriptionBool in GetDefaultFields(Guid.Empty))
			{
				if (codeDescriptionBool.Code == GetCode(type))
				{
					return codeDescriptionBool.Bool;
				}
			}

			return true;
		}

		internal static MultilingualString GetWebHint(MultilingualString direstion)
		{
			return ResString.GetMultilingualString("cafc9fee-6ed3-4605-bba9-da169469fa87", @"Some regional regulations stipulate that airline and flight details must not be disclosed to any person prior to the goods being received by a Regulated Agent. To comply with these regulations this registry is set to suppress selected flight details in WebTracker until flight arrival. Override the defaults if you want some (or all) of the details to be disclosed on the WebTracker.", direstion);
		}

		public static CodeDescriptionBoolCollection GetDefaultFields(Guid branchPK)
		{
			return GetDefaultFields(branchPK, null);
		}

		public static CodeDescriptionBoolCollection GetDefaultFields(Guid branchPK, List<string> countriesWithDefaultSuppression)
		{
			ZString countryCode = ZString.Empty;
			if (branchPK == Guid.Empty)
			{
				countryCode = ((ZString)Env.CurrentBranch.NKUNLOCO).SubstringSafe(0, 2);
			}
			else
			{
				BusinessObject branch = (BusinessObject)new BusinessObjectFactory().Load<IGlbBranch>(branchPK);
				if (branch != null)
				{
					countryCode = ((ZString)branch[GlbBranchSchema.GB_RL_NKHomePort]).SubstringSafe(0, 2);
				}
			}
			return GetDefaultFields(countryCode, countriesWithDefaultSuppression == null || countriesWithDefaultSuppression.Contains(countryCode));
		}

		public static bool TryGetType(ZString code, out SuppressFields type)
		{
			bool result = false;
			var suppressFields = Enum.GetValues(typeof(SuppressFields));

			foreach (SuppressFields field in suppressFields)
			{
				if (code == GetCode(field))
				{
					result = true;
					type = field;
					return result;
				}
			}

			type = SuppressFields.ETA; // doesn't matter what the value is if result is false.
			return result;
		}

		static CodeDescriptionBoolCollection GetDefaultFields(ZString countryCode, bool defaultValue)
		{
			List<SuppressFields> defaults = null;

			if (!defaultValue)
			{
				defaults = new List<SuppressFields>();
			}
			switch (countryCode)
			{
				case Core.Constants.CountryCodes.UnitedStates:
					defaults = new List<SuppressFields> { SuppressFields.FlightNumber, SuppressFields.TransportInfo };
					break;
			}

			return GetCollection(defaults);
		}

		static string GetCode(SuppressFields type)
		{
			if (((int)type).ToString().Length > 3)
			{
				return ((int)type).ToString().Substring(0, 3);
			}
			return ((int)type).ToString().PadLeft(3, '0');
		}

		static MultilingualString GetHumanReadableName(SuppressFields type)
		{
			switch (type)
			{
				case SuppressFields.FlightNumber:
					return ResString.GetMultilingualString("b2f3f440-d584-46d2-9937-13c195f7b8f8", "Flight Number");
				case SuppressFields.MasterBill:
					return ResString.GetMultilingualString("af0bb9b0-6850-40d6-a11f-ed07573d7cd7", "Master Bill");
				case SuppressFields.TransportInfo:
					return ResString.GetMultilingualString("b0269a51-75e0-4bf5-b7b1-6a9c29d7447a", "Transport Info");
				case SuppressFields.DeclarationExportDate:
					return ResString.GetMultilingualString("79bdb238-0396-40a0-b6e6-bd11662055b1", "Declaration Export Date");
				case SuppressFields.DeclarationDateAtOrigin:
					return ResString.GetMultilingualString("614a45bf-a4d7-4111-ba7a-9d62aeb0e209", "Declaration Date At Origin");
				case SuppressFields.DeclarationFolio:
					return ResString.GetMultilingualString("0e91cb9e-9773-4d3d-95cf-e365ca4ce231", "Declaration Folio");
				default:
					return (NoResString)type.ToString();
			}
		}
	}
}
