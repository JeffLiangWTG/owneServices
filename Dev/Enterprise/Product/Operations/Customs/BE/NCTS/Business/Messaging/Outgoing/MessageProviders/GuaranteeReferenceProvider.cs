using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class GuaranteeReferenceProvider : IGuaranteeReference
	{
		readonly EU.NCTS.Business.NctsGuarantee nctsGuarantee;

		public GuaranteeReferenceProvider(EU.NCTS.Business.NctsGuarantee nctsGuarantee, ZInt sequence)
		{
			this.nctsGuarantee = Argument.NotNull(nctsGuarantee, nameof(nctsGuarantee));
			SequenceNumber = sequence;
		}

		public int SequenceNumber { get; }

		public virtual string GRN => nctsGuarantee.PW_BondNumber;

		public string AccessCode => nctsGuarantee.PW_Password;

		public decimal AmountToBeCovered => nctsGuarantee.PW_BondAmount.Round(2);

		public string Currency
		{
			get
			{
				var result = Core.Constants.CurrencyCodes.EuropeanUnion;
				if (nctsGuarantee.CusGuarantee is CusGuaranteeHeader cusGuarantee && !cusGuarantee.CPH_UnitOfMeasure.IsEmpty)
				{
					result = cusGuarantee.CPH_UnitOfMeasure.ToString();
				}
				return result;
			}
		}

		public string CCQualifier => throw new System.NotImplementedException();

		public string OtherGuaranteeReference => throw new System.NotImplementedException();

		public string CustomsOfficeOfGuaranteeReferenceNumber => throw new System.NotImplementedException();
	}
}
