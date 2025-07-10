using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using PkgUnit = Enterprise.Core.Constants.PkgUnit;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRBillsynchroniser : BusinessObjectSynchroniser
	{
		public JPAFRBillsynchroniser(JPAFRBills destination, ForwardingShipment source)
			: base(destination, source)
		{
			var header = destination.Header;
			this.consolSource = header == null ? null : header.Consol;
		}

		public new JPAFRBills Destination
		{
			get { return (JPAFRBills)base.Destination; }
		}

		public new ForwardingShipment Source
		{
			get { return (ForwardingShipment)base.Source; }
		}

		readonly ForwardingConsol consolSource;

		#region Implementation

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.JPB_BillNumberInfo, GetBillNumber, GetInfosAffectingBillNumber));
			Synchronisers.Add(new FieldSynchroniser(Destination.JPB_RL_NKOriginInfo, Source.JS_RL_NKOriginInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.JPB_RL_NKDeliveryInfo, Source.JS_RL_NKDestinationInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.JPB_RL_NKFinalDestinationInfo, Source.JS_RL_NKDestinationInfo));
			harmonizedCodeSynchroniser = new FieldSynchroniser(Destination.JPB_TariffInfo, GetHarmonizedCode, GetInfosAffectingHarmonizedCode);
			Synchronisers.Add(harmonizedCodeSynchroniser);
			Synchronisers.Add(new FieldSynchroniser(Destination.JPB_GrossWeightInfo, GetGrossWeight, GetInfosAffectingGrossWeight));
			Synchronisers.Add(new FieldSynchroniser(Destination.JPB_GrossWeightUQInfo, GetGrossWeightUQ, GetInfosAffectingGrossWeightUQ));
			Synchronisers.Add(new FieldSynchroniser(Destination.JPB_VolumeInfo, GetVolume, GetInfosAffectingVolume));
			Synchronisers.Add(new FieldSynchroniser(Destination.JPB_VolumeUQInfo, GetVolumeUQ, GetInfosAffectingVolumeUQ));
			Synchronisers.Add(new FieldSynchroniser(Destination.JPB_ManifestQtyInfo, GetManifestQty, GetSourcevaluesAffectingPackQty));
			Synchronisers.Add(new FieldSynchroniser(Destination.JPB_ManifestUQInfo, GetManifestUQ, GetInfosAffectingManifestUQ));
			goodsOriginSynchroniser = new FieldSynchroniser(Destination.JPB_RN_NKGoodsOriginInfo, GetGoodsOrigin, GetInfosAffectingGoodsOrigin);
			Synchronisers.Add(goodsOriginSynchroniser);
			Synchronisers.Add(new FieldSynchroniser(Destination.JPB_MarksAndNumbersInfo, Source.JS_MarksAndNumbersInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.JPB_GoodsDescriptionInfo, GetGoodsDescription, GetInfosAffectingGoodsDescription));
			undgSynchroniser = new FieldSynchroniser(Destination.JPB_DGInfo, GetUNDG, GetInfosAffectingUNDG);
			Synchronisers.Add(undgSynchroniser);

			Synchronisers.Add(new JobDocAddressSynchroniser(Destination.Consignee, Source.ConsigneeDocumentaryAddress));
			Synchronisers.Add(new JobDocAddressSynchroniser(Destination.Consignor, Source.ConsignorDocumentaryAddress));
			Synchronisers.Add(new JobDocAddressSynchroniserWithFallback(Destination.NotifyParty1, Source.NotifyPartyDocumentaryAddress, Source.ConsigneeDocumentaryAddress));
			Synchronisers.Add(new JobDocAddressSynchroniser(Destination.NotifyParty2, Source.NotifyParty2DocumentaryAddress));

			Synchronisers.Add(new JPAFRContainerCollectionSynchroniser(Source, Destination, consolSource));
			Synchronisers.Add(new NotificationForwardingPartyCollectionSynchroniser(Source, Destination));
			HookToEventsAffectingPacklinesInSortOrderCalculation();
		}

		FieldSynchroniser harmonizedCodeSynchroniser;
		FieldSynchroniser goodsOriginSynchroniser;
		FieldSynchroniser undgSynchroniser;

		IZType GetGoodsDescription()
		{
			return !Source.DetailedGoodsDescriptionNoteText.IsEmpty
				? Source.DetailedGoodsDescriptionNoteText
				: Source.JS_GoodsDescription;
		}

		IEnumerable<ZPropertyInfo> GetInfosAffectingGoodsDescription()
		{
			yield return Source.DetailedGoodsDescriptionNoteTextInfo;
			yield return Source.JS_GoodsDescriptionInfo;
		}

		IZType GetUNDG()
		{
			return GetUNDGSubstanceFromPacklineList(PacklinesInSortOrder);
		}

		internal static ZGuid GetUNDGSubstanceFromPacklineList(IEnumerable<PackLine> sourcePacklinesInSortOrder)
		{
			var foundUNDG = sourcePacklinesInSortOrder.Select((PackLine x) =>
				{
					MasterFiles.Business.UNDGDataItem result = null;

					if (x.UNDGs.Count > 0)
					{
						var undg = x.UNDGs[0];
						if (!undg.DI_DG.IsEmpty)
						{
							result = undg;
						}
					}

					return result;
				}).FirstOrDefault(y => y != null);
			return foundUNDG?.UNDGSubstance?.PK ?? ZGuid.Empty;
		}

		IEnumerable<ZPropertyInfo> GetInfosAffectingUNDG()
		{
			foreach (var info in GetInfosAffectingPacklinesInSortOrder())
			{
				yield return info;
			}

			foreach (ForwardingPackLine packLine in Source.OuterPackLines)
			{
				foreach (var undg in packLine.UNDGs)
				{
					yield return undg.DI_DGInfo;
				}
			}
		}

		IZType GetHarmonizedCode()
		{
			var packline = GetMostRelevantPackline();
			return packline == null ? ZString.Empty : packline.JL_HarmonisedCode.Replace(".", "");
		}

		IEnumerable<ZPropertyInfo> GetInfosAffectingHarmonizedCode()
		{
			foreach (var info in GetInfosAffectingPacklinesInSortOrder())
			{
				yield return info;
			}
		}

		IZType GetGoodsOrigin()
		{
			var packline = GetMostRelevantPackline();
			return packline == null ? ZString.Empty : packline.JL_RN_NKOrigin;
		}

		IEnumerable<ZPropertyInfo> GetInfosAffectingGoodsOrigin()
		{
			foreach (var info in GetInfosAffectingPacklinesInSortOrder(ForwardingPackLine.Schema.JL_RN_NKOrigin))
			{
				yield return info;
			}
		}

		IZType GetGrossWeight()
		{
			return ConvertWeightIfNecessary(Source.JS_ActualWeight, Source.JS_UnitOfWeight);
		}

		internal static ZDecimal ConvertWeightIfNecessary(ZDecimal sourceVolumne, ZString sourceUnit)
		{
			var result = sourceVolumne;
			if (sourceUnit != Core.Constants.Weight.Pounds && sourceUnit != Core.Constants.Weight.Tonnes && Core.Constants.Weight.ContainsCode(sourceUnit))
			{
				result = Core.Constants.Weight.Convert(result, sourceUnit, Core.Constants.Weight.Kilograms);
			}
			return result;
		}

		IEnumerable<ZPropertyInfo> GetInfosAffectingGrossWeight()
		{
			yield return Source.JS_UnitOfWeightInfo;
			yield return Source.JS_ActualWeightInfo;
		}

		IZType GetGrossWeightUQ()
		{
			return TranslateWeightUQ(Source.JS_UnitOfWeight);
		}

		internal static ZString TranslateWeightUQ(ZString sourceUQ)
		{
			var result = sourceUQ;
			if (result != Core.Constants.Weight.Pounds && result != Core.Constants.Weight.Tonnes && Core.Constants.Weight.ContainsCode(result))
			{
				result = Core.Constants.Weight.Kilograms;
			}
			return result;
		}

		IEnumerable<ZPropertyInfo> GetInfosAffectingGrossWeightUQ()
		{
			yield return Source.JS_UnitOfWeightInfo;
		}

		IZType GetVolume()
		{
			return ConvertVolumeIfNecessary(Source.JS_ActualVolume, Source.JS_UnitOfVolume);
		}

		internal static ZDecimal ConvertVolumeIfNecessary(ZDecimal sourceVolumne, ZString sourceUnit)
		{
			var result = sourceVolumne;
			if (sourceUnit != Core.Constants.Volume.CubicFeet && Core.Constants.Volume.ContainsCode(sourceUnit))
			{
				result = Core.Constants.Volume.Convert(result, sourceUnit, Core.Constants.Volume.CubicMetres);
			}
			return result;
		}

		IEnumerable<ZPropertyInfo> GetInfosAffectingVolume()
		{
			yield return Source.JS_UnitOfVolumeInfo;
			yield return Source.JS_ActualVolumeInfo;
		}

		IZType GetVolumeUQ()
		{
			return TranslateVolumeUQ(Source.JS_UnitOfVolume);
		}

		internal static ZString TranslateVolumeUQ(ZString sourceVolumeUQ)
		{
			var result = sourceVolumeUQ;
			if (result != Core.Constants.Volume.CubicFeet && Core.Constants.Volume.ContainsCode(result))
			{
				result = Core.Constants.Volume.CubicMetres;
			}
			return result;
		}

		IEnumerable<ZPropertyInfo> GetInfosAffectingVolumeUQ()
		{
			yield return Source.JS_UnitOfVolumeInfo;
		}

		IZType GetManifestQty()
		{
			var refPack = CusRefPacksHelper.LoadRefPack(Destination.Factory, Source.JS_F3_NKPackType, RPTypeList.Codes.AFRManifest, Core.Constants.CountryCodes.Japan);
			return refPack == null ? Source.JS_OuterPacks : new ZDecimal(refPack.ConversionFactor * Source.JS_OuterPacks).ToZInt();
		}

		IEnumerable<ZPropertyInfo> GetSourcevaluesAffectingPackQty()
		{
			yield return Source.JS_OuterPacksInfo;
			yield return Source.JS_F3_NKPackTypeInfo;
		}

		IZType GetManifestUQ()
		{
			var refPack = CusRefPacksHelper.LoadRefPack(Destination.Factory, Source.JS_F3_NKPackType, RPTypeList.Codes.AFRManifest, Core.Constants.CountryCodes.Japan);
			return refPack == null ? TranslateManifestUQ(Source.JS_F3_NKPackType) : refPack.RP_CustomsPack;
		}

		internal static ZString TranslateManifestUQ(ZString sourceManifestUQ)
		{
			var result = sourceManifestUQ;
			switch (result)
			{
				case PkgUnit.Bag:
					result = PackageTypeList.Codes.Bag;
					break;
				case PkgUnit.BaleCompressed:
					result = PackageTypeList.Codes.BaleCompressed;
					break;
				case PkgUnit.BaleUncompressed:
					result = PackageTypeList.Codes.BaleNonCompressed;
					break;
				case PkgUnit.Basket:
					result = PackageTypeList.Codes.Basket;
					break;
				case PkgUnit.Box:
					result = PackageTypeList.Codes.Box;
					break;
				case PkgUnit.Bundle:
					result = PackageTypeList.Codes.Bundle;
					break;
				case PkgUnit.Carton:
					result = PackageTypeList.Codes.Carton;
					break;
				case PkgUnit.Case:
					result = PackageTypeList.Codes.Case;
					break;
				case PkgUnit.Coil:
					result = PackageTypeList.Codes.Coil;
					break;
				case PkgUnit.Container:
					result = PackageTypeList.Codes.Container;
					break;
				case PkgUnit.Crate:
					result = PackageTypeList.Codes.Crate;
					break;
				case PkgUnit.Cylinder:
					result = PackageTypeList.Codes.Cylinder;
					break;
				case PkgUnit.Drum:
					result = PackageTypeList.Codes.Drum;
					break;
				case PkgUnit.Keg:
					result = PackageTypeList.Codes.Keg;
					break;
				case PkgUnit.Package:
					result = PackageTypeList.Codes.Package;
					break;
				case PkgUnit.Pail:
					result = PackageTypeList.Codes.Pail;
					break;
				case PkgUnit.Pallet:
					result = PackageTypeList.Codes.PalletAndPackage;
					break;
				case PkgUnit.Piece:
				case JPPkgUnit.Pieces:
					result = PackageTypeList.Codes.Piece;
					break;
				case PkgUnit.Reel:
					result = PackageTypeList.Codes.Reel;
					break;
				case PkgUnit.Roll:
					result = PackageTypeList.Codes.Roll;
					break;
				case PkgUnit.Sheet:
					result = PackageTypeList.Codes.Sheet;
					break;
				case PkgUnit.Skid:
					result = PackageTypeList.Codes.Skid;
					break;
				case JPPkgUnit.Tin:
					result = PackageTypeList.Codes.Tin;
					break;
			}
			return result;
		}

		IEnumerable<ZPropertyInfo> GetInfosAffectingManifestUQ()
		{
			yield return Source.JS_F3_NKPackTypeInfo;
		}

		IZType GetBillNumber()
		{
			var result = ZString.Empty;
			var header = Destination.Header;
			if (Destination.IsBillAlreadyRegistered)
			{
				result = Destination.JPB_BillNumber;
			}
			else
			{
				var companyPK = header == null ? Guid.Empty : header.RegistryCompanyPK;
				result = Source.GetTargetBillNumber(companyPK);
			}
			return result;
		}

		IEnumerable<ZPropertyInfo> GetInfosAffectingBillNumber()
		{
			yield return consolSource.JK_AgentTypeInfo;
			yield return Source.JS_HouseBillInfo;
			yield return Destination.JPB_ReleaseStatusInfo;
			yield return Destination.JPB_JPH_HeaderInfo;
			if (Destination.Header != null)
			{
				yield return Destination.Header.JPH_GB_BranchInfo;
			}
		}

		ForwardingPackLine GetMostRelevantPackline()
		{
			return PacklinesInSortOrder.FirstOrDefault(x => !x.JL_HarmonisedCode.IsEmpty) ?? PacklinesInSortOrder.FirstOrDefault();
		}

		IEnumerable<ZPropertyInfo> GetInfosAffectingPacklinesInSortOrder(params string[] propertyNames)
		{
			var list = new HashSet<string>(new[] { ForwardingPackLine.Schema.JL_LinePrice, ForwardingPackLine.Schema.JL_ActualWeight, ForwardingPackLine.Schema.JL_PackageCount, ForwardingPackLine.Schema.JL_HarmonisedCode });
			if (propertyNames != null)
			{
				list.UnionWith(propertyNames);
			}
			foreach (var info in GetInfos(Source.OuterPackLines, list.ToArray()))
			{
				yield return info;
			}
		}

		void HookToEventsAffectingPacklinesInSortOrderCalculation()
		{
			foreach (ForwardingPackLine packline in Source.OuterPackLines)
			{
				HookPackline(packline);
			}
			Source.OuterPackLines.CountChanged += Packlines_CountChanged;
		}

		void UnHookToEventsAffectingPacklinesInSortOrderCalculation()
		{
			foreach (ForwardingPackLine packline in Source.OuterPackLines)
			{
				UnHookPackline(packline);
			}
			Source.OuterPackLines.CountChanged -= Packlines_CountChanged;
		}

		void UnHookPackline(ForwardingPackLine packline)
		{
			packline.JL_LinePriceInfo.ValueChanged -= PacklinesInSortOrderCalculation_ValueChanged;
			packline.JL_ActualWeightInfo.ValueChanged -= PacklinesInSortOrderCalculation_ValueChanged;
			packline.JL_PackageCountInfo.ValueChanged -= PacklinesInSortOrderCalculation_ValueChanged;
			packline.JL_HarmonisedCodeInfo.ValueChanged -= PacklinesInSortOrderCalculation_ValueChanged;
			packline.UNDGs.CollectionCountChange -= UNDGs_CountChanged;
		}

		void HookPackline(ForwardingPackLine packline)
		{
			UnHookPackline(packline);
			packline.JL_LinePriceInfo.ValueChanged += PacklinesInSortOrderCalculation_ValueChanged;
			packline.JL_ActualWeightInfo.ValueChanged += PacklinesInSortOrderCalculation_ValueChanged;
			packline.JL_PackageCountInfo.ValueChanged += PacklinesInSortOrderCalculation_ValueChanged;
			packline.JL_HarmonisedCodeInfo.ValueChanged += PacklinesInSortOrderCalculation_ValueChanged;
			packline.UNDGs.CollectionCountChange += UNDGs_CountChanged;
		}

		void UNDGs_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			UpdateInfoEventsAndReSynchronise(undgSynchroniser);
		}

		void Packlines_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				HookPackline((ForwardingPackLine)e.BizObject);
			}
			else if (e.ItemRemoved)
			{
				UnHookPackline((ForwardingPackLine)e.BizObject);
			}
			PacklinesInSortOrderCalculation_ValueChanged(sender, e);
		}

		void PacklinesInSortOrderCalculation_ValueChanged(object sender, EventArgs e)
		{
			shouldCalculatePacklinesInSortOrder = true;
			UpdateInfoEventsAndReSynchronise(harmonizedCodeSynchroniser);
			UpdateInfoEventsAndReSynchronise(goodsOriginSynchroniser);
			UpdateInfoEventsAndReSynchronise(undgSynchroniser);
		}

		bool shouldCalculatePacklinesInSortOrder = true;

		ForwardingPackLine[] PacklinesInSortOrder
		{
			get
			{
				if (shouldCalculatePacklinesInSortOrder)
				{
					shouldCalculatePacklinesInSortOrder = false;
					packlinesInSortOrderCached = Source.OuterPackLines.OfType<ForwardingPackLine>().OrderByDescending(x => GetPacklineOrderKey(x)).ToArray();
				}
				return packlinesInSortOrderCached;
			}
		}
		ForwardingPackLine[] packlinesInSortOrderCached;

		internal static ZString GetPacklineOrderKey(PackLine packline)
		{
			var result = new ZStringBuilder();
			result.Append(packline.JL_LinePrice.ToString(4).PadLeft(19));
			result.Append(packline.JL_ActualWeight.ToString(3).PadLeft(9));
			result.Append(packline.JL_PackageCount.ToString().PadLeft(10));
			result.Append(packline.JL_HarmonisedCode.PadLeft(PackLine.Schema.JL_HarmonisedCodeMaxLength));
			return result.ToString();
		}

		IEnumerable<ZPropertyInfo> GetInfos(IBusinessObjectCollection collection, params string[] propertyNames)
		{
			foreach (BusinessObject bizObj in collection)
			{
				foreach (var propertyName in propertyNames)
				{
					if (bizObj.ZPropertyInfoHash.ContainsKey(propertyName))
					{
						yield return bizObj.ZPropertyInfoHash[propertyName];
					}
				}
			}
		}

		protected override void UnHookSynchronisers()
		{
			UnHookToEventsAffectingPacklinesInSortOrderCalculation();
			base.UnHookSynchronisers();
		}

		#endregion
	}
}
