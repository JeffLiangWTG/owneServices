using System.Linq;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusEntryLineValidation : EU.Business.Declaration.CusEntryLineValidation
	{
		public CusEntryLineValidation(EU.Business.Declaration.CusEntryLine parent) : base(parent)
		{
		}

		public new CusEntryLine Parent => (CusEntryLine)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckRuleNAT_175();
			CheckRuleNAT_184();
			CheckRuleNAT_188();
		}

		void CheckRuleNAT_175()
		{
			var rowMessageError = Res.GetString("7B914BAF-99D6-4931-AC91-AF1137B1AD1A", "[NAT_175] If one invoice line has concession C07, all other lines must have concession C07 (negligible value) also.");
			var parent = Parent;
			if (parent.Validation.ValidationDecider is IEntryLineValidationDecider entryLineValidationDecider && entryLineValidationDecider.IsRuleNAT_175Active  && !parent.HasNegligibleValueProcedure && parent.Header is CusEntryHeader header && header.MergedLines.Cast<CusEntryLine>().Any(x => x.HasNegligibleValueProcedure))
			{
				parent.AddRowMessageError(rowMessageError);
			}
			else
			{
				parent.RemoveRowMessageError(rowMessageError);
			}
		}

		void CheckRuleNAT_184()
		{
			var rowMessageError = Res.GetString("4AE35EBA-1754-4C3C-87D6-9F05C61E7677", "[NAT_184] if one entry line has CANA 0090 (promotional product to DROM), all other lines must have CANA 0090 also.");
			var parent = Parent;
			if (parent.Validation.ValidationDecider is IEntryLineValidationDecider entryLineValidationDecider && entryLineValidationDecider.IsRuleNAT_184Active && parent.IsPromotionalProductToDROM && parent.Header is CusEntryHeader header && !header.IsPromotionalProductToDROM)
			{
				parent.AddRowMessageError(rowMessageError);
			}
			else
			{
				parent.RemoveRowMessageError(rowMessageError);
			}
		}

		void CheckRuleNAT_188()
		{
			var rowMessageError = Res.GetString("8C4C0C9F-3B5E-464D-B922-360474380769", "[NAT_188] if one entry line has CANA 0089 (product of negligible value to DROM), all other lines must have CANA 0089 also.");
			var parent = Parent;
			if (parent.Validation.ValidationDecider is IEntryLineValidationDecider entryLineValidationDecider && entryLineValidationDecider.IsRuleNAT_188Active && parent.IsProductOfNegligibleValueToDROM && parent.Header is CusEntryHeader header && !header.IsProductOfNegligibleValueToDROM)
			{
				parent.AddRowMessageError(rowMessageError);
			}
			else
			{
				parent.RemoveRowMessageError(rowMessageError);
			}
		}
	}
}
