using System.Collections;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AsycudaManifestHeaderLookups : ASYCUDA.Business.AsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderLookups(ASYCUDA.Business.AsycudaManifestHeader parent) : base(parent)
		{
		}

		public new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		protected override CodeDescriptionPairList SpecificCircumstanceListCore => Factory.GetCachedValue("ICS2|EUSpecificCircumstanceList_" + Parent.AMA_ApplicationCode + Parent.AMA_TransportMode, delegate
		{
			var parent = Parent;
			var typeList = new CodeDescriptionPairList();
			var transportMode = parent.AMA_TransportMode;
			switch (parent.AMA_ApplicationCode)
			{
				case ApplicationCodeTypeList.Codes.Consolidator:

					typeList.AddPair(EUICS2SpecificCircumstanceList.Codes.F43, EUICS2SpecificCircumstanceList.Descriptions.F43);
					typeList.AddPair(EUICS2SpecificCircumstanceList.Codes.F44, EUICS2SpecificCircumstanceList.Descriptions.F44);

					if (transportMode.In(new ZString[] { Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.InlandWaterwayTransport }))
					{
						typeList.AddPair(EUICS2SpecificCircumstanceList.Codes.F14, EUICS2SpecificCircumstanceList.Descriptions.F14);
						typeList.AddPair(EUICS2SpecificCircumstanceList.Codes.F15, EUICS2SpecificCircumstanceList.Descriptions.F15);
						typeList.AddPair(EUICS2SpecificCircumstanceList.Codes.F16, EUICS2SpecificCircumstanceList.Descriptions.F16);
						typeList.AddPair(EUICS2SpecificCircumstanceList.Codes.F17, EUICS2SpecificCircumstanceList.Descriptions.F17);
					}
					else if (transportMode == Core.Constants.TransportModes.Air)
					{
						typeList.AddPair(EUICS2SpecificCircumstanceList.Codes.F22, EUICS2SpecificCircumstanceList.Descriptions.F22);
						typeList.AddPair(EUICS2SpecificCircumstanceList.Codes.F23, EUICS2SpecificCircumstanceList.Descriptions.F23);
						typeList.AddPair(EUICS2SpecificCircumstanceList.Codes.F24, EUICS2SpecificCircumstanceList.Descriptions.F24);
						typeList.AddPair(EUICS2SpecificCircumstanceList.Codes.F25, EUICS2SpecificCircumstanceList.Descriptions.F25);
						typeList.AddPair(EUICS2SpecificCircumstanceList.Codes.F26, EUICS2SpecificCircumstanceList.Descriptions.F26);
					}
					typeList.Sort();
					return typeList;

				case ApplicationCodeTypeList.Codes.ShippingLine:

					if (transportMode.In(new ZString[] { Core.Constants.TransportModes.Road }))
					{
						typeList.AddPair(EUICS2SpecificCircumstanceList.Codes.F40,
							EUICS2SpecificCircumstanceList.Descriptions.F40);
						typeList.AddPair(EUICS2SpecificCircumstanceList.Codes.F50,
							EUICS2SpecificCircumstanceList.Descriptions.F50);
					}
					else if (transportMode.In(new ZString[] { Core.Constants.TransportModes.Rail }))
					{
						typeList.AddPair(EUICS2SpecificCircumstanceList.Codes.F41, EUICS2SpecificCircumstanceList.Descriptions.F41);
						typeList.AddPair(EUICS2SpecificCircumstanceList.Codes.F51, EUICS2SpecificCircumstanceList.Descriptions.F51);
					}
					else if (transportMode.In(new ZString[] { Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.InlandWaterwayTransport }))
					{
						typeList.AddPair(EUICS2SpecificCircumstanceList.Codes.F10, EUICS2SpecificCircumstanceList.Descriptions.F10);
						typeList.AddPair(EUICS2SpecificCircumstanceList.Codes.F11, EUICS2SpecificCircumstanceList.Descriptions.F11);
						typeList.AddPair(EUICS2SpecificCircumstanceList.Codes.F12, EUICS2SpecificCircumstanceList.Descriptions.F12);
						typeList.AddPair(EUICS2SpecificCircumstanceList.Codes.F13, EUICS2SpecificCircumstanceList.Descriptions.F13);
					}

					return typeList;

				default:
					return Factory.GetCachedValue<EUICS2SpecificCircumstanceList>();
			}
		});

		public override CodeDescriptionPairList TransportModeList
		{
			get
			{
				var parent = Parent;
				var result = base.TransportModeList;
				result = new CodeDescriptionPairList(result);
				var isTransportModeEnabledROA = FuncsHelper.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.ICS2TransportModeROA, ZDateTime.Today, options: FuncsHelper.ValidationOptions.UseCountryDefinitionFirstOtherwiseEUN, priorityToPilotFunctionality: true);

				if (!isTransportModeEnabledROA)
				{
					result.RemoveCode(Customs.Business.TransportTypeList.Codes.Road);
				}

				if (!parent.IsTransportModeEnabledRAI)
				{
					result.RemoveCode(Customs.Business.TransportTypeList.Codes.Rail);
				}

				if (parent.AMA_ApplicationCode == ApplicationCodeTypeList.Codes.ShippingLine)
				{
					result.RemoveCode(Customs.Business.TransportTypeList.Codes.Air);
				}

				return result;
			}
		}

		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<MessageStatusCodeList>();

		public CodeDescriptionPairList MOTIdentifierTypeList => Factory.GetCachedValue<EUICS2ModeOfTransportIdentifierTypeList>();

		public CodeDescriptionPairList PaymentMethodList => Factory.GetCachedValue<EUICS2PaymentMethodList>();

		public CodeDescriptionPairList CountryCodeICS2MS => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_IC2MS, ZDateTime.Today);

		public CodeDescriptionPairList CustomsProfileList => Factory.GetCachedValue("ICS2|EUProfileList", delegate
		{
			var codeDescriptionPairList = new CodeDescriptionPairList();
			foreach (var validICS2Password in Parent.ValidICS2Credentials)
			{
				var company = validICS2Password.Company;
				codeDescriptionPairList.AddPair(company.GC_Code, company.CompanyName);
			}

			codeDescriptionPairList.Sort();
			return codeDescriptionPairList;
		});

		public CodeDescriptionPairList MeansOfTransportTypeList => RefCusCodeListTypes.GetCachedList(Factory, Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_IC2MT, ZDateTime.Today);

		protected override ICollection CustomsOfficesCore => EUCustomsOfficeCodeCollection.AllEuropeanUnionAndOtherCountriesCustomsOfficesWithRequiredRoles(Factory, [Core.Constants.CountryCodes.Norway, Core.Constants.CountryCodes.Switzerland]);

		public OrganisationsFindBoxCollection DeclarantAddresses => declarantAddresses ??= new OrganisationsFindBoxCollection(Factory);
		OrganisationsFindBoxCollection declarantAddresses;
	}
}
