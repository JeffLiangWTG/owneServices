using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineColsDirectionLookups : AutoQuarantineColsDirectionLookups
	{
		public QuarantineColsDirectionLookups(AutoQuarantineColsDirection parent) : base(parent)
		{
		}

		public new QuarantineColsDirection Parent => (QuarantineColsDirection)base.Parent;

		public IEnumerable<CusContainer> ContainersList
		{
			get
			{
				return Factory.GetCachedValue<IEnumerable<CusContainer>>("COLSDirectionLookup|ContainersList", () =>
				{
					return Parent.ColsHeader.JobDeclaration.CusContainers.OrderBy(e => e.CO_ContainerNumber).ToArray();
				},
				CacheStalenessPolicy.StaleOnFactorySave);
			}
		}

		public IEnumerable<CusEntryLine> EntryLinesList
		{
			get
			{
				return Factory.GetCachedValue<IEnumerable<CusEntryLine>>("COLSDirectionLookup|EntryLinesList", () =>
				{
					return Parent.ColsHeader.CusEntryHeader.AllEntryLines.Cast<CusEntryLine>().OrderBy(e => e.CL_LineNumber).ToArray();
				},
				CacheStalenessPolicy.StaleOnFactorySave);
			}
		}

		public CodeDescriptionPairList DirectionsList
		{
			get
			{
				return Factory.GetCachedValue("COLSDirectionLookup|DirectionsList", () =>
				{
					var result = new CodeDescriptionPairList();
					var cusCodes = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Australia,
						Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AUCOLSDirectionRequestType,
						ZDateTime.Today);
					foreach (var cusCode in cusCodes)
					{
						result.AddPair(cusCode.ZZD_Description);
					}
					result.Sort();
					return result;
				});
			}
		}

		public CodeDescriptionPairList AAIDList
		{
			get
			{
				var docAddress = Parent.ColsHeader.DeliveryOrUnpack;
				return Factory.GetCachedValue($"COLSDirectionLookup|AAIDList|{docAddress.OrganisationPK}", () =>
				{
					var result = new CodeDescriptionPairList();
					var aanCodes = docAddress.Organisation?.CustomsCodes?.GetOrgCusCodesForCodeAndCountry(AustraliaCodeTypes.ApprovedArrangementNumber, Core.Constants.CountryCodes.Australia) ?? Array.Empty<OrgCusCode>();
					foreach (var aanCode in aanCodes)
					{
						var configCode = aanCode.OK_CustomsRegNo;
						if (!result.ContainsCode(configCode))
						{
							var address = Parent.GetAddressWithMatchedAANCode(configCode)?.Address1 ?? ZString.Empty;
							result.AddPair(configCode, address);
						}
					}
					result.Sort();
					return result;
				});
			}
		}

		public BusinessObjectCollection TreatmentTypeList
		{
			get
			{
				return Factory.GetCachedValue("COLSDirectionLookup|TreatmentTypeList", () =>
				{
					var premisesIds = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.COLTT, ZDateTime.Today);
					premisesIds.Sort(AutoZZRefCusCodeListCombined.Schema.ZZD_Code, ListSortDirection.Ascending);
					return premisesIds;
				});
			}
		}
	}
}
