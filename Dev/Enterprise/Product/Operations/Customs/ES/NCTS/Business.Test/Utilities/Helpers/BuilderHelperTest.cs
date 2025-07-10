using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public static class BuilderHelperTest
	{
		public static void AddSecurityData(this NctsHeader nctsHeader)
		{
			nctsHeader.MovementHeader.BM_BTAIndicator = "S";
			nctsHeader.MovementHeader.BM_MethodOfPayment = "P";
			nctsHeader.MovementHeader.BM_AdditionalText = "7";
			nctsHeader.MovementHeader.BM_ConveyanceNumber = "S7";
			nctsHeader.PlaceOfUnloadingCode = "UNLOD";
			nctsHeader.SecurityConsignor.E2_OA_Address = nctsHeader.Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			nctsHeader.SecurityConsignee.E2_OA_Address = nctsHeader.Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			nctsHeader.BH_OH_Carrier = nctsHeader.Factory.NewWithValidTestData<OrgHeader>().PK;
			nctsHeader.BH_UniqueVoyageIdentifier = "112233";
		}

		public static void SetValues(this NctsPackage package, ZString status, ZString type, ZString marks, ZLong qty, ZDecimal grossWeight, string vin = "", string brand = "", string model = "",
										string typeDif = "", string marksDif = "", string vinDif = "", string brandDif = "", string modelDif = "", long qtyDif = 0)
		{
			package.B5_TypeOfDifference = status;
			package.B5_UnitType = type;
			package.B5_MarksAndNumbers = marks;
			package.B5_PackageID = vin;
			package.B5_Brand = brand;
			package.B5_Model = model;
			package.B5_UnitCount = qty;
			package.B5_GrossWeight = grossWeight;
			if (status == "DIF")
			{
				var packageDif = package.PackDifference;
				packageDif.B5_UnitType = typeDif;
				packageDif.B5_MarksAndNumbers = marksDif;
				packageDif.B5_PackageID = vinDif;
				packageDif.B5_Brand = brandDif;
				packageDif.B5_Model = modelDif;
				packageDif.B5_UnitCount = qtyDif;
			}
		}

		public static void SetValues(this NctsArrivalCargoDesc item, ZString status, ZShort lineNum, ZInt declGoodsLineNum, ZString tariff, ZString cusCode, ZString description, ZDecimal grossWeight,
										string tariffDif = "", string cusCodeDif = "", string descriptionDif = "", decimal grossWeightDif = 0)
		{
			item.BY_UnloadedState = status;
			item.BY_LineNo = lineNum;
			item.BY_DeclarationGoodsItemNumber = declGoodsLineNum;
			item.BY_HarmonisedTariff = tariff;
			item.BY_CusC4Number = cusCode;
			item.BY_Description = description;
			item.BY_GrossWeight = grossWeight;
			item.BY_MonetaryValue = 1000m;
			item.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Spain;
			var supplementaryCode1 = item.AdditionalSupplementaryCodes.AddNew();
			supplementaryCode1.CY_Code = "AC01";
			var supplementaryCode2 = item.AdditionalSupplementaryCodes.AddNew();
			supplementaryCode2.CY_Code = "AC02";

			if (status == "DIF")
			{
				var unloadedGoodsItem = item.UnloadedGoodsItem;
				unloadedGoodsItem.BY_HarmonisedTariff = tariffDif;
				unloadedGoodsItem.BY_CusC4Number = cusCodeDif;
				unloadedGoodsItem.BY_Description = descriptionDif;
				unloadedGoodsItem.BY_GrossWeight = grossWeightDif;
				item.LiabilityTariff = tariffDif;
			}
		}
	}
}
