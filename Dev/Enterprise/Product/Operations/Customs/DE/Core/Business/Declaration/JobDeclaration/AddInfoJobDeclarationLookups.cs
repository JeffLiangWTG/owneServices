using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public partial class JobDeclarationLookups
	{
		public override CodeDescriptionPairList SpecificCircumstanceIndicatorList => Factory.GetCachedValue<SpecificCircumstanceIndicatorForUCCList>();

		public override CodeDescriptionPairList DeferTypeList => Declaration.Lookups.PaymentPartyList;

		public CodeDescriptionPairList VATAccountNumberList => DeferralPaymentPartyList.GetDeferralAccountNumberList(Parent.ZG_VATDeferType, Parent);

		public override CodeDescriptionPairList MethodOfPaymentList
		{
			get
			{
				return Factory.GetCachedValue("MethodOfPaymentList_DE", () =>
				{
					var excludeCodesFilter = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code,
						SQLComparisonOperator.NotEqual,
						new[]
						{
							UniversalReferenceConstants.MethodOfPaymentTypes.S,
							UniversalReferenceConstants.MethodOfPaymentTypes.L,
						});

					var result = new CodeDescriptionPairList();
					result.AddRange(ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Germany,
						Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, ZDateTime.Now, additionalFilter: excludeCodesFilter, includeParentDataGrouping: false)
						.OrderBy(x => x.ZZD_Code).ToArray());
					return result;
				});
			}
		}

		public override CodeDescriptionPairList BorderTransportMeansList
		{
			get
			{
				var isImport = Declaration.IsImport;
				var transportMode = Declaration.JE_TransportMode;
				return Factory.GetCachedValue(string.Join("|", "DE.AddInfoJobDeclarationLookups.BorderTransportMeansList", isImport, transportMode), () =>
				{
					var result = new CodeDescriptionPairList();
					if (isImport)
					{
						result = GetBorderTransportMeansListForImport(transportMode, result);
					}
					else
					{
						result = GetBorderTransportMeansListForExport(transportMode, result);
					}
					return result;
				});
			}
		}

		CodeDescriptionPairList GetBorderTransportMeansListForImport(ZString transportMode, CodeDescriptionPairList result)
		{
			switch (transportMode)
			{
				case Customs.Business.TransportTypeList.Codes.Road:
					result.AddPair(ImportBorderTransportMeansList.Codes.Truck, ImportBorderTransportMeansList.Descriptions.Truck);
					result.AddPair(ImportBorderTransportMeansList.Codes.Car, ImportBorderTransportMeansList.Descriptions.Car);
					result.AddPair(ImportBorderTransportMeansList.Codes.Without, ImportBorderTransportMeansList.Descriptions.Without);
					result.DefaultCode = ImportBorderTransportMeansList.Codes.Truck;
					break;
				case Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport:
				case Customs.Business.TransportTypeList.Codes.Sea:
					result.AddPair(ImportBorderTransportMeansList.Codes.Vessel, ImportBorderTransportMeansList.Descriptions.Vessel);
					result.AddPair(ImportBorderTransportMeansList.Codes.Without, ImportBorderTransportMeansList.Descriptions.Without);
					result.DefaultCode = ImportBorderTransportMeansList.Codes.Vessel;
					break;
				case Customs.Business.TransportTypeList.Codes.Rail:
					result.AddPair(ImportBorderTransportMeansList.Codes.Wagon, ImportBorderTransportMeansList.Descriptions.Wagon);
					result.AddPair(ImportBorderTransportMeansList.Codes.Without, ImportBorderTransportMeansList.Descriptions.Without);
					result.DefaultCode = ImportBorderTransportMeansList.Codes.Wagon;
					break;
				case Customs.Business.TransportTypeList.Codes.Air:
					result.AddPair(ImportBorderTransportMeansList.Codes.Aircraft, ImportBorderTransportMeansList.Descriptions.Aircraft);
					result.AddPair(ImportBorderTransportMeansList.Codes.Without, ImportBorderTransportMeansList.Descriptions.Without);
					result.DefaultCode = ImportBorderTransportMeansList.Codes.Aircraft;
					break;
				case Customs.Business.TransportTypeList.Codes.FixedTransportInstallations:
				case Customs.Business.TransportTypeList.Codes.OwnPropulsion:
				case Customs.Business.TransportTypeList.Codes.Mail:
					result.AddPair(ImportBorderTransportMeansList.Codes.Without, ImportBorderTransportMeansList.Descriptions.Without);
					result.AddPair(ImportBorderTransportMeansList.Codes.Other, ImportBorderTransportMeansList.Descriptions.Other);
					break;
				default:
					break;
			}
			return result;
		}

		public override ICollection AgreedPlaceCodeList => Factory.GetAgreedPlaceCodeList(Parent.CountryCode);
	}
}
