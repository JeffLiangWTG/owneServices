using Enterprise.Customs.CA.Messaging;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusSCAPivotLookups : Customs.Business.CusSCAPivotLookups
	{
		public CusSCAPivotLookups(CusSCAPivot parent)
			: base(parent)
		{
			cusSCAPivot = parent;
		}

		readonly CusSCAPivot cusSCAPivot;

		public CodeDescriptionPairList CV_OceanBillContainers_List
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				if (cusSCAPivot.OceanBill != null)
				{
					foreach (CusSCAContainer container in cusSCAPivot.OceanBill.Containers)
					{
						if (!container.CN_ContainerNumber.IsEmpty)
						{
							result.AddPair(container.CN_ContainerNumber);
						}
					}
				}
				return result;
			}
		}

		public CodeDescriptionPairList AcrossPackageTypes
		{
			get { return Factory.GetCachedValue<ACROSSPackageTypes>(); }
		}

		public CodeDescriptionPairList ACIWeightUnits
		{
			get
			{
				return Factory.GetCachedValue("ACIWeightUnits",
					delegate
					{
						CodeDescriptionPairList result = new CodeDescriptionPairList();
						result.AddPair(Core.Constants.Weight.Kilograms, Res.GetString("1edf9178-2f8f-4458-8103-16889f7e5b99", "Kilogram"));
						result.AddPair(Core.Constants.Weight.Tonnes, Res.GetString("ec18c7be-b3dc-48a6-89ed-9a645464844b", "Metric Ton"));
						result.AddPair(Core.Constants.Weight.Pounds, Res.GetString("4ff0c322-b7f5-4302-9bab-d742a5e18ba1", "Pound"));
						return result;
					}
				);
			}
		}

		public CodeDescriptionPairList VolumeUQList
		{
			get
			{
				return Factory.GetCachedValue("VolumeUQList",
					delegate
					{
						CodeDescriptionPairList result = new CodeDescriptionPairList(OLookUpEditType.Volume);
						result.AddPair(MessageConstants.ACIVolumeUnits.CubicCentimetre, Res.GetString("64a52210-7514-4952-b0d5-1f129f18addb", "Cubic Centimeter"));
						result.AddPair(MessageConstants.ACIVolumeUnits.Cord, Res.GetString("54d9d663-bd5e-4e95-9be4-84a4b2eeff40", "Cord"));
						result.AddPair(MessageConstants.ACIVolumeUnits.BoardFoot100, Res.GetString("1adb0892-6e80-47f6-84f6-34357b08fa38", "100 Board Foot"));
						result.AddPair(MessageConstants.ACIVolumeUnits.GallonsUK, Res.GetString("13d7a19a-32aa-437b-a295-5e4661cd5030", "Gallons UK"));
						result.AddPair(MessageConstants.ACIVolumeUnits.HundredsTTTons, Res.GetString("4f041c88-4513-401d-965d-f155707a9989", "Hundreds of Measurement TT-Tons"));
						result.AddPair(MessageConstants.ACIVolumeUnits.GallonsUSDry, Res.GetString("5fbc1faf-f9aa-47d7-805b-173e36c0e45f", "Gallons US Dry"));
						result.AddPair(MessageConstants.ACIVolumeUnits.GallonsUSLiquid, Res.GetString("68feb50b-062c-4655-ae7c-01d31f669054", "Gallons US Liquid"));
						result.AddPair(MessageConstants.ACIVolumeUnits.HundredsTTTonsShort, Res.GetString("96e52f2e-bfee-423b-9910-b5557c3b2aa0", "Hundreds of Measurement TT-Ton-Short"));
						result.AddPair(MessageConstants.ACIVolumeUnits.TonShort, Res.GetString("f6e1f6e9-b8be-4eae-834f-9976baff7860", "Measurement Ton-Short"));
						result.AddPair(MessageConstants.ACIVolumeUnits.TonMetric, Res.GetString("fdfab52a-45e1-4c34-ba92-c205b691d004", "Measurement Ton-Metric"));
						result.AddPair(MessageConstants.ACIVolumeUnits.Car, Res.GetString("52ca6bd3-2fe0-4b29-82d1-b72e2921af3a", "Car"));
						result.AddPair(MessageConstants.ACIVolumeUnits.TonLong, Res.GetString("51e2edca-dee3-491b-8d5e-1d1f07eaded5", "Measurement Ton-Long"));
						result.AddPair(MessageConstants.ACIVolumeUnits.VolumetricUnit, Res.GetString("55624ddd-6fa0-44a7-a01f-8faf95e8fc45", "Volumetric Unit"));
						result.AddPair(MessageConstants.ACIVolumeUnits.Barge, Res.GetString("9b458e91-3737-49c3-9433-9859415219ac", "Barge"));
						result.AddPair(MessageConstants.ACIVolumeUnits.Container, Res.GetString("c3508aed-ebaf-402e-8839-f83066475771", "Container"));
						result.AddPair(MessageConstants.ACIVolumeUnits.LoadForEnterprise, Res.GetString("a85d9974-8f65-45ab-af7a-4b4c9ccc7522", "Load"));
						return result;
					}
				);
			}
		}
	}
}
