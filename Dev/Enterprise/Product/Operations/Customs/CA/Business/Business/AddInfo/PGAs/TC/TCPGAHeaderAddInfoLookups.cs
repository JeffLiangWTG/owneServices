//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoTCPGAHeaderAddInfoLookups
//
//    This class should be used for overriding collections in AutoTCPGAHeaderAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class TCPGAHeaderAddInfoLookups : AutoTCPGAHeaderAddInfoLookups
	{
		public TCPGAHeaderAddInfoLookups(AutoTCPGAHeaderAddInfo parent) : base(parent)
		{
		}

		TCPGAHeader PGAHeader => (TCPGAHeader)((TCPGAHeaderAddInfo)Parent).Parent;

		public CodeDescriptionPairList ProgramCodesList => Factory.GetCachedValue<TCPGADepartmentCodes>();

		public CodeDescriptionPairList SubProgramCodesList => Factory.GetCachedValue<TCPGAVehicleProgramCodes>();

		public CodeDescriptionPairList ProductClassList
		{
			get
			{
				var result = new CodeDescriptionPairList();

				if (PGAHeader.CA_TPRProgramInd == YesNoList.Codes.Yes)
				{
					result.AddPairIfNotExist(TCProductCategories.Codes.TC01, TCProductCategories.Descriptions.TC01);
					result.AddPairIfNotExist(TCProductCategories.Codes.TC02, TCProductCategories.Descriptions.TC02);
					result.AddPairIfNotExist(TCProductCategories.Codes.TC03, TCProductCategories.Descriptions.TC03);
				}

				if (PGAHeader.CA_VPRProgramInd == YesNoList.Codes.Yes)
				{
					result.AddRangeOverwriteIfExists(Factory.GetCachedValue<TCTireTypes>());
				}

				return result;
			}
		}

		public CodeDescriptionPairList ChassisYearList
		{
			get { return Factory.GetCachedValue("TCChassisYearList", () => YearListHelper.GetYearList(ZDateTime.Today.Year + 2)); }
		}

		public CodeDescriptionPairList ManufactureYearList
		{
			get { return Factory.GetCachedValue("TCManufactureYearList", () => YearListHelper.GetYearList(ZDateTime.Today.Year + 1)); }
		}

		public CodeDescriptionPairList ManufactureMonthList
		{
			get
			{
				return Factory.GetCachedValue(
					"TCManufactureMonthList",
					() =>
					{
						var result = new CodeDescriptionPairList();
						result.AddPair("1", Res.GetString("af5f3b2e-4022-4947-b4de-c6a3e2e5f9cf", "January"));
						result.AddPair("2", Res.GetString("d0215c19-5653-44f4-92dd-3a9b59c8e050", "February"));
						result.AddPair("3", Res.GetString("02e6ecf9-3a2f-4c67-a706-d88173f6fbcd", "March"));
						result.AddPair("4", Res.GetString("3a6351a9-4501-4bf1-b7fc-0bcc23602235", "April"));
						result.AddPair("5", Res.GetString("70a88f68-98f4-4623-9f12-cc67f02ba643", "May"));
						result.AddPair("6", Res.GetString("d4c6856b-cbce-44bf-80d6-98eb7514fe16", "June"));
						result.AddPair("7", Res.GetString("bd0c70e5-dc78-41bf-a3dd-cb64720bb4bf", "July"));
						result.AddPair("8", Res.GetString("567e0361-e779-436a-a6eb-7930e4456e80", "August"));
						result.AddPair("9", Res.GetString("b5db2dc6-5034-4cc1-9025-5b6076003d4b", "September"));
						result.AddPair("10", Res.GetString("ca80da7e-8ffd-48d9-84a4-0d395d31ebd8", "October"));
						result.AddPair("11", Res.GetString("7f2769e8-d903-4bb0-a2f0-c0875da0f3a5", "November"));
						result.AddPair("12", Res.GetString("d55126b0-60e0-4f14-af31-1fd40a739c88", "December"));
						return result;
					});
			}
		}

		public CodeDescriptionPairList ProductTypeList
		{
			get
			{
				return Factory.GetCachedValue(
					"TCPGAProductTypeList",
					() =>
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(TCProductCategories.Codes.TC04, TCProductCategories.Descriptions.TC04);
						result.AddPair(TCProductCategories.Codes.TC05, TCProductCategories.Descriptions.TC05);

						return result;
					});
			}
		}

		public CodeDescriptionPairList ProductSizeList
		{
			get
			{
				return Factory.GetCachedValue(
					"TCPGAProductSizeList",
					() =>
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(TCProductCategories.Codes.TC06, TCProductCategories.Descriptions.TC06);
						result.AddPair(TCProductCategories.Codes.TC07, TCProductCategories.Descriptions.TC07);
						result.AddPair(TCProductCategories.Codes.TC08, TCProductCategories.Descriptions.TC08);
						result.AddPair(TCProductCategories.Codes.TC09, TCProductCategories.Descriptions.TC09);
						result.AddPair(TCProductCategories.Codes.TC10, TCProductCategories.Descriptions.TC10);
						result.AddPair(TCProductCategories.Codes.TC11, TCProductCategories.Descriptions.TC11);
						result.AddPair(TCProductCategories.Codes.TC12, TCProductCategories.Descriptions.TC12);
						result.AddPair(TCProductCategories.Codes.TC13, TCProductCategories.Descriptions.TC13);

						return result;
					});
			}
		}

		public CodeDescriptionPairList ImportReasonCodeList => Factory.GetCachedValue<TCIntendedUseCodes>();

		public CodeDescriptionPairList TitleStatusList
		{
			get
			{
				return Factory.GetCachedValue("TCPGATitleStatusList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(TCProductCategories.Codes.TC14, TCProductCategories.Descriptions.TC14);
					result.AddPair(TCProductCategories.Codes.TC15, TCProductCategories.Descriptions.TC15);
					result.AddPair(TCProductCategories.Codes.TC16, TCProductCategories.Descriptions.TC16);
					result.AddPair(TCProductCategories.Codes.TC17, TCProductCategories.Descriptions.TC17);

					return result;
				});
			}
		}

		public CodeDescriptionPairList VehicleConditionList => Factory.GetCachedValue<TCVehicleConditions>();

		public CodeDescriptionPairList VehicleStatusList
		{
			get
			{
				return Factory.GetCachedValue("TCPGAVehicleStatusList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(TCProductCategories.Codes.TC18, TCProductCategories.Descriptions.TC18);
					result.AddPair(TCProductCategories.Codes.TC19, TCProductCategories.Descriptions.TC19);

					return result;
				});
			}
		}

		public CodeDescriptionPairList CriteriaConformanceList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				if (PGAHeader.CA_TPRProgramInd == YesNoList.Codes.Yes)
				{
					result.AddPairIfNotExist(TCComplicanceStatements.Codes.TC03, TCComplicanceStatements.Descriptions.TC03);
				}

				if (PGAHeader.CA_VPRProgramInd == YesNoList.Codes.Yes)
				{
					result.AddPairIfNotExist(TCComplicanceStatements.Codes.TC03, TCComplicanceStatements.Descriptions.TC03);
					result.AddPairIfNotExist(TCComplicanceStatements.Codes.TC05, TCComplicanceStatements.Descriptions.TC05);
				}

				return result;
			}
		}

		public CodeDescriptionPairList ImporterDeclarationList => Factory.GetCachedValue<TCComplicanceStatements>();

		public RefCountryCollection CountryOfOriginsLookup
		{
			get
			{
				if (PGAHeader is TCPGAHeader pgaHeader)
				{
					if (pgaHeader.RequirementsParent is IHasPGARequirements requirementsParent)
					{
						return requirementsParent.CountryOfOriginsLookup;
					}
				}
				return new RefCountryCollection(Factory);
			}
		}
	}
}
