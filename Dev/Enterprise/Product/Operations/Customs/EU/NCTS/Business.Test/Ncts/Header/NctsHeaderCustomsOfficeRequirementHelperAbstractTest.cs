using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestsSubclassesOf(typeof(NctsHeaderCustomsOfficeRequirementHelper))]
	public abstract class NctsHeaderCustomsOfficeRequirementHelperAbstractTest<T> : TestCaseWithFactory
		where T : NctsHeaderCustomsOfficeRequirementHelper
	{
		public void TestCacheKeyCombination() => AssertEquals(ExpectedCacheKey, officeHelper.CacheKeyCombination);

		public void TestOtherRequirements_Phase4()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			foreach (var role in Phase4CustomsOfficeRequirementWithRoles())
			{
				AssertCustomsOfficeRequirementWithRole(role);
			}
		}

		public void TestOtherRequirements_Phase5()
		{
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			foreach (var role in Phase5CustomsOfficeRequirementWithRoles())
			{
				AssertCustomsOfficeRequirementWithRole(role);
			}
		}

		void AssertCustomsOfficeRequirementEquals(ZString shortComment, CustomsOfficeRequirement first, CustomsOfficeRequirement second)
		{
			CombineAssertions(() =>
			{
				AssertEquals(shortComment + "Type", first.GetType(), second.GetType());
				AssertEquals(shortComment + "Office Role", first.OfficeRole, second.OfficeRole);
				AssertEquals(shortComment + "Office Roles For Lookup", first.OfficeRolesForLookup.First(), second.OfficeRolesForLookup.First());
				AssertEquals(shortComment + "Is Mandatory", first.IsMandatory, second.IsMandatory);
				AssertEquals(shortComment + "Is Local Country Only", first.IsLocalCountryOnly, second.IsLocalCountryOnly);
				AssertEquals(shortComment + "Friendly Name", first.FriendlyName, second.FriendlyName);
				AssertEquals(shortComment + "Is Foreign Country Only", first.IsForeignCountryOnly, second.IsForeignCountryOnly);
				AssertEquals(shortComment + "Is Recommended", first.IsRecommended, second.IsRecommended);
				AssertEquals(shortComment + "Max Offices Supported", first.MaxOfficeCountLimit, second.MaxOfficeCountLimit);
			});
		}

		protected virtual void AssertCustomsOfficeRequirementWithRole(string officeRole)
		{
			AssertCustomsOfficeRequirementEquals(officeRole, ExpectedOtherRequirements.Single(x => x.OfficeRole == officeRole), officeHelper.OtherRequirements.Single(x => x.OfficeRole == officeRole));
		}

		protected virtual string ExpectedCacheKey => "NctsHeaderCustomsOfficeRequirementHelper.OtherRequirements.D.." + header.BH_ApplicationCode;

		protected virtual IEnumerable<CustomsOfficeRequirement> ExpectedOtherRequirements
		{
			get
			{
				var list = new List<CustomsOfficeRequirement>();
				list.Add(new CustomsOfficeRequirement(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, true, false, "NCTS Office of departure"));
				list.Add(new CustomsOfficeRequirement(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, true, false, "NCTS Office of destination"));
				list.Add(new CustomsOfficeRequirement(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, false, false, "NCTS Office of transit")
				{
					MaxOfficeCountLimit = header.IsPhase5 ? 9 : null
				});
				if (header.IsPhase5)
				{
					list.Add(new CustomsOfficeRequirement(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit, false, false, "NCTS Office of exit for transit")
					{
						MaxOfficeCountLimit = 9
					});
				}
				return list;
			}
		}

		protected virtual IEnumerable<ZString> Phase4CustomsOfficeRequirementWithRoles() => new ZString[]
		{
			OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture,
			OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination,
			OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit
		};

		protected virtual IEnumerable<ZString> Phase5CustomsOfficeRequirementWithRoles() => new ZString[]
		{
			OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture,
			OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination,
			OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit,
			OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit
		};

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.CustomsOffices.RemoveAndDeleteAll();
			officeHelper = header.CustomsOfficeRequirementHelper;
		}
		protected CustomsOfficeRequirementHelper officeHelper;
		protected NctsHeader header;
	}
}
