using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common.Stat;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Common.PostUpdateProcesses.ScreeningStatus
{
	class OrgScreeningStatusPostUpdateProcess : ScreeningStatusPostUpdate<OrgHeader>
	{
		public OrgScreeningStatusPostUpdateProcess(IEntityContext context, IEntity rootEntity) : base(context, rootEntity)
		{
		}

		protected override bool ShouldRun
		{
			get
			{
				var statistics = Context.Statistics;
				return RootEntity != null &&
					   RootEntity.TableName == OrgHeaderSchema.Constants.TableName &&
					   RootEntity.Action != EntityAction.DELETE &&
					   (statistics.GetEntityStatistics(OrgHeaderSchema.Constants.TableName).Entities.Count > 0 ||
						statistics.GetEntityStatistics(OrgAddressSchema.Constants.TableName).Entities.Count > 0 ||
						statistics.GetEntityStatistics(OrgBrandOrRelatedNameSchema.Constants.TableName).Entities.Count > 0);
			}
		}

		protected override void SetScreeningStatus(ZGuid pk, string screeningStatus)
		{
			var orgHeaderRow = Context.RowFactory.GetRow(OrgHeaderSchema.Constants.TableName, pk);
			if (orgHeaderRow != null)
			{
				orgHeaderRow[OrgHeaderSchema.Constants.OH_ScreeningStatus] = screeningStatus;
			}
		}

		protected override bool HasChanges(OrgHeader header)
		{
			return header != null &&
				   !header.IsDeleted &&
				   (PropertyValueComparisonUtils.StringValueHasChanges(RootEntity, "FullName", header.OH_FullName.ToString()) ||
					PropertyValueComparisonUtils.BoolValueHasChanges(RootEntity, "IsActive", header.OH_IsActive) ||
					ClosestPortHasChanges(header) ||
					AddressInfoHasChanges(header) ||
					BrandOrRelatedNameChanges(header) ||
					CusCodeInfoHasChanges(header));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		bool ClosestPortInfoHasChanges(OrgHeader org)
		{
			var result = false;
			var closestPortEntity = RootEntity.Parents.FirstOrDefault(x => x.EntityName == "ClosestPort");
			if (closestPortEntity != null)
			{
				var code = closestPortEntity.GetPropertyOrBlankString("Code");
				result = !string.IsNullOrWhiteSpace(code) && org.OH_RL_NKClosestPort.ToString() != code;
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		bool RelatedPortInfoHasChanges(OrgHeader org)
		{
			var addressEntities = RootEntity.Children.Where(x => x.EntityName == "OrgAddress");
			foreach (var addressEntity in addressEntities)
			{
				var capabilityEntities = addressEntity.Children.Where(x => x.EntityName == "OrgAddressCapability" && x.GetPropertyOrBlankString("AddressType") == OrgAddressType.Office.Code && PropertyValueComparisonUtils.IsTrue(x, "IsMainAddress"));
				if (capabilityEntities.Any())
				{
					var relatedPortCodeEntity = addressEntity.Parents?.FirstOrDefault(x => x.EntityName == "RelatedPortCode");
					return PropertyValueComparisonUtils.StringValueHasChanges(relatedPortCodeEntity, "Code", org.MainAddress.OA_RL_NKRelatedPortCode);
				}
			}

			return false;
		}

		bool ClosestPortHasChanges(OrgHeader org)
		{
			return org.OH_IsGlobalAccount && org.MainAddress != null ? RelatedPortInfoHasChanges(org) : ClosestPortInfoHasChanges(org);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		bool AddressInfoHasChanges(OrgHeader org)
		{
			var result = false;
			if (HasInsertOrDeleteInformation("OrgAddress"))
			{
				result = true;
			}
			else
			{
				var addressEntities = RootEntity.Children.Where(x => x.EntityName == "OrgAddress");
				var addressList = org.Addresses.Cast<OrgAddress>().ToList();
				foreach (var addressEntity in addressEntities)
				{
					OrgAddress address = null;
					if (addressEntity.InternalPK != Guid.Empty)
					{
						address = addressList.SingleOrDefault(x => x.PK.ToGuid() == addressEntity.InternalPK);

						if (address != null)
						{
							var countryEntity = addressEntity.Parents?.FirstOrDefault(x => x.EntityName == "CountryCode");
							if (PropertyValueComparisonUtils.StringValueHasChanges(addressEntity, "Address1", address.Address1) ||
								PropertyValueComparisonUtils.StringValueHasChanges(addressEntity, "Address2", address.Address2) ||
								PropertyValueComparisonUtils.StringValueHasChanges(addressEntity, "City", address.City) ||
								PropertyValueComparisonUtils.StringValueHasChanges(addressEntity, "State", address.StateCode) ||
								PropertyValueComparisonUtils.StringValueHasChanges(addressEntity, "PostCode", address.Postcode) ||
								PropertyValueComparisonUtils.StringValueHasChanges(countryEntity, "Code", address.OA_RN_NKCountryCode) ||
								PropertyValueComparisonUtils.StringValueHasChanges(addressEntity, "CompanyNameOverride", address.OA_CompanyNameOverride))
							{
								result = true;
								break;
							}
						}
					}
				}
			}

			return result;
		}

		bool BrandOrRelatedNameChanges(OrgHeader org)
		{
			var result = false;
			if (HasInsertOrDeleteInformation("OrgBrandOrRelatedName"))
			{
				result = true;
			}
			else
			{
				var relatedNameEntities = RootEntity.Children.Where(x => x.EntityName == "OrgBrandOrRelatedName");
				foreach (var relatedNameEntity in relatedNameEntities)
				{
					var name = relatedNameEntity.GetPropertyOrBlankString("RelatedName");
					if (!string.IsNullOrWhiteSpace(name))
					{
						if (relatedNameEntity.InternalPK != Guid.Empty)
						{
							var brand = org.BrandsOrRelatedNames.Cast<OrgBrandOrRelatedName>().SingleOrDefault(x => x.PK.ToGuid() == relatedNameEntity.InternalPK);
							if (brand != null && PropertyValueComparisonUtils.StringValueHasChanges(relatedNameEntity, "RelatedName", brand.P1_RelatedName))
							{
								result = true;
								break;
							}
						}
					}
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		bool CusCodeInfoHasChanges(OrgHeader org)
		{
			var result = false;
			if (HasInsertOrDeleteInformation("OrgCusCode"))
			{
				result = true;
			}
			else
			{
				var cusCodeEntities = RootEntity.Children.Where(x => x.EntityName == "OrgCusCode");
				var cusCodeList = org.CustomsCodes.Cast<OrgCusCode>().ToList();
				foreach (var cusCodeEntity in cusCodeEntities)
				{
					OrgCusCode cusCode = null;
					if (cusCodeEntity.InternalPK != Guid.Empty)
					{
						cusCode = cusCodeList.SingleOrDefault(x => x.PK.ToGuid() == cusCodeEntity.InternalPK);

						if (cusCode != null)
						{
							var countryEntity = cusCodeEntity.Parents?.FirstOrDefault(x => x.EntityName == "CodeCountry");
							if (PropertyValueComparisonUtils.StringValueHasChanges(cusCodeEntity, "CodeType", cusCode.OK_CodeType) ||
								PropertyValueComparisonUtils.StringValueHasChanges(cusCodeEntity, "CustomsRegNo", cusCode.OK_CustomsRegNo) ||
								PropertyValueComparisonUtils.StringValueHasChanges(countryEntity, "Code", cusCode.OK_RN_NKCodeCountry))
							{
								result = true;
								break;
							}
						}
					}
				}
			}

			return result;
		}

		bool HasInsertOrDeleteInformation(string entityName)
		{
			return Context.Statistics.EntityAffected.Any(dbEntity => dbEntity.Name == entityName && (dbEntity.Action == DBEntity.DbAction.Insert ||
																									 dbEntity.Action == DBEntity.DbAction.Delete));
		}
	}
}
