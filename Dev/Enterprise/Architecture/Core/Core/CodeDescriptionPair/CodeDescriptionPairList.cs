using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common.Collections;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core
{
	public interface IHaveCodeDescriptionPairList
	{
		ReadOnlyCodeDescriptionPairList CodeDescriptionPairList
		{
			get;
		}
	}

	[ValueDisplayMembers("Code", "Description")]
	public class CodeDescriptionPairList : ReadOnlyCodeDescriptionPairList, IList, ICachedValueManager, IAdditionalInformationWithSetter
	{
		#region Construction

		public CodeDescriptionPairList()
			: this(OLookUpEditType.CustomType)
		{
		}

		public CodeDescriptionPairList(ReadOnlyCodeDescriptionPairList listToClone)
			: base(listToClone)
		{
		}

		public CodeDescriptionPairList(byte[] xmlByteArray)
			: base(xmlByteArray)
		{
		}

		#endregion

		#region Elements

		public new ICodeDescription this[int index]
		{
			get { return base[index]; }
			set { Elements[index] = value; }
		}

		public ICodeDescription this[MultilingualString code] => this[code.GetUnresolvedString()];

		public ICodeDescription this[string code]
		{
			get { return this[code, StringComparison.Ordinal]; }
		}

		public ICodeDescription this[string code, StringComparison comparison]
		{
			get
			{
				foreach (ICodeDescription element in Elements)
				{
					if (element.Code.Equals(code, comparison))
					{
						return element;
					}
				}
				return null;
			}
		}

		protected override bool ContainsCodeCore(object code)
		{
			var trimmedCode = code.ToString().TrimEnd();
			foreach (ICodeDescription element in this)
			{
				if (string.Equals(element.Code.Trim(), trimmedCode, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}

				if (element is CodeDescriptionPair pair && pair.MultilingualCode.Equals(code))
				{
					return true;
				}
			}
			return false;
		}

		public int Add(ICodeDescription item)
		{
			return AddInternal(item);
		}

		public int AddOverwriteIfExists(ICodeDescription item)
		{
			if (ContainsCode(item.Code))
			{
				RemoveCode(item.Code);
			}
			return AddInternal(item);
		}

		int AddInternal(ICodeDescription element)
		{
			EnsureCanAdd();
			return AddCore(element);
		}

		protected virtual int AddCore(ICodeDescription element)
		{
			Elements.Add(element);
			return (Count - 1);
		}

		public void Clear()
		{
			EnsureCanClear();
			Elements.Clear();
		}

		public void AddRange(ICollection collection)
		{
			foreach (ICodeDescription element in collection)
			{
				Add(element);
			}
		}

		public void AddRangeOverwriteIfExists(ICollection collection)
		{
			foreach (ICodeDescription element in collection)
			{
				AddOverwriteIfExists(element);
			}
		}

		public int IndexOf(ICodeDescription item)
		{
			return Elements.IndexOf(item);
		}

		public void Remove(ICodeDescription item)
		{
			EnsureCanRemove();
			Elements.Remove(item);
		}

		public void InsertInSortOrder(ICodeDescription item)
		{
			Elements.InsertInSortOrder(item, new CodeComparer());
		}

		public void InsertInSortOrderByDescription(ICodeDescription item)
		{
			Elements.InsertInSortOrder(item, new DescriptionComparer());
		}

		/// <summary>
		/// Sorts by Code.
		/// </summary>
		public void Sort()
		{
			Elements.Sort(new CodeComparer());
		}

		/// <summary>
		/// Sorts by Description.
		/// </summary>
		public void SortByDescription()
		{
			Elements.Sort(new DescriptionComparer());
		}

		/// <summary>
		/// Sorts by Description. Also combines description if they share the same code.
		/// </summary>
		public void SortByDescriptionAndCombineIfSameCode()
		{
			var descriptionComparer = new DescriptionComparer();
			var duplicateCodes = Elements.GroupBy(x => new { x.Code }).Select(x => new { x.Key.Code, Duplicates = x.Count() }).Where(x => x.Duplicates > 1).Select(x => x.Code);

			foreach (var duplicateCode in duplicateCodes)
			{
				var duplicatePairs = Elements.Cast<ICodeDescription>().Where(x => x.Code == duplicateCode).OrderBy(x => x, descriptionComparer).ToList();
				var combinedDescription = duplicatePairs[0].Description;
				for (int i = 1; i < duplicatePairs.Count; i++)
				{
					if (descriptionComparer.Compare(duplicatePairs[i - 1], duplicatePairs[i]) == 0)
					{
						break;
					}

					var potentialCombinedDescription = string.Format("{0}, {1}", combinedDescription, duplicatePairs[i].Description);
					if (potentialCombinedDescription.Length > 50)
					{
						combinedDescription += ", ...";
						break;
					}
					else
					{
						combinedDescription = potentialCombinedDescription;
					}
				}

				Elements.RemoveAll((ICodeDescription pair) => { return pair.Code == duplicateCode; });
				AddPair(duplicateCode, combinedDescription);
			}
			Elements.Sort(descriptionComparer);
		}

		public ICodeDescription[] ToArray()
		{
			return Elements.ToArray();
		}

		#endregion

		#region Defined Lookups

		public CodeDescriptionPairList(OLookUpEditType lookupEditType)
		{
			this.lookupEditType = lookupEditType;

			switch (LookupEditType)
			{
				#region Invoice Detail Display Options

				case OLookUpEditType.InvoiceDetailDisplayOption:
					AddRange(new InvoiceDescriptionOptionsList());
					break;

				#endregion

				#region Account Order

				case OLookUpEditType.AccountOrderType:
					AddPair(nameof(AccountOrderType.BalanceSheet));
					AddPair(nameof(AccountOrderType.ProfitAndLoss));
					break;

				#endregion

				#region AWB Accounting Codes

				case OLookUpEditType.AWBAccountingCodes:
					AddPair(Constants.AWB.AccountingCodes.MCO, SourceGenerated.ResString.GetMultilingualString("20aadbf7-9aaa-48b2-aad0-1ad1aa188467", "Miscellaneous Charges Order"));
					AddPair(Constants.AWB.AccountingCodes.GEN, SourceGenerated.ResString.GetMultilingualString("0c398c52-b6a1-4608-b470-5beb27c42010", "General"));
					AddPair(Constants.AWB.AccountingCodes.STL, "");
					AddPair(Constants.AWB.AccountingCodes.RET, SourceGenerated.ResString.GetMultilingualString("8a6e92ce-e77b-4fda-baa3-f8a6b7af4829", "Returned"));
					AddPair(Constants.AWB.AccountingCodes.SRN, SourceGenerated.ResString.GetMultilingualString("9e1d8157-0685-481a-a726-698d3c89a5ce", "Shippers Reference Number"));
					AddPair(Constants.AWB.AccountingCodes.GBL, SourceGenerated.ResString.GetMultilingualString("592d722d-5c69-4b63-8404-6082d210bd50", "Government Bill of Lading"));

					AddPair(Constants.AWB.AccountingCodes.SPE, SourceGenerated.ResString.GetMultilingualString("5846946B-DEF7-4726-91C4-963133B2BF40", "Shipper's Email"));
					AddPair(Constants.AWB.AccountingCodes.CNE, SourceGenerated.ResString.GetMultilingualString("04327D40-111E-432E-A79C-3563C4AED18A", "Consignee's Email"));
					AddPair(Constants.AWB.AccountingCodes.AHD, SourceGenerated.ResString.GetMultilingualString("9CB9099F-EA0F-464A-99E0-241DB906C0AD", "Customer Account Holder"));
					AddPair(Constants.AWB.AccountingCodes.ANM, SourceGenerated.ResString.GetMultilingualString("A0749DF6-DF6D-45F6-99CB-2D6D7038C99C", "Customer Account Name"));
					AddPair(Constants.AWB.AccountingCodes.ANB, SourceGenerated.ResString.GetMultilingualString("060CD057-C0FE-46A7-AFE4-AB9730777F67", "Customer Account Number"));
					AddPair(Constants.AWB.AccountingCodes.AIS, SourceGenerated.ResString.GetMultilingualString("EB9FE5AF-5F84-4985-99A3-B8424993C275", "Customer Account Issuer"));
					AddPair(Constants.AWB.AccountingCodes.ASF, SourceGenerated.ResString.GetMultilingualString("F9465EC6-10C2-48B2-980E-787065C04BF9", "Customer Account Shipping Frequency"));
					AddPair(Constants.AWB.AccountingCodes.VKC, SourceGenerated.ResString.GetMultilingualString("A002EEB0-4CB6-409D-8531-737C01E8F855", "Verified Known Consignor"));
					AddPair(Constants.AWB.AccountingCodes.AED, SourceGenerated.ResString.GetMultilingualString("61C2A6FC-AC95-45CD-B529-11F37D2E82AA", "Customer Account Establishment Date"));
					AddPair(Constants.AWB.AccountingCodes.ABT, SourceGenerated.ResString.GetMultilingualString("3ACF0F88-F150-4F26-8E54-2B4DDC950EED", "Customer Account Billing Type"));
					AddPair(Constants.AWB.AccountingCodes.IPA, SourceGenerated.ResString.GetMultilingualString("017EA340-AB66-4203-B5E4-836EC9EA98AC", "IP Address for Customer Account Creation"));
					AddPair(Constants.AWB.AccountingCodes.IPW, SourceGenerated.ResString.GetMultilingualString("516E9EF5-A331-4E96-9948-1E54FFFADE00", "IP Address for AWB Creation"));
					AddPair(Constants.AWB.AccountingCodes.BDT, SourceGenerated.ResString.GetMultilingualString("19B4BD54-4747-497E-A773-99F1EE158491", "Biographic Data Type"));
					AddPair(Constants.AWB.AccountingCodes.BDC, SourceGenerated.ResString.GetMultilingualString("C0FCDE56-F379-4D27-B89F-D43A818C2ED4", "Biographic Data Country"));
					AddPair(Constants.AWB.AccountingCodes.BDN, SourceGenerated.ResString.GetMultilingualString("9B9993C1-71D5-4652-BA28-AF318ED22F83", "Biographic Data Number"));
					break;

				#endregion

				#region AWB Contact Codes

				case OLookUpEditType.AWBContactCodes:
					AddPair(Constants.AWB.ContactCodes.FAX, Constants.ContactNotifyModeDescriptions.Fax);
					AddPair(Constants.AWB.ContactCodes.TELEPHONE, SourceGenerated.ResString.GetMultilingualString("1de258fe-ba74-40ae-9d07-94635a9ae9bb", "Telephone"));
					AddPair(Constants.AWB.ContactCodes.TELEX, SourceGenerated.ResString.GetMultilingualString("b4fb223d-0a7b-4aa4-ac22-893d39f75d69", "Telex"));
					break;

				#endregion

				#region AWB Charge Codes

				case OLookUpEditType.AWBChargeCodes:
					AddAWBChargeCodes();
					break;

				#endregion

				#region AWB Dimensions

				case OLookUpEditType.AWBDimensions:
					AddPair(Constants.AWB.Dimensions.DEF, SourceGenerated.ResString.GetMultilingualString("e0da3aba-6697-415c-9db3-966033a845aa", "Default (Dims, fallback to Vol)"));
					AddPair(Constants.AWB.Dimensions.M3, SourceGenerated.ResString.GetMultilingualString("7f62fdac-d5fd-44e5-8d42-153fb47ff478", "Volume Only (consolidated as one movable part)"));
					AddPair(Constants.AWB.Dimensions.ALL, SourceGenerated.ResString.GetMultilingualString("12639263-d187-49d4-8791-ddb37ba95df2", "Both Dimensions and Volume if available"));
					AddPair(Constants.AWB.Dimensions.PKS, SourceGenerated.ResString.GetMultilingualString("98dfe95a-e3f7-458b-b2ae-7beb83e6c0cb", "Dimensions of outer Packs only"));
					AddPair(Constants.AWB.Dimensions.NDA, SourceGenerated.ResString.GetMultilingualString("20c63388-31ea-43ed-986e-51847083fa7c", "No Dimensions Available"));
					break;

				#endregion

				#region AWB Entitlement Codes

				case OLookUpEditType.AWBEntitlementCodes:
					AddPair(Constants.AWB.EntitlementCode.Agent, SourceGenerated.ResString.GetMultilingualString("914e82ff-6c96-4989-af97-d4b6f9767406", "Agent"));
					AddPair(Constants.AWB.EntitlementCode.Carrier, SourceGenerated.ResString.GetMultilingualString("54344b72-50e8-49c3-a0c2-0ab2d610e8b5", "Carrier"));
					break;

				#endregion

				#region AWB Prepay / Collect

				case OLookUpEditType.AWBPrepayCollect:
					AddPair(Constants.AWB.PPDCollect.Prepaid, SourceGenerated.ResString.GetMultilingualString("7bc12dfa-1652-4a29-a942-17513e02c691", "Prepaid"));
					AddPair(Constants.AWB.PPDCollect.Collect, SourceGenerated.ResString.GetMultilingualString("a3a4740b-526d-4ff6-ad00-73d4f99ecbe6", "Collect"));
					break;

				#endregion

				#region AWB Rate Units

				case OLookUpEditType.AWBRateUQ:
					AddPair(Constants.AWB.RateLineUQ.Kilos, SourceGenerated.ResString.GetMultilingualString("75393129-118f-4719-a7fb-c402eb191fa8", "Kilograms"));
					AddPair(Constants.AWB.RateLineUQ.Pounds, SourceGenerated.ResString.GetMultilingualString("ae05a158-ef38-4bcf-abb1-ea639409be78", "Pounds"));
					break;

				#endregion

				#region Chargeable Weight Rounding (AWB + shipments)

				case OLookUpEditType.ChargeableWeightRounding:
					AddPair(nameof(ChargeableWeightRoundingType.Down), SourceGenerated.ResString.GetMultilingualString("0e7a866f-3cb0-48f2-a3f5-29e04b07e4fa", "Round Down"));
					AddPair(nameof(ChargeableWeightRoundingType.Up), SourceGenerated.ResString.GetMultilingualString("685384f4-eefa-4ca7-ad19-a4c4ed6bbe60", "Round Up"));
					AddPair(nameof(ChargeableWeightRoundingType.None), SourceGenerated.ResString.GetMultilingualString("59fa41c5-a6a2-4ec2-9983-3671f72dee18", "No Rounding"));
					break;

				#endregion

				#region AWB MAWBBillingSellRateModes

				case OLookUpEditType.MAWBBillingSellRateModes:
					AddPair(Constants.AWB.MAWBBillingSellRateModes.None, SourceGenerated.ResString.GetMultilingualString("d3caeb27-c1f0-4d45-8d52-be7a6ac84126", "No Sell Rate to populate"));
					AddPair(Constants.AWB.MAWBBillingSellRateModes.CollectOnly, SourceGenerated.ResString.GetMultilingualString("930048fa-7813-4df5-a880-b5d48c0be55f", "Sell Rates to populate for Collect direct consols"));
					AddPair(Constants.AWB.MAWBBillingSellRateModes.PrepaidOnly, SourceGenerated.ResString.GetMultilingualString("3775866e-2bee-4bb0-99ee-bc8dae1b8f55", "Sell Rates to populate for Prepaid direct consols"));
					AddPair(Constants.AWB.MAWBBillingSellRateModes.Both, SourceGenerated.ResString.GetMultilingualString("f7c5009d-16b9-4aa0-9069-3d1a92957572", "Sell Rates to populate for both prepaid and collect direct consols"));
					break;

				#endregion

				#region AWB Rate Class

				case OLookUpEditType.AWBRateClass:
					AddPair(Constants.AWB.RateClass.MinimumCharge, SourceGenerated.ResString.GetMultilingualString("6b78cbf9-a423-4e72-b71e-7447286f8f48", "Minimum Charge"));
					AddPair(Constants.AWB.RateClass.NormalCharge, SourceGenerated.ResString.GetMultilingualString("55f9746f-885f-40e7-8bc7-b2739b761d8e", "Normal Charge"));
					AddPair(Constants.AWB.RateClass.QuantityRate, SourceGenerated.ResString.GetMultilingualString("438c24df-30d3-4c12-966c-bb25de9d9e03", "Quantity Rate"));
					AddPair(Constants.AWB.RateClass.BasicCharge, SourceGenerated.ResString.GetMultilingualString("137bc18b-f6d9-43e5-aae2-3e7ddb3deafc", "Basic Charge"));
					AddPair(Constants.AWB.RateClass.RatePerKilogram, SourceGenerated.ResString.GetMultilingualString("52940f68-507d-46cd-a3cc-eaf871995ba5", "Rate Per Kilogram"));
					AddPair(Constants.AWB.RateClass.InternationalPriorityService, SourceGenerated.ResString.GetMultilingualString("d420c5f0-bb16-4f5e-8366-d83f0905ff3c", "International priority service rate"));
					AddPair(Constants.AWB.RateClass.SpecificCommodityRate, SourceGenerated.ResString.GetMultilingualString("336a7ba9-5655-4c66-a94b-444929a8cc5d", "Specific Commodity Rate"));
					AddPair(Constants.AWB.RateClass.ClassRateReduction, SourceGenerated.ResString.GetMultilingualString("6c937f6d-422f-4850-8885-747f044c7e59", "Class Rate Reduction"));
					AddPair(Constants.AWB.RateClass.ClassRateSurcharge, SourceGenerated.ResString.GetMultilingualString("0eb3ee52-b9d0-45e4-bf70-cfa31e20b8a2", "Class Rate Surcharge"));
					AddPair(Constants.AWB.RateClass.UnitLoadDeviceBasicCharge, SourceGenerated.ResString.GetMultilingualString("e5def14e-fe50-4c33-9470-72399742dde9", "Unit Load Device Basic Charge"));
					AddPair(Constants.AWB.RateClass.UnitLoadDeviceAdditionalCharge, SourceGenerated.ResString.GetMultilingualString("0a5cb9aa-a728-40fe-ad17-cf64ef524e15", "Unit Load Device Additional Charge"));
					AddPair(Constants.AWB.RateClass.UnitLoadDeviceAdditionalInformation, SourceGenerated.ResString.GetMultilingualString("030de021-f985-4ba4-a668-1f9cb350a871", "Unit Load Device Additional Information"));
					AddPair(Constants.AWB.RateClass.UnitLoadDeviceDiscount, SourceGenerated.ResString.GetMultilingualString("053e9387-f2e8-47ef-bf44-df4f8ec28cc5", "Unit Load Device Discount"));
					AddPair(Constants.AWB.RateClass.WeightIncrease, SourceGenerated.ResString.GetMultilingualString("d594eaca-a67c-4c26-b115-b2dece303412", "Weight Increase"));
					break;

				#endregion

				#region AWB Nature and Quantity of Goods Type

				case OLookUpEditType.AWBNatureAndQtyOfGoodsType:
					AddPair(Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, SourceGenerated.ResString.GetMultilingualString("297b45bf-af4f-40be-8157-36ac5d905c33", "Goods Description"));
					AddPair(Constants.AWB.NatureAndQtyOfGoodsTypes.Consolidation, SourceGenerated.ResString.GetMultilingualString("86a74fc5-87df-4073-aa0b-a5d1567f57f6", "Consolidation"));
					AddPair(Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions, SourceGenerated.ResString.GetMultilingualString("c0f05d0d-4df7-47c2-98ee-27278d29f048", "Dimensions"));
					AddPair(Constants.AWB.NatureAndQtyOfGoodsTypes.Volume, SourceGenerated.ResString.GetMultilingualString("44f8d6e9-0622-428e-824b-5828a37e2e0b", "Volume"));
					AddPair(Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber, SourceGenerated.ResString.GetMultilingualString("fa984cb2-f133-430c-84e8-c3b9650350b5", "ULD Number"));
					AddPair(Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount, SourceGenerated.ResString.GetMultilingualString("7fb8a598-a5fe-42e9-9edd-039cf87c5887", "Shipper's Load and Count"));
					AddPair(Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, SourceGenerated.ResString.GetMultilingualString("f6db0f97-8718-4688-88c4-529271268357", "Harmonized Commodity Code"));
					AddPair(Constants.AWB.NatureAndQtyOfGoodsTypes.CountryOfGoodsOrigin, SourceGenerated.ResString.GetMultilingualString("b2bd495a-612a-4159-8217-72c008f15e57", "Country/Region of Origin of Goods"));
					AddPair(Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery, SourceGenerated.ResString.GetMultilingualString("31bd9777-249b-40ec-9414-7c6e1bed137b", "Lithium Battery"));
					break;

				#endregion

				#region AWB Lithium Battery Type

				case OLookUpEditType.AWBLithiumBatteryType:
					AddPair(Constants.AWB.LithiumBatteryTypes.Codes.PI965, Constants.AWB.LithiumBatteryTypes.Descriptions.PI965);
					AddPair(Constants.AWB.LithiumBatteryTypes.Codes.PI966, Constants.AWB.LithiumBatteryTypes.Descriptions.PI966);
					AddPair(Constants.AWB.LithiumBatteryTypes.Codes.PI967, Constants.AWB.LithiumBatteryTypes.Descriptions.PI967);
					AddPair(Constants.AWB.LithiumBatteryTypes.Codes.PI968, Constants.AWB.LithiumBatteryTypes.Descriptions.PI968);
					AddPair(Constants.AWB.LithiumBatteryTypes.Codes.PI969, Constants.AWB.LithiumBatteryTypes.Descriptions.PI969);
					AddPair(Constants.AWB.LithiumBatteryTypes.Codes.PI970, Constants.AWB.LithiumBatteryTypes.Descriptions.PI970);
					AddPair(Constants.AWB.LithiumBatteryTypes.Codes.LMB, Constants.AWB.LithiumBatteryTypes.Descriptions.LMB);
					break;

				#endregion

				#region AWB AsAgreed First Set Types

				case OLookUpEditType.AWBAsAgreedFirstSetType:
					AddPair(Constants.AWB.AsAgreedTypes.Codes.All, Constants.AWB.AsAgreedTypes.Descriptions.All);
					AddPair(Constants.AWB.AsAgreedTypes.Codes.Collect, Constants.AWB.AsAgreedTypes.Descriptions.Collect);
					AddPair(Constants.AWB.AsAgreedTypes.Codes.None, Constants.AWB.AsAgreedTypes.Descriptions.None);
					break;

				#endregion

				#region AWB AsAgreed Second Set Types

				case OLookUpEditType.AWBAsAgreedSecondSetType:
					AddPair(Constants.AWB.AsAgreedTypes.Codes.All, Constants.AWB.AsAgreedTypes.Descriptions.All);
					AddPair(Constants.AWB.AsAgreedTypes.Codes.Prepaid, Constants.AWB.AsAgreedTypes.Descriptions.Prepaid);
					AddPair(Constants.AWB.AsAgreedTypes.Codes.None, Constants.AWB.AsAgreedTypes.Descriptions.None);
					break;

				#endregion

				#region Accounting Period Count

				case OLookUpEditType.ACPeriodCountType:
					AddPair(Constants.ACPeriodFormat.Month, SourceGenerated.ResString.GetMultilingualString("a0cd2b7f-1b22-4544-a695-0411538b4f61", "Calendar Months"));
					AddPair(Constants.ACPeriodFormat.FourWeeks, SourceGenerated.ResString.GetMultilingualString("168ebea8-46d5-4057-aa7e-bdc0e849a4f3", "4 WEEKS"));
					AddPair(Constants.ACPeriodFormat.FourFourFive, SourceGenerated.ResString.GetMultilingualString("cb50f861-0442-4f36-aa9d-3dfc9675d195", "4-4-5"));
					AddPair(Constants.ACPeriodFormat.Weeks, SourceGenerated.ResString.GetMultilingualString("3d8187b8-721c-4567-9f88-950ce63f45da", "WEEKS"));
					break;

				#endregion

				#region Accounting Period Apportionment Methods

				case OLookUpEditType.PeriodApportionmentMethods:
					AddPair(Constants.PeriodApportionmentMethods.Codes.Default, Constants.PeriodApportionmentMethods.Descriptions.Default);
					AddPair(Constants.PeriodApportionmentMethods.Codes.EquallyOverPeriods, Constants.PeriodApportionmentMethods.Descriptions.EquallyOverPeriods);
					AddPair(Constants.PeriodApportionmentMethods.Codes.Manual, Constants.PeriodApportionmentMethods.Descriptions.Manual);
					AddPair(Constants.PeriodApportionmentMethods.Codes.Day, Constants.PeriodApportionmentMethods.Descriptions.Day);
					break;

				#endregion

				#region Payment Remittance Print Options

				case OLookUpEditType.PaymentRemittancePrintOption:
					AddPair(Constants.PaymentRemittancePrintOption.PrintPaymentVoucher, Constants.PaymentRemittancePrintOption.PrintPaymentVoucher);
					AddPair(Constants.PaymentRemittancePrintOption.PrintRemittanceAdvice, Constants.PaymentRemittancePrintOption.PrintRemittanceAdvice);
					AddPair(Constants.PaymentRemittancePrintOption.PrintBoth, Constants.PaymentRemittancePrintOption.PrintBoth);
					break;

				#endregion

				#region Ageing

				case OLookUpEditType.Ageing:
					AddPair(Constants.Ageing.Current, SourceGenerated.ResString.GetMultilingualString("7a04cd30-0ec1-459b-b052-a626476a2194", "Current"));
					AddPair(Constants.Ageing.OnePeriod, SourceGenerated.ResString.GetMultilingualString("a07032ed-e64e-4191-95c6-c9d4980c65e7", "1 Period"));
					AddPair(Constants.Ageing.TwoPeriods, SourceGenerated.ResString.GetMultilingualString("8006ebb5-f3ad-4329-a0d6-8dfee878d2ab", "2 Periods"));
					AddPair(Constants.Ageing.ThreePeriods, SourceGenerated.ResString.GetMultilingualString("fbab6715-88e3-43cb-b4cd-27b9307078c0", "3 Periods"));
					break;

				#endregion

				#region Agent Type

				case OLookUpEditType.AgentType:
					AddPair(Constants.AgentType.Direct, Constants.AgentTypeDescriptions.Direct);
					AddPair(Constants.AgentType.CoLoad, Constants.AgentTypeDescriptions.CoLoad);
					AddPair(Constants.AgentType.Agent, Constants.AgentTypeDescriptions.Agent);
					AddPair(Constants.AgentType.Charter, Constants.AgentTypeDescriptions.Charter);
					AddPair(Constants.AgentType.Courier, Constants.AgentTypeDescriptions.Courier);
					AddPair(Constants.AgentType.Other, Constants.AgentTypeDescriptions.Other);
					break;

				#endregion

				#region Agents Reference Defaulting

				case OLookUpEditType.AgentsReferenceDefaulting:
					AddPair(Constants.AgentsReferenceDefaulting.FAR, SourceGenerated.ResString.GetMultilingualString("3606a1eb-d9d2-4953-bf4d-a295c23572bd", "Force the agents reference to be retained"));
					AddPair(Constants.AgentsReferenceDefaulting.NSR, SourceGenerated.ResString.GetMultilingualString("4eb60fdf-70a7-40c1-9390-636a84b4873e", "Force the Agents reference to never be sent"));
					AddPair(Constants.AgentsReferenceDefaulting.PAR, SourceGenerated.ResString.GetMultilingualString("668fc0f6-d149-4ccb-b23b-5dd81e5d3f19", "Pre-populate Agents Reference but allow override"));
					AddPair(Constants.AgentsReferenceDefaulting.DEF, SourceGenerated.ResString.GetMultilingualString("de807710-497e-42c3-8575-995924d51018", "Do not pre-pend the job number in transmitted Agents reference if it causes the reference to truncate"));
					break;

				#endregion

				#region Air Density Types

				case OLookUpEditType.AirDensity:
					AddPair(Constants.AirDensity.Light, SourceGenerated.ResString.GetMultilingualString("b8534f60-5220-4416-b06a-0a63aeeb8464", "Light"));
					AddPair(Constants.AirDensity.Normal, SourceGenerated.ResString.GetMultilingualString("34701517-0b4b-4862-a130-00adbdf8ec8f", "Normal"));
					AddPair(Constants.AirDensity.Heavy, SourceGenerated.ResString.GetMultilingualString("2a4afb87-e5ef-4c46-9903-8afb3cd41479", "Heavy"));
					break;

				#endregion

				#region AWB Paper Types

				case OLookUpEditType.AirWaybillPaperTypes:
					AddPair(Constants.AWB.PaperTypes.Iata, SourceGenerated.ResString.GetMultilingualString("73a430a9-0c3a-47c5-bb5b-a0ffdbac37db", "IATA"));
					AddPair(Constants.AWB.PaperTypes.IataOld, SourceGenerated.ResString.GetMultilingualString("eb11fbd4-49d1-49a0-9bb0-1427143e9878", "IATA Old"));
					AddPair(Constants.AWB.PaperTypes.Traxon, SourceGenerated.ResString.GetMultilingualString("1ccac377-29fd-45e4-adc6-02809ca54769", "Traxon"));
					AddPair(Constants.AWB.PaperTypes.Letter, SourceGenerated.ResString.GetMultilingualString("93e1fc32-f79a-413e-8ffe-bb15b0e18683", "Letter"));
					break;

				#endregion

				#region Allocation Method

				case OLookUpEditType.AllocationMethod:
					AddPair(AllocationMethod.Manual, SourceGenerated.ResString.GetMultilingualString("2f7862da-bd8c-4b22-9074-690e46ad2bcf", "Manual"));
					AddPair(AllocationMethod.Shipment, SourceGenerated.ResString.GetMultilingualString("6b59e210-6395-431a-9039-0f997e9f9edc", "Shipment"));
					AddPair(AllocationMethod.Revenue, SourceGenerated.ResString.GetMultilingualString("Common|ChargeType|Revenue", "Revenue"));
					AddPair(AllocationMethod.ChargeableUnits, SourceGenerated.ResString.GetMultilingualString("0df31242-d567-4006-b2b7-021b74c765f5", "Chargeable Units"));
					AddPair(AllocationMethod.GrossWeight, SourceGenerated.ResString.GetMultilingualString("1f3d19fc-0e5f-4b66-b93d-d8fc335fefcb", "Gross Weight"));
					AddPair(AllocationMethod.GrossVolume, SourceGenerated.ResString.GetMultilingualString("35AC9B05-652D-4831-896D-1C20A5295EF0", "Gross Volume"));
					AddPair(AllocationMethod.ContainerCount, SourceGenerated.ResString.GetMultilingualString("c96d4743-16fb-4abf-aaac-9f222f4aee46", "Container Count"));
					AddPair(AllocationMethod.OuterPackTotal, SourceGenerated.ResString.GetMultilingualString("76a23231-14dd-42cf-9f7b-f7a4ed8ec381", "Outer Pack Total"));
					AddPair(AllocationMethod.TwentyFootEquivalentUnit, SourceGenerated.ResString.GetMultilingualString("b2dd04f0-5d2e-4e30-94fa-b48a0d20ed2a", "Twenty-Foot Equivalent Unit"));
					AddPair(AllocationMethod.CapacityPerContainer, SourceGenerated.ResString.GetMultilingualString("31e25000-e0ef-4236-9c76-f2ed0220f593", "Capacity Per Container"));
					AddPair(AllocationMethod.FreeSpaceContribution, SourceGenerated.ResString.GetMultilingualString("50B60CB4-BB22-4DDC-95F1-60EFB8F6BF4E", "Free Space Contribution"));
					break;

				#endregion

				#region AR / AP Transaction Types

				case OLookUpEditType.ARAPTransactionTypes:
					AddPair("ALL", SourceGenerated.ResString.GetMultilingualString("2256303a-30d8-4126-b61a-87f346c324cb", "ALL Transactions")); // May be a some code abbreviature.
					AddPair(TransactionTypes.AdjustmentNote, SourceGenerated.ResString.GetMultilingualString("Accounting|InvoicePrintingTransactionTypes|AdjustmentNote", "Adjustment Note"));
					AddPair(TransactionTypes.Contra, SourceGenerated.ResString.GetMultilingualString("0eb14c5b-1807-4e13-be2f-1ff0129137f7", "Contra"));
					AddPair(TransactionTypes.CreditNote, SourceGenerated.ResString.GetMultilingualString("Accounting|InvoicePrintingTransactionTypes|CreditNote", "Credit Note"));
					AddPair(TransactionTypes.Discount, SourceGenerated.ResString.GetMultilingualString("8716c2e0-9cb9-4539-88f3-2278492b6135", "Discount"));
					AddPair(TransactionTypes.ExchangeDifference, SourceGenerated.ResString.GetMultilingualString("d7615f4f-fc56-4d2a-9e75-1d8287aa4e22", "Exchange Difference"));
					AddPair(TransactionTypes.Invoice, SourceGenerated.ResString.GetMultilingualString("Accounting|InvoicePrintingTransactionTypes|Invoice", "Invoice"));
					AddPair(TransactionTypes.Journal, SourceGenerated.ResString.GetMultilingualString("166301ad-428c-4415-b6de-438504fbaa4c", "Journal"));
					AddPair(TransactionTypes.Overpayment, SourceGenerated.ResString.GetMultilingualString("11bb4229-4438-4316-8fdf-01f8e0fb7fea", "Overpayment"));
					AddPair(TransactionTypes.Payment, SourceGenerated.ResString.GetMultilingualString("b099e09a-cd84-4bc6-acf9-4707cf842b29", "Payment"));
					AddPair(TransactionTypes.Receipt, SourceGenerated.ResString.GetMultilingualString("36bcdb16-c60e-4c66-971a-ed6cae9f1042", "Receipt"));
					AddPair(TransactionTypes.Transfer, SourceGenerated.ResString.GetMultilingualString("d508f6a8-d3d0-4b23-afac-ab9c9d54578e", "Transfer"));
					break;

				#endregion

				#region AU Customs Import Messaging Mode

				case OLookUpEditType.AUImportMessagingMode:
					AddPair(Constants.AUCustoms.ImportMessagingMode.Default, SourceGenerated.ResString.GetMultilingualString("2ded24e5-5444-4a1c-b08c-1a3546cca4cb", "Default"));
					AddPair(Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages, SourceGenerated.ResString.GetMultilingualString("0da743b6-f572-4d9b-95f7-2bda97628fad", "Force CMR"));
					AddPair(Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages, SourceGenerated.ResString.GetMultilingualString("e224e681-3f6e-47ec-ad7f-6180c41e4ea1", "Force Legacy"));
					break;

				#endregion

				#region Direct Debit File Format

				case OLookUpEditType.AutoDDRFileFormat:
					AddPair(Constants.DDRFileFormat.ANZ, SourceGenerated.ResString.GetMultilingualString("37b4cd79-6c7e-43f7-8f1c-62997c81dda6", "ANZ Bank"));
					AddPair(Constants.DDRFileFormat.BNZ, SourceGenerated.ResString.GetMultilingualString("03b195d3-e4e9-4cad-89f5-cd864bb4f2d6", "Bank of New Zealand"));
					AddPair(Constants.DDRFileFormat.NAB, SourceGenerated.ResString.GetMultilingualString("324846da-b68c-49b0-8091-dc6dd57e68a5", "National Australia Bank"));
					break;

				#endregion

				#region Available Commences

				case OLookUpEditType.AvailableCommences:
					AddPair(nameof(AvailableCommence.IME), SourceGenerated.ResString.GetMultilingualString("66cae005-00fa-4c49-b6cb-e4593b705b6f", "Immediately Unpack is advised"));
					AddPair(nameof(AvailableCommence.NCD), SourceGenerated.ResString.GetMultilingualString("e3e0d3ec-c179-49d7-8b63-f2acb14d3bb3", "Next Calendar Day Unpack Advised"));
					AddPair(nameof(AvailableCommence.NBD), SourceGenerated.ResString.GetMultilingualString("bef9c468-6b5c-4384-a74a-be838ba8c52b", "Next Business Day Unpack Advised"));
					break;

				#endregion

				#region Bank Charge Types

				case OLookUpEditType.BankChargeTypes:
					AddPair(ReceiptTypes.AccountMaintenanceFee, SourceGenerated.ResString.GetMultilingualString("49f8d1c4-8d11-4022-8d4e-29e795236176", "Account Maintenance Fee"));
					AddPair(ReceiptTypes.BankDebitTax, SourceGenerated.ResString.GetMultilingualString("b9123e60-e3b7-48b4-b7f6-66190f1e7834", "Bank Debit Tax"));
					AddPair(ReceiptTypes.BankDepositFee, SourceGenerated.ResString.GetMultilingualString("b3c9cfc3-e8e9-4c58-b560-d9fa6b60402d", "Bank Deposit Fee"));
					AddPair(ReceiptTypes.InterestPaid, SourceGenerated.ResString.GetMultilingualString("598e300e-9201-43e5-81c8-30b5f7b9c0a7", "Interest Paid"));
					AddPair(ReceiptTypes.InterestReceived, SourceGenerated.ResString.GetMultilingualString("de446cc0-2802-44f0-b863-e0d2bf456139", "Interest Received"));
					AddPair(ReceiptTypes.PeriodicPayment, SourceGenerated.ResString.GetMultilingualString("f53dd071-cd20-4a9e-9041-b36acfd2719a", "Periodic Payment"));
					AddPair(ReceiptTypes.StampDuty, SourceGenerated.ResString.GetMultilingualString("e54b8f7e-67fa-40f0-b680-eeb385176cc3", "Stamp Duty"));
					AddPair(ReceiptTypes.MiscellaneousReceipt, SourceGenerated.ResString.GetMultilingualString("9da1d9fc-c70c-4e31-8e1c-4ddbfec31310", "Miscellaneous Receipt"));
					AddPair(ReceiptTypes.MiscellaneousFees, SourceGenerated.ResString.GetMultilingualString("f56cad97-08b9-4b46-b9fd-655e39e52fa8", "Miscellaneous Fees"));
					break;

				#endregion

				#region Bank Reconcilliation Transaction Types

				case OLookUpEditType.BankReconTransactionTypes:
					AddPair("ALL", SourceGenerated.ResString.GetMultilingualString("62d60c66-426c-4fa7-a28c-0075d4049219", "ALL"));
					AddPair(TransactionTypes.ReceiptBatch, SourceGenerated.ResString.GetMultilingualString("aa73a3b8-0ee5-4c2e-9f07-90504bdc66e6", "Receipt Batch"));
					AddPair(TransactionTypes.Payment, SourceGenerated.ResString.GetMultilingualString("b099e09a-cd84-4bc6-acf9-4707cf842b29", "Payment"));
					AddPair(TransactionTypes.DirectPayment, SourceGenerated.ResString.GetMultilingualString("c857813f-231d-4912-9e8d-8e02bcebc52f", "Direct Payment"));
					AddPair(TransactionTypes.Transfer, SourceGenerated.ResString.GetMultilingualString("42906fa2-40ae-4ca7-a124-0fbc1f60a045", "Bank Transfer"));
					AddPair(TransactionTypes.OpeningReceipt, SourceGenerated.ResString.GetMultilingualString("0a71a2b3-4b70-493d-930a-30a5c2ec11a6", "Opening Receipt"));
					AddPair(TransactionTypes.OpeningPayment, SourceGenerated.ResString.GetMultilingualString("2edc7edd-ffa2-41d4-a1d4-a8cdd7fb8e36", "Opening Payment"));
					break;

				#endregion

				#region CASS Import Auto Create Claim
				case OLookUpEditType.CASSAutoCreateClaim:
					AddPair(CASSAutoCreateClaim.OverBilled, SourceGenerated.ResString.GetMultilingualString("1ce82e83-00a3-4fdb-bef4-6554fa8991fc", "Create claims for over-billing"));
					AddPair(CASSAutoCreateClaim.BothOverUnderBilled, SourceGenerated.ResString.GetMultilingualString("5d6bed9d-43b8-4c7c-9bb5-fb11260153d2", "Create claims for over-billing and under-billing"));
					AddPair(CASSAutoCreateClaim.NotCreated, SourceGenerated.ResString.GetMultilingualString("bc34c20a-b7cf-4597-a0b2-0fb105cdce90", "Do NOT create claims"));
					break;
				#endregion

				#region Consol Invoicing Styles

				case OLookUpEditType.ConsolInvoicingStyles:
					AddPair(Constants.ConsolInvoicingStyles.Master, SourceGenerated.ResString.GetMultilingualString("ConsolInvoicingStyles|MAS", "All Charges invoiced and rated from the Lead Shipment"));
					AddPair(Constants.ConsolInvoicingStyles.Apportion, SourceGenerated.ResString.GetMultilingualString("ConsolInvoicingStyles|APP", "Charges rated, apportioned and invoiced on individual shipments"));
					AddPair(Constants.ConsolInvoicingStyles.ApportionInvoiceMaster, SourceGenerated.ResString.GetMultilingualString("ConsolInvoicingStyles|MAB", "Charges entered/rated/apportioned to individual shipments, but invoiced from the Lead Shipment"));

					break;

				#endregion

				#region Charge Code Types

				case OLookUpEditType.ChargeTypes:
					AddPair(Constants.ChargeType.Margin, SourceGenerated.ResString.GetMultilingualString("Common|ChargeType|Margin", "Margin"));
					AddPair(Constants.ChargeType.Disbursement, SourceGenerated.ResString.GetMultilingualString("Common|ChargeType|Disbursement", "Disbursement"));
					AddPair(Constants.ChargeType.Revenue, SourceGenerated.ResString.GetMultilingualString("Common|ChargeType|Revenue", "Revenue"));
					AddPair(Constants.ChargeType.NonAccrual, SourceGenerated.ResString.GetMultilingualString("Common|ChargeType|NonAccrual", "Non Accrual"));
					AddPair(Constants.ChargeType.Overhead, SourceGenerated.ResString.GetMultilingualString("Common|ChargeType|Overhead", "Overhead"));
					AddPair(Constants.ChargeType.Comment, SourceGenerated.ResString.GetMultilingualString("Common|ChargeType|Comment", "Comment"));
					AddPair(Constants.ChargeType.ManualJobAccrual, SourceGenerated.ResString.GetMultilingualString("Common|ChargeType|ManualJobAccrual", "Manual Job Accrual"));
					break;

				#endregion

				#region Compliance Rollup Behaviour Type

				case OLookUpEditType.ComplianceRollupBehaviourType:
					AddPair(Constants.ComplianceRollupBehaviourType.SinglePageSummarize, SourceGenerated.ResString.GetMultilingualString("Common|ComplianceRollupBehaviourType|SinglePageSummarize", "Pre-Printed Paper, Single Page – Summarize Excess Rows as Other Charges"));
					AddPair(Constants.ComplianceRollupBehaviourType.SinglePageReferAttached, SourceGenerated.ResString.GetMultilingualString("Common|ComplianceRollupBehaviourType|SinglePageReferAttached", "Pre-Printed Paper, Single Page – Print Refer Attached when Rows exceed Maximum"));
					AddPair(Constants.ComplianceRollupBehaviourType.MultiPageNoLimitation, SourceGenerated.ResString.GetMultilingualString("Common|ComplianceRollupBehaviourType|MultiPageNoLimitation", "No limitation to permissible number of Rows / No Separate Compliance Document Required"));
					break;

				#endregion

				#region Compliance Book Allocation Level

				case OLookUpEditType.ComplianceBookAllocationLevel:
					AddPair(Constants.ComplianceBookAllocationLevel.Company, SourceGenerated.ResString.GetMultilingualString("Common|ComplianceBookAllocationLevel|Company", "Company"));
					AddPair(Constants.ComplianceBookAllocationLevel.Branch, SourceGenerated.ResString.GetMultilingualString("Common|ComplianceBookAllocationLevel|Branch", "Branch"));
					AddPair(Constants.ComplianceBookAllocationLevel.BranchDepartment, SourceGenerated.ResString.GetMultilingualString("Common|ComplianceBookAllocationLevel|BDP", "Branch & Department"));
					AddPair(Constants.ComplianceBookAllocationLevel.Counter, SourceGenerated.ResString.GetMultilingualString("Common|ComplianceBookAllocationLevel|CTR", "Counter"));
					break;

				#endregion

				#region Cash Book Transaction Types

				case OLookUpEditType.CashBookTransactionTypes:
					AddPair("ALL", SourceGenerated.ResString.GetMultilingualString("2256303a-30d8-4126-b61a-87f346c324cb", "ALL Transactions"));
					AddPair(TransactionTypes.Payment, SourceGenerated.ResString.GetMultilingualString("b099e09a-cd84-4bc6-acf9-4707cf842b29", "Payment"));
					AddPair(TransactionTypes.Receipt, SourceGenerated.ResString.GetMultilingualString("36bcdb16-c60e-4c66-971a-ed6cae9f1042", "Receipt"));
					AddPair(TransactionTypes.DirectPayment, SourceGenerated.ResString.GetMultilingualString("c857813f-231d-4912-9e8d-8e02bcebc52f", "Direct Payment"));
					AddPair(TransactionTypes.DirectReceipt, SourceGenerated.ResString.GetMultilingualString("72769dc8-2d24-4159-92e5-acf29ae1edb0", "Direct Receipt"));
					AddPair(TransactionTypes.OpeningReceipt, SourceGenerated.ResString.GetMultilingualString("0a71a2b3-4b70-493d-930a-30a5c2ec11a6", "Opening Receipt"));
					AddPair(TransactionTypes.OpeningPayment, SourceGenerated.ResString.GetMultilingualString("2edc7edd-ffa2-41d4-a1d4-a8cdd7fb8e36", "Opening Payment"));
					AddPair(TransactionTypes.Transfer, SourceGenerated.ResString.GetMultilingualString("d508f6a8-d3d0-4b23-afac-ab9c9d54578e", "Transfer"));
					AddPair(TransactionTypes.ExchangeDifference, SourceGenerated.ResString.GetMultilingualString("d7615f4f-fc56-4d2a-9e75-1d8287aa4e22", "Exchange Difference"));
					break;

				#endregion

				#region Competitor Activity

				case OLookUpEditType.CompetitorActivity:
					Clear();
					AddRange(EnvProxy.Instance.Registry.CompetitorActivityList);
					break;

				#endregion

				#region Commercial Invoice Method

				case OLookUpEditType.CommercialInvoiceMergeMethod:
					AddPair(Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge, SourceGenerated.ResString.GetMultilingualString("MasterFiles|MergeInvoiceLines|NotMerge", "No Merge"));
					AddPair(Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMergeUsingProductNumberInDescription, SourceGenerated.ResString.GetMultilingualString("MasterFiles|MergeInvoiceLines|NotMergeUsingProductNumberInDescription", "No Merge (include product number in description)"));
					AddPair(Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff, SourceGenerated.ResString.GetMultilingualString("MasterFiles|MergeInvoiceLines|Tariff", "Tariff"));
					AddPair(Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.TariffAndDescription, SourceGenerated.ResString.GetMultilingualString("MasterFiles|MergeInvoiceLines|TariffAndDescription", "Tariff and Description"));
					AddPair(Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.Classification, SourceGenerated.ResString.GetMultilingualString("MasterFiles|MergeInvoiceLines|Classification", "Classification"));
					AddPair(Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.ClassificationUsingClassificationDescriptionAlways, SourceGenerated.ResString.GetMultilingualString("MasterFiles|MergeInvoiceLines|ClassificationUsingClassificationDescriptionAlways", "Classification (use classification description always)"));
					AddPair(Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.PartNumber, SourceGenerated.ResString.GetMultilingualString("MasterFiles|MergeInvoiceLines|PartNumber", "Product Number"));
					AddPair(Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.PartNumberUsingProductNumberInDescription, SourceGenerated.ResString.GetMultilingualString("MasterFiles|MergeInvoiceLines|PartNumberUsingProductNumberInDescription", "Product Number (include product number in description)"));
					break;

				#endregion

				#region Container Detention Free Day Type

				case OLookUpEditType.ContainerDetentionFreeDayTypes:
					AddPair(Constants.ContainerDetentionFreeDayType.CTOAvailable, SourceGenerated.ResString.GetMultilingualString("c3b7e04f-2778-4531-92eb-7b1cf15a9e7e", "CTO Available"));
					AddPair(Constants.ContainerDetentionFreeDayType.FCLUnload, SourceGenerated.ResString.GetMultilingualString("81505f1a-f2e1-4b1c-8ab1-0329c9dfb6a9", "FCL Unload"));
					AddPair(Constants.ContainerDetentionFreeDayType.DayAfterFCLUnload, SourceGenerated.ResString.GetMultilingualString("8cfe8578-54f8-4b49-94ad-8f80fbbd8d19", "Day After FCL Unload"));
					AddPair(Constants.ContainerDetentionFreeDayType.VesselArrival, SourceGenerated.ResString.GetMultilingualString("d6e3c4af-b7b3-4de9-9373-a7f9efb45a4a", "Vessel Arrival"));
					AddPair(Constants.ContainerDetentionFreeDayType.CTOGateOut, SourceGenerated.ResString.GetMultilingualString("09be932f-d16d-4b42-8f31-7241827cc6c4", "CTO Gate Out"));
					AddPair(Constants.ContainerDetentionFreeDayType.WharfGateIn, SourceGenerated.ResString.GetMultilingualString("b7280b9d-aa4c-4cf6-ab72-991f3da164de", "Wharf Gate In"));
					AddPair(Constants.ContainerDetentionFreeDayType.FCLLoad, SourceGenerated.ResString.GetMultilingualString("aaff50c2-1e6f-43b3-9f62-a852d37458a3", "FCL Load"));
					AddPair(Constants.ContainerDetentionFreeDayType.DayBeforeFCLLoad, SourceGenerated.ResString.GetMultilingualString("99829519-7f1e-420b-81a8-e92a7b569f07", "Day before FCL Load"));
					AddPair(Constants.ContainerDetentionFreeDayType.VesselDeparture, SourceGenerated.ResString.GetMultilingualString("6d163603-68d0-435c-a1e6-644a9eb753e5", "Vessel Departure"));
					AddPair(Constants.ContainerDetentionFreeDayType.DayBeforeVesselDeparture, SourceGenerated.ResString.GetMultilingualString("1015e81e-189f-49bb-8e5a-97467cd690e5", "Day before Vessel Departure"));
					break;

				#endregion

				#region Container Mode

				case OLookUpEditType.ContainerMode:
					AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
					AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					AddPair(Constants.ContainerModes.AIR, Constants.ContainerModeDescriptions.AIR);
					AddPair(Constants.ContainerModes.BreakBulk, Constants.ContainerModeDescriptions.BreakBulk);
					AddPair(Constants.ContainerModes.Bulk, Constants.ContainerModeDescriptions.Bulk);
					AddPair(Constants.ContainerModes.Liquid, Constants.ContainerModeDescriptions.Liquid);
					AddPair(Constants.ContainerModes.Mail, Constants.ContainerModeDescriptions.Mail);
					AddPair(Constants.ContainerModes.FTL, Constants.ContainerModeDescriptions.FTL);
					AddPair(Constants.ContainerModes.LTL, Constants.ContainerModeDescriptions.LTL);
					AddPair(Constants.ContainerModes.ULD, Constants.ContainerModeDescriptions.ULD);
					AddPair(Constants.ContainerModes.Groupage, Constants.ContainerModeDescriptions.Groupage);
					AddPair(Constants.ContainerModes.BuyersConsol, Constants.ContainerModeDescriptions.BuyersConsol);
					AddPair(Constants.ContainerModes.ShippersConsol, Constants.ContainerModeDescriptions.ShippersConsol);
					break;

				case OLookUpEditType.ContainerModeFclLcl:
					AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
					AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					break;

				#endregion

				#region Container Type

				case OLookUpEditType.ContainerType:
					AddPair(Constants.ContainerTypes.Refrigerated, Constants.ContainerTypeDescriptions.Refrigerated);
					AddPair(Constants.ContainerTypes.DryStorage, Constants.ContainerTypeDescriptions.DryStorage);
					AddPair(Constants.ContainerTypes.OpenTop, Constants.ContainerTypeDescriptions.OpenTop);
					AddPair(Constants.ContainerTypes.FlatRack, Constants.ContainerTypeDescriptions.FlatRack);
					AddPair(Constants.ContainerTypes.Bolster, Constants.ContainerTypeDescriptions.Bolster);
					AddPair(Constants.ContainerTypes.Tank, Constants.ContainerTypeDescriptions.Tank);
					AddPair(Constants.ContainerTypes.Other, Constants.ContainerTypeDescriptions.Other);
					AddPair(Constants.ContainerTypes.MAFI, Constants.ContainerTypeDescriptions.MAFI);
					break;

				#endregion

				#region Container Storage Class

				case OLookUpEditType.ContainerStorageClass:
					Clear();
					AddRange(EnvProxy.Instance.Registry.ContainerStorageClass);
					break;

				#endregion

				#region Colour Depth

				case OLookUpEditType.ColourDepth:
					AddPair(Constants.ColourDepth.BlackAndWhite);
					AddPair(Constants.ColourDepth.Colour256);
					break;

				#endregion

				#region Conversion Factor

				case OLookUpEditType.ConversionFactor:
					AddPair("CON", SourceGenerated.ResString.GetMultilingualString("deca026a-8dcc-4151-b8f5-ebf332041913", "Show actual conversion factor"));
					AddPair("W/M", SourceGenerated.ResString.GetMultilingualString("76843618-38e1-44be-bc06-269277fcd594", "Show 'Per W/M'"));
					break;

				#endregion

				#region Custom Documents

				case OLookUpEditType.CustomDocuments:

					AddPair(Constants.CustomDocuments.ProfitShareCalculationWorkSheet.HideRevenueCostFigures, SourceGenerated.ResString.GetMultilingualString("6be33442-c604-45f7-a8a8-1995a2578244", "Hide Revenue, Cost, Profit and Agreement Information"));

					break;

				#endregion

				#region Custom Labels

				case OLookUpEditType.CustomLabels:
					var prefix = SourceGenerated.ResString.GetMultilingualString("4f9414fb-ad34-4c53-a8e8-3c2858eb6bd6", "Order");
					AddPair(Constants.CustomLabels.Order.CustomDate1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(1)));
					AddPair(Constants.CustomLabels.Order.CustomDate2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(2)));
					AddPair(Constants.CustomLabels.Order.CustomAttribute1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(1)));
					AddPair(Constants.CustomLabels.Order.CustomAttribute2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(2)));
					AddPair(Constants.CustomLabels.Order.CustomAttribute3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(3)));
					AddPair(Constants.CustomLabels.Order.CustomAttribute4, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(4)));
					AddPair(Constants.CustomLabels.Order.CustomAttribute5, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(5)));
					AddPair(Constants.CustomLabels.Order.CustomFlag1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(1)));
					AddPair(Constants.CustomLabels.Order.CustomFlag2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(2)));
					AddPair(Constants.CustomLabels.Order.CustomFlag3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(3)));
					AddPair(Constants.CustomLabels.Order.CustomFlag4, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(4)));
					AddPair(Constants.CustomLabels.Order.CustomFlag5, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(5)));
					AddPair(Constants.CustomLabels.Order.CustomDecimal1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(1)));
					AddPair(Constants.CustomLabels.Order.CustomDecimal2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(2)));
					AddPair(Constants.CustomLabels.Order.CustomDecimal3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(3)));
					AddPair(Constants.CustomLabels.Order.CustomDecimal4, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(4)));
					AddPair(Constants.CustomLabels.Order.CustomDecimal5, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(5)));
					AddPair(Constants.CustomLabels.Order.CustomContact1, CustomLabelDescription(prefix, Constants.CustomLabels.Order.Descriptions.CustomContact1));
					AddPair(Constants.CustomLabels.Order.CustomContact2, CustomLabelDescription(prefix, Constants.CustomLabels.Order.Descriptions.CustomContact2));
					AddPair(Constants.CustomLabels.Order.GoodsOrigin, CustomLabelDescription(prefix, Constants.CustomLabels.Order.Descriptions.GoodsOrigin));
					AddPair(Constants.CustomLabels.Order.GoodsDestination, CustomLabelDescription(prefix, Constants.CustomLabels.Order.Descriptions.GoodsDestination));
					AddPair(Constants.CustomLabels.Order.UserTrackDate1, CustomLabelDescription(prefix, Constants.CustomLabels.Order.Descriptions.UserTrackDate1));
					AddPair(Constants.CustomLabels.Order.UserTrackDate2, CustomLabelDescription(prefix, Constants.CustomLabels.Order.Descriptions.UserTrackDate2));
					AddPair(Constants.CustomLabels.Order.UserTrackDate3, CustomLabelDescription(prefix, Constants.CustomLabels.Order.Descriptions.UserTrackDate3));
					AddPair(Constants.CustomLabels.Order.UserTrackDate4, CustomLabelDescription(prefix, Constants.CustomLabels.Order.Descriptions.UserTrackDate4));

					prefix = SourceGenerated.ResString.GetMultilingualString("f02b4aa9-b98e-4f7c-b012-9f47046d2430", "Order Line");
					AddPair(Constants.CustomLabels.OrderLine.CustomAttribute1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(1)));
					AddPair(Constants.CustomLabels.OrderLine.CustomAttribute2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(2)));
					AddPair(Constants.CustomLabels.OrderLine.CustomAttribute3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(3)));
					AddPair(Constants.CustomLabels.OrderLine.CustomAttribute4, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(4)));
					AddPair(Constants.CustomLabels.OrderLine.CustomAttribute5, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(5)));
					AddPair(Constants.CustomLabels.OrderLine.CustomAttribute6, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(6)));
					AddPair(Constants.CustomLabels.OrderLine.CustomText1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomText(1)));
					AddPair(Constants.CustomLabels.OrderLine.CustomFlag1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(1)));
					AddPair(Constants.CustomLabels.OrderLine.CustomFlag2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(2)));
					AddPair(Constants.CustomLabels.OrderLine.CustomFlag3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(3)));
					AddPair(Constants.CustomLabels.OrderLine.CustomFlag4, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(4)));
					AddPair(Constants.CustomLabels.OrderLine.CustomFlag5, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(5)));
					AddPair(Constants.CustomLabels.OrderLine.CustomDate1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(1)));
					AddPair(Constants.CustomLabels.OrderLine.CustomDate2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(2)));
					AddPair(Constants.CustomLabels.OrderLine.CustomDate3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(3)));
					AddPair(Constants.CustomLabels.OrderLine.CustomDate4, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(4)));
					AddPair(Constants.CustomLabels.OrderLine.CustomDate5, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(5)));
					AddPair(Constants.CustomLabels.OrderLine.CustomDecimal1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(1)));
					AddPair(Constants.CustomLabels.OrderLine.CustomDecimal2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(2)));
					AddPair(Constants.CustomLabels.OrderLine.CustomDecimal3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(3)));
					AddPair(Constants.CustomLabels.OrderLine.CustomDecimal4, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(4)));
					AddPair(Constants.CustomLabels.OrderLine.CustomDecimal5, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(5)));

					prefix = SourceGenerated.ResString.GetMultilingualString("94053385-f432-4e5e-ad9a-7f2b024140e0", "Order Delivery");
					AddPair(Constants.CustomLabels.OrderLineDelivery.CustomAttribute1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(1)));
					AddPair(Constants.CustomLabels.OrderLineDelivery.CustomAttribute2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(2)));
					AddPair(Constants.CustomLabels.OrderLineDelivery.CustomAttribute3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(3)));
					AddPair(Constants.CustomLabels.OrderLineDelivery.CustomAttribute4, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(4)));
					AddPair(Constants.CustomLabels.OrderLineDelivery.CustomAttribute5, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(5)));
					AddPair(Constants.CustomLabels.OrderLineDelivery.CustomFlag1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(1)));
					AddPair(Constants.CustomLabels.OrderLineDelivery.CustomFlag2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(2)));
					AddPair(Constants.CustomLabels.OrderLineDelivery.CustomFlag3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(3)));
					AddPair(Constants.CustomLabels.OrderLineDelivery.CustomFlag4, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(4)));
					AddPair(Constants.CustomLabels.OrderLineDelivery.CustomFlag5, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(5)));
					AddPair(Constants.CustomLabels.OrderLineDelivery.CustomDate1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(1)));
					AddPair(Constants.CustomLabels.OrderLineDelivery.CustomDate2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(2)));
					AddPair(Constants.CustomLabels.OrderLineDelivery.CustomDate3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(3)));
					AddPair(Constants.CustomLabels.OrderLineDelivery.CustomDate4, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(4)));
					AddPair(Constants.CustomLabels.OrderLineDelivery.CustomDate5, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(5)));
					AddPair(Constants.CustomLabels.OrderLineDelivery.CustomDecimal1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(1)));
					AddPair(Constants.CustomLabels.OrderLineDelivery.CustomDecimal2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(2)));
					AddPair(Constants.CustomLabels.OrderLineDelivery.CustomDecimal3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(3)));
					AddPair(Constants.CustomLabels.OrderLineDelivery.CustomDecimal4, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(4)));
					AddPair(Constants.CustomLabels.OrderLineDelivery.CustomDecimal5, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(5)));

					prefix = SourceGenerated.ResString.GetMultilingualString("91fe1350-082f-4e18-9c92-b5046cb04a1d", "Order Container");
					AddPair(Constants.CustomLabels.OrderLineDeliverContainer.CustomAttribute1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(1)));
					AddPair(Constants.CustomLabels.OrderLineDeliverContainer.CustomAttribute2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(2)));
					AddPair(Constants.CustomLabels.OrderLineDeliverContainer.CustomAttribute3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(3)));
					AddPair(Constants.CustomLabels.OrderLineDeliverContainer.CustomFlag1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(1)));
					AddPair(Constants.CustomLabels.OrderLineDeliverContainer.CustomFlag2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(2)));
					AddPair(Constants.CustomLabels.OrderLineDeliverContainer.CustomFlag3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(3)));
					AddPair(Constants.CustomLabels.OrderLineDeliverContainer.CustomDate1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(1)));
					AddPair(Constants.CustomLabels.OrderLineDeliverContainer.CustomDate2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(2)));
					AddPair(Constants.CustomLabels.OrderLineDeliverContainer.CustomDate3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(3)));
					AddPair(Constants.CustomLabels.OrderLineDeliverContainer.CustomDecimal1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(1)));
					AddPair(Constants.CustomLabels.OrderLineDeliverContainer.CustomDecimal2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(2)));
					AddPair(Constants.CustomLabels.OrderLineDeliverContainer.CustomDecimal3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(3)));
					AddPair(Constants.CustomLabels.OrderLineDeliverContainer.QuantityInvoiced, CustomLabelDescription(prefix, Constants.CustomLabels.OrderLineDeliverContainer.Descriptions.QuantityInvoiced));
					AddPair(Constants.CustomLabels.OrderLineDeliverContainer.QuantityDelivered, CustomLabelDescription(prefix, Constants.CustomLabels.OrderLineDeliverContainer.Descriptions.QuantityDelivered));

					prefix = SourceGenerated.ResString.GetMultilingualString("284e36de-9871-46a2-8e02-1f972e35267f", "Parts");
					AddPair(Constants.CustomLabels.Parts.CustomAttribute1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(1)));
					AddPair(Constants.CustomLabels.Parts.CustomAttribute2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(2)));
					AddPair(Constants.CustomLabels.Parts.CustomAttribute3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(3)));
					AddPair(Constants.CustomLabels.Parts.CustomAttribute4, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(4)));
					AddPair(Constants.CustomLabels.Parts.CustomAttribute5, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(5)));
					AddPair(Constants.CustomLabels.Parts.CustomFlag1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(1)));
					AddPair(Constants.CustomLabels.Parts.CustomFlag2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(2)));
					AddPair(Constants.CustomLabels.Parts.CustomFlag3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(4)));
					AddPair(Constants.CustomLabels.Parts.CustomFlag4, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(5)));
					AddPair(Constants.CustomLabels.Parts.CustomFlag5, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(5)));
					AddPair(Constants.CustomLabels.Parts.CustomDate1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(1)));
					AddPair(Constants.CustomLabels.Parts.CustomDate2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(2)));
					AddPair(Constants.CustomLabels.Parts.CustomDate3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(3)));
					AddPair(Constants.CustomLabels.Parts.CustomDate4, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(4)));
					AddPair(Constants.CustomLabels.Parts.CustomDate5, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(5)));
					AddPair(Constants.CustomLabels.Parts.CustomDecimal1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(1)));
					AddPair(Constants.CustomLabels.Parts.CustomDecimal2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(2)));
					AddPair(Constants.CustomLabels.Parts.CustomDecimal3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(3)));
					AddPair(Constants.CustomLabels.Parts.CustomDecimal4, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(4)));
					AddPair(Constants.CustomLabels.Parts.CustomDecimal5, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(5)));

					AddPair(Constants.CustomLabels.Parts.OrderMultiple, CustomLabelDescription(prefix, Constants.CustomLabels.Parts.Descriptions.OrderMultiple));
					AddPair(Constants.CustomLabels.Parts.VendorPack, CustomLabelDescription(prefix, Constants.CustomLabels.Parts.Descriptions.VendorPack));
					AddPair(Constants.CustomLabels.Parts.Department, CustomLabelDescription(prefix, Constants.CustomLabels.Parts.Descriptions.Department));
					AddPair(Constants.CustomLabels.Parts.Division, CustomLabelDescription(prefix, Constants.CustomLabels.Parts.Descriptions.Division));

					prefix = SourceGenerated.ResString.GetMultilingualString("6e661f52-2102-448b-9d9b-9424ac214c69", "Organization");
					AddPair(Constants.CustomLabels.Organisation.CustomAttribute1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(1)));
					AddPair(Constants.CustomLabels.Organisation.CustomAttribute2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(2)));
					AddPair(Constants.CustomLabels.Organisation.CustomAttribute3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(3)));
					AddPair(Constants.CustomLabels.Organisation.CustomFlag1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(1)));
					AddPair(Constants.CustomLabels.Organisation.CustomFlag2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(2)));
					AddPair(Constants.CustomLabels.Organisation.CustomFlag3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(3)));
					AddPair(Constants.CustomLabels.Organisation.CustomFlag4, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(4)));
					AddPair(Constants.CustomLabels.Organisation.CustomDate1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(1)));
					AddPair(Constants.CustomLabels.Organisation.CustomDate2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(2)));
					AddPair(Constants.CustomLabels.Organisation.CustomDate3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(3)));
					AddPair(Constants.CustomLabels.Organisation.CustomDecimal1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(1)));
					AddPair(Constants.CustomLabels.Organisation.CustomDecimal2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(2)));
					AddPair(Constants.CustomLabels.Organisation.CustomDecimal3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(3)));

					prefix = SourceGenerated.ResString.GetMultilingualString("d3afd9ec-7f20-4a64-b64c-b35da34cff3a", "Invoice Line");
					AddPair(Constants.CustomLabels.ComInvoiceLine.CustomAttribute1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(1)));
					AddPair(Constants.CustomLabels.ComInvoiceLine.CustomAttribute2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(2)));
					AddPair(Constants.CustomLabels.ComInvoiceLine.CustomAttribute3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(3)));
					AddPair(Constants.CustomLabels.ComInvoiceLine.CustomAttribute4, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(4)));
					AddPair(Constants.CustomLabels.ComInvoiceLine.CustomAttribute5, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(5)));
					AddPair(Constants.CustomLabels.ComInvoiceLine.CustomAttribute6, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(6)));
					AddPair(Constants.CustomLabels.ComInvoiceLine.CustomText1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomText(1)));
					AddPair(Constants.CustomLabels.ComInvoiceLine.CustomFlag1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(1)));
					AddPair(Constants.CustomLabels.ComInvoiceLine.CustomFlag2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(2)));
					AddPair(Constants.CustomLabels.ComInvoiceLine.CustomFlag3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(3)));
					AddPair(Constants.CustomLabels.ComInvoiceLine.CustomDate1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(1)));
					AddPair(Constants.CustomLabels.ComInvoiceLine.CustomDate2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(2)));
					AddPair(Constants.CustomLabels.ComInvoiceLine.CustomDate3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(3)));
					AddPair(Constants.CustomLabels.ComInvoiceLine.CustomDecimal1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(1)));
					AddPair(Constants.CustomLabels.ComInvoiceLine.CustomDecimal2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(2)));
					AddPair(Constants.CustomLabels.ComInvoiceLine.CustomDecimal3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(3)));

					prefix = SourceGenerated.ResString.GetMultilingualString("6ff0b174-8593-48da-b7fa-b154922fd14b", "Customs Container");
					AddPair(Constants.CustomLabels.CusContainer.CustomAttribute1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(1)));
					AddPair(Constants.CustomLabels.CusContainer.CustomFlag1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(1)));
					AddPair(Constants.CustomLabels.CusContainer.CustomDate1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(1)));
					AddPair(Constants.CustomLabels.CusContainer.CustomDecimal1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(1)));

					prefix = SourceGenerated.ResString.GetMultilingualString("01d6f9b3-9d21-473b-b81e-150d318e3fe7", "Consol");
					AddPair(Constants.CustomLabels.Consol.CustomDate1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(1)));
					AddPair(Constants.CustomLabels.Consol.CustomDate2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(2)));
					AddPair(Constants.CustomLabels.Consol.CustomFlag1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(1)));
					AddPair(Constants.CustomLabels.Consol.CustomFlag2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(2)));
					AddPair(Constants.CustomLabels.Consol.CustomString1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomText(1)));
					AddPair(Constants.CustomLabels.Consol.CustomString2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomText(2)));
					AddPair(Constants.CustomLabels.Consol.CustomNumber1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(1)));
					AddPair(Constants.CustomLabels.Consol.CustomNumber2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(2)));

					prefix = SourceGenerated.ResString.GetMultilingualString("e64aa6b2-b3d2-46d9-a71e-d005b979c2d6", "Warehouse Docket");
					AddPair(Constants.CustomLabels.WhsDocket.CustomAttribute1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(1)));
					AddPair(Constants.CustomLabels.WhsDocket.CustomAttribute2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(2)));
					AddPair(Constants.CustomLabels.WhsDocket.CustomAttribute3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(3)));
					AddPair(Constants.CustomLabels.WhsDocket.CustomAttribute4, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(4)));
					AddPair(Constants.CustomLabels.WhsDocket.CustomAttribute5, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(5)));
					AddPair(Constants.CustomLabels.WhsDocket.CustomDecimal1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(1)));
					AddPair(Constants.CustomLabels.WhsDocket.CustomDecimal2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(2)));
					AddPair(Constants.CustomLabels.WhsDocket.CustomDecimal3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(3)));
					AddPair(Constants.CustomLabels.WhsDocket.CustomDecimal4, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(4)));
					AddPair(Constants.CustomLabels.WhsDocket.CustomDecimal5, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(5)));
					AddPair(Constants.CustomLabels.WhsDocket.CustomDate1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(1)));
					AddPair(Constants.CustomLabels.WhsDocket.CustomDate2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(2)));
					AddPair(Constants.CustomLabels.WhsDocket.CustomFlag1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(1)));
					AddPair(Constants.CustomLabels.WhsDocket.CustomFlag2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(2)));
					AddPair(Constants.CustomLabels.WhsDocket.CustomFlag3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(3)));
					AddPair(Constants.CustomLabels.WhsDocket.CustomFlag4, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(4)));
					AddPair(Constants.CustomLabels.WhsDocket.CustomFlag5, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(5)));

					prefix = SourceGenerated.ResString.GetMultilingualString("a9d5d2ba-18d8-4a00-9eb8-d451527ff98b", "Warehouse Docket Line");
					AddPair(Constants.CustomLabels.WhsDocketLine.CustomAttribute1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(1)));
					AddPair(Constants.CustomLabels.WhsDocketLine.CustomAttribute2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(2)));
					AddPair(Constants.CustomLabels.WhsDocketLine.CustomAttribute3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(3)));
					AddPair(Constants.CustomLabels.WhsDocketLine.CustomAttribute4, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(4)));
					AddPair(Constants.CustomLabels.WhsDocketLine.CustomAttribute5, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(5)));
					AddPair(Constants.CustomLabels.WhsDocketLine.CustomAttribute6, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(6)));
					AddPair(Constants.CustomLabels.WhsDocketLine.CustomDecimal1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(1)));
					AddPair(Constants.CustomLabels.WhsDocketLine.CustomDecimal2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(2)));
					AddPair(Constants.CustomLabels.WhsDocketLine.CustomDecimal3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(3)));
					AddPair(Constants.CustomLabels.WhsDocketLine.CustomDecimal4, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(4)));
					AddPair(Constants.CustomLabels.WhsDocketLine.CustomDecimal5, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(5)));
					AddPair(Constants.CustomLabels.WhsDocketLine.CustomDate1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(1)));
					AddPair(Constants.CustomLabels.WhsDocketLine.CustomDate2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(2)));
					AddPair(Constants.CustomLabels.WhsDocketLine.CustomDate3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(3)));
					AddPair(Constants.CustomLabels.WhsDocketLine.CustomDate4, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(4)));
					AddPair(Constants.CustomLabels.WhsDocketLine.CustomDate5, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(5)));
					AddPair(Constants.CustomLabels.WhsDocketLine.CustomFlag1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(1)));
					AddPair(Constants.CustomLabels.WhsDocketLine.CustomFlag2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(2)));
					AddPair(Constants.CustomLabels.WhsDocketLine.CustomFlag3, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(3)));
					AddPair(Constants.CustomLabels.WhsDocketLine.CustomFlag4, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(4)));
					AddPair(Constants.CustomLabels.WhsDocketLine.CustomFlag5, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(5)));
					AddPair(Constants.CustomLabels.WhsDocketLine.CustomTextBlob1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomText(1)));

					prefix = SourceGenerated.ResString.GetMultilingualString("4bc005dc-1f9e-4e30-ad2d-30e8480eb06f", "Customs Packing List");
					AddPair(Constants.CustomLabels.CustomsPackingList.CustomAttribute1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(1)));
					AddPair(Constants.CustomLabels.CustomsPackingList.CustomAttribute2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(2)));
					AddPair(Constants.CustomLabels.CustomsPackingList.CustomFlag1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(1)));
					AddPair(Constants.CustomLabels.CustomsPackingList.CustomFlag2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(2)));
					AddPair(Constants.CustomLabels.CustomsPackingList.CustomDate1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(1)));
					AddPair(Constants.CustomLabels.CustomsPackingList.CustomDate2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(2)));
					AddPair(Constants.CustomLabels.CustomsPackingList.CustomDecimal1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(1)));
					AddPair(Constants.CustomLabels.CustomsPackingList.CustomDecimal2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(2)));

					prefix = SourceGenerated.ResString.GetMultilingualString("e68bc5e6-b9a5-4e18-8167-23f056a9cdfa", "Customs Package");
					AddPair(Constants.CustomLabels.CusPackage.CustomAttribute1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(1)));
					AddPair(Constants.CustomLabels.CusPackage.CustomAttribute2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomAttribute(2)));
					AddPair(Constants.CustomLabels.CusPackage.CustomFlag1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(1)));
					AddPair(Constants.CustomLabels.CusPackage.CustomFlag2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomFlag(2)));
					AddPair(Constants.CustomLabels.CusPackage.CustomDate1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(1)));
					AddPair(Constants.CustomLabels.CusPackage.CustomDate2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomDate(2)));
					AddPair(Constants.CustomLabels.CusPackage.CustomDecimal1, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(1)));
					AddPair(Constants.CustomLabels.CusPackage.CustomDecimal2, CustomLabelDescription(prefix, Constants.CustomLabels.Descriptions.CustomNumber(2)));

					break;

				#endregion

				#region Custom Empty List

				case OLookUpEditType.CustomType:
					break;

				#endregion

				#region Local Cartage Transport Modes

				case OLookUpEditType.LocalCartageTransportModes:
					AddPair(Constants.TransportModes.Air, Constants.TransportModeDescriptions.Air);
					AddPair(Constants.TransportModes.Sea, Constants.TransportModeDescriptions.Sea);
					AddPair(Constants.TransportModes.Road, Constants.TransportModeDescriptions.Road);
					AddPair(Constants.TransportModes.Rail, Constants.TransportModeDescriptions.Rail);
					break;

				#endregion

				#region Local Cartage Container Modes

				case OLookUpEditType.LocalCartageContainerModes:
					AddPair(Constants.ContainerModes.Containerised, Constants.ContainerModeDescriptions.Containerised);
					AddPair(Constants.ContainerModes.Loose, Constants.ContainerModeDescriptions.Loose);
					AddPair(Constants.ContainerModes.RollOnRollOff, Constants.ContainerModeDescriptions.RollOnRollOff);
					AddPair(Constants.ContainerModes.BreakBulk, Constants.ContainerModeDescriptions.BreakBulk);

					break;

				#endregion

				#region Departure And Arrival Date

				case OLookUpEditType.DateFilterType:
					AddPair(Constants.RatingDateFilterTypes.Codes.Standard, Constants.RatingDateFilterTypes.Descriptions.Standard);
					AddPair(Constants.RatingDateFilterTypes.Codes.Departure, Constants.RatingDateFilterTypes.Descriptions.Departure);
					AddPair(Constants.RatingDateFilterTypes.Codes.Arrival, Constants.RatingDateFilterTypes.Descriptions.Arrival);
					AddPair(Constants.RatingDateFilterTypes.Codes.Custom, Constants.RatingDateFilterTypes.Descriptions.Custom);
					break;

				#endregion

				#region Discrepancy Reason

				case OLookUpEditType.DiscrepancyReason:
					AddPair(Constants.DiscrepancyReason.ShortLanded, SourceGenerated.ResString.GetMultilingualString("34907d98-1a2f-46e3-bb48-abba1317a41f", "Short Landed"));
					AddPair(Constants.DiscrepancyReason.Surplus, SourceGenerated.ResString.GetMultilingualString("eb58ad00-cefa-4c16-8178-c5c86c018311", "Surplus"));
					AddPair(Constants.DiscrepancyReason.Damaged, SourceGenerated.ResString.GetMultilingualString("484c95fe-f169-457b-871e-5fd149d49aeb", "Damaged"));
					AddPair(Constants.DiscrepancyReason.Pillaged, SourceGenerated.ResString.GetMultilingualString("b51ca738-b4a8-4b0b-ae49-e0bdc3e0e81e", "Pillaged"));
					AddPair(Constants.DiscrepancyReason.Lost, SourceGenerated.ResString.GetMultilingualString("f3730618-4cd7-4788-835c-2d67bc34654a", "Lost"));
					break;

				#endregion

				#region Debit / Credit

				case OLookUpEditType.DebitCredit:
					AddPair("DR", SourceGenerated.ResString.GetMultilingualString("dd59a0cb-0583-4432-96d3-3e9890c26e98", "Debit"));
					AddPair("CR", SourceGenerated.ResString.GetMultilingualString("691e951c-ee36-4bb7-890c-87fae9325566", "Credit"));
					break;

				#endregion

				#region Default Cargo Report Consignee Option

				case OLookUpEditType.DefaultCargoReportConsigneeOption:
					AddPair(Constants.AUCustoms.DefaultConsigneeOption.Consignee, SourceGenerated.ResString.GetMultilingualString("d7c16581-8345-4c69-b1bf-1875b556d06c", "Consignee"));
					AddPair(Constants.AUCustoms.DefaultConsigneeOption.DeliverTo, SourceGenerated.ResString.GetMultilingualString("5bcb6a90-e301-491d-a6d9-3382e1f51d79", "Deliver To"));
					AddPair(Constants.AUCustoms.DefaultConsigneeOption.None, SourceGenerated.ResString.GetMultilingualString("a7ab1c55-bceb-4fe5-a88e-6311ca6da6a8", "None"));
					break;

				#endregion

				#region Document Transport Mode

				case OLookUpEditType.DocumentTransportMode:
					AddPair(Constants.TransportModes.All, SourceGenerated.ResString.GetMultilingualString("a2a094c3-d3b0-472a-a2ea-71d5f08fe942", "All"));
					AddPair(Constants.TransportModes.Air, SourceGenerated.ResString.GetMultilingualString("09378d44-8680-4fc7-bfab-1f144ed49095", "Air"));
					AddPair(Constants.TransportModes.Courier, SourceGenerated.ResString.GetMultilingualString("09378d44-8680-4fc7-bfab-1f144ed49096", "Courier"));
					AddPair(Constants.TransportModes.Sea, SourceGenerated.ResString.GetMultilingualString("ed825a11-2454-48a9-8454-76c6836ad104", "Sea"));
					AddPair(Constants.TransportModes.Rail, SourceGenerated.ResString.GetMultilingualString("13e41807-2503-447a-ba1d-f9849eaf89ab", "Rail"));
					AddPair(Constants.TransportModes.Road, SourceGenerated.ResString.GetMultilingualString("96ebcfe9-2f8c-4d91-a0a6-1222f81ee1e0", "Road"));
					AddPair(Constants.TransportModes.Other, SourceGenerated.ResString.GetMultilingualString("96ebcfe9-2f8c-4d91-a0a6-1222f81ee1e1", "Other"));
					break;

				#endregion

				#region Equipment Group

				case OLookUpEditType.EquipmentGroup:
					AddRange(EnvProxy.Instance.Registry.ReferenceFiles.EquipmentGroup);
					break;

				#endregion

				#region Exchange Rate Types

				case OLookUpEditType.ExchangeRateType:
					AddPair(Constants.ExchangeRateTypes.Code.BuyRate, Constants.ExchangeRateTypes.Description.BuyRate);
					AddPair(Constants.ExchangeRateTypes.Code.SellRate, Constants.ExchangeRateTypes.Description.SellRate);
					break;

				#endregion

				#region Export Goods Type

				case OLookUpEditType.ExportGoodsType:
					AddPair("OT", SourceGenerated.ResString.GetMultilingualString("d9e0b676-754a-45e1-9f33-0cb6d02aae0d", "General Consigned Cargo"));
					AddPair("ST", SourceGenerated.ResString.GetMultilingualString("1c6d4b48-1448-4f99-b249-d13edbe4e5d9", "Stores to be consumed on board the vessel/aircraft"));
					AddPair("SP", SourceGenerated.ResString.GetMultilingualString("6412de25-72ac-4c2a-aef9-838d93ae8628", "Spare parts for the vessel/aircraft"));
					AddPair("OP", SourceGenerated.ResString.GetMultilingualString("f4519f1d-1cf5-4e94-9533-3a1cc5243a59", "Own Power"));
					AddPair("AB", SourceGenerated.ResString.GetMultilingualString("8b706f43-9781-4a0a-b8ec-11c5769a4f93", "Accompanied Baggage"));
					AddPair("PO", SourceGenerated.ResString.GetMultilingualString("b3d67962-f102-404a-8fc8-bfa1bd767a1d", "Postal"));
					break;

				#endregion

				#region Freight Container Mode

				case OLookUpEditType.FreightContainerMode:
					AddPair(Constants.ContainerModes.LCL, SourceGenerated.ResString.GetMultilingualString("Core|PackingModeList|LCL", "Less Container Load"));
					AddPair(Constants.ContainerModes.FCL, SourceGenerated.ResString.GetMultilingualString("Core|PackingModeList|FCL", "Full Container Load"));
					AddPair(Constants.ContainerModes.Groupage, SourceGenerated.ResString.GetMultilingualString("Core|PackingModeList|Groupage", "Groupage / Freight All Kinds"));
					AddPair(Constants.ContainerModes.BuyersConsol, SourceGenerated.ResString.GetMultilingualString("Core|PackingModeList|BuyersConsol", "Buyer's Consolidation"));
					AddPair(Constants.ContainerModes.Loose, SourceGenerated.ResString.GetMultilingualString("Core|PackingModeList|Loose", "Loose"));
					AddPair(Constants.ContainerModes.ULD, SourceGenerated.ResString.GetMultilingualString("Core|PackingModeList|ULD", "Unit Load Device"));
					AddPair(Constants.ContainerModes.BreakBulk, SourceGenerated.ResString.GetMultilingualString("Core|PackingModeList|BreakBulk", "Break Bulk"));
					AddPair(Constants.ContainerModes.Bulk, SourceGenerated.ResString.GetMultilingualString("Core|PackingModeList|Bulk", "Bulk"));
					AddPair(Constants.ContainerModes.Liquid, SourceGenerated.ResString.GetMultilingualString("Core|PackingModeList|Liquid", "Liquid"));
					AddPair(Constants.ContainerModes.RollOnRollOff, SourceGenerated.ResString.GetMultilingualString("Core|PackingModeList|RollOnRollOff", "Roll On/Roll Off"));
					AddPair(Constants.ContainerModes.AgentConsol, SourceGenerated.ResString.GetMultilingualString("Core|PackingModeList|AgentConsol", "Agent Consolidation"));
					AddPair(Constants.ContainerModes.LTL, SourceGenerated.ResString.GetMultilingualString("Core|PackingModeList|LTL", "Less Truck Load"));
					AddPair(Constants.ContainerModes.FTL, SourceGenerated.ResString.GetMultilingualString("Core|PackingModeList|FTL", "Full Truck Load"));
					AddPair(Constants.ContainerModes.OnBoardCourier, SourceGenerated.ResString.GetMultilingualString("Core|PackingModeList|OnBoardCourier", "On Board Courier"));
					AddPair(Constants.ContainerModes.Unaccompanied, SourceGenerated.ResString.GetMultilingualString("Core|PackingModeList|Unaccompanied", "Unaccompanied"));
					AddPair(Constants.ContainerModes.Other, SourceGenerated.ResString.GetMultilingualString("Core|PackingModeList|Other", "Other"));
					break;

				#endregion

				#region Area / Weight / Volume / Length

				case OLookUpEditType.Area:
					AddAreaPairs();
					break;

				case OLookUpEditType.Weight:
					AddWeightPairs(Constants.PluralState.Plural);
					break;

				case OLookUpEditType.Volume:
					AddVolumePairs(Constants.PluralState.Plural);
					break;

				case OLookUpEditType.Length:
					AddLengthPairs();
					break;

				case OLookUpEditType.Distance:
					AddDistancePairs();
					break;

				case OLookUpEditType.Dimension:
					AddDimensionPairs();
					break;

				#endregion

				#region Container Packing Modes

				case OLookUpEditType.ContainerPackingModes:
					AddPair(Constants.ContainerPackingMode.Import, SourceGenerated.ResString.GetMultilingualString("ce880a48-a6b6-48c2-9be2-a1995ccd38f8", "Imports"));
					AddPair(Constants.ContainerPackingMode.Export, SourceGenerated.ResString.GetMultilingualString("75edc082-6550-4120-acad-e6b2153c2dd4", "Exports"));
					AddPair(Constants.ContainerPackingMode.All, SourceGenerated.ResString.GetMultilingualString("a2a094c3-d3b0-472a-a2ea-71d5f08fe942", "All"));
					break;

				#endregion

				#region Rounding Rules

				case OLookUpEditType.RoundingRules:
					AddPair(Constants.RoundingRules.Codes.None, Constants.RoundingRules.Descriptions.None);
					AddPair(Constants.RoundingRules.Codes.JapanYen, Constants.RoundingRules.Descriptions.JapanYen);
					AddPair(Constants.RoundingRules.Codes.JapanYenWithCharge, Constants.RoundingRules.Descriptions.JapanYenWithCharge);
					break;

				#endregion

				#region GL Account Types

				case OLookUpEditType.GLAccountType:
					AddPair(AccountTypeComboBoxConstants.BalanceSheetAccount, SourceGenerated.ResString.GetMultilingualString("Accounting|GLAccountDescriptorType|BalanceSheetAccount", "Balance Sheet Account"));
					AddPair(AccountTypeComboBoxConstants.ProfitAndLossAccount, SourceGenerated.ResString.GetMultilingualString("Accounting|GLAccountDescriptorType|ProfitAndLossAccount", "Profit & Loss Account"));
					AddPair(AccountTypeComboBoxConstants.Total, SourceGenerated.ResString.GetMultilingualString("Accounting|GLAccountDescriptorType|Total", "Total"));
					AddPair(AccountTypeComboBoxConstants.Header, SourceGenerated.ResString.GetMultilingualString("Accounting|GLAccountDescriptorType|Header", "Header"));
					AddPair(AccountTypeComboBoxConstants.OpeningBalance, SourceGenerated.ResString.GetMultilingualString("ecc5b790-8556-4297-a76b-2e6048cb2a01", "Opening Balance"));
					AddPair(AccountTypeComboBoxConstants.ClosingBalance, SourceGenerated.ResString.GetMultilingualString("38bc6960-b2fe-4545-86f6-8b643b9b837c", "Closing Balance"));
					AddPair(AccountTypeComboBoxConstants.Consolidation, SourceGenerated.ResString.GetMultilingualString("Accounting|GLAccountDescriptorType|Consolidation", "Consolidation"));
					AddPair(AccountTypeComboBoxConstants.Alternate, SourceGenerated.ResString.GetMultilingualString("Accounting|GLAccountDescriptorType|Alternate", "Alternate"));
					AddPair(AccountTypeComboBoxConstants.Note, SourceGenerated.ResString.GetMultilingualString("Accounting|GLAccountDescriptorType|Note", "Note"));
					break;

				#endregion

				#region GL Account Descriptor Types

				case OLookUpEditType.GLAccountDescriptorType:
					AddPair(AccountTypeComboBoxConstants.BalanceSheetAccount, SourceGenerated.ResString.GetMultilingualString("Accounting|GLAccountDescriptorType|BalanceSheetAccount", "Balance Sheet Account"));
					AddPair(AccountTypeComboBoxConstants.ProfitAndLossAccount, SourceGenerated.ResString.GetMultilingualString("Accounting|GLAccountDescriptorType|ProfitAndLossAccount", "Profit & Loss Account"));
					AddPair(AccountTypeComboBoxConstants.Total, SourceGenerated.ResString.GetMultilingualString("Accounting|GLAccountDescriptorType|Total", "Total"));
					AddPair(AccountTypeComboBoxConstants.Header, SourceGenerated.ResString.GetMultilingualString("Accounting|GLAccountDescriptorType|Header", "Header"));
					AddPair(AccountTypeComboBoxConstants.Consolidation, SourceGenerated.ResString.GetMultilingualString("Accounting|GLAccountDescriptorType|Consolidation", "Consolidation"));
					AddPair(AccountTypeComboBoxConstants.Alternate, SourceGenerated.ResString.GetMultilingualString("Accounting|GLAccountDescriptorType|Alternate", "Alternate"));
					AddPair(AccountTypeComboBoxConstants.CarriedForwardAccount, SourceGenerated.ResString.GetMultilingualString("Accounting|GLAccountDescriptorType|CarriedForwardAccount", "Carried Forward Account"));
					AddPair(AccountTypeComboBoxConstants.Note, SourceGenerated.ResString.GetMultilingualString("Accounting|GLAccountDescriptorType|Note", "Note"));
					break;

				#endregion

				#region GL Journal Types

				case OLookUpEditType.GLJournalTypes:
					AddPair(TransactionTypes.GLAutoJournal, SourceGenerated.ResString.GetMultilingualString("Accounting|GLJournalTypes|GLAutoJournal", "Auto Journal"));
					AddPair(TransactionTypes.GLReversingJournal, SourceGenerated.ResString.GetMultilingualString("Accounting|GLJournalTypes|GLReversingJournal", "Reversing Journal"));
					AddPair(TransactionTypes.GLStandardJournal, SourceGenerated.ResString.GetMultilingualString("Accounting|GLJournalTypes|GLStandardJournal", "General Journal"));
					AddPair(TransactionTypes.GLNoteJournal, SourceGenerated.ResString.GetMultilingualString("Accounting|GLJournalTypes|NoteJournal", "Note Journal"));
					break;

				#endregion

				#region Job Costing Types

				case OLookUpEditType.JobCostingTransactionTypes:
					AddPair(TransactionTypes.Journal, SourceGenerated.ResString.GetMultilingualString("166301ad-428c-4415-b6de-438504fbaa4c", "Journal"));
					AddPair(TransactionTypes.JobRevenueJournal, SourceGenerated.ResString.GetMultilingualString("4AA81839-1E54-4F2C-AA81-839F9BBAD584", "Job Revenue Journal"));
					break;

				#endregion

				#region Domestic Payment Terms

				case OLookUpEditType.DomesticPaymentTerms:
					AddPair(Constants.DomesticPaymentTerms.Prepaid, SourceGenerated.ResString.GetMultilingualString("7bc12dfa-1652-4a29-a942-17513e02c691", "Prepaid"));
					AddPair(Constants.DomesticPaymentTerms.Collect, SourceGenerated.ResString.GetMultilingualString("a3a4740b-526d-4ff6-ad00-73d4f99ecbe6", "Collect"));
					AddPair(Constants.DomesticPaymentTerms.CollectThirdParty, SourceGenerated.ResString.GetMultilingualString("fd0d4bf4-538b-4555-be92-b4a0efc60ceb", "Collect 3rd Party"));
					AddPair(Constants.DomesticPaymentTerms.CollectCOD, SourceGenerated.ResString.GetMultilingualString("9ca2def0-f66d-4444-ad3f-c6388b622b80", "Collect COD"));
					break;

				#endregion

				#region Invoice Printing Transaction Types

				case OLookUpEditType.InvoicePrintingTransactionTypes:
					AddPair(TransactionTypes.Invoice, SourceGenerated.ResString.GetMultilingualString("Accounting|InvoicePrintingTransactionTypes|Invoice", "Invoice"));
					AddPair(TransactionTypes.CreditNote, SourceGenerated.ResString.GetMultilingualString("Accounting|InvoicePrintingTransactionTypes|CreditNote", "Credit Note"));
					AddPair(TransactionTypes.AdjustmentNote, SourceGenerated.ResString.GetMultilingualString("Accounting|InvoicePrintingTransactionTypes|AdjustmentNote", "Adjustment Note"));
					break;

				#endregion

				#region Job Required Document Periods

				case OLookUpEditType.JobRequiredDocumentPeriods:
					AddPair(Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, Constants.JobRequiredDocuments.DocumentPeriodDescriptions.OncePerShipment);
					AddPair(Constants.JobRequiredDocuments.DocumentPeriods.Periodic, Constants.JobRequiredDocuments.DocumentPeriodDescriptions.Periodic);
					break;

				#endregion

				#region Languages / Languages for GL Mapping

				case OLookUpEditType.GLLanguage:
				case OLookUpEditType.Language:
					AddLanguages();
					if (LookupEditType == OLookUpEditType.GLLanguage)
					{
						AddPair(Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, SourceGenerated.ResString.GetMultilingualString("Common|Languages|ZZZ_ExternalLinkToGeneralLedger", "External Link to General Ledger"));
					}
					break;

				#endregion

				#region Months

				case OLookUpEditType.Months:
					AddPair("JAN", SourceGenerated.ResString.GetMultilingualString("920b9d58-e12c-472e-a47e-657aac31ce3d", "January"));
					AddPair("FEB", SourceGenerated.ResString.GetMultilingualString("10a1a88c-8d39-411b-9aee-fd30e6f8bb76", "February"));
					AddPair("MAR", SourceGenerated.ResString.GetMultilingualString("94965994-f1cb-4dcd-a0aa-3e82f9a71996", "March"));
					AddPair("APR", SourceGenerated.ResString.GetMultilingualString("49b3e9cf-d3e9-4d1c-ac7d-53ab745b2313", "April"));
					AddPair("MAY", SourceGenerated.ResString.GetMultilingualString("f953a857-3538-42a7-915c-5844a964b41d", "May"));
					AddPair("JUN", SourceGenerated.ResString.GetMultilingualString("6cc95742-bdfe-4ecb-8300-dedab9fa6cf2", "June"));
					AddPair("JUL", SourceGenerated.ResString.GetMultilingualString("8cf66450-9e91-43c5-b27f-7aacfa2b7b10", "July"));
					AddPair("AUG", SourceGenerated.ResString.GetMultilingualString("72cd6279-7356-40a5-972c-bb55c9efe324", "August"));
					AddPair("SEP", SourceGenerated.ResString.GetMultilingualString("0b08a3e1-d813-40c9-87cf-313972732065", "September"));
					AddPair("OCT", SourceGenerated.ResString.GetMultilingualString("ba5115f2-3696-451c-9e0b-2c491ee1cd79", "October"));
					AddPair("NOV", SourceGenerated.ResString.GetMultilingualString("c797f7af-e433-4158-bdb5-9c436a2f6e4c", "November"));
					AddPair("DEC", SourceGenerated.ResString.GetMultilingualString("0a4300f1-e17c-478f-b360-fdfd3eedbf6e", "December"));
					break;

				#endregion

				#region Nationality Types

				case OLookUpEditType.NationalityType:
					AddPair("M", SourceGenerated.ResString.GetMultilingualString("de788746-ca56-49f5-9f19-9c1ff95bd1e9", "Malaysian"));
					AddPair("P", SourceGenerated.ResString.GetMultilingualString("d2aeac12-eb38-453f-add6-8d9601a23672", "Passport"));
					AddPair("S", SourceGenerated.ResString.GetMultilingualString("f6995666-6e46-4a76-9a17-97cc909b9214", "Singaporean"));
					AddPair("T", SourceGenerated.ResString.GetMultilingualString("f6995666-6e46-4a76-9a17-97cc909b9214", "Singaporean"));
					break;

				#endregion

				#region Notify Mode

				case OLookUpEditType.NotifyMode:
					AddPair(Constants.ContactNotifyModes.Email, Constants.ContactNotifyModeDescriptions.Email);
					AddPair(Constants.ContactNotifyModes.Fax, Constants.ContactNotifyModeDescriptions.Fax);
					AddPair(Constants.ContactNotifyModes.Print, Constants.ContactNotifyModeDescriptions.Print);
					AddPair(Constants.ContactNotifyModes.EPrint, Constants.ContactNotifyModeDescriptions.EPrint);
					break;

				#endregion

				#region Emails

				case OLookUpEditType.EmailTo:
					AddPair(Constants.EmailTo.NoEmails, SourceGenerated.ResString.GetMultilingualString("5797bbf3-3053-4463-8592-e323984a2ed3", "No Emails"));
					AddPair(Constants.EmailTo.StaffMember, SourceGenerated.ResString.GetMultilingualString("c0d563a7-d935-43ce-8ca2-286f39cbbb5c", "Email Staff Member"));
					AddPair(Constants.EmailTo.NominatedGroup, SourceGenerated.ResString.GetMultilingualString("39e4ca70-65b9-418b-a167-bbf7ff435b4e", "Email Nominated Group"));
					AddPair(Constants.EmailTo.StaffMemberAndNominatedGroup, SourceGenerated.ResString.GetMultilingualString("bdae382c-4fa1-40d9-8b99-c769169ab78a", "Email Staff Member and Nominated Group"));
					break;

				case OLookUpEditType.EmailToNotifiedGroup:
					AddPair(Constants.EmailTo.NoEmails, SourceGenerated.ResString.GetMultilingualString("5797bbf3-3053-4463-8592-e323984a2ed3", "No Emails"));
					AddPair(Constants.EmailTo.NominatedGroup, SourceGenerated.ResString.GetMultilingualString("39e4ca70-65b9-418b-a167-bbf7ff435b4e", "Email Nominated Group"));
					break;

				#endregion

				#region Numbers 1-10

				case OLookUpEditType.Numbers1To10:
					AddPair("1", SourceGenerated.ResString.GetMultilingualString("f6c77e81-74d3-474e-b111-a2af1b7cf471", "One"));
					AddPair("2", SourceGenerated.ResString.GetMultilingualString("5b7090db-d05e-46a2-8e28-72bb48d8b25b", "Two"));
					AddPair("3", SourceGenerated.ResString.GetMultilingualString("fe4ef9de-1c65-47d8-82bb-734829a4cfc3", "Three"));
					AddPair("4", SourceGenerated.ResString.GetMultilingualString("e31cae60-f62e-465e-9606-75c4c110b7b4", "Four"));
					AddPair("5", SourceGenerated.ResString.GetMultilingualString("eb3d168a-60c3-4601-a644-74b1b1adb388", "Five"));
					AddPair("6", SourceGenerated.ResString.GetMultilingualString("7668a0e5-02e7-4dbe-b3de-3308067a8fe7", "Six"));
					AddPair("7", SourceGenerated.ResString.GetMultilingualString("45e9c426-bf73-44d4-a59a-01a5930b1cb2", "Seven"));
					AddPair("8", SourceGenerated.ResString.GetMultilingualString("9c4247d2-b4d1-4704-8fe6-2597dd37e4e3", "Eight"));
					AddPair("9", SourceGenerated.ResString.GetMultilingualString("68c5e94a-485c-43cd-86db-b58d92ca39a9", "Nine"));
					AddPair("10", SourceGenerated.ResString.GetMultilingualString("a9e1d89a-dc2a-4166-a18b-63a1a499c51a", "Ten"));
					break;

				#endregion

				#region Gender

				case OLookUpEditType.Gender:
					AddPair(Constants.Genders.Woman, Constants.GenderDescriptions.Woman);
					AddPair(Constants.Genders.Man, Constants.GenderDescriptions.Man);
					break;

				#endregion

				#region NZ Processing Port

				case OLookUpEditType.NZProcessingPort:
					AddPair("NZAKL", SourceGenerated.ResString.GetMultilingualString("bdc9e4ad-9ab1-4975-bcc3-32eaf89af7d3", "Auckland"));
					AddPair("NZCHC", SourceGenerated.ResString.GetMultilingualString("33dcc273-5036-4bec-aac6-774fc69e935a", "Christchurch/Lyttelton"));
					AddPair("NZDUD", SourceGenerated.ResString.GetMultilingualString("622e69f7-8fd6-499a-93f2-e1521c8322dc", "Dunedin/Port Chalmers"));
					AddPair("NZIVC", SourceGenerated.ResString.GetMultilingualString("47d0930d-374f-412f-bc27-cbf302562ec9", "Invercargill"));
					AddPair("NZNSN", SourceGenerated.ResString.GetMultilingualString("4a0336c2-d8aa-41e7-8d63-c0996a727f81", "Nelson"));
					AddPair("NZNPL", SourceGenerated.ResString.GetMultilingualString("246168af-3e87-4c31-b5d6-a2700f40834f", "New Plymouth/Port Taranaki"));
					AddPair("NZTRG", SourceGenerated.ResString.GetMultilingualString("7262dea2-1204-499b-8880-df7655c3588e", "Tauranga"));
					AddPair("NZNPE", SourceGenerated.ResString.GetMultilingualString("d42c4b06-88f4-4de2-a247-d4e28e17280e", "Napier"));
					AddPair("NZWLG", SourceGenerated.ResString.GetMultilingualString("dd59c22d-15a4-41ce-b032-f82c66f2aed7", "Wellington"));
					break;

				#endregion

				#region Order Header Status

				case OLookUpEditType.OrderHeaderStatus:
					Clear();
					AddPair(Constants.OrderStatus.Incomplete, SourceGenerated.ResString.GetMultilingualString("28923ade-307b-4a69-bfa7-9cf1634cde3e", "Incomplete"));
					AddPair(Constants.OrderStatus.Open, SourceGenerated.ResString.GetMultilingualString("19ac8d84-e902-4703-a2c6-e3a7adb9aba2", "Placed"));
					AddPair(Constants.OrderStatus.Confirmed, SourceGenerated.ResString.GetMultilingualString("eb913903-81e5-4785-b87b-ed5037c0ed55", "Confirmed"));
					AddPair(Constants.OrderStatus.Shipped, SourceGenerated.ResString.GetMultilingualString("989fc8ef-e7ea-4ba3-a6f7-8bd8f6a8ef0b", "Shipped"));
					AddPair(Constants.OrderStatus.PartDelivered, SourceGenerated.ResString.GetMultilingualString("88737e1c-790a-41af-bc25-1d3b0bb0bb0e", "Part Delivered"));
					AddPair(Constants.OrderStatus.Delivered, SourceGenerated.ResString.GetMultilingualString("2ebfc95c-85ba-4442-ab4d-6ee45d8ff503", "Delivered"));
					AddPair(Constants.OrderStatus.Cancelled, SourceGenerated.ResString.GetMultilingualString("cec88fac-156b-41be-998a-e10dd093b122", "Canceled"));
					break;

				#endregion

				#region Order Line Status

				case OLookUpEditType.OrderLineStatus:
					Clear();
					AddPair(Constants.OrderStatus.Open, SourceGenerated.ResString.GetMultilingualString("19ac8d84-e902-4703-a2c6-e3a7adb9aba2", "Placed"));
					AddPair(Constants.OrderStatus.PartDelivered, SourceGenerated.ResString.GetMultilingualString("88737e1c-790a-41af-bc25-1d3b0bb0bb0e", "Part Delivered"));
					AddPair(Constants.OrderStatus.Delivered, SourceGenerated.ResString.GetMultilingualString("2ebfc95c-85ba-4442-ab4d-6ee45d8ff503", "Delivered"));
					AddPair(Constants.OrderStatus.Cancelled, SourceGenerated.ResString.GetMultilingualString("cec88fac-156b-41be-998a-e10dd093b122", "Canceled"));
					AddPair(Constants.OrderStatus.Incomplete, SourceGenerated.ResString.GetMultilingualString("034493d0-9186-4748-86c2-45c9942dce2f", "Incomplete"));
					break;

				#endregion

				#region Org Header Category

				case OLookUpEditType.OrgHeaderCategory:
					Clear();
					AddPair(OrgConstants.Category.Business, OrgDescriptions.Category.Business);
					AddPair(OrgConstants.Category.Government, OrgDescriptions.Category.Government);
					AddPair(OrgConstants.Category.NaturalPersonIndividual, OrgDescriptions.Category.NaturalPersonIndividual);
					AddPair(OrgConstants.Category.NonGovernmentOrganisation, OrgDescriptions.Category.NonGovernmentOrganisation);
					break;

				#endregion

				#region Payable Order Stage

				case OLookUpEditType.PayableOrderStage:
					Clear();
					AddPair(Constants.PayableOrderStage.Request, SourceGenerated.ResString.GetMultilingualString("f496ee70-9621-4884-b5ef-a077651d73bb", "Request"));
					AddPair(Constants.PayableOrderStage.Order, SourceGenerated.ResString.GetMultilingualString("34a9561b-b465-4b18-a0c4-9a1567935878", "Order"));
					AddPair(Constants.PayableOrderStage.Track, SourceGenerated.ResString.GetMultilingualString("d869f760-b535-488a-9d98-fc1a99b979aa", "Track"));
					AddPair(Constants.PayableOrderStage.Receive, SourceGenerated.ResString.GetMultilingualString("7d1dc2ad-5928-4619-9792-f53d86e49ac3", "Receive"));
					break;

				#endregion

				#region Payable Order Disposition

				case OLookUpEditType.PayableOrderDisposition:
					Clear();
					AddPair(Constants.PayableOrderDisposition.OrderIncomplete, SourceGenerated.ResString.GetMultilingualString("686294b6-524d-4061-abe1-f0b21256ea54", "Order Incomplete"));
					AddPair(Constants.PayableOrderDisposition.PendingApproval, SourceGenerated.ResString.GetMultilingualString("2888509c-de91-4a43-809c-f52d78aca9ca", "Pending Approval"));
					AddPair(Constants.PayableOrderDisposition.OrderToBePlaced, SourceGenerated.ResString.GetMultilingualString("315963be-2ebc-470b-ba1d-61d077e30171", "Order To Be Placed"));
					AddPair(Constants.PayableOrderDisposition.PendingConfirmation, SourceGenerated.ResString.GetMultilingualString("514f4905-433a-4f6c-87cf-a383face4f5a", "Pending Confirmation"));
					AddPair(Constants.PayableOrderDisposition.ExpectedDLVPending, SourceGenerated.ResString.GetMultilingualString("89ed6be2-e7c1-418d-969d-5c8b4c8003dc", "Pending Departure/Arrival Estimate"));
					AddPair(Constants.PayableOrderDisposition.DeliveryInProgress, SourceGenerated.ResString.GetMultilingualString("2b068b3c-a42d-4742-849d-d5c645d74530", "Delivery In Progress"));
					AddPair(Constants.PayableOrderDisposition.APInvoiceToBePosted, SourceGenerated.ResString.GetMultilingualString("3b64e8c0-d838-40b1-b330-e753a8a359cd", "AP Invoice To Be Posted"));
					AddPair(Constants.PayableOrderDisposition.PendingGoodsReceivedAudit, SourceGenerated.ResString.GetMultilingualString("ccf713f8-2aa9-49a1-9f9b-da2529011ae3", "Pending Goods Received Audit"));
					AddPair(Constants.PayableOrderDisposition.Complete, SourceGenerated.ResString.GetMultilingualString("385c03e9-9167-49af-88df-4feb1c75e270", "Complete"));
					break;

				#endregion

				#region Payable Order Type

				case OLookUpEditType.PayableOrderType:
					Clear();
					AddPair(Constants.PayableOrderType.VariableOverhead, SourceGenerated.ResString.GetMultilingualString("0085522e-de7e-4c62-a7d4-8ad2ae27d017", "Variable Overhead"));
					AddPair(Constants.PayableOrderType.CapitalExpense, SourceGenerated.ResString.GetMultilingualString("24b804cc-2761-42d1-b55d-6b50c22a6a86", "Capital Expense"));
					AddPair(Constants.PayableOrderType.IndirectCostOfSale, SourceGenerated.ResString.GetMultilingualString("cc1a97cf-1ad2-4192-8f3d-905b96d6b045", "Indirect Cost Of Sale"));
					AddPair(Constants.PayableOrderType.BulkPurchase, SourceGenerated.ResString.GetMultilingualString("90b622d7-19ee-4f6e-8056-c12be4ff4ea0", "Bulk Purchase"));
					break;

				#endregion

				#region Payable Order Goods Received Status

				case OLookUpEditType.PayableOrderGoodsStatus:
					Clear();
					AddPair(Constants.PayableOrderGoodsStatus.NotReceived, SourceGenerated.ResString.GetMultilingualString("fb3834cb-3f76-449a-9f06-0b90df593dd0", "Not Received"));
					AddPair(Constants.PayableOrderGoodsStatus.FullyReceived, SourceGenerated.ResString.GetMultilingualString("19158e2f-901b-492c-983c-adee1dd8da7f", "Fully Received"));
					AddPair(Constants.PayableOrderGoodsStatus.PartiallyReceived, SourceGenerated.ResString.GetMultilingualString("eee725a1-2e3b-456e-bb78-75602618a68b", "Partially Received"));
					AddPair(Constants.PayableOrderGoodsStatus.OverSupplied, SourceGenerated.ResString.GetMultilingualString("9dacff41-ee94-4639-8674-b57c7947291c", "Over Supplied"));
					AddPair(Constants.PayableOrderGoodsStatus.OverInvoiced, SourceGenerated.ResString.GetMultilingualString("f131be1e-573e-4798-98ca-e8af432ec9a2", "Over Invoiced"));
					AddPair(Constants.PayableOrderGoodsStatus.UnderInvoiced, SourceGenerated.ResString.GetMultilingualString("de7da8fa-1d4d-48be-9220-fb567848852f", "Under Invoiced"));
					break;

				#endregion

				#region Payable Order Line Status

				case OLookUpEditType.PayableOrderLineStatus:
					Clear();
					AddPair(Constants.PayableOrderLineStatus.Placed, SourceGenerated.ResString.GetMultilingualString("d1b4d696-8104-4b97-8356-5e7771683c75", "Placed"));
					AddPair(Constants.PayableOrderLineStatus.PartDelivered, SourceGenerated.ResString.GetMultilingualString("c395c233-f9f5-42ba-89fd-291bcb3325e7", "Part Delivered"));
					AddPair(Constants.PayableOrderLineStatus.Delivered, SourceGenerated.ResString.GetMultilingualString("b2b3921c-c016-4bb3-b3cf-cffa346b95c7", "Delivered"));
					AddPair(Constants.PayableOrderLineStatus.Cancelled, SourceGenerated.ResString.GetMultilingualString("fb2e7ed7-e6a0-44bd-9b0c-697b18a08059", "Cancel"));
					break;

				#endregion

				#region Payment Method

				case OLookUpEditType.PaymentMethod:
					AddPair(ReceiptTypes.Cheque, SourceGenerated.ResString.GetMultilingualString("41267ae5-0e21-449f-83d2-d02e1b76ed75", "Check"));
					AddPair(ReceiptTypes.Cash, SourceGenerated.ResString.GetMultilingualString("2e9fd229-b254-46a9-985a-8ef785379371", "Cash"));
					AddPair(ReceiptTypes.CreditCard, SourceGenerated.ResString.GetMultilingualString("74e24123-fca1-47b4-812c-5c5001ddf2a3", "Credit card"));
					AddPair(ReceiptTypes.DirectDebit, SourceGenerated.ResString.GetMultilingualString("80b58564-5f82-4b79-96aa-c78a2350b722", "Direct debit"));
					AddPair(ReceiptTypes.EFT, SourceGenerated.ResString.GetMultilingualString("34887b95-f49a-4547-b83a-af8bfe3b7d0b", "Electronic Funds Transfer"));
					AddPair(ReceiptTypes.ScheduledEFT, SourceGenerated.ResString.GetMultilingualString("0B0E73CA-1400-4595-AED2-B73998441C41", "Scheduled EFT"));
					AddPair(ReceiptTypes.CollectionRequest, SourceGenerated.ResString.GetMultilingualString("20D5002C-18FF-4FA1-B1AE-A233895DB5F2", "Collection Request"));
					AddPair(ReceiptTypes.eNettDirectDebit, SourceGenerated.ResString.GetMultilingualString("b1d84889-b144-4522-9cf0-3e2f48a425db", "Pay via ComPay Direct Debit"));
					AddPair(ReceiptTypes.eNettCreditCard, SourceGenerated.ResString.GetMultilingualString("b4eebfdc-6283-45f4-a812-7b460ea41407", "Pay via ComPay Credit Card"));
					if (ObjectFactory.Get<IAccounting>().IsEPaymentFunctionalityEnabledForAnyProvider(EnvProxy.Instance.CurrentCompany.PK))
					{
						AddPair(ReceiptTypes.EPayment, SourceGenerated.ResString.GetMultilingualString("10446fa9-e325-4d9e-b728-5f62cb7b3eec", "E-Payment"));
					}
					AddRange(new CodeDescriptionPairList(OLookUpEditType.BankChargeTypes));
					break;

				#endregion

				#region Cheque Transaction Header

				case OLookUpEditType.ChequeTransactionHeader:
					AddPair(ChequeTransactionTypes.ChequeEntryTransaction, SourceGenerated.ResString.GetMultilingualString("734C3FEB-B94A-4879-BAFC-6D325AC4D298", "Cheque Entry Transaction"));
					AddPair(ChequeTransactionTypes.ChequeOutToCreditor, SourceGenerated.ResString.GetMultilingualString("9E6A0CAD-9E4F-4F77-AD5B-8D1CAF7014EA", "Cheque Out To Creditor"));
					AddPair(ChequeTransactionTypes.ChequeOutToBankForCollection, SourceGenerated.ResString.GetMultilingualString("877F3FB5-C4D9-4D0A-A4C6-FB7C821737FA", "Cheque Out To Bank For Collection"));
					AddPair(ChequeTransactionTypes.ChequeOutToBankAsGuarantee, SourceGenerated.ResString.GetMultilingualString("4BA87781-2443-40EB-A274-597EE83B4BE6", "Cheque Out To Bank As Guarantee"));
					AddPair(ChequeTransactionTypes.ChequeCollectedAtBank, SourceGenerated.ResString.GetMultilingualString("1B9520DF-DC50-4FAF-82E5-90621EA81DD8", "Cheque Collected At Bank"));
					AddPair(ChequeTransactionTypes.BadChequeAtBank, SourceGenerated.ResString.GetMultilingualString("6936ABAB-23AF-42F4-8B7E-2257D8963CF8", "Bad Cheque At Bank"));
					AddPair(ChequeTransactionTypes.ChequeReturnFromBank, SourceGenerated.ResString.GetMultilingualString("5A5D238F-DFEF-4F3A-8121-3FC626E60400", "Cheque Return From Bank"));
					AddPair(ChequeTransactionTypes.ChequeReturnToTheDebtor, SourceGenerated.ResString.GetMultilingualString("B9BA0BAD-522B-4CB1-8CB2-A3BFCDDE5309", "Cheque Return To The Debtor"));
					AddPair(ChequeTransactionTypes.ChequeCollectedInPortfolio, SourceGenerated.ResString.GetMultilingualString("9E1F000B-9C2A-4CC0-9B77-85E5E90C3DB6", "Cheque Collected In Portfolio"));
					AddPair(ChequeTransactionTypes.BadChequeInPortfolio, SourceGenerated.ResString.GetMultilingualString("49347708-0105-4E1A-B3A3-F4A209652EFD", "Bad Cheque In Portfolio"));
					AddPair(ChequeTransactionTypes.CollectionOfEndorsedCheque, SourceGenerated.ResString.GetMultilingualString("E0854CF5-DEE0-4A25-9A98-A1BF5D782997", "Collection Of Endorsed Cheque"));
					AddPair(ChequeTransactionTypes.ChequeReturnFromCreditor, SourceGenerated.ResString.GetMultilingualString("0F7452E0-BE8C-4471-AB54-C48C824183EB", "Cheque Return From Creditor"));
					AddPair(ChequeTransactionTypes.ReturnedBadChequeFromCreditor, SourceGenerated.ResString.GetMultilingualString("DCB2B4DB-EC31-4005-B2D7-091726B24D53", "Returned Bad Cheque From Creditor"));
					AddPair(ChequeTransactionTypes.UncollectibleCheques, SourceGenerated.ResString.GetMultilingualString("25817C0C-BB60-4AB7-9BA0-3512B6803EB8", "Un-collectible Cheques"));
					AddPair(ChequeTransactionTypes.ChequeInJudicialProcess, SourceGenerated.ResString.GetMultilingualString("F0077127-0E63-4A2F-8EA4-ADB79645CE75", "Cheque In Judicial Process"));
					break;

				#endregion

				#region Payment / Receipt Method

				case OLookUpEditType.PaymentOrReceiptMethod:
					AddPair(ReceiptTypes.Cheque, SourceGenerated.ResString.GetMultilingualString("41267ae5-0e21-449f-83d2-d02e1b76ed75", "Check"));
					AddPair(ReceiptTypes.Cash, SourceGenerated.ResString.GetMultilingualString("2e9fd229-b254-46a9-985a-8ef785379371", "Cash"));
					AddPair(ReceiptTypes.CreditCard, SourceGenerated.ResString.GetMultilingualString("74e24123-fca1-47b4-812c-5c5001ddf2a3", "Credit card"));
					AddPair(ReceiptTypes.DirectCredit + "/" + ReceiptTypes.DirectDebit, SourceGenerated.ResString.GetMultilingualString("58e9bd22-3098-4e80-9e94-53e39721d733", "Direct credit/Direct debit"));
					AddPair(ReceiptTypes.EFT, SourceGenerated.ResString.GetMultilingualString("34887b95-f49a-4547-b83a-af8bfe3b7d0b", "Electronic Funds Transfer"));
					AddPair(ReceiptTypes.ScheduledEFT, SourceGenerated.ResString.GetMultilingualString("0B0E73CA-1400-4595-AED2-B73998441C41", "Scheduled EFT"));
					AddPair(ReceiptTypes.CollectionRequest, SourceGenerated.ResString.GetMultilingualString("20D5002C-18FF-4FA1-B1AE-A233895DB5F2", "Collection Request"));
					AddPair(ReceiptTypes.eNettDirectCredit, SourceGenerated.ResString.GetMultilingualString("68519f6e-fb7e-4c06-84a8-6ad545258019", "ComPay Direct Credit"));
					AddPair(ReceiptTypes.eNettDirectDebit, SourceGenerated.ResString.GetMultilingualString("b1d84889-b144-4522-9cf0-3e2f48a425db", "Pay via ComPay Direct Debit"));
					AddPair(ReceiptTypes.eNettCreditCard, SourceGenerated.ResString.GetMultilingualString("b4eebfdc-6283-45f4-a812-7b460ea41407", "Pay via ComPay Credit Card"));
					AddRange(new CodeDescriptionPairList(OLookUpEditType.BankChargeTypes));
					break;

				#endregion

				#region Payment Status

				case OLookUpEditType.PaymentStatus:
					AddPair(Constants.PaymentStatusTypes.AllTransactions, Constants.PaymentStatusTypes.AllTransactions);
					AddPair(Constants.PaymentStatusTypes.UnpaidTransactions, Constants.PaymentStatusTypes.UnpaidTransactions);
					break;

				#endregion

				#region Payment Type

				case OLookUpEditType.PaymentType:
					AddPair(Constants.PaymentType.Prepaid, SourceGenerated.ResString.GetMultilingualString("7bc12dfa-1652-4a29-a942-17513e02c691", "Prepaid"));
					AddPair(Constants.PaymentType.Collect, SourceGenerated.ResString.GetMultilingualString("a3a4740b-526d-4ff6-ad00-73d4f99ecbe6", "Collect"));
					break;

				#endregion

				#region AutoPrint Cheque Book Type

				case OLookUpEditType.AutoPrintTypes:
					AddPair(Constants.AutoPrintTypes.AutoPrint, SourceGenerated.ResString.GetMultilingualString("29e4fcd0-83af-4981-99dc-5b8b4a6edb2e", "Auto-Print Check Books"));
					AddPair(Constants.AutoPrintTypes.Manual, SourceGenerated.ResString.GetMultilingualString("7713eb82-e2fb-4891-af79-e32761dabd91", "Manual Check Books"));
					break;

				#endregion

				#region Print Copy Type

				case OLookUpEditType.PrintCopyType:
					AddPrintCopyTypes();
					break;

				#endregion

				#region Rating Calculators

				case OLookUpEditType.RateCalculators:
					AddPair("AGY", SourceGenerated.ResString.GetMultilingualString("67cc3015-dbab-4062-9551-4a6647171201", "Agency Calculator"));
					AddPair("CMB", SourceGenerated.ResString.GetMultilingualString("7d290329-4992-483b-95e3-b5aae938d964", "Sliding or Per Unit with Base, Minimum, Maximum Calculator"));
					AddPair("CST", SourceGenerated.ResString.GetMultilingualString("7c02a8a8-9a9d-4a36-9365-e7a84fa316d2", "Cost Based Calculator"));
					AddPair("CTB", SourceGenerated.ResString.GetMultilingualString("205eaf65-a2a6-43d1-85ec-bdf1d55d2769", "Company Tariff Based Calculator"));
					AddPair("CTG", SourceGenerated.ResString.GetMultilingualString("f246c828-ab70-41b1-aa57-766bd071e033", "Transport Calculator"));
					AddPair("CTZ", SourceGenerated.ResString.GetMultilingualString("bb9d03f7-8e69-4653-9f17-e87d112de042", "Transport Zone Distance Based Calculator"));
					AddPair("EXL", SourceGenerated.ResString.GetMultilingualString("8048df42-bb67-4c96-b4ef-10552efee63c", "Exclude from Company Tariffs Calculator"));
					AddPair("FPA", SourceGenerated.ResString.GetMultilingualString("32de6995-11ad-4058-80eb-2f2f8fb493e6", "First plus Additional Calculator"));
					AddPair("FLT", SourceGenerated.ResString.GetMultilingualString("c522ff84-6af6-4e9f-ac1b-1a4277592e4c", "Flat Calculator"));
					AddPair("FPU", SourceGenerated.ResString.GetMultilingualString("42d27ee0-354d-4f6d-8cfd-08a8f80e710e", "Flat plus Per Unit Calculator"));
					AddPair("HCC", SourceGenerated.ResString.GetMultilingualString("b259f261-d96f-4c4f-87c4-400469bf925d", "Highest Charge Calculator"));
					AddPair("HRC", SourceGenerated.ResString.GetMultilingualString("73e00111-f720-44f1-8451-ebfd8dfdf98e", "Highest Rate Calculator"));
					AddPair("IAT", SourceGenerated.ResString.GetMultilingualString("b48cd334-3f67-4671-a2e6-7f8493bb4d9b", "Package Count Calculator"));
					AddPair("FRT", SourceGenerated.ResString.GetMultilingualString("a280f27d-1e9b-4ded-99bd-fe1e09c55292", "Freight Inclusive Calculator"));
					AddPair("IXC", SourceGenerated.ResString.GetMultilingualString("8be663ce-43a1-4ca1-b52a-ef310fce0604", "Value Range Calculator"));
					AddPair("MIN", SourceGenerated.ResString.GetMultilingualString("83213ee1-3302-41d1-bbbe-c2509c40832a", "Minimum Calculator"));
					AddPair("MPU", SourceGenerated.ResString.GetMultilingualString("1d3a6c2a-fe54-42ac-b846-58e822316928", "Minimum Or Per Unit Calculator"));
					AddPair("NTE", SourceGenerated.ResString.GetMultilingualString("3b9db8c2-56da-42b2-b404-7d51d4ea9391", "Free-Text Note Calculator"));
					AddPair("HRT", SourceGenerated.ResString.GetMultilingualString("1792e6fa-55c1-479b-b2fd-bb711a95f419", "House bill Release Type Calculator"));
					AddPair("PER", SourceGenerated.ResString.GetMultilingualString("1858b71c-caa6-4644-93d4-aef6898c3a01", "Percentage Calculator"));
					AddPair("PEB", SourceGenerated.ResString.GetMultilingualString("32ed0911-12ff-4ee9-8ab6-117c82762c2c", "Percentage Break Calculator"));
					AddPair("PSR", SourceGenerated.ResString.GetMultilingualString("ce46c2f2-50d9-4b2b-8821-81cb4e138c0b", "Profit Share/Rebate Calculator"));
					AddPair("SMB", SourceGenerated.ResString.GetMultilingualString("054e401b-a579-4731-a599-8e361a00d42f", "Split Month Calculator"));
					AddPair("CBI", SourceGenerated.ResString.GetMultilingualString("7f5a4c36-d03e-4f3b-9e80-4c9ad7ff145e", "Combined Breaks with Increments Calculator"));
					AddPair("UNT", SourceGenerated.ResString.GetMultilingualString("dee737e8-e146-4d05-a548-9462225193b9", "Unit Calculator"));
					AddPair("TME", SourceGenerated.ResString.GetMultilingualString("772f5cfe-f803-4fd1-bd64-3cd209fe64a1", "Time Calculator"));
					AddPair("DIN", SourceGenerated.ResString.GetMultilingualString("b6cc28cd-dbd9-4f1f-95b5-f095ddf09f8b", "Disbursement Interest Calculator"));
					AddPair("VED", SourceGenerated.ResString.GetMultilingualString("8c0d7843-d874-4a17-9b8d-cac435d6bfc9", "Volume Equalization Discount Calculator"));
					AddPair("WPK", SourceGenerated.ResString.GetMultilingualString("d22c7f2c-a5b5-4440-835b-38a8670e77ac", "Warehouse Pack Type Calculator"));
					AddPair("WLT", SourceGenerated.ResString.GetMultilingualString("ed27a1f5-82ec-4124-aed8-f0ec4cb9425b", "Warehouse Location Type Calculator"));
					break;

				#endregion

				#region Receipt Method

				case OLookUpEditType.ReceiptMethod:
					AddPair(ReceiptTypes.Cheque, SourceGenerated.ResString.GetMultilingualString("41267ae5-0e21-449f-83d2-d02e1b76ed75", "Check"));
					AddPair(ReceiptTypes.Cash, SourceGenerated.ResString.GetMultilingualString("2e9fd229-b254-46a9-985a-8ef785379371", "Cash"));
					AddPair(ReceiptTypes.CreditCard, SourceGenerated.ResString.GetMultilingualString("74e24123-fca1-47b4-812c-5c5001ddf2a3", "Credit card"));
					AddPair(ReceiptTypes.DirectCredit, SourceGenerated.ResString.GetMultilingualString("2a881525-4189-46b9-8fa7-5bc98b380b7e", "Direct credit"));
					AddPair(ReceiptTypes.eNettDirectCredit, SourceGenerated.ResString.GetMultilingualString("68519f6e-fb7e-4c06-84a8-6ad545258019", "ComPay Direct Credit"));
					AddRange(new CodeDescriptionPairList(OLookUpEditType.BankChargeTypes));
					break;

				#endregion

				#region Reference Types

				case OLookUpEditType.ReferenceTypes:
					AddPair(Constants.ReferenceTypes.Accounting, Constants.ReferenceTypeDescriptions.Accounting);
					AddPair(Constants.ReferenceTypes.All, Constants.ReferenceTypeDescriptions.All);
					AddPair(Constants.ReferenceTypes.GeneralReferenceTables, Constants.ReferenceTypeDescriptions.GeneralReferenceTables);
					AddPair(Constants.ReferenceTypes.HumanResourcesStaffEmployment, Constants.ReferenceTypeDescriptions.HumanResourcesStaffEmployment);
					AddPair(Constants.ReferenceTypes.BusinessEntityProcessWorkflow, Constants.ReferenceTypeDescriptions.BusinessEntityProcessWorkflow);
					AddPair(Constants.ReferenceTypes.ComplianceReport, Constants.ReferenceTypeDescriptions.ComplianceReport);

					if (!DataRegistry.Instance.ProductivityWiseModeEnabled)
					{
						AddPair(Constants.ReferenceTypes.ClientSupplierRelationship, Constants.ReferenceTypeDescriptions.ClientSupplierRelationship);
						AddPair(Constants.ReferenceTypes.SupplyChainLogistics, Constants.ReferenceTypeDescriptions.SupplyChainLogistics);
					}

					break;

				#endregion

				#region Ocean Carrier

				case OLookUpEditType.OceanCarrierShipmentTypes:
					AddPair(Constants.OceanCarrierShipmentTypes.Codes.All, Constants.OceanCarrierShipmentTypes.Descriptions.All);
					AddPair(Constants.OceanCarrierShipmentTypes.Codes.BookingRequest, Constants.OceanCarrierShipmentTypes.Descriptions.BookingRequest);
					AddPair(Constants.OceanCarrierShipmentTypes.Codes.CarrierShipment, Constants.OceanCarrierShipmentTypes.Descriptions.CarrierShipment);
					AddPair(Constants.OceanCarrierShipmentTypes.Codes.ShippingInstruction, Constants.OceanCarrierShipmentTypes.Descriptions.ShippingInstruction);
					AddPair(Constants.OceanCarrierShipmentTypes.Codes.BillOfLading, Constants.OceanCarrierShipmentTypes.Descriptions.BillOfLading);
					break;

				case OLookUpEditType.OceanCarrierCargoTypes:
					AddPair(Constants.OceanCarrierCargoTypes.Codes.All, Constants.OceanCarrierCargoTypes.Descriptions.All);
					AddPair(Constants.OceanCarrierCargoTypes.Codes.BreakBulk, Constants.OceanCarrierCargoTypes.Descriptions.BreakBulk);
					AddPair(Constants.OceanCarrierCargoTypes.Codes.Container, Constants.OceanCarrierCargoTypes.Descriptions.Container);
					AddPair(Constants.OceanCarrierCargoTypes.Codes.RoRo, Constants.OceanCarrierCargoTypes.Descriptions.RoRo);
					break;

				case OLookUpEditType.RoRoTypes:
					AddPair(Constants.RoRoTypes.Codes.Bus, Constants.RoRoTypes.Descriptions.Bus);
					AddPair(Constants.RoRoTypes.Codes.Car, Constants.RoRoTypes.Descriptions.Car);
					AddPair(Constants.RoRoTypes.Codes.HighHeavyVehicle, Constants.RoRoTypes.Descriptions.HighHeavyVehicle);
					AddPair(Constants.RoRoTypes.Codes.IncompleteVehicle, Constants.RoRoTypes.Descriptions.IncompleteVehicle);
					AddPair(Constants.RoRoTypes.Codes.LowSpeedVehicle, Constants.RoRoTypes.Descriptions.LowSpeedVehicle);
					AddPair(Constants.RoRoTypes.Codes.MultiPurposePassengerVehicle, Constants.RoRoTypes.Descriptions.MultiPurposePassengerVehicle);
					AddPair(Constants.RoRoTypes.Codes.Motorcycle, Constants.RoRoTypes.Descriptions.Motorcycle);
					AddPair(Constants.RoRoTypes.Codes.OffRoadVehicle, Constants.RoRoTypes.Descriptions.OffRoadVehicle);
					AddPair(Constants.RoRoTypes.Codes.SUV, Constants.RoRoTypes.Descriptions.SUV);
					AddPair(Constants.RoRoTypes.Codes.SmallVan, Constants.RoRoTypes.Descriptions.SmallVan);
					AddPair(Constants.RoRoTypes.Codes.Trailer, Constants.RoRoTypes.Descriptions.Trailer);
					AddPair(Constants.RoRoTypes.Codes.Truck, Constants.RoRoTypes.Descriptions.Truck);
					AddPair(Constants.RoRoTypes.Codes.Van, Constants.RoRoTypes.Descriptions.Van);
					break;

				#endregion

				#region Sales Effect on Cost

				case OLookUpEditType.SalesEffectOnCost:
					Clear();
					AddRange(EnvProxy.Instance.Registry.SalesEffectOnCostList);
					break;

				#endregion

				#region Sales Growth Outlook

				case OLookUpEditType.SalesGrowthOutlook:
					Clear();
					AddRange(EnvProxy.Instance.Registry.SalesGrowthOutlookList);
					break;

				#endregion

				#region Sales Mode

				case OLookUpEditType.SalesMode:
					AddPair(Constants.Sales.Mode.Import, SourceGenerated.ResString.GetMultilingualString("Commmon|SalesMode|Import", "Import"));
					AddPair(Constants.Sales.Mode.Export, SourceGenerated.ResString.GetMultilingualString("Commmon|SalesMode|Export", "Export"));
					break;

				#endregion

				#region Sales StyleWI00014540-CargoWise.Types

				case OLookUpEditType.SalesStyle:
					Clear();
					AddRange(EnvProxy.Instance.Registry.SalesStyleList);
					break;

				#endregion

				#region Vessel Type

				case OLookUpEditType.VesselType:
					AddPair(Constants.VesselType.Barge, SourceGenerated.ResString.GetMultilingualString("Commmon|VesselType|Barge", "Barge"));
					AddPair(Constants.VesselType.BulkCarrier, SourceGenerated.ResString.GetMultilingualString("Commmon|VesselType|BulkCarrier", "Bulk Carrier"));
					AddPair(Constants.VesselType.CableShip, SourceGenerated.ResString.GetMultilingualString("Commmon|VesselType|CableShip", "Cable Ship"));
					AddPair(Constants.VesselType.CarCarringVessel, SourceGenerated.ResString.GetMultilingualString("Commmon|VesselType|CarCarringVessel", "Car Caring Vessel"));
					AddPair(Constants.VesselType.CargoVessel, SourceGenerated.ResString.GetMultilingualString("Commmon|VesselType|CargoVessel", "Cargo Vessel"));
					AddPair(Constants.VesselType.ContainerisedVessel, SourceGenerated.ResString.GetMultilingualString("Commmon|VesselType|ContainerisedVessel", "Containerized Vessel"));
					AddPair(Constants.VesselType.Dredger, SourceGenerated.ResString.GetMultilingualString("Commmon|VesselType|Dredger", "Dredger"));
					AddPair(Constants.VesselType.DrillShip, SourceGenerated.ResString.GetMultilingualString("Commmon|VesselType|DrillShip", "Drill Ship"));
					AddPair(Constants.VesselType.DryCargoVessel, SourceGenerated.ResString.GetMultilingualString("Commmon|VesselType|DryCargoVessel", "Dry Cargo Vessel"));
					AddPair(Constants.VesselType.FishingVessel, SourceGenerated.ResString.GetMultilingualString("Commmon|VesselType|FishingVessel", "Fishing Vessel"));
					AddPair(Constants.VesselType.LiquidNaturalGasTanker, SourceGenerated.ResString.GetMultilingualString("Commmon|VesselType|LiquidNaturalGasTanker", "Liquid Natural Gas Tanker"));
					AddPair(Constants.VesselType.LiveStockVessel, SourceGenerated.ResString.GetMultilingualString("Commmon|VesselType|LiveStockVessel", "Live Stock Vessel"));
					AddPair(Constants.VesselType.NavalVessel, SourceGenerated.ResString.GetMultilingualString("Commmon|VesselType|NavalVessel", "Naval Vessel"));
					AddPair(Constants.VesselType.OilRig, SourceGenerated.ResString.GetMultilingualString("Commmon|VesselType|OilRig", "Oil Rig"));
					AddPair(Constants.VesselType.OilTanker, SourceGenerated.ResString.GetMultilingualString("Commmon|VesselType|OilTanker", "Oil Tanker"));
					AddPair(Constants.VesselType.OtherTanker, SourceGenerated.ResString.GetMultilingualString("Commmon|VesselType|OtherTanker", "Other Tanker (Oil / Chemical / Other)"));
					AddPair(Constants.VesselType.OtherVessels, SourceGenerated.ResString.GetMultilingualString("Commmon|VesselType|OtherVessels", "Other Vessels"));
					AddPair(Constants.VesselType.PassengerVessel, SourceGenerated.ResString.GetMultilingualString("Commmon|VesselType|PassengerVessel", "Passenger Vessel"));
					AddPair(Constants.VesselType.ResearchVessel, SourceGenerated.ResString.GetMultilingualString("Commmon|VesselType|ResearchVessel", "Research Vessel"));
					AddPair(Constants.VesselType.RollOnRollOff, SourceGenerated.ResString.GetMultilingualString("Commmon|VesselType|RollOnRollOff", "Roll on Roll off Vessel"));
					AddPair(Constants.VesselType.SupplyBoat, SourceGenerated.ResString.GetMultilingualString("Commmon|VesselType|SupplyBoat", "Supply Boat"));
					AddPair(Constants.VesselType.TugBoat, SourceGenerated.ResString.GetMultilingualString("Commmon|VesselType|TugBoat", "Tug Boat"));
					AddPair(Constants.VesselType.Yacht, SourceGenerated.ResString.GetMultilingualString("Commmon|VesselType|Yacht", "Yacht"));

					break;

				#endregion

				#region Shipment Release Type

				case OLookUpEditType.ShipmentReleaseType:
					AddPair(Constants.ShipmentReleaseTypes.BankLetterOfCredit, SourceGenerated.ResString.GetMultilingualString("01d28b44-ca84-4953-beb8-12af7a2c5a57", "Letter of Credit (Bank Release)"));
					AddPair(Constants.ShipmentReleaseTypes.BankSightDraft, SourceGenerated.ResString.GetMultilingualString("b10a4bd3-b014-46d4-90a5-c31d1e6646d1", "Sight Draft (Bank Release)"));
					AddPair(Constants.ShipmentReleaseTypes.BankTimeDraft, SourceGenerated.ResString.GetMultilingualString("37ea5895-c57a-4897-a765-876425891821", "Time Draft (Bank Release)"));
					AddPair(Constants.ShipmentReleaseTypes.Cheque, SourceGenerated.ResString.GetMultilingualString("0f03b547-a631-444f-bd81-41ec71883d6b", "Company/Cashier Check"));
					AddPair(Constants.ShipmentReleaseTypes.CashDoc, SourceGenerated.ResString.GetMultilingualString("6966e7a4-ea41-494f-b5d8-77b13a9a38d2", "Cash Against Documents"));
					AddPair(Constants.ShipmentReleaseTypes.ExpressBofL, SourceGenerated.ResString.GetMultilingualString("2d413ee3-f341-4a9f-b471-612d565d4577", "Express Bill of Lading"));
					AddPair(Constants.ShipmentReleaseTypes.Indemnity, SourceGenerated.ResString.GetMultilingualString("59b1c694-2553-4732-b706-6216171dbff8", "Letter of Indemnity"));
					AddPair(Constants.ShipmentReleaseTypes.NonNegotiable, SourceGenerated.ResString.GetMultilingualString("9920b9d1-e824-4c33-b1d9-43476f6c1f71", "Not Negotiable unless consigned to Order"));
					AddPair(Constants.ShipmentReleaseTypes.OriginalReqSurrender, SourceGenerated.ResString.GetMultilingualString("a8d7ad8b-abc7-401b-b658-e68440c543df", "Original Bill - Surrendered at Origin"));
					AddPair(Constants.ShipmentReleaseTypes.OriginalReq, SourceGenerated.ResString.GetMultilingualString("3276af5e-1a72-4b3a-8240-311dcc0c1e95", "Original Bill Required at Destination"));
					AddPair(Constants.ShipmentReleaseTypes.SeaWaybill, SourceGenerated.ResString.GetMultilingualString("1db6fe21-a444-40c9-ab0c-b629d3672e2f", "Sea Waybill"));
					break;

				#endregion

				#region Shipment Screen Layout

				case OLookUpEditType.ShipmentScreenLayout:
					AddPair(Constants.ShipmentScreenOptions.Auto, SourceGenerated.ResString.GetMultilingualString("a62c8922-c438-4267-996f-a37e2dede02f", "Automatic"));
					AddPair(Constants.ShipmentScreenOptions.Consignor, SourceGenerated.ResString.GetMultilingualString("ecc9c9b9-551f-40ce-acac-7d8656a34adc", "Show Consignor before Consignee"));
					AddPair(Constants.ShipmentScreenOptions.Consignee, SourceGenerated.ResString.GetMultilingualString("816793e7-6d91-4e9c-858c-5ea026fc65f3", "Show Consignee before Consignor"));
					break;

				#endregion

				#region OSMG Security Level

				case OLookUpEditType.OSMGSecurityLevel:
					AddPair(Constants.OSMGSecurityLevels.Standard, SourceGenerated.ResString.GetMultilingualString("463d3984-b003-44e8-b489-3f06d5cc05a0", "Standard"));
					AddPair(Constants.OSMGSecurityLevels.Enhanced, SourceGenerated.ResString.GetMultilingualString("9fd00a6b-c32e-43e3-8903-ff70acfb8b35", "Enhanced"));
					break;

				#endregion

				#region Staff Certificate Type

				case OLookUpEditType.StaffCertificateType:
					AddPair(Constants.StaffCertificateType.IATA, SourceGenerated.ResString.GetMultilingualString("73a430a9-0c3a-47c5-bb5b-a0ffdbac37db", "IATA"));
					AddPair(Constants.StaffCertificateType.DG, SourceGenerated.ResString.GetMultilingualString("d7018f7e-b00b-43ed-affc-be1a18fa1172", "Dangerous Goods"));
					AddPair(Constants.StaffCertificateType.Broker, SourceGenerated.ResString.GetMultilingualString("99d3db94-5a7a-467b-9e2c-abd30a254c33", "Broker"));
					AddPair(Constants.StaffCertificateType.Car, SourceGenerated.ResString.GetMultilingualString("4af72ab9-c6fd-4b3e-8a86-e19fdc154c35", "Drivers License"));
					AddPair(Constants.StaffCertificateType.Truck, SourceGenerated.ResString.GetMultilingualString("c1b72f05-3c28-4a33-aecb-b24b9459b9fb", "Truck (Specify Type)"));
					AddPair(Constants.StaffCertificateType.FKL, SourceGenerated.ResString.GetMultilingualString("dd66365b-4813-43a4-bcf2-3c3456a05733", "Fork Lift"));
					AddPair(Constants.StaffCertificateType.NID, SourceGenerated.ResString.GetMultilingualString("c7c7ac41-312c-4c6e-a22b-4bb58ad7aa55", "National Identity Document"));
					AddPair(Constants.StaffCertificateType.PAS, SourceGenerated.ResString.GetMultilingualString("09cb1397-59db-42ed-b61b-0cbcd73e72bb", "Passport Number"));
					AddPair(Constants.StaffCertificateType.Misc, SourceGenerated.ResString.GetMultilingualString("d301bcf3-a4b3-47bc-8d32-2396d7fd6c73", "Miscellaneous"));
					break;

				#endregion

				#region Staff Membership Type

				case OLookUpEditType.StaffMembershipType:
					Clear();
					AddRange(EnvProxy.Instance.Registry.StaffMembershipTypesList);
					break;

				#endregion

				#region Transaction Types

				case OLookUpEditType.TransactionTypes:
					AddPair(TransactionTypes.AdjustmentNote, SourceGenerated.ResString.GetMultilingualString("Accounting|InvoicePrintingTransactionTypes|AdjustmentNote", "Adjustment Note"));
					AddPair(TransactionTypes.Contra, SourceGenerated.ResString.GetMultilingualString("0eb14c5b-1807-4e13-be2f-1ff0129137f7", "Contra"));
					AddPair(TransactionTypes.CreditNote, SourceGenerated.ResString.GetMultilingualString("Accounting|InvoicePrintingTransactionTypes|CreditNote", "Credit Note"));
					AddPair(TransactionTypes.DirectPayment, SourceGenerated.ResString.GetMultilingualString("c857813f-231d-4912-9e8d-8e02bcebc52f", "Direct Payment"));
					AddPair(TransactionTypes.DirectReceipt, SourceGenerated.ResString.GetMultilingualString("72769dc8-2d24-4159-92e5-acf29ae1edb0", "Direct Receipt"));
					AddPair(TransactionTypes.Discount, SourceGenerated.ResString.GetMultilingualString("8716c2e0-9cb9-4539-88f3-2278492b6135", "Discount"));
					AddPair(TransactionTypes.ExchangeDifference, SourceGenerated.ResString.GetMultilingualString("d7615f4f-fc56-4d2a-9e75-1d8287aa4e22", "Exchange Difference"));
					AddPair(TransactionTypes.GLAutoJournal, SourceGenerated.ResString.GetMultilingualString("Accounting|GLJournalTypes|GLAutoJournal", "Auto Journal"));
					AddPair(TransactionTypes.GLReversingJournal, SourceGenerated.ResString.GetMultilingualString("Accounting|GLJournalTypes|GLReversingJournal", "Reversing Journal"));
					AddPair(TransactionTypes.GLStandardJournal, SourceGenerated.ResString.GetMultilingualString("Accounting|GLJournalTypes|GLStandardJournal", "General Journal"));
					AddPair(TransactionTypes.Invoice, SourceGenerated.ResString.GetMultilingualString("Accounting|InvoicePrintingTransactionTypes|Invoice", "Invoice"));
					AddPair(TransactionTypes.Journal, SourceGenerated.ResString.GetMultilingualString("dd5ccdbe-ffe4-4ec8-bdb0-44c17b679f75", "AR/AP Journal"));
					AddPair(TransactionTypes.OpeningPayment, SourceGenerated.ResString.GetMultilingualString("2edc7edd-ffa2-41d4-a1d4-a8cdd7fb8e36", "Opening Payment"));
					AddPair(TransactionTypes.OpeningReceipt, SourceGenerated.ResString.GetMultilingualString("0a71a2b3-4b70-493d-930a-30a5c2ec11a6", "Opening Receipt"));
					AddPair(TransactionTypes.Overpayment, SourceGenerated.ResString.GetMultilingualString("11bb4229-4438-4316-8fdf-01f8e0fb7fea", "Overpayment"));
					AddPair(TransactionTypes.Payment, SourceGenerated.ResString.GetMultilingualString("b099e09a-cd84-4bc6-acf9-4707cf842b29", "Payment"));
					AddPair(TransactionTypes.Receipt, SourceGenerated.ResString.GetMultilingualString("36bcdb16-c60e-4c66-971a-ed6cae9f1042", "Receipt"));
					AddPair(TransactionTypes.Transfer, SourceGenerated.ResString.GetMultilingualString("d508f6a8-d3d0-4b23-afac-ab9c9d54578e", "Transfer"));
					AddPair(TransactionTypes.WIPAccrualJournal, SourceGenerated.ResString.GetMultilingualString("73ff617c-ac77-4c58-bde6-994e1feb2d2a", "WIP/Accrual Journal"));
					AddPair(TransactionTypes.JobRevenueJournal, SourceGenerated.ResString.GetMultilingualString("4AA81839-1E54-4F2C-AA81-839F9BBAD584", "Job Revenue Journal"));
					break;

				#endregion

				#region Transport Modes

				case OLookUpEditType.TransportType:
					AddPair(Constants.TransportModes.Air, SourceGenerated.ResString.GetMultilingualString("Core|TransportModeList|Air", "Air Freight"));
					AddPair(Constants.TransportModes.Sea, SourceGenerated.ResString.GetMultilingualString("Core|TransportModeList|Sea", "Sea Freight"));
					AddPair(Constants.TransportModes.SeaAir, SourceGenerated.ResString.GetMultilingualString("Freight|TransportModeList|SeaAir", "First by Sea then by Air Freight"));
					AddPair(Constants.TransportModes.AirSea, SourceGenerated.ResString.GetMultilingualString("Freight|TransportModeList|AirSea", "First by Air then by Sea Freight"));
					AddPair(Constants.TransportModes.Road, SourceGenerated.ResString.GetMultilingualString("Core|TransportModeList|Road", "Road Freight"));
					AddPair(Constants.TransportModes.Rail, SourceGenerated.ResString.GetMultilingualString("Core|TransportModeList|Rail", "Rail Freight"));
					AddPair(Constants.TransportModes.Courier, SourceGenerated.ResString.GetMultilingualString("Core|TransportModeList|Courier", "Courier"));
					break;

				#endregion

				#region Temperature Types

				case OLookUpEditType.TemperatureTypes:
					AddPair(Constants.Temperature.Centigrade, Constants.Temperature.GetDescription(Constants.Temperature.Centigrade));
					AddPair(Constants.Temperature.Fahrenheit, Constants.Temperature.GetDescription(Constants.Temperature.Fahrenheit));
					break;

				#endregion

				#region Shipment Types

				case OLookUpEditType.ShipmentType:
					AddPair(Constants.ShipmentTypes.StandardHouse, Constants.ShipmentTypeDescriptions.StandardHouse);
					AddPair(Constants.ShipmentTypes.CoLoadMaster, Constants.ShipmentTypeDescriptions.CoLoadMaster);
					AddPair(Constants.ShipmentTypes.BlindCoLoadMaster, Constants.ShipmentTypeDescriptions.BlindCoLoadMaster);
					AddPair(Constants.ShipmentTypes.BuyersConsolLead, Constants.ShipmentTypeDescriptions.BuyersConsolLead);
					AddPair(Constants.ShipmentTypes.AssemblyMaster, Constants.ShipmentTypeDescriptions.AssemblyMaster);
					AddPair(Constants.ShipmentTypes.HighVolumeLowValue, Constants.ShipmentTypeDescriptions.HighVolumeLowValue);
					AddPair(Constants.ShipmentTypes.HighVolumeLowValueMaster, Constants.ShipmentTypeDescriptions.HighVolumeLowValueMaster);
					AddPair(Constants.ShipmentTypes.ThirdPartyOwnershipHouse, Constants.ShipmentTypeDescriptions.ThirdPartyOwnershipHouse);
					AddPair(Constants.ShipmentTypes.ShippersConsolLead, Constants.ShipmentTypeDescriptions.ShippersConsolLead);
					break;

				#endregion

				#region US Customs Charge Types

				case OLookUpEditType.USCustomsChargeTypes:
					AddPair(Constants.USCustoms.ChargeTypes.Codes.Allowance, Constants.USCustoms.ChargeTypes.Descriptions.Allowance);
					AddPair(Constants.USCustoms.ChargeTypes.Codes.Charge, Constants.USCustoms.ChargeTypes.Descriptions.Charge);
					AddPair(Constants.USCustoms.ChargeTypes.Codes.CBPAdjustment, Constants.USCustoms.ChargeTypes.Descriptions.CBPAdjustment);
					break;

				#endregion

				#region US Delivery Terms

				case OLookUpEditType.USDeliveryTerms:
					AddPair(Constants.USCustoms.DeliveryTerms.Codes.EXW, Constants.USCustoms.DeliveryTerms.Descriptions.EXW);
					AddPair(Constants.USCustoms.DeliveryTerms.Codes.FAS, Constants.USCustoms.DeliveryTerms.Descriptions.FAS);
					AddPair(Constants.USCustoms.DeliveryTerms.Codes.FOB, Constants.USCustoms.DeliveryTerms.Descriptions.FOB);
					AddPair(Constants.USCustoms.DeliveryTerms.Codes.FOA, Constants.USCustoms.DeliveryTerms.Descriptions.FOA);
					AddPair(Constants.USCustoms.DeliveryTerms.Codes.FOR, Constants.USCustoms.DeliveryTerms.Descriptions.FOR);
					AddPair(Constants.USCustoms.DeliveryTerms.Codes.FOT, Constants.USCustoms.DeliveryTerms.Descriptions.FOT);
					AddPair(Constants.USCustoms.DeliveryTerms.Codes.FPC, Constants.USCustoms.DeliveryTerms.Descriptions.FPC);
					AddPair(Constants.USCustoms.DeliveryTerms.Codes.CAF, Constants.USCustoms.DeliveryTerms.Descriptions.CAF);
					AddPair(Constants.USCustoms.DeliveryTerms.Codes.CAI, Constants.USCustoms.DeliveryTerms.Descriptions.CAI);
					AddPair(Constants.USCustoms.DeliveryTerms.Codes.CIF, Constants.USCustoms.DeliveryTerms.Descriptions.CIF);
					AddPair(Constants.USCustoms.DeliveryTerms.Codes.DAF, Constants.USCustoms.DeliveryTerms.Descriptions.DAF);
					AddPair(Constants.USCustoms.DeliveryTerms.Codes.EXQ, Constants.USCustoms.DeliveryTerms.Descriptions.EXQ);
					AddPair(Constants.USCustoms.DeliveryTerms.Codes.DDP, Constants.USCustoms.DeliveryTerms.Descriptions.DDP);
					break;

				#endregion

				#region Weight / Volume Display Types

				case OLookUpEditType.WeightAndVolumeDisplayTypes:
					AddPair(WeightAndVolumeDisplayTypes.Codes.Actual, WeightAndVolumeDisplayTypes.Descriptions.Actual);
					AddPair(WeightAndVolumeDisplayTypes.Codes.Client, WeightAndVolumeDisplayTypes.Descriptions.Client);
					AddPair(WeightAndVolumeDisplayTypes.Codes.Carrier, WeightAndVolumeDisplayTypes.Descriptions.Carrier);
					break;

				#endregion

				#region WIP / Accrual Transaction Types

				case OLookUpEditType.WIPAccrualTransactionTypes:
					AddPair(TransactionLineTypes.WIP, SourceGenerated.ResString.GetMultilingualString("Accounting|WIPAccrualTransactionTypes|WIP", "Work in progress"));
					AddPair(TransactionLineTypes.Accrual, SourceGenerated.ResString.GetMultilingualString("Accounting|WIPAccrualTransactionTypes|Accrual", "Accrual"));
					break;

				#endregion

				#region Work Permit Status

				case OLookUpEditType.WorkPermitStatuses:
					AddPair(Constants.WorkPermitStatuses.Residence, Constants.WorkPermitStatuseDescriptions.Residence);
					AddPair(Constants.WorkPermitStatuses.SkillVisa, Constants.WorkPermitStatuseDescriptions.SkillVisa);
					AddPair(Constants.WorkPermitStatuses.WorkPermit, Constants.WorkPermitStatuseDescriptions.WorkPermit);
					AddPair(Constants.WorkPermitStatuses.Student, Constants.WorkPermitStatuseDescriptions.Student);
					AddPair(Constants.WorkPermitStatuses.None, Constants.WorkPermitStatuseDescriptions.None);
					break;

				#endregion

				#region Availability

				case OLookUpEditType.Availabilities:
					AddPair(Constants.Availabilties.FullTime, SourceGenerated.ResString.GetMultilingualString("797565e0-8055-45e4-b5b7-9444aac89c94", "Full Time"));
					AddPair(Constants.Availabilties.PartTime, SourceGenerated.ResString.GetMultilingualString("8f8a46e3-72e5-4498-9cf1-2f104a471012", "Part Time"));
					AddPair(Constants.Availabilties.Casual, SourceGenerated.ResString.GetMultilingualString("7d09dd70-43ba-4aee-b9c9-fcca67c18573", "Casual"));
					AddPair(Constants.Availabilties.Contract, SourceGenerated.ResString.GetMultilingualString("3640d65c-9b5d-4275-93b1-208afa2d47f0", "Contract"));
					break;

				#endregion

				#region AP Payment Method

				case OLookUpEditType.APPaymentMethod:
					AddPair(ReceiptTypes.Cheque, SourceGenerated.ResString.GetMultilingualString("41267ae5-0e21-449f-83d2-d02e1b76ed75", "Check"));
					AddPair(ReceiptTypes.DirectDebit, SourceGenerated.ResString.GetMultilingualString("6d354bb3-89e3-45cd-8639-8e2132cef718", "Direct Debit"));
					AddPair(ReceiptTypes.eNettDirectDebit, SourceGenerated.ResString.GetMultilingualString("85ce5de1-3d7f-4885-9640-f7274cccbe83", "ComPay Direct Debit"));
					if (ObjectFactory.Get<IAccounting>().IsEPaymentFunctionalityEnabledForOFX(EnvProxy.Instance.CurrentCompany.PK))
					{
						AddPair(EPaymentMethods.EPaymentViaOFX, SourceGenerated.ResString.GetMultilingualString("7b5318f1-fb6c-47c2-b4c5-0fb534845492", "E-Payment via OFX"));
					}
					break;

				#endregion

				#region Storage Calculation Period

				case OLookUpEditType.StorageCalculationPeriod:
					AddPair(Constants.StorageCalculationPeriods.Daily, SourceGenerated.ResString.GetMultilingualString("d2c047b2-ad33-436f-8852-9b42473e5fda", "Daily"));
					AddPair(Constants.StorageCalculationPeriods.Weekly, SourceGenerated.ResString.GetMultilingualString("48729e55-e2e2-4d6e-b37b-09bd34452e5e", "Weekly"));
					AddPair(Constants.StorageCalculationPeriods.Fortnightly, SourceGenerated.ResString.GetMultilingualString("23e4699a-166d-41a2-992c-16e6cdcb2d78", "Biweekly/Fortnightly"));
					AddPair(Constants.StorageCalculationPeriods.Monthly, SourceGenerated.ResString.GetMultilingualString("7691d6a4-09a4-46cf-b715-0c3b0f6519c4", "Monthly"));
					AddPair(Constants.StorageCalculationPeriods.BillingPeriod, SourceGenerated.ResString.GetMultilingualString("1c1544c4-5944-439e-8985-111c6667c66d", "As per billing period"));
					break;

				#endregion

				#region Autorating Mode

				case OLookUpEditType.FreightRateAutoratingMode:
					AddPair(Constants.FreightRateAutoratingModes.Code.StandardRate, Constants.FreightRateAutoratingModes.Description.StandardRate);
					AddPair(Constants.FreightRateAutoratingModes.Code.FreightPlusRate, Constants.FreightRateAutoratingModes.Description.FreightPlusRate);
					AddPair(Constants.FreightRateAutoratingModes.Code.AllInRate, Constants.FreightRateAutoratingModes.Description.AllInRate);
					break;

				#endregion

				#region Supplier Booking Line Status

				case OLookUpEditType.SupplierBookingLineStatus:
					AddPair(Constants.SupplierBookingLineStatus.Codes.Incomplete, Constants.SupplierBookingLineStatus.Descriptions.Incomplete);
					AddPair(Constants.SupplierBookingLineStatus.Codes.Confirmed, Constants.SupplierBookingLineStatus.Descriptions.Confirmed);
					AddPair(Constants.SupplierBookingLineStatus.Codes.AcceptedAtOriginDepot, Constants.SupplierBookingLineStatus.Descriptions.AcceptedAtOriginDepot);
					AddPair(Constants.SupplierBookingLineStatus.Codes.DepartedFromOriginDepot, Constants.SupplierBookingLineStatus.Descriptions.DepartedFromOriginDepot);
					AddPair(Constants.SupplierBookingLineStatus.Codes.ArrivedAtDestination, Constants.SupplierBookingLineStatus.Descriptions.ArrivedAtDestination);
					AddPair(Constants.SupplierBookingLineStatus.Codes.CustomsClearedAtDestination, Constants.SupplierBookingLineStatus.Descriptions.CustomsClearedAtDestination);
					AddPair(Constants.SupplierBookingLineStatus.Codes.AwaitingLocalDelivery, Constants.SupplierBookingLineStatus.Descriptions.AwaitingLocalDelivery);
					AddPair(Constants.SupplierBookingLineStatus.Codes.Delivered, Constants.SupplierBookingLineStatus.Descriptions.Delivered);
					break;

				#endregion

				#region Service Level Amount 1 Type

				case OLookUpEditType.ServiceLevelAmount1Type:
					AddPair(OrgConstants.ServiceLevelAmountTypes.Code.Excess, OrgConstants.ServiceLevelAmountTypes.Description.Excess);
					AddPair(OrgConstants.ServiceLevelAmountTypes.Code.Minimum, OrgConstants.ServiceLevelAmountTypes.Description.Minimum);
					AddPair(OrgConstants.ServiceLevelAmountTypes.Code.None, OrgConstants.ServiceLevelAmountTypes.Description.None);
					break;

				#endregion

				#region Service Level Amount 2 Type

				case OLookUpEditType.ServiceLevelAmount2Type:
					AddPair(OrgConstants.ServiceLevelAmountTypes.Code.Maximum, OrgConstants.ServiceLevelAmountTypes.Description.Maximum);
					AddPair(OrgConstants.ServiceLevelAmountTypes.Code.None, OrgConstants.ServiceLevelAmountTypes.Description.None);
					break;

				#endregion

				#region e-Freight Status

				case OLookUpEditType.EFreightStatus:
					AddPair(Constants.EFreightStatus.Code.NON, Constants.EFreightStatus.Description.NON);
					AddPair(Constants.EFreightStatus.Code.EAP, Constants.EFreightStatus.Description.EAP);
					AddPair(Constants.EFreightStatus.Code.EAW, Constants.EFreightStatus.Description.EAW);
					break;

				#endregion

				#region CHIEF C88 (UK customs)

				case OLookUpEditType.ChiefC88:
					AddPair(Constants.ChiefC88Options.Code.None, Constants.ChiefC88Options.Descriptions.None);
					AddPair(Constants.ChiefC88Options.Code.Rich, Constants.ChiefC88Options.Descriptions.Rich);
					AddPair(Constants.ChiefC88Options.Code.Plain, Constants.ChiefC88Options.Descriptions.Plain);
					break;

				#endregion

				#region Outturn Scanning CONDClear Release Status

				case OLookUpEditType.CondClearReleaseStatus:
					AddPair(Constants.AUCustoms.CondClearReleaseStatus.Clear, SourceGenerated.ResString.GetMultilingualString("4E60E76B-BBAE-4E7A-9E77-4240498BD7D1", "CLEAR"));
					AddPair(Constants.AUCustoms.CondClearReleaseStatus.Held, SourceGenerated.ResString.GetMultilingualString("B9E66CBB-F461-4166-9B1B-252A8350968C", "HELD"));
					break;

				#endregion

				#region Report Statistics Status

				case OLookUpEditType.ReportStatisticsStatus:
					AddPair(Constants.StmReportRunState.Error, SourceGenerated.ResString.GetMultilingualString("ReportStatisticsStatus|Error", "Error"));
					AddPair(Constants.StmReportRunState.Finished, SourceGenerated.ResString.GetMultilingualString("ReportStatisticsStatus|Finished", "Finished"));
					AddPair(Constants.StmReportRunState.Queued, SourceGenerated.ResString.GetMultilingualString("ReportStatisticsStatus|Queued", "Queued"));
					AddPair(Constants.StmReportRunState.Running, SourceGenerated.ResString.GetMultilingualString("ReportStatisticsStatus|Running", "Running"));
					AddPair(Constants.StmReportRunState.Stopped, SourceGenerated.ResString.GetMultilingualString("ReportStatisticsStatus|Stopped", "Stopped"));
					AddPair(Constants.StmReportRunState.Cancelled, SourceGenerated.ResString.GetMultilingualString("ReportStatisticsStatus|Cancelled", "Canceled"));
					break;

				#endregion

				#region EInvoicing Batch Status

				case OLookUpEditType.EInvoicingBatchState:
					AddPair(Constants.EInvoicingBatchState.Ready, SourceGenerated.ResString.GetMultilingualString("EInvoicingBatchState|Ready", "Ready"));
					AddPair(Constants.EInvoicingBatchState.Sent, SourceGenerated.ResString.GetMultilingualString("EInvoicingBatchState|Sent", "Sent"));
					AddPair(Constants.EInvoicingBatchState.Discarded, SourceGenerated.ResString.GetMultilingualString("EInvoicingBatchState|Discarded", "Discarded"));
					break;

				#endregion

				#region EInvoicing Pivot Status

				case OLookUpEditType.EInvoicingPivotState:
					AddPair(Constants.EInvoicingPivotState.Pending, SourceGenerated.ResString.GetMultilingualString("EInvoicingPivotState|Pending", "Pending user action"));
					AddPair(Constants.EInvoicingPivotState.Queued, SourceGenerated.ResString.GetMultilingualString("EInvoicingPivotState|Queued", "Queued"));
					AddPair(Constants.EInvoicingPivotState.Batched, SourceGenerated.ResString.GetMultilingualString("EInvoicingPivotState|Batched", "Batched"));
					AddPair(Constants.EInvoicingPivotState.BatchedWithError, SourceGenerated.ResString.GetMultilingualString("EInvoicingPivotState|BatchedWithError", "Batched With Error"));
					AddPair(Constants.EInvoicingPivotState.Sent, SourceGenerated.ResString.GetMultilingualString("EInvoicingPivotState|Sent", "Sent"));
					AddPair(Constants.EInvoicingPivotState.Delivered, SourceGenerated.ResString.GetMultilingualString("EInvoicingPivotState|Delivered", "Delivered"));
					AddPair(Constants.EInvoicingPivotState.Succeed, SourceGenerated.ResString.GetMultilingualString("EInvoicingPivotState|Succeed", "Succeed"));
					AddPair(Constants.EInvoicingPivotState.Failed, SourceGenerated.ResString.GetMultilingualString("EInvoicingPivotState|Failed", "Failed"));
					AddPair(Constants.EInvoicingPivotState.Discarded, SourceGenerated.ResString.GetMultilingualString("EInvoicingPivotState|Discarded", "Discarded"));
					AddPair(Constants.EInvoicingPivotState.AwaitingReview, SourceGenerated.ResString.GetMultilingualString("EInvoicingPivotState|AwaitingReview", "Awaiting Review"));
					AddPair(Constants.EInvoicingPivotState.InProcessing, SourceGenerated.ResString.GetMultilingualString("EInvoicingPivotState|InProcessing", "In Processing"));
					AddPair(Constants.EInvoicingPivotState.NotEligible, SourceGenerated.ResString.GetMultilingualString("EInvoicingPivotState|NotEligible", "Not Eligible"));
					break;

				#endregion

				#region EInvoicing Pivot Action Type

				case OLookUpEditType.EInvoicingPivotActionType:
					AddPair(Constants.EInvoicingPivotActionType.Submit, SourceGenerated.ResString.GetMultilingualString("EInvoicingPivotActionType|Submit", "Submit"));
					AddPair(Constants.EInvoicingPivotActionType.StatusCheck, SourceGenerated.ResString.GetMultilingualString("EInvoicingPivotActionType|StatusCheck", "Request Status"));
					AddPair(Constants.EInvoicingPivotActionType.DocumentAction, SourceGenerated.ResString.GetMultilingualString("EInvoicingPivotActionType|DocumentAction", "Document Action"));
					AddPair(Constants.EInvoicingPivotActionType.Cancel, SourceGenerated.ResString.GetMultilingualString("EInvoicingPivotActionType|Cancel", "Cancel"));
					break;

				#endregion

				#region eDocs Storage Provider

				case OLookUpEditType.EDocsStorageProvider:
					AddPair(Constants.EDocsStorageProviders.Code.DB, Constants.EDocsStorageProviders.Description.DB);
					AddPair(Constants.EDocsStorageProviders.Code.S3, Constants.EDocsStorageProviders.Description.S3);
					break;

				#endregion

				#region Compliance Document Status

				case OLookUpEditType.ComplianceDocumentStatus:
					AddPair(Constants.ComplianceDocumentStatus.Added, SourceGenerated.ResString.GetMultilingualString("ComplianceDocumentStatus|Added", "Document Record Added"));
					AddPair(Constants.ComplianceDocumentStatus.Voided, SourceGenerated.ResString.GetMultilingualString("ComplianceDocumentStatus|Voided", "Document Record Voided"));
					AddPair(Constants.ComplianceDocumentStatus.NumberSet, SourceGenerated.ResString.GetMultilingualString("ComplianceDocumentStatus|Set", "Document Number Set"));
					AddPair(Constants.ComplianceDocumentStatus.Finalised, SourceGenerated.ResString.GetMultilingualString("ComplianceDocumentStatus|Finalized", "Document Record Finalized"));
					break;
				#endregion

				#region Rate Modes
				case OLookUpEditType.RateModes:
					AddPair(Constants.RateMode.ALL, SourceGenerated.ResString.GetMultilingualString("B07BBD95-C9B5-453F-B56D-A67F6EFF4C39", "All Freight Modes"));
					AddPair(Constants.RateMode.AIR, SourceGenerated.ResString.GetMultilingualString("6818145A-3825-488C-AB6B-A5ACCF18BF9D", "Air Freight (ULD and LSE)"));
					AddPair(Constants.RateMode.ULD, SourceGenerated.ResString.GetMultilingualString("BEE5D929-9363-45C1-A4D7-DA3D7ADE5325", "Air Freight (ULD)"));
					AddPair(Constants.RateMode.LSE, SourceGenerated.ResString.GetMultilingualString("12E17C2E-E131-4669-9423-44E896F8D81A", "Air Freight (LSE)"));
					AddPair(Constants.RateMode.SEA, SourceGenerated.ResString.GetMultilingualString("9508704A-E1F1-4A06-8061-27B64B299E69", "Sea Freight (LCL and FCL)"));
					AddPair(Constants.RateMode.LCL, SourceGenerated.ResString.GetMultilingualString("4FD707BF-22E8-492E-AA65-AEFE775542D2", "Sea Freight (LCL)"));
					AddPair(Constants.RateMode.FCL, SourceGenerated.ResString.GetMultilingualString("DD202E46-0C64-4087-BE20-AB58ECF56040", "Sea Freight (FCL)"));
					AddPair(Constants.RateMode.ROA, SourceGenerated.ResString.GetMultilingualString("F8617A03-F20C-417A-8E01-F8EDA2F5B599", "Road Freight (LCL, FCL and FTL)"));
					AddPair(Constants.RateMode.LRO, SourceGenerated.ResString.GetMultilingualString("ED75CBC7-D7A4-4474-A632-6A08AD7C6654", "Road Freight (LCL)"));
					AddPair(Constants.RateMode.FRO, SourceGenerated.ResString.GetMultilingualString("5DDD521A-97DF-41F7-86E5-52DB85954DC0", "Road Freight (FCL)"));
					AddPair(Constants.RateMode.FTL, SourceGenerated.ResString.GetMultilingualString("2DBBC8AD-E215-4D41-AB0D-09F5AFFCCE40", "Road Freight (FTL)"));
					AddPair(Constants.RateMode.RAI, SourceGenerated.ResString.GetMultilingualString("4C754BED-3AAC-48A2-86F4-B6A555AAF058", "Rail Freight (LCL, FCL and FWL)"));
					AddPair(Constants.RateMode.LRA, SourceGenerated.ResString.GetMultilingualString("894D9E10-43E9-4B06-A796-18ED6E74A15B", "Rail Freight (LCL)"));
					AddPair(Constants.RateMode.FRA, SourceGenerated.ResString.GetMultilingualString("0A3B48B8-F14C-4F78-ACFB-ED432E6B03CE", "Rail Freight (FCL)"));
					AddPair(Constants.RateMode.FWL, SourceGenerated.ResString.GetMultilingualString("DE292516-77B0-4ACE-BF6C-EED7E2C80730", "Rail Freight (FWL)"));
					AddPair(Constants.RateMode.MAI, SourceGenerated.ResString.GetMultilingualString("20D38189-AC6A-419C-8B42-5A166F215505", "Post"));
					break;
				#endregion

				#region Debtor Types

				case OLookUpEditType.DebtorTypes:
					AddPair(Constants.DebtorTypes.Code.DebtorGroup, Constants.DebtorTypes.Description.DebtorGroup);
					AddPair(Constants.DebtorTypes.Code.DebtorOrganisation, Constants.DebtorTypes.Description.DebtorOrganisation);
					break;

				#endregion

				#region Tax Override Transaction Context

				case OLookUpEditType.TaxOverrideTransactionContext:
					AddPair(Constants.TaxOverrideTransactionContext.Codes.All, Constants.TaxOverrideTransactionContext.Descriptions.All);
					AddPair(Constants.TaxOverrideTransactionContext.Codes.IntercompanyInvoiceImport, Constants.TaxOverrideTransactionContext.Descriptions.IntercompanyInvoiceImport);
					AddPair(Constants.TaxOverrideTransactionContext.Codes.Standard, Constants.TaxOverrideTransactionContext.Descriptions.Standard);
					break;

				#endregion

				#region Tax Override Defaulting Rule

				case OLookUpEditType.TaxOverrideDefaultingRule:
					AddPair(Constants.TaxOverrideDefaultingRule.Codes.CopyARAmount, Constants.TaxOverrideDefaultingRule.Descriptions.CopyARAmount);
					AddPair(Constants.TaxOverrideDefaultingRule.Codes.NotApplicable, Constants.TaxOverrideDefaultingRule.Descriptions.NotApplicable);
					AddPair(Constants.TaxOverrideDefaultingRule.Codes.SumARAmount, Constants.TaxOverrideDefaultingRule.Descriptions.SumARAmount);
					break;

				#endregion

				#region Payment Batch Status

				case OLookUpEditType.PaymentBatchStatus:
					AddPair(Constants.AccPaymentBatchStatus.Working, Constants.AccPaymentBatchStatusDescription.Working);
					AddPair(Constants.AccPaymentBatchStatus.Completed, Constants.AccPaymentBatchStatusDescription.Completed);
					AddPair(Constants.AccPaymentBatchStatus.Cancelled, Constants.AccPaymentBatchStatusDescription.Cancelled);
					break;

				#endregion

				#region Radioactive Types

				case OLookUpEditType.RadioactiveTypes:
					foreach (var code in Constants.RadioactiveUnits.Codes)
					{
						AddPair(code, Constants.RadioactiveUnits.GetDescription(code));
					}
					break;

				#endregion

				#region Facility Types

				case OLookUpEditType.FacilityTypes:
					AddPair(Constants.FacilityType.Code.Terminal, Constants.FacilityType.Description.Terminal);
					AddPair(Constants.FacilityType.Code.ContainerYard, Constants.FacilityType.Description.ContainerYard);
					AddPair(Constants.FacilityType.Code.TransitWarehouse, Constants.FacilityType.Description.TransitWarehouse);
					break;

				#endregion

				#region SRRErrorNotificationOptions

				case OLookUpEditType.SRRErrorNotificationOptions:
					AddPair(Constants.ErrorNotificationOptions.Code.DEF, Constants.ErrorNotificationOptions.Description.DEF);
					AddPair(Constants.ErrorNotificationOptions.Code.ROL, Constants.ErrorNotificationOptions.Description.ROL);
					AddPair(Constants.ErrorNotificationOptions.Code.GRP, Constants.ErrorNotificationOptions.Description.GRP);
					break;

					#endregion
			}
		}

		public OLookUpEditType LookupEditType
		{
			get { return lookupEditType; }
		}

		readonly OLookUpEditType lookupEditType;

		#endregion

		#region Add

		public void AddPair(string code)
		{
			AddPair(code, "");
		}

		public void AddPair(MultilingualString code)
		{
			AddPair(code, "");
		}

		public void AddPair(string code, string description)
		{
			Add(new CodeDescriptionPair(code, description));
		}

		public void AddPair(MultilingualString code, string description)
		{
			Add(new CodeDescriptionPair(code, description));
		}

		public void AddPair(string code, MultilingualString description)
		{
			Add(new CodeDescriptionPair(code, description));
		}

		public void AddPair(MultilingualString code, MultilingualString description)
		{
			Add(new CodeDescriptionPair(code, description));
		}

		public void AddPair(object pk, string code, string description)
		{
			Add(new CodeElement(pk, code, description));
		}

		public void AddPair(object pk, MultilingualString code, MultilingualString description)
		{
			Add(new CodeElement(pk, code, description));
		}

		public void AddPairIfNotExist(string code, string description)
		{
			if (!ContainsCode(code))
			{
				AddPair(code, description);
			}
		}

		public void AddPairIfNotExist(string code, MultilingualString description)
		{
			if (!ContainsCode(code))
			{
				AddPair(code, description);
			}
		}

		public void OverridePair(string code, string description)
		{
			var index = IndexOfCode(code);

			if (index == -1)
			{
				throw new ArgumentException($"Code {code} does not exist in the list");
			}

			this[index] = new CodeDescriptionPair(code, description);
		}

		public void OverridePair(string code, MultilingualString description)
		{
			var index = IndexOfCode(code);

			if (index == -1)
			{
				throw new ArgumentException($"Code {code} does not exist in the list");
			}

			this[index] = new CodeDescriptionPair(code, description);
		}

		public void AddPairsIfNotExist(IEnumerable<ICodeDescription> pairs)
		{
			var hash = new HashSet<ICodeDescription>(Elements, new CodeEqualityComparer());
			foreach (var pair in pairs)
			{
				if (hash.Add(pair))
				{
					Add(pair);
				}
			}
		}

		public void AddPairsEvenIfThisWillCauseDuplicateEntries(IEnumerable<ICodeDescription> pairs)
		{
			foreach (var pair in pairs)
			{
				Add(pair);
			}
		}

		class CodeEqualityComparer : IEqualityComparer<ICodeDescription>
		{
			public bool Equals(ICodeDescription x, ICodeDescription y)
			{
				return StringComparer.OrdinalIgnoreCase.Equals(x.Code, y.Code);
			}

			public int GetHashCode(ICodeDescription obj)
			{
				return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Code);
			}
		}

		#endregion

		#region Remove

		public void RemoveCode(object code)
		{
			EnsureCanRemove();
			int index = IndexOfCode(code);
			if (index != -1)
			{
				Elements.RemoveAt(index);
			}
		}

		#endregion

		#region Overwrite In-Place
		public bool OverwriteDescriptionForCodeInPlace(object code, MultilingualString description)
		{
			if (code == null || description == null)
			{
				return false;
			}
			return OverwriteCodeDescriptionPairInPlace(new CodeDescriptionPair(code, description));
		}
		public bool OverwriteDescriptionForCodeInPlace(object code, string description)
		{
			if (code == null || description == null)
			{
				return false;
			}
			return OverwriteCodeDescriptionPairInPlace(new CodeDescriptionPair(code, description));
		}
		bool OverwriteCodeDescriptionPairInPlace(CodeDescriptionPair updatedCodeDescriptionPair)
		{
			if (updatedCodeDescriptionPair == null)
			{
				return false;
			}
			var indexToOverwrite = IndexOfCode(updatedCodeDescriptionPair.Code);
			if (indexToOverwrite == -1)
			{
				return false;
			}
			EnsureCanRemove();
			EnsureCanInsert();
			RemoveAt(indexToOverwrite);
			Insert(indexToOverwrite, updatedCodeDescriptionPair);
			return true;
		}
		#endregion

		#region Implementation

		void EnsureCanRemove()
		{
			if (isCacheEnabled)
			{
				throw new InvalidOperationException("Cannot remove item from a cached list");
			}
		}

		void EnsureCanAdd()
		{
			if (isCacheEnabled)
			{
				throw new InvalidOperationException("Cannot add item to a cached list");
			}
		}

		void EnsureCanClear()
		{
			if (isCacheEnabled)
			{
				throw new InvalidOperationException("Cannot clear item from a cached list");
			}
		}

		void EnsureCanInsert()
		{
			if (isCacheEnabled)
			{
				throw new InvalidOperationException("Cannot insert item to a cached list");
			}
		}

		new ReadOnlyCodeDescriptionList Elements => base.Elements; // To stop sub classes from adding or removing without going through base method

		protected void AddAreaPairs()
		{
			AddPairsFromClassConstants(typeof(Constants.Area), Constants.Area.GetDescription);
			Sort();
		}

		protected void AddLengthPairs()
		{
			foreach (var unit in Constants.Length.Codes)
			{
				AddPair(unit, Constants.Length.GetDescription(unit, Constants.PluralState.Plural));
			}
			Sort();
		}

		protected void AddDistancePairs()
		{
			foreach (var unit in Constants.Distance.Codes)
			{
				AddPair(unit, Constants.Distance.GetDescription(unit, Constants.PluralState.Plural));
			}
			Sort();
		}

		protected void AddDimensionPairs()
		{
			foreach (var unit in Constants.Dimension.Codes)
			{
				AddPair(unit, Constants.Dimension.GetDescription(unit, Constants.PluralState.Plural));
			}
			Sort();
		}

		protected void AddVolumePairs(Constants.PluralState pluralState)
		{
			foreach (var volumeCode in Constants.Volume.Codes)
			{
				AddPair(volumeCode, Constants.Volume.GetDescription(volumeCode, pluralState));
			}

			Sort();
		}

		protected void AddWeightPairs(Constants.PluralState pluralState)
		{
			AddPairsFromClassConstants(typeof(Constants.Weight), Constants.Weight.GetDescription, pluralState);
			Sort();
		}

		protected void AddPairsFromClassConstants(Type classType, Constants.UnitDescriptionCallback unitDescription, Constants.PluralState pluralState = Constants.PluralState.Plural)
		{
			FieldInfo[] infos = classType.GetFields(BindingFlags.Public | BindingFlags.Static);
			foreach (FieldInfo info in infos)
			{
				if (info.FieldType == typeof(string))
				{
					string code = (string)info.GetValue(null);
					AddPair(code, unitDescription(code, pluralState));
				}
			}
		}

		void AddPrintCopyTypes()
		{
			var types = Enum.GetValues(typeof(PrintCopyType)).OfType<PrintCopyType>();
			foreach (var t in types)
			{
				switch (t)
				{
					case PrintCopyType.ALL:
						AddPair(t.ToString(), SourceGenerated.ResString.GetMultilingualString("62d60c66-426c-4fa7-a28c-0075d4049219", "ALL"));
						break;
					case PrintCopyType.EML:
						AddPair(t.ToString(), Constants.ContactNotifyModeDescriptions.Email);
						break;
					case PrintCopyType.FAX:
						AddPair(t.ToString(), Constants.ContactNotifyModeDescriptions.Fax);
						break;
					case PrintCopyType.PRN:
						AddPair(t.ToString(), Constants.ContactNotifyModeDescriptions.Print);
						break;
					default:
						AddPair(t.ToString());
						break;
				}
			}
		}

		void AddAWBChargeCodes()
		{
			#region SuppressResourceStringsCheckRegion

			AddPair(Constants.AWB.ChargeCodes.AC, "Animal container");
			AddPair(Constants.AWB.ChargeCodes.AS, "Assembly");
			AddPair(Constants.AWB.ChargeCodes.AT, "Attendant");
			AddPair(Constants.AWB.ChargeCodes.AW, "Air Waybill/ shipment record preparation fee");
			AddPair(Constants.AWB.ChargeCodes.BF, "Copies of documents");
			AddPair(Constants.AWB.ChargeCodes.BI, "Import/export documents processing");
			AddPair(Constants.AWB.ChargeCodes.BM, "Withdrawal of shipment after acceptance by carrier");
			AddPair(Constants.AWB.ChargeCodes.BR, "Bank hold fee for bank release");
			AddPair(Constants.AWB.ChargeCodes.CA, "Bonding");
			AddPair(Constants.AWB.ChargeCodes.CB, "Completion/preparation of documents");
			AddPair(Constants.AWB.ChargeCodes.CC, "Manual data entry for customs purposes");
			AddPair(Constants.AWB.ChargeCodes.CD, "Customs/ regulatory handling at destination");
			AddPair(Constants.AWB.ChargeCodes.CF, "Inventory and/or inspection for customs purposes");
			AddPair(Constants.AWB.ChargeCodes.CG, "Electronic processing or transmission of data for customs purposes");
			AddPair(Constants.AWB.ChargeCodes.CH, "Customs/ regulatory handling at origin");
			AddPair(Constants.AWB.ChargeCodes.CI, "Customs overtime fee and other charges");
			AddPair(Constants.AWB.ChargeCodes.CJ, "Removal (carrier warehouse to warehouse)");
			AddPair(Constants.AWB.ChargeCodes.DB, "Disbursement fee collected from consignee for advance charges");
			AddPair(Constants.AWB.ChargeCodes.DC, "Certificate of Origin");
			AddPair(Constants.AWB.ChargeCodes.DD, "Preparation of Cargo manifest");
			AddPair(Constants.AWB.ChargeCodes.DF, "Non-standard distribution channel service fee");
			AddPair(Constants.AWB.ChargeCodes.DG, "Air Waybill cancellation before acceptance");
			AddPair(Constants.AWB.ChargeCodes.DH, "Air Waybill amendment by Cargo Charges Correction Advice");
			AddPair(Constants.AWB.ChargeCodes.DI, "AWB re-waybilling");
			AddPair(Constants.AWB.ChargeCodes.DJ, "Proof of delivery");
			AddPair(Constants.AWB.ChargeCodes.DK, "Release order");
			AddPair(Constants.AWB.ChargeCodes.DV, "Documentation for veterinary and/or phytosanitary purposes");
			AddPair(Constants.AWB.ChargeCodes.EA, "Handling");
			AddPair(Constants.AWB.ChargeCodes.FA, "Airport arrival");
			AddPair(Constants.AWB.ChargeCodes.FB, "Domestic shipments");
			AddPair(Constants.AWB.ChargeCodes.FC, "Charges collect fee");
			AddPair(Constants.AWB.ChargeCodes.FE, "General");
			AddPair(Constants.AWB.ChargeCodes.FF, "Loading/unloading");
			AddPair(Constants.AWB.ChargeCodes.FI, "Weighing");
			AddPair(Constants.AWB.ChargeCodes.GA, "Diplomatic consignment");
			AddPair(Constants.AWB.ChargeCodes.GT, "Government tax");
			AddPair(Constants.AWB.ChargeCodes.HB, "Mortuary");
			AddPair(Constants.AWB.ChargeCodes.HR, "Human remains");
			AddPair(Constants.AWB.ChargeCodes.IA, "Very important cargo (VIC)");
			AddPair(Constants.AWB.ChargeCodes.IN, "Insurance premium");
			AddPair(Constants.AWB.ChargeCodes.JA, "Customs/ regulatory clearance");
			AddPair(Constants.AWB.ChargeCodes.KA, "Handling");
			AddPair(Constants.AWB.ChargeCodes.LA, "Live animals related services");
			AddPair(Constants.AWB.ChargeCodes.LC, "Cleaning");
			AddPair(Constants.AWB.ChargeCodes.LE, "Hotel");
			AddPair(Constants.AWB.ChargeCodes.LF, "Quarantine");
			AddPair(Constants.AWB.ChargeCodes.LG, "Veterinary physical/ documentary inspection");
			AddPair(Constants.AWB.ChargeCodes.LH, "Storage");
			AddPair(Constants.AWB.ChargeCodes.MA, "Miscellaneous - due agent");
			AddPair(Constants.AWB.ChargeCodes.MB, "Miscellaneous - unassigned");
			AddPair(Constants.AWB.ChargeCodes.MC, "Miscellaneous - due carrier");
			AddPair(Constants.AWB.ChargeCodes.MD, "Miscellaneous - due last carrier");
			AddPair(Constants.AWB.ChargeCodes.ME, "Miscellaneous - due last carrier");
			AddPair(Constants.AWB.ChargeCodes.MF, "Miscellaneous - due last carrier");
			AddPair(Constants.AWB.ChargeCodes.MG, "Miscellaneous - due last carrier");
			AddPair(Constants.AWB.ChargeCodes.MH, "Miscellaneous - due last carrier");
			AddPair(Constants.AWB.ChargeCodes.MI, "Miscellaneous - due last carrier");
			AddPair(Constants.AWB.ChargeCodes.MJ, "Miscellaneous - due last carrier");
			AddPair(Constants.AWB.ChargeCodes.MK, "Miscellaneous - due last carrier");
			AddPair(Constants.AWB.ChargeCodes.ML, "Miscellaneous - due last carrier");
			AddPair(Constants.AWB.ChargeCodes.MM, "Miscellaneous - due last carrier");
			AddPair(Constants.AWB.ChargeCodes.MN, "Miscellaneous - due last carrier");
			AddPair(Constants.AWB.ChargeCodes.MO, "Miscellaneous - due issuing carrier");
			AddPair(Constants.AWB.ChargeCodes.MP, "Miscellaneous - due issuing carrier");
			AddPair(Constants.AWB.ChargeCodes.MQ, "Miscellaneous - due issuing carrier");
			AddPair(Constants.AWB.ChargeCodes.MR, "Miscellaneous - due issuing carrier");
			AddPair(Constants.AWB.ChargeCodes.MS, "Miscellaneous - due issuing carrier");
			AddPair(Constants.AWB.ChargeCodes.MT, "Miscellaneous - due issuing carrier");
			AddPair(Constants.AWB.ChargeCodes.MU, "Miscellaneous - due issuing carrier");
			AddPair(Constants.AWB.ChargeCodes.MV, "Miscellaneous - due issuing carrier");
			AddPair(Constants.AWB.ChargeCodes.MW, "Miscellaneous - due issuing carrier");
			AddPair(Constants.AWB.ChargeCodes.MX, "Miscellaneous - due issuing carrier");
			AddPair(Constants.AWB.ChargeCodes.MY, "Fuel surcharge - due issuing carrier");
			AddPair(Constants.AWB.ChargeCodes.MZ, "Miscellaneous - due issuing carrier");
			AddPair(Constants.AWB.ChargeCodes.NE, "Manual entry in the system for shipment processing");
			AddPair(Constants.AWB.ChargeCodes.NS, "Navigation surcharge - due issuing carrier");
			AddPair(Constants.AWB.ChargeCodes.PA, "Handling");
			AddPair(Constants.AWB.ChargeCodes.PB, "Cool/cold room, freezer");
			AddPair(Constants.AWB.ChargeCodes.PK, "Packing/Repacking");
			AddPair(Constants.AWB.ChargeCodes.PU, "Pick-up service");
			AddPair(Constants.AWB.ChargeCodes.RA, "Dangerous goods physical/ documentary inspection");
			AddPair(Constants.AWB.ChargeCodes.RB, "Rejection");
			AddPair(Constants.AWB.ChargeCodes.RC, "Referral of charge");
			AddPair(Constants.AWB.ChargeCodes.RD, "Radio-active room");
			AddPair(Constants.AWB.ChargeCodes.SA, "Delivery service");
			AddPair(Constants.AWB.ChargeCodes.SB, "Delivery notification");
			AddPair(Constants.AWB.ChargeCodes.SC, "Security charge");
			AddPair(Constants.AWB.ChargeCodes.SD, "Delivery service surface charge - destination");
			AddPair(Constants.AWB.ChargeCodes.SE, "Proof of delivery");
			AddPair(Constants.AWB.ChargeCodes.SF, "Delivery Order");
			AddPair(Constants.AWB.ChargeCodes.SI, "Shipment stopped in transit at customer request");
			AddPair(Constants.AWB.ChargeCodes.SO, "Storage - origin");
			AddPair(Constants.AWB.ChargeCodes.SP, "Early release of shipment");
			AddPair(Constants.AWB.ChargeCodes.SR, "Storage - destination");
			AddPair(Constants.AWB.ChargeCodes.SS, "Signature service");
			AddPair(Constants.AWB.ChargeCodes.ST, "State sales tax");
			AddPair(Constants.AWB.ChargeCodes.SU, "Pick-up service surface charge - origin");
			AddPair(Constants.AWB.ChargeCodes.TC, "Stamp");
			AddPair(Constants.AWB.ChargeCodes.TI, "Value Added Tax for import only");
			AddPair(Constants.AWB.ChargeCodes.TR, "Transit handling");
			AddPair(Constants.AWB.ChargeCodes.TV, "Value Added Tax general or for export");
			AddPair(Constants.AWB.ChargeCodes.TX, "General");
			AddPair(Constants.AWB.ChargeCodes.UB, "Disassembly");
			AddPair(Constants.AWB.ChargeCodes.UC, "Adjusting of improperly loaded Unit Load Device");
			AddPair(Constants.AWB.ChargeCodes.UD, "Demurrage");
			AddPair(Constants.AWB.ChargeCodes.UE, "Leasing");
			AddPair(Constants.AWB.ChargeCodes.UF, "Recontouring");
			AddPair(Constants.AWB.ChargeCodes.UG, "Unloading");
			AddPair(Constants.AWB.ChargeCodes.UH, "Handling");
			AddPair(Constants.AWB.ChargeCodes.VA, "Handling");
			AddPair(Constants.AWB.ChargeCodes.VB, "Security (armed guard/escort) handling");
			AddPair(Constants.AWB.ChargeCodes.VC, "Strongroom");
			AddPair(Constants.AWB.ChargeCodes.WA, "Handling");
			AddPair(Constants.AWB.ChargeCodes.XB, "Security");
			AddPair(Constants.AWB.ChargeCodes.XD, "War risk");
			AddPair(Constants.AWB.ChargeCodes.ZA, "Re-warehousing");
			AddPair(Constants.AWB.ChargeCodes.ZB, "General");
			AddPair(Constants.AWB.ChargeCodes.ZC, "Cool/Cold room, freezer");
			AddPair(Constants.AWB.ChargeCodes.ZD, "Contribution towards Sustainable Aviation Fuel (SAF)");
			AddPair(Constants.AWB.ChargeCodes.ZE, "Contribution towards reduction of CO2 emissions");

			#endregion
		}

		void AddLanguages()
		{
			foreach (var defaultLanguge in LanguageHelper.GetDefaultLanguageForOLookUpEditType())
			{
				AddPair(defaultLanguge.Key, defaultLanguge.Value);
			}

			var languages = LanguageHelper.GetSecurityLanguageReferences();
			foreach (var language in languages)
			{
				AddPairIfNotExist(language.FullLanguageCode, language.Description);
			}
		}

		MultilingualString CustomLabelDescription(MultilingualString prefix, MultilingualString caption)
		{
			return MultilingualString.Join(" - ", prefix, caption);
		}

		#region class CodeComparer

		class CodeComparer : IComparer<ICodeDescription>
		{
			public int Compare(ICodeDescription x, ICodeDescription y)
			{
				return string.Compare(x.Code, y.Code, StringComparison.OrdinalIgnoreCase);
			}
		}

		#endregion

		#region class DescriptionComparer

		public class DescriptionComparer : IComparer<ICodeDescription>
		{
			public int Compare(ICodeDescription x, ICodeDescription y)
			{
				int result;
				if (x is IMultilingualDescription && y is IMultilingualDescription && Res.CurrentLanguage == Res.DefaultLanguage)
				{
					result = string.Compare(((IMultilingualDescription)x).MultilingualDescription.GetUnresolvedString(), ((IMultilingualDescription)y).MultilingualDescription.GetUnresolvedString());
				}
				else
				{
					result = string.Compare(x.Description, y.Description);
				}
				if (result == 0)
				{
					result = string.Compare(x.Code, y.Code);
				}
				return result;
			}
		}

		#endregion

		#endregion

		#region Operator Overloads

		public static CodeDescriptionPairList operator +(CodeDescriptionPairList x, CodeDescriptionPairList y)
		{
			if (x == null)
			{
				throw new ArgumentNullException(nameof(x));
			}

			if (y == null)
			{
				throw new ArgumentNullException(nameof(y));
			}

			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddRange(x);
			result.AddRange(y);
			return result;
		}

		#endregion

		#region IList Members

		public void RemoveAt(int index)
		{
			EnsureCanRemove();
			Elements.RemoveAt(index);
		}

		public void Insert(int index, object value)
		{
			EnsureCanInsert();
			Elements.Insert(index, (ICodeDescription)value);
		}

		int IList.Add(object value)
		{
			return Add((ICodeDescription)value);
		}

		void IList.Clear()
		{
			Clear();
		}

		object IList.this[int index]
		{
			get { return this[index]; }
			set { this[index] = (ICodeDescription)value; }
		}

		void IList.Remove(object value)
		{
			Remove((ICodeDescription)value);
		}

		#endregion

		#region ICachedValueManager Members

		bool ICachedValueManager.IsCacheEnabled
		{
			set { isCacheEnabled = value; }
		}
		bool isCacheEnabled;

		#endregion

		#region IAdditionalInformationWithSetter Members

		string IAdditionalInformation.AdditionalInformation => additionalInformation;

		void IAdditionalInformationWithSetter.SetAdditionalInformation(string information)
		{
			additionalInformation = information;
		}
		string additionalInformation;

		#endregion
	}
}
