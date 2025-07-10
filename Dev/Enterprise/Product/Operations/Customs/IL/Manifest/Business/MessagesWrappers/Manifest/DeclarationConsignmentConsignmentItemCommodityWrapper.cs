using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using CargoWise.Types;
using Enterprise.Customs.IL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class DeclarationConsignmentConsignmentItemCommodityWrapper : IDeclarationConsignmentConsignmentItemCommodity
	{
		DeclarationConsignmentConsignmentItemCommodityWrapper(AsycudaPackedItem asycudaPackedItem)
		{
			this.asycudaPackedItem = Argument.NotNull(asycudaPackedItem, nameof(asycudaPackedItem));
		}

		public static IDeclarationConsignmentConsignmentItemCommodity NewOrNull(AsycudaPackedItem asycudaPackedItem) => asycudaPackedItem == null ? null : new DeclarationConsignmentConsignmentItemCommodityWrapper(asycudaPackedItem);

		public ITextType CargoDescription => TextTypeWrapper.NewOrNull(asycudaPackedItem.API_GoodsDescription);

		public ICollection<IDeclarationConsignmentConsignmentItemCommodityClassification> Classification => GetClassification();

		public ICodeType CategoryCode => null;

		public ICodeType CategoryQualifierCode => null;

		public IIDType Id => (asycudaPackedItem.Header?.AMA_TransportMode ?? string.Empty) == Core.Constants.TransportModes.Road ? IDTypeWrapper.NewOrNull(asycudaPackedItem.Header?.TransportMeans.Cast<TransportMean>().FirstOrDefault(t => t.JW_RL_NKDiscPort.StartsWith(Core.Constants.CountryCodes.Israel))?.JW_Vessel ?? ZString.Empty) : null;

		public ICodeType IdentityQualifierCode => null;

		public ICodeType IDTypeCode => null;

		public ICollection<IDeclarationConsignmentConsignmentItemCommodityTemperature> Temperature
			=> new List<IDeclarationConsignmentConsignmentItemCommodityTemperature>().AsReadOnly();

		ICollection<IDeclarationConsignmentConsignmentItemCommodityCommodityRelatedPackaging> IDeclarationConsignmentConsignmentItemCommodity.CommodityRelatedPackaging
			=> asycudaPackedItem.UNDGs.Cast<UNDGDataItem>()
			.Select(dataItemUNDG => DeclarationConsignmentItemCommodityCommodityRelatedPackagingWrapper.NewOrNull(dataItemUNDG))
			.WhereNotNull().ToList().AsReadOnly();

		ICollection<IDeclarationConsignmentConsignmentItemCommodityClassification> GetClassification()
		{
			var list = new List<IDeclarationConsignmentConsignmentItemCommodityClassification>();
			list.Add(DeclarationConsignmentConsignmentItemCommodityClassificationWrapper.NewOrNull(typeCode: IL.Business.Constants.CustomsDeclaration.ClassificationIdentificationTypeCodeRegular, id: asycudaPackedItem.API_Tariff.Left(4)));

			if (asycudaPackedItem.API_PackStatus == ILPackStatusList.Codes.D)
			{
				foreach (var di in asycudaPackedItem.UNDGs.Cast<UNDGDataItem>().Where(dg => dg.UNDGSubstance != null))
				{
					list.Add(DeclarationConsignmentConsignmentItemCommodityClassificationWrapper.NewOrNull(typeCode: IL.Business.Constants.CustomsDeclaration.ClassificationIdentificationTypeCodeDangerous, id: di.UNDGSubstance.DG_UNNO));
				}
			}
			return list.WhereNotNull().ToList().AsReadOnly();
		}

		readonly AsycudaPackedItem asycudaPackedItem;
	}
}
