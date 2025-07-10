using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;

namespace Enterprise.Customs.ES.Business.Documents.DocDataObjects
{
	public class TotalATRCertificateItem : NonPersistentBusinessObject, IATRCertificateItem
	{
		public TotalATRCertificateItem(CusEntryHeader entryHeader)
		{
			Argument.NotNull(entryHeader, nameof(entryHeader));

			totalItemsCount = entryHeader.PackagesCount;
			totalItemsWeight += entryHeader.MergedLines.Sum(line => line.EffectiveGrossWeight.InKilogramsSafe);
		}

		readonly ZInt totalItemsCount;
		readonly ZDecimal totalItemsWeight;

		#region LineNumber

		public ZString LineNumber
		{
			get => (lineNumber ?? (lineNumber = ZString.Empty)).Value;
			set => SetNonPersistentPropertyValue(LineNumberInfo, ref lineNumber, value);
		}
		ZString? lineNumber;

		public ZPropertyInfo LineNumberInfo => GetZPropertyInfo(nameof(LineNumber));

		#endregion

		#region GoodsDescription

		public ZString GoodsDescription
		{
			get => (goodsDescription ?? (goodsDescription = GetGoodsDescription())).Value;
			set => SetNonPersistentPropertyValue(GoodsDescriptionInfo, ref goodsDescription, value);
		}
		ZString? goodsDescription;

		public ZPropertyInfo GoodsDescriptionInfo => GetZPropertyInfo(nameof(GoodsDescription));

		#endregion

		#region Weight

		public ZString Weight
		{
			get => (weight ?? (weight = GetWeight())).Value;
			set => SetNonPersistentPropertyValue(WeightInfo, ref weight, value);
		}
		ZString? weight;

		public ZPropertyInfo WeightInfo => GetZPropertyInfo(nameof(Weight));

		#endregion

		#region Volume

		public ZString Volume
		{
			get => (volume ?? (volume = ZString.Empty)).Value;
			set => SetNonPersistentPropertyValue(VolumeInfo, ref volume, value);
		}
		ZString? volume;

		public ZPropertyInfo VolumeInfo => GetZPropertyInfo(nameof(Volume));

		#endregion

		ZString GetGoodsDescription()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Spanish))
			{
				var description = new ZStringBuilder();
				description.Append(FormattableString.Invariant($"{System.Environment.NewLine}"));
				description.Append(FormattableString.Invariant($"{DocumentsResStrings.GoodsDescription.TotalGrossWeight} {Weight}{System.Environment.NewLine}"));
				description.Append(FormattableString.Invariant($"{DocumentsResStrings.GoodsDescription.TotalCaption} {totalItemsCount} {DocumentsResStrings.GoodsDescription.PackagesCaption}"));

				return description.ToStringWithNewLineBetweenAppends();
			}
		}

		ZString GetWeight() => DecimalHelper.GetESStringDecimalFormat(totalItemsWeight) + " KG";
	}
}
