//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusCAeMHHouseLookups
//
//    This class should be used for overriding collections in AutoCusCAeMHHouseLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Types;
using Enterprise.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHHouseLookups : AutoCusCAeMHHouseLookups
	{
		public CusCAeMHHouseLookups(AutoCusCAeMHHouse parent) : base(parent)
		{
		}

		public eMHMovementTypeList MovementTypes
		{
			get { return Factory.GetCachedValue<eMHMovementTypeList>(); }
		}

		public CodeDescriptionPairList WeightUnits
		{
			get { return Factory.GetCachedValue<EManifestUnitOfWeightList>(); }
		}

		public CodeDescriptionPairList VolumnUnits
		{
			get
			{
				return Factory.GetCachedValue("CAeMHHouse.VolumnUQ", delegate
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(CustomsUnitOfMeasureList.Codes.CubicCentimetre, CustomsUnitOfMeasureList.Descriptions.CubicCentimetre);
						result.AddPair(CustomsUnitOfMeasureList.Codes.CubicDecimetre, CustomsUnitOfMeasureList.Descriptions.CubicDecimetre);
						result.AddPair(CustomsUnitOfMeasureList.Codes.CubicMetre, CustomsUnitOfMeasureList.Descriptions.CubicMetre);
						result.AddPair(CustomsUnitOfMeasureList.Codes.Litre, CustomsUnitOfMeasureList.Descriptions.Litre);
						result.AddPair(CustomsUnitOfMeasureList.Codes.CubicMillimetre, CustomsUnitOfMeasureList.Descriptions.CubicMillimetre);
						result.AddPair(CustomsUnitOfMeasureList.Codes.ThousandCubicMetres, CustomsUnitOfMeasureList.Descriptions.ThousandCubicMetres);
						result.AddPair(CustomsUnitOfMeasureList.Codes.MillionCubicMetres, CustomsUnitOfMeasureList.Descriptions.MillionCubicMetres);
						result.AddPair(CustomsUnitOfMeasureList.Codes.Millilitre, CustomsUnitOfMeasureList.Descriptions.Millilitre);
						result.AddPair(CustomsUnitOfMeasureList.Codes.Centilitre, CustomsUnitOfMeasureList.Descriptions.Centilitre);
						result.AddPair(CustomsUnitOfMeasureList.Codes.Decilitre, CustomsUnitOfMeasureList.Descriptions.Decilitre);
						result.AddPair(CustomsUnitOfMeasureList.Codes.Hectolitre, CustomsUnitOfMeasureList.Descriptions.Hectolitre);
						result.AddPair(CustomsUnitOfMeasureList.Codes.Megalitre, CustomsUnitOfMeasureList.Descriptions.Megalitre);
						result.AddPair(CustomsUnitOfMeasureList.Codes.LitrePureAlcohol, CustomsUnitOfMeasureList.Descriptions.LitrePureAlcohol);
						result.AddPair(CustomsUnitOfMeasureList.Codes.HectolitrePureAlcohol, CustomsUnitOfMeasureList.Descriptions.HectolitrePureAlcohol);
						return result;
					});
			}
		}

		public ZZRefCusCodeListCombinedCollection ReleasePorts
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public CACSubLocationCollection ReleaseSubLocations
		{
			get { return new CACSubLocationCollection(Factory); }
		}

		public EManifestAmendmentReasonCodes AmendmentCodes
		{
			get { return Factory.GetCachedValue<EManifestAmendmentReasonCodes>(); }
		}

		public EManifestForwarderJobStatusList CustomsStatuses
		{
			get { return Factory.GetCachedValue<EManifestForwarderJobStatusList>(); }
		}

		public MessageStatusList MessageStatuses
		{
			get { return Factory.GetCachedValue<MessageStatusList>(); }
		}
	}
}
