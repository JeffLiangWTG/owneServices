using System.Linq;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class OrgSupplierPartValidation : Customs.Business.OrgSupplierPartValidation
	{
		public OrgSupplierPartValidation(AUOrgSupplierPart parent)
			: base(parent)
		{
		}

		protected new AUOrgSupplierPart Parent
		{
			get { return (AUOrgSupplierPart)base.Parent; }
		}

		protected override void CheckOP_StockKeepingUnit()
		{
			base.CheckOP_StockKeepingUnit();
			var targetInfo = Parent.OP_StockKeepingUnitInfo;
			if (!targetInfo.HasNotifications())
			{
				var partUnits = Parent.PartUnits;
				var stockKeepingUQ = Parent.OP_StockKeepingUnit;
				var warningList = new System.Collections.Generic.List<string>();
				foreach (var pivot in Parent.GetPivots<CusClassPartPivot>(Core.Constants.CountryCodes.Australia))
				{
					var uq = pivot.TariffCustomsUnitQuantity.Trim().ToUpper();
					if (!uq.IsEmpty && uq != stockKeepingUQ)
					{
						var message = partUnits.GetConversionErrors(Parent.OP_StockKeepingUnit, uq);
						if (!string.IsNullOrEmpty(message))
						{
							warningList.Add(message);
						}
					}
				}
				if (warningList.Any())
				{
					var warningMessage = warningList.Distinct().Aggregate((x, y) => x + "\r\n" + y);
					targetInfo.AddWarning(warningMessage);
				}
			}
		}
	}
}
