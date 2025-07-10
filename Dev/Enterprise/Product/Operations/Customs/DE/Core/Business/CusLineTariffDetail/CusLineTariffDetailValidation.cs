using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class CusLineTariffDetailValidation : EU.Business.CusLineTariffDetailValidation
	{
		public CusLineTariffDetailValidation(CusLineTariffDetail parent)
			: base(parent)
		{
		}

		protected new CusLineTariffDetail Parent => (CusLineTariffDetail)base.Parent;

		protected override void CheckBZ_Type()
		{
			base.CheckBZ_Type();
			var info = Parent.BZ_TypeInfo;
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(info);

			if (Parent.InvoiceLine?.CusLineTariffDetails.Count > 3)
			{
				info.AddMessageError(Res.GetString("B7FA5947-DB0F-4040-A624-CF8F6DF9F4C6", "A maximum of 3 'Additional Tariffs' are permitted."));
			}
		}

		protected override void CheckBZ_Tariff()
		{
			base.CheckBZ_Tariff();
			var info = Parent.BZ_TariffInfo;
			MandatoryValidation.MessageErrorIfNotEntered(info);

			if (!Parent.BZ_Tariff.IsEmpty && Parent.BZ_Tariff.Length != 4)
			{
				info.AddMessageError((NoResString)"The entered Excise Code must have 4 digits.");
			}

			if (Parent.InvoiceLine?.CusLineTariffDetails
					.Any(x => x.BZ_Tariff != UniversalReferenceConstants.CusLineTariffCodes._1042 && x.BZ_Tariff == Parent.BZ_Tariff && x.PK != Parent.PK) ?? false)
			{
				info.AddMessageError(Res.GetString("9ACAB17D-8C94-4993-886F-4ED9CD0EB47B", "Each additional tariff 'Code' must be unique."));
			}

			var count1042 = Parent.InvoiceLine?.CusLineTariffDetails.Count(x => x.BZ_Tariff == UniversalReferenceConstants.CusLineTariffCodes._1042);
			if (count1042 > 2)
			{
				info.AddMessageError(Res.GetString("F6F700EC-C874-417E-B303-D3529697B3EE", "Code {0} may be repeated a maximum of two times.", UniversalReferenceConstants.CusLineTariffCodes._1042));
			}
		}

		protected override void CheckBZ_Qty1()
		{
			base.CheckBZ_Qty1();
			var info = Parent.BZ_Qty1Info;
			if (!Parent.BZ_Qty1.IsInRange(0.001, 999999999.999))
			{
				info.AddMessageError(Res.GetString("81D2234B-8A45-42A3-A7F3-E71E0728710A", "The 'Quantity' must be between 0,001 and 999.999.999,999"));
			}

			var uq = Parent.BZ_UQ1;
			if (Parent.Factory.IsIntegerRequiredUnitOfQuantity(uq) && !Parent.BZ_Qty1.IsInteger)
			{
				info.AddMessageError(Res.GetString("8EB1F311-D3F7-40CF-8C79-755505EB6AD0", "For 'UOM' {0}, the 'Quantity' must be an integer (a number with no decimal value).", uq));
			}
		}

		protected override void CheckBZ_UQ1()
		{
			base.CheckBZ_UQ1();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BZ_UQ1Info, Res.GetString("041B430B-B2E8-4083-B206-A366217381E9", "Unit of Measurement"));
		}

		protected override void CheckBZ_Value()
		{
			base.CheckBZ_Value();

			var parent = Parent;
			if (Parent.IsTobaccoRelatedTariff && parent.BZ_Value <= 0)
			{
				parent.BZ_ValueInfo.AddMessageError(Res.GetString("AC4003EF-07ED-4B1F-9538-6350CC41F4AB", "Retail Price is mandatory for Tobacco Tariffs. Please enter a value greater than zero."));
			}
		}
	}
}
