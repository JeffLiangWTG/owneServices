using System.Text.RegularExpressions;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class AddInfoJobComInvoiceLineValidation : CAAddInfoValidation
	{
		public AddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected new AddInfoJobComInvoiceLine Parent
		{
			get { return (AddInfoJobComInvoiceLine)base.Parent; }
		}

		protected AddInfoJobComInvoiceLineLookups Lookups
		{
			get { return Parent.Lookups; }
		}

		protected override void CheckCA_OriginalLineNo()
		{
			base.CheckCA_OriginalLineNo();
			var declaration = Parent.Declaration;
			if (declaration != null && (declaration.IsB2Adjustments || declaration.IsB3X))
			{
				if (Parent.CA_IsAccountForLine)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_OriginalLineNoInfo);
					if (!Parent.CA_OriginalLineNo.IsNumbersOnlyOrEmpty)
					{
						Parent.CA_OriginalLineNoInfo.AddWarning(AsAccountedOriginalLineNoWarning);
					}
				}
				else if (!Regex.Match(Parent.CA_OriginalLineNo, @"^\d+(\/SL)?$").Success)
				{
					Parent.CA_OriginalLineNoInfo.AddWarning(AsClaimedOriginalLineNoWarning);
				}
			}
		}

		public const string AsAccountedOriginalLineNoWarning = "Original Line No. should be composed of digits.";
		public const string AsClaimedOriginalLineNoWarning = "Original Line No. should be digits or digits + '/SL'.";
	}
}
