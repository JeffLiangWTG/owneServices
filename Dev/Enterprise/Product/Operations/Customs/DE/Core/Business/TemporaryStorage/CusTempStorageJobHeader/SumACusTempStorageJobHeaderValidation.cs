using System.Collections.Immutable;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class SumACusTempStorageJobHeaderValidation : CusTempStorageJobHeaderValidation
	{
		public SumACusTempStorageJobHeaderValidation(CusTempStorageJobHeader header) : base(header)
		{
		}

		protected override void CheckSJH_TransportMeansDescription()
		{
			base.CheckSJH_TransportMeansDescription();
			if (Parent.SJH_TransportMeansCode == TemporaryStorageTransportMeansList.Codes.Other)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SJH_TransportMeansDescriptionInfo);
			}
		}

		protected override void CheckSJH_ContainerCount()
		{
			base.CheckSJH_ContainerCount();
			if (Parent.SJH_ContainerCount > 9999)
			{
				Parent.SJH_ContainerCountInfo.AddMessageError(Res.GetString("89F2589C-7C2E-428E-8884-1A3458CD6DBC", "Container count should be between 0 and 9999."));
			}
		}

		protected override void CheckSJH_CustomsOfficeOfEntryIntoEU()
		{
			base.CheckSJH_CustomsOfficeOfEntryIntoEU();
			ListValidation.MessageErrorIfInvalidCode(Parent.SJH_CustomsOfficeOfEntryIntoEUInfo);
			var previousReferenceType = Parent.SJH_PreviousReferenceType;
			if (previousReferenceType == PreviousReferenceType.Codes._ESUMA || previousReferenceType == PreviousReferenceType.Codes._ENST2L)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SJH_CustomsOfficeOfEntryIntoEUInfo);
			}
		}

		protected override void CheckSJH_PreviousReferenceType()
		{
			base.CheckSJH_PreviousReferenceType();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.SJH_PreviousReferenceTypeInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.SJH_PreviousReferenceTypeInfo);
		}

		protected override void CheckSJH_PreviousReferenceNumber()
		{
			base.CheckSJH_PreviousReferenceNumber();
			var previousReferenceType = Parent.SJH_PreviousReferenceType;
			if (!Parent.SJH_PreviousReferenceNumber.IsEmpty)
			{
				if (keyPreviousReferenceTypes.Contains(previousReferenceType))
				{
					Parent.SJH_PreviousReferenceNumberInfo.AddMessageError(Res.GetString("8a4014ca-0cff-499e-a350-b241af4bf8f6", "{0} must be blank for {1} '{2}'.",
						Parent.SJH_PreviousReferenceNumberInfo.HumanReadableName,
						Parent.SJH_PreviousReferenceTypeInfo.HumanReadableName,
						previousReferenceType));
				}
				else if (Parent.SJH_NCTSFlag || previousReferenceTypesAllowPopulatePrevRefNumInHeaderOrLine.Contains(previousReferenceType))
				{
					var mrnError = MRNFormatValidator.CheckMRNFormat(Parent.SJH_PreviousReferenceNumber, factory, Res.GetString("c8e056bd-9c99-477a-9437-d1217502d8eb", "When NCTS-Flag is ticked or previous reference type is 'ESUMA', 'N355' or 'POUS'"));
					if (!mrnError.IsEmpty)
					{
						Parent.SJH_PreviousReferenceNumberInfo.AddMessageError(mrnError);
					}
					if (!Parent.SJH_NCTSFlag)
					{
						var countryCodeOfEntryOffice = Parent.SJH_CustomsOfficeOfEntryIntoEU.Left(2);
						var previousRefNumCountryCode = Parent.SJH_PreviousReferenceNumber.SubstringSafe(2, 2);
						if (!Parent.SJH_CustomsOfficeOfEntryIntoEU.IsEmpty &&
							!allowedEntryOfficeCountries.Contains(countryCodeOfEntryOffice) &&
							countryCodeOfEntryOffice != previousRefNumCountryCode)
						{
							Parent.SJH_PreviousReferenceNumberInfo.AddMessageError(Res.GetString("07a0d74c-c0b9-4f3f-9646-12e4d02242ab"
								, @"When Previous Reference Type is ESUMA and Entry Customs Office does not start with {0} then the Previous Reference Number's country/region code ({1}) should match the Entry Customs Office country/region code ({2})."
								, FormattetdAllowedEntryOfficeCountryList, previousRefNumCountryCode, countryCodeOfEntryOffice));
						}
					}
				}
			}
			else if (!previousReferenceType.IsEmpty
				&& previousReferenceType.Left(1) != "4"
				&& !keyPreviousReferenceTypes.Contains(previousReferenceType)
				&& !previousReferenceTypesAllowPopulatePrevRefNumInHeaderOrLine.Contains(previousReferenceType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SJH_PreviousReferenceNumberInfo);
			}
		}

		protected override void CheckSJH_ArrivalDate()
		{
			base.CheckSJH_ArrivalDate();
			var transportMeansCode = Parent.SJH_TransportMeansCode;
			if (transportMeansCode == TemporaryStorageTransportMeansList.Codes.Vessel || transportMeansCode == TemporaryStorageTransportMeansList.Codes.Aircraft)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SJH_ArrivalDateInfo);
			}
		}

		protected override void CheckSJH_PresentationDate()
		{
			base.CheckSJH_PresentationDate();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.SJH_PresentationDateInfo);
		}

		protected override void CheckSJH_TransportMeansCode_Mandatory()
		{
			if (Parent.CUSPRLCusTempStorageDec != null)
			{
				base.CheckSJH_TransportMeansCode_Mandatory();
			}
		}

		string FormattetdAllowedEntryOfficeCountryList
		{
			get
			{
				if (!formattetdAllowedEntryOfficeCountryList.HasValue)
				{
					var builder = new StringBuilder();
					var secondLastElement = allowedEntryOfficeCountries.Count - 1;
					var count = 1;
					foreach (var country in allowedEntryOfficeCountries.OrderBy(x => x))
					{
						builder.Append(country);
						if (count == secondLastElement)
						{
							builder.Append(Res.GetString("8d37bf5d-858f-4f52-9efd-911219ee206a", " or "));
						}
						else
						{
							builder.Append(", ");
						}
						count++;
					}
					formattetdAllowedEntryOfficeCountryList = builder.ToString().Trim();
				}
				return formattetdAllowedEntryOfficeCountryList.Value;
			}
		}
		ZString? formattetdAllowedEntryOfficeCountryList;

		ImmutableHashSet<string> allowedEntryOfficeCountries => factory.GetCachedValue((NoResString)"Enterprise.Customs.DE.Business.SumACusTempStorageJobHeaderValidation | AllowedEntryOfficeCountries", // Cache Key
			() => ImmutableHashSet.Create(
				Core.Constants.CountryCodes.Austria
				, Core.Constants.CountryCodes.Bulgaria
				, Core.Constants.CountryCodes.Cyprus
				, Core.Constants.CountryCodes.CzechRepublic
				, Core.Constants.CountryCodes.Denmark
				, Core.Constants.CountryCodes.Latvia
				, Core.Constants.CountryCodes.Portugal
				, Core.Constants.CountryCodes.Romania
				, Core.Constants.CountryCodes.Slovakia
			)
		);

		ImmutableHashSet<string> keyPreviousReferenceTypes => factory.GetCachedValue((NoResString)"Enterprise.Customs.DE.Business.SumACusTempStorageJobHeaderValidation | KeyPreviousReferenceTypes", // Cache Key
			() => ImmutableHashSet.Create(
				PreviousReferenceType.Codes._199
				, PreviousReferenceType.Codes._200
				, PreviousReferenceType.Codes._ENST2L
				, PreviousReferenceType.Codes._FREIZ
				, PreviousReferenceType.Codes._OHNE
				, PreviousReferenceType.Codes._PVV
			)
		);

		ImmutableHashSet<string> previousReferenceTypesAllowPopulatePrevRefNumInHeaderOrLine => factory.GetCachedValue((NoResString)"Enterprise.Customs.DE.Business.SumACusTempStorageJobHeaderValidation | PreviousReferenceTypesAllowPopulatePrevRefNumInHeaderOrLine", // Cache Key
			() => ImmutableHashSet.Create(
				PreviousReferenceType.Codes._ESUMA
				, PreviousReferenceType.Codes._N355
				, PreviousReferenceType.Codes._POUS
			)
		);

		BusinessObjectFactory factory => Parent.Factory;
	}
}
