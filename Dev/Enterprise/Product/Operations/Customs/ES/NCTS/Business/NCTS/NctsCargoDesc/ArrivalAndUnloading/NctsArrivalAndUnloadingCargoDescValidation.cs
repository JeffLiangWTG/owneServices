using System.Linq;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsArrivalAndUnloadingCargoDescValidation : EU.NCTS.Business.NctsArrivalAndUnloadingCargoDescValidation
	{
		public NctsArrivalAndUnloadingCargoDescValidation(NctsArrivalAndUnloadingCargoDesc parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			if (Parent.MoveHeader.IsUnloadingMovementHeader)
			{
				Parent.ClearRowNotifications();
				CheckDataMissingForUnloadedItemsWithDifferences();
			}
		}

		void CheckDataMissingForUnloadedItemsWithDifferences()
		{
			if (Parent.HasDifferences)
			{
				var arrivalGoodsItem = Parent.MoveHeader.Header?.ArrivalMovementHeader?.GoodsItems.Cast<NctsArrivalAndUnloadingCargoDesc>().FirstOrDefault(x => x.BY_LineNo == Parent.BY_LineNo);
				if (arrivalGoodsItem != null)
				{
					if (Parent.SupportingDocuments.Count == 0 && arrivalGoodsItem.SupportingDocuments.Count != 0)
					{
						Parent.AddRowMessageError(Res.GetString("F432CB4B-8746-45CB-8CAC-9C2E541CC1EB", "You have not entered Unloaded Documents"));
					}

					if (Parent.Containers.Count == 0 && arrivalGoodsItem.Containers.Count != 0)
					{
						Parent.AddRowMessageError(Res.GetString("7E574C4C-74FB-4160-A3C5-DEC0FC805F82", "You have not entered Unloaded Containers"));
					}

					if (Parent.Packages.Count == 0 && arrivalGoodsItem.Packages.Count != 0)
					{
						Parent.AddRowMessageError(Res.GetString("550D2BF5-EAA6-42A3-905D-65FA4805BEE6", "You have not entered Unloaded Packages"));
					}
				}
			}
		}

		protected new NctsArrivalAndUnloadingCargoDesc Parent => (NctsArrivalAndUnloadingCargoDesc)base.Parent;
	}
}
