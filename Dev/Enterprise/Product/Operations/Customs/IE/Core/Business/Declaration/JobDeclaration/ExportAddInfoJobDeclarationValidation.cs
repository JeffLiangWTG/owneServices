using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public partial class ExportJobDeclarationValidation
	{
		protected override void CheckJE_BorderTransportMeans()
		{
			base.CheckJE_BorderTransportMeans();
			var declaration = Parent;

			var instructions = declaration.CustomsEntryInstructions;
			var hasExportOrReExportEntry = instructions.Any(instruction => instruction.IsB1Declaration && instruction.EntryHeader != null);
			var hasCustomsWarehousingOfUnionGoodsEntry = !hasExportOrReExportEntry && instructions.Any(instruction => instruction.IsCustomsWarehousingOfUnionGoods && instruction.EntryHeader != null);

			if (hasExportOrReExportEntry)
			{
				if (!(declaration.IsRoad || declaration.IsMail || declaration.IsFixedInstallation))
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_BorderTransportMeansInfo);
				}
			}
			else if (hasCustomsWarehousingOfUnionGoodsEntry && !(declaration.IsMail || declaration.IsFixedInstallation))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_BorderTransportMeansInfo);
			}
		}
	}
}
