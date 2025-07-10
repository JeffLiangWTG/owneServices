using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;

namespace Enterprise.Client.CLE.MattelARInvoiceExport
{
	public class AUINVOICMessageBuilder : INVOICMessageBuilder
	{
		public AUINVOICMessageBuilder(JobInvoiceRecord invoiceRecord, BusinessObjectFactory factory)
			: base(invoiceRecord, factory)
		{
		}

		protected override string DutyRate(BaseJobComInvoiceLine line)
		{
			return line.AdValoremDutyPercent.ToString(2);
		}

		protected override ZDecimal DutyAmount
		{
			get
			{
				ZDecimal result = 0;

				foreach (BaseJobComInvoiceHeader header in Declaration.Invoices)
				{
					foreach (BaseJobComInvoiceLine line in header.JobComInvoiceLines)
					{
						result += line.JI_Calc_DutyAmount;
					}
				}
				return result;
			}
		}

		AccChargeCode DutyChargeCode
		{
			get
			{
				if (dutyChargeCode == null)
				{
					ZGuid dutyChargePK = ZGuid.Empty;

					EntryChargeTypeSettingCollection chargeSettings = RatingDataRegistry.Instance.EntryChargeTypesAndCodes.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

					foreach (EntryChargeTypeSetting chargeSetting in chargeSettings)
					{
						if (chargeSetting.ChargeType == CusEntryChargeTypeList.Codes.DutyAmount)
						{
							dutyChargePK = chargeSetting.AC_ChargeCode;
							break;
						}
					}

					if (!dutyChargePK.IsEmpty)
					{
						dutyChargeCode = Factory.Load<AccChargeCode>(dutyChargePK);
					}
				}

				return dutyChargeCode;
			}
		}

		AccChargeCode dutyChargeCode;

		protected override bool ShouldOutputThisCharge(AccChargeCode chargeCode)
		{
			return !(DutyChargeCode != null) || DutyChargeCode.PK != chargeCode.PK;
		}
	}
}
