using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Client.EDI.Billing.Business.BillingConstants;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.Billing.Module
{
	public class EdiLicenceSettingFlattenedDataTransferProcessor : SimpleModuleDataTransferProcessor<EdiLicenceSetting, EdiLicenceSettingFlattened>
	{
		public EdiLicenceSettingFlattenedDataTransferProcessor(EdiLicenceSettingCollectionNonDependent collection, IImportCollectionInfo collectionInfo)
			: base(collection, collectionInfo)
		{
		}

		public override void Import()
		{
			if (flattenedCollection.Count == 0)
			{
				return;
			}

			var settings = flattenedCollection.Cast<EdiLicenceSettingFlattened>();

			OrgCodeMap = Factory.Load<EDIOrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, settings.Where(x => !x.OrgCode.IsEmpty).Select(x => x.OrgCode)))
				.ToDictionary(x => (string)x.OH_Code);

			EntCodeMap = Factory.Load<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, settings.Where(x => !x.EnterpriseCode.IsEmpty).Select(x => x.EnterpriseCode)))
				.ToDictionary(x => (string)x.LE_EnterpriseCode);

			EntIDMap = Factory.Load<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseID, settings.Where(x => !x.EnterpriseID.IsEmpty).Select(x => x.EnterpriseID)))
				.ToDictionary(x => (string)x.LE_EnterpriseID);

			base.Import();
		}

		public override void Rollback()
		{
			foreach (var setting in headerCollection.OfType<EdiLicenceSetting>().ToArray())
			{
				if (setting.IsInDatabase)
				{
					setting.CancelChanges();
				}
				else
				{
					setting.Delete();
				}
			}
		}

		protected override EdiLicenceSetting CreateHeader(IBusinessObjectCollection headerCollection, EdiLicenceSettingFlattened flat)
		{
			var licenceDatabase = GetLicenceDatabase(flat);

			if (licenceDatabase == null)
			{
				return null;
			}

			if (!ValidateSetting(licenceDatabase.LicenceSettings, flat))
			{
				return null;
			}

			var ediLicenceSetting = Import(licenceDatabase.LicenceSettings, flat);

			if (ediLicenceSetting != null)
			{
				headerCollection.Add(ediLicenceSetting);
			}

			return ediLicenceSetting;
		}

		LicenceDatabase GetLicenceDatabase(EdiLicenceSettingFlattened flat)
		{
			LicenceEnterprise entByCode = null;
			LicenceEnterprise entByID = null;
			EDIOrgHeader org = null;

			if (!flat.EnterpriseID.IsEmpty)
			{
				if (!EntIDMap.TryGetValue(flat.EnterpriseID, out entByID))
				{
					AddMessage(flat, Res.GetString("a826a5d7-b309-4f81-b210-1694f527aebc", "Ent. ID not found"));
					return null;
				}
			}

			if (!flat.EnterpriseCode.IsEmpty)
			{
				if (!EntCodeMap.TryGetValue(flat.EnterpriseCode, out entByCode))
				{
					AddMessage(flat, Res.GetString("955bebd9-24c6-424e-85f0-2ada359bbd32", "Ent. Code not found"));
					return null;
				}
			}

			if (!flat.OrgCode.IsEmpty)
			{
				if (!OrgCodeMap.TryGetValue(flat.OrgCode, out org))
				{
					AddMessage(flat, Res.GetString("5b9bca19-49d6-4050-8f9d-4e81c1f12232", "Org. Code not found"));
					return null;
				}
			}

			var enterprises = new[] { entByCode, entByID, org?.LicEnterprise }.Where(x => x != null).Distinct();
			if (!enterprises.Any())
			{
				AddMessage(flat, Res.GetString("8cb0e4c6-ab2b-48c3-92b0-969be444bdbd", "Ent. not found"));
				return null;
			}
			else if (enterprises.Count() != 1)
			{
				AddMessage(flat, Res.GetString("e2987c6a-14f6-4ad8-a81a-0dc8dd3d7f69", "Ent. ID, Ent. Code and Org. Code are not related to the same Ent."));
				return null;
			}

			var licenceDatabase = !flat.ServerCode.IsEmpty ? enterprises.Single().Databases.OfType<LicenceDatabase>().FirstOrDefault(x => x.LD_ServerCode == flat.ServerCode) : null;
			if (licenceDatabase == null)
			{
				AddMessage(flat, Res.GetString("4be8ca63-5609-4643-94dd-c539590b5e49", "Database not found"));
				return null;
			}

			return licenceDatabase;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		bool ValidateSetting(EdiLicenceSettingCollection ediLicenceSettings, EdiLicenceSettingFlattened flat)
		{
			if (flat.HasErrors)
			{
				var errMsg = string.Join("\r\n", flat.Notifications.Where(x => x.Type.IsFatal).Select(x => x.Message));
				AddMessage(flat, errMsg);
				return false;
			}

			if (!IsValidDateRange(flat.ValidFrom, flat.ValidTo))
			{
				AddMessage(flat, Res.GetString("bdd6709f-9c77-4b85-b605-cd3fd864468d", "Please enter valid From Date / To Date"));
				return false;
			}

			var isDiscountValid = false;
			var isPriceValid = false;
			var isBWLegacyValid = false;

			//Discount
			if (flat.DiscountName.IsEmpty && (flat.IsDiscountPercentProvided || flat.IsDiscountActiveProvided))
			{
				AddMessage(flat, Res.GetString("7e435cbd-995e-4d92-98f2-427700b11cc0", "Please enter Discount Name"));
				return false;
			}
			else if (!flat.IsDiscountPercentProvided && (!flat.DiscountName.IsEmpty || flat.IsDiscountActiveProvided))
			{
				AddMessage(flat, Res.GetString("56ff4ca5-ecef-4c5b-bd83-fbb9c6a5a584", "Please enter Discount Percent"));
				return false;
			}
			else if (!flat.DiscountName.IsEmpty && flat.IsDiscountPercentProvided)
			{
				if (ediLicenceSettings.Any(x => x.LS9_ValidFrom == flat.ValidFrom && x.LS9_Type == LicenceSetting.Discount && x.LS9_Name.EqualsIgnoringCase(flat.DiscountName)))
				{
					AddMessage(flat, Res.GetString("aa3c2c34-6e5e-4e75-8b70-06521dee4e68", "Another Discount setting with same values exists"));
					return false;
				}
				else
				{
					if (!flat.IsDiscountActiveProvided)
					{
						flat.DiscountActive = true;
					}
					isDiscountValid = true;
				}
			}

			//Price
			if (!flat.IsPriceProvided && !flat.PriceCode.IsEmpty && !flat.IsBWPurchasedLicencesProvided)
			{
				AddMessage(flat, Res.GetString("dd5f943d-ad1c-4de3-8fe3-52eb52cee120", "Please enter Price"));
				return false;
			}
			else if (flat.PriceCode.IsEmpty && flat.IsPriceProvided)
			{
				AddMessage(flat, Res.GetString("8598510b-b7b7-48f9-86b7-eb42ebc4cdea", "Please enter Price Code"));
				return false;
			}
			else if (flat.PriceCategory.IsEmpty && flat.IsPriceProvided)
			{
				AddMessage(flat, Res.GetString("47A79B60-D43C-4AC1-BFA5-9A2B59AD2B9E", "Please enter Price Category"));
				return false;
			}
			else if (flat.IsPriceProvided && !flat.PriceCode.IsEmpty && !flat.PriceCategory.IsEmpty)
			{
				var categoryUpper = flat.PriceCategory.ToUpperInvariant();
				if (!EDIDataRegistry.Instance.BillingUsageCategoryCodes.Value.ContainsCode(categoryUpper))
				{
					AddMessage(flat, Res.GetString("3CADF12C-D9EB-4803-B172-D79BC0D431D3", "Category {0} is not defined in registry {1}", categoryUpper, EDIDataRegistry.Instance.BillingUsageCategoryCodes.Caption));
					return false;
				}

				var codeKey = new UsageCodeKey(flat.PriceCategory.ToUpperInvariant(), flat.PriceCode.ToUpperInvariant());
				if (ediLicenceSettings.Any(x => x.LS9_ValidFrom == flat.ValidFrom
					&& x.LS9_Type == LicenceSetting.Price
					&& x.PriceKey == codeKey))
				{
					AddMessage(flat, Res.GetString("0d961b1a-19e0-47a7-852c-000178aacc37", "Another Price setting with same values exists"));
					return false;
				}
				else
				{
					isPriceValid = true;
				}
			}

			//BorderWisePurchasedLicences
			if (flat.IsBWPurchasedLicencesProvided)
			{
				if (flat.PriceCode.IsEmpty)
				{
					AddMessage(flat, Res.GetString("8598510b-b7b7-48f9-86b7-eb42ebc4cdea", "Please enter Price Code"));
					return false;
				}

				if (ediLicenceSettings.Any(x => x.LS9_ValidFrom == flat.ValidFrom
					&& x.LS9_Type == LicenceSetting.BorderWisePurchasedLicences
					&& x.LS9_Name.EqualsIgnoringCase(flat.PriceCode)
					))
				{
					AddMessage(flat, Res.GetString("363eec18-231f-4a30-839a-bcf6ce6dacf2", "Another BorderWise Legacy Licenses setting with same values exists"));
					return false;
				}
				else
				{
					isBWLegacyValid = true;
				}
			}

			var validSettingCount = new[] { isDiscountValid, isPriceValid, isBWLegacyValid }.Where(x => x).Count();

			if (validSettingCount == 0)
			{
				AddMessage(flat, Res.GetString("f66411f3-6055-4ac6-8475-74d4bbc3efb8", "No setting defined"));
				return false;
			}
			else if (validSettingCount != 1)
			{
				AddMessage(flat, Res.GetString("d6da804d-739c-421a-9daa-d6f12735a37b", "Can't define multiple settings on the same line"));
				return false;
			}

			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		EdiLicenceSetting Import(EdiLicenceSettingCollection ediLicenceSettings, EdiLicenceSettingFlattened flat)
		{
			EdiLicenceSetting mergeableSetting = null;
			EdiLicenceSetting newSetting = null;

			if (!flat.DiscountName.IsEmpty)
			{
				mergeableSetting = ediLicenceSettings.FirstOrDefault(x => x.LS9_Type == LicenceSetting.Discount && x.LS9_Name.EqualsIgnoringCase(flat.DiscountName)
					&& x.LS9_Percent == flat.DiscountPercent && x.LS9_IsActive == flat.DiscountActive
					&& ShouldCombineDates(x, flat));

				if (mergeableSetting == null)
				{
					newSetting = ediLicenceSettings.Factory.New<DiscountLicenceSetting>();
					newSetting.LS9_Name = flat.DiscountName.ToUpperInvariant();
					newSetting.LS9_Percent = flat.DiscountPercent;
					newSetting.LS9_IsActive = flat.DiscountActive;
					newSetting.LS9_ValidFrom = flat.ValidFrom;
					newSetting.LS9_ValidTo = flat.ValidTo;
					newSetting.LS9_Comment = flat.Comment;
					ediLicenceSettings.Add(newSetting);
				}
			}
			else if (!flat.PriceCategory.IsEmpty && !flat.PriceCode.IsEmpty && !flat.IsBWPurchasedLicencesProvided)
			{
				var priceKey = new UsageCodeKey(flat.PriceCategory.ToUpperInvariant(), flat.PriceCode.ToUpperInvariant());
				mergeableSetting = ediLicenceSettings.FirstOrDefault(x => x.LS9_Type == LicenceSetting.Price && x.PriceKey.EqualsIgnoringCase(priceKey)
					&& x.LS9_Price == flat.Price
					&& ShouldCombineDates(x, flat));

				if (mergeableSetting == null)
				{
					newSetting = ediLicenceSettings.Factory.New<PriceLicenceSetting>();
					newSetting.PriceKey = priceKey;
					newSetting.LS9_Price = flat.Price;
					newSetting.LS9_ApplyDiscounts = flat.ApplyDiscounts;
					newSetting.LS9_Units = flat.LicenceUnits;
					newSetting.LS9_ApplyDiscounts = flat.ApplyDiscounts;
					newSetting.LS9_ValidFrom = flat.ValidFrom;
					newSetting.LS9_ValidTo = flat.ValidTo;
					newSetting.LS9_Comment = flat.Comment;
					ediLicenceSettings.Add(newSetting);
				}
			}
			else if (flat.IsBWPurchasedLicencesProvided)
			{
				var priceCodeUpper = flat.PriceCode.ToUpperInvariant();
				mergeableSetting = ediLicenceSettings.FirstOrDefault(x => x.LS9_Type == LicenceSetting.BorderWisePurchasedLicences
					&& x.LS9_Price == flat.BWPurchasedLicences
					&& x.LS9_Name == priceCodeUpper
					&& ShouldCombineDates(x, flat));

				if (mergeableSetting == null)
				{
					var bwSetting = ediLicenceSettings.Factory.New<BorderWisePurchasedLicenceSetting>();
					newSetting = bwSetting;
					bwSetting.LicenceCount = (int)flat.BWPurchasedLicences;
					bwSetting.PriceCode = priceCodeUpper;
					bwSetting.LS9_ValidFrom = flat.ValidFrom;
					bwSetting.LS9_ValidTo = flat.ValidTo;
					bwSetting.LS9_Comment = flat.Comment;
					ediLicenceSettings.Add(bwSetting);
				}
			}

			if (mergeableSetting != null)
			{
				var fromDate = mergeableSetting.LS9_ValidFrom;
				var toDate = mergeableSetting.LS9_ValidTo;
				var comment = mergeableSetting.LS9_Comment;

				mergeableSetting.LS9_ValidFrom = new[] { flat.ValidFrom, mergeableSetting.LS9_ValidFrom }.Min();
				var validToDates = new[] { flat.ValidTo, mergeableSetting.LS9_ValidTo };
				mergeableSetting.LS9_ValidTo = validToDates.Any(x => x.IsEmpty) ? ZDateTime.Empty : validToDates.Max();

				if (!flat.Comment.IsEmpty)
				{
					mergeableSetting.LS9_Comment = flat.Comment;
				}

				mergeableSetting.RunPreSaveValidation();
				if (mergeableSetting.HasErrors)
				{
					var errMsg = string.Join("\r\n", mergeableSetting.Notifications.Where(x => x.Type.IsFatal).Select(x => x.Message));
					AddMessage(flat, errMsg);
					mergeableSetting.LS9_ValidFrom = fromDate;
					mergeableSetting.LS9_ValidTo = toDate;
					mergeableSetting.LS9_Comment = comment;
					mergeableSetting = null;
				}
			}
			else if (newSetting != null)
			{
				newSetting.RunPreSaveValidation();
				if (newSetting.HasErrors)
				{
					var errMsg = string.Join("\r\n", newSetting.Notifications.Where(x => x.Type.IsFatal).Select(x => x.Message));
					AddMessage(flat, errMsg);
					newSetting.Delete();
					newSetting = null;
				}
			}

			return mergeableSetting ?? newSetting;
		}

		static bool ShouldCombineDates(EdiLicenceSetting setting, EdiLicenceSettingFlattened flat)
		{
			return ShouldCombineDates(setting.LS9_ValidFrom, setting.LS9_ValidTo, flat.ValidFrom, flat.ValidTo)
				|| ShouldCombineDates(flat.ValidFrom, flat.ValidTo, setting.LS9_ValidFrom, setting.LS9_ValidTo);
		}

		static bool ShouldCombineDates(ZDateTime fromDate1, ZDateTime toDate1, ZDateTime fromDate2, ZDateTime toDate2)
		{
			/*
			Combine if one setting:
			- has a Valid To within one day of the other Valid From.
			- has a Valid To that is empty, and the other has Valid To empty or after the other Valid From
			*/
			return IsValidDateRange(fromDate1, toDate1) && IsValidDateRange(fromDate2, toDate2)
				&& ((!toDate1.IsEmpty && !fromDate2.IsEmpty && (fromDate2.Date - toDate1.Date).Days == 1)
					|| (toDate1.IsEmpty && (toDate2.IsEmpty || (!fromDate1.IsEmpty && toDate2.Date > fromDate1.Date))));
		}

		static bool IsValidDateRange(ZDateTime fromDate, ZDateTime toDate)
		{
			return !fromDate.IsEmpty && (toDate.IsEmpty || toDate.Date >= fromDate.Date);
		}

		void AddMessage(EdiLicenceSettingFlattened flat, string reason)
		{
			Log += Res.GetString("3da0d2bf-0510-44fa-bff1-700e8612d3f6", "Record [Ent. ID: {0}, Ent. Code: {1}, Org. Code: {2}, Server Code: {3}] excluded: {4}"
				, flat.EnterpriseID, flat.EnterpriseCode, flat.OrgCode, flat.ServerCode, reason) + System.Environment.NewLine;
		}

		Dictionary<string, EDIOrgHeader> OrgCodeMap;
		Dictionary<string, LicenceEnterprise> EntCodeMap;
		Dictionary<string, LicenceEnterprise> EntIDMap;
	}
}
