using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class C21EMessageBuilder : ExportsMessageBuilder
	{
		public C21EMessageBuilder(CusEntryHeader cusEntryHeader, ErrorCollector errorCollector, string functionCode)
			: base(cusEntryHeader, errorCollector, functionCode)
		{
		}

		protected override void PopulateTotalPackageQuantity()
		{
			if (TotalPackageQuantity > 0 && IsMovementThroughAnInventoryLinkingLocation())
			{
				decMessage.TotalPackageQuantity = new DeclarationTotalPackageQuantityType
				{
					Value = TotalPackageQuantity
				};
			}
		}

		protected override void PopulateOrigins(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateConsignor(DeclarationConsignment consignment, IOrganisation consignorProvider)
		{
			if (consignorProvider != null)
			{
				if (!consignorProvider.ID.IsEmpty && !consignorProvider.IsForeignEori)
				{
					consignment.Consignor = new DeclarationConsignmentConsignor
					{
						ID = new ConsignorIdentificationIDType { Value = consignorProvider.ID }
					};
				}
				else
				{
					consignment.Consignor = new DeclarationConsignmentConsignor
					{
						Name = new ConsignorNameTextType { Value = consignorProvider.Name },
						Address = GetIOrgAddress<DeclarationConsignmentConsignorAddress>(consignorProvider)
					};
				}
			}
		}
	}
}
