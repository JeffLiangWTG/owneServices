using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocVoyageAccountDisbursementLineCollection : DocBaseWrapperCollection<DocVoyageAccountDisbursementLine>
	{
		DocVoyageAccountDisbursementLineCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public static DocVoyageAccountDisbursementLineCollection New(Charge[] charges, BusinessObjectFactory factory)
		{
			DocVoyageAccountDisbursementLineCollection result = new DocVoyageAccountDisbursementLineCollection(factory);

			if (charges != null && charges.Length > 0)
			{
				GlbBranch[] branches = ExtractBranches(charges);
				ZDecimal[] localAmounts = new ZDecimal[branches.Length];
				Charge previousCharge = null;
				RefCurrency currency = GlbCompany.CurrentCompany.LocalCurrency;

				Array.Sort(charges, ChargeComparer);

				result.heading = DocVoyageAccountDisbursementLine.New(null, "", factory);

				foreach (GlbBranch branch in branches)
				{
					result.heading.Amounts.Add(DocVoyageAccountDisbursementAmount.New(branch.GB_Code, new Money(1, currency), factory));
				}

				if (branches.Length > 1)
				{
					result.heading.Amounts.Add(DocVoyageAccountDisbursementAmount.New(Res.GetString("ac08028f-3c2c-461e-abd3-63f9e763a8c9", "Total"), new Money(1, currency), factory));
				}

				foreach (Charge charge in charges)
				{
					if (previousCharge != null && previousCharge.ChargeCode != charge.ChargeCode)
					{
						result.AddNewLine(previousCharge, branches, localAmounts, currency);
						localAmounts = new ZDecimal[branches.Length];
					}

					int index = Array.IndexOf(branches, charge.Branch);
					localAmounts[index] += charge.JR_LocalSellAmt;

					previousCharge = charge;
				}

				if (previousCharge != null)
				{
					result.AddNewLine(previousCharge, branches, localAmounts, currency);
				}
			}

			return result;
		}

		protected override IBODocDataProvider GetRow(ZString index)
		{
			if (StringComparer.OrdinalIgnoreCase.Equals(index, (NoResString)"heading"))
			{
				return heading;
			}
			else
			{
				return base.GetRow(index);
			}
		}

		#region Implementation

		static GlbBranch[] ExtractBranches(Charge[] charges)
		{
			List<GlbBranch> branches = new List<GlbBranch>();

			foreach (Charge charge in charges)
			{
				GlbBranch branch = charge.Branch;

				if (branch != null)
				{
					if (!branches.Contains(branch))
					{
						branches.Add(branch);
					}
				}
			}
			return branches.ToArray();
		}
		static int ChargeComparer(Charge charge1, Charge charge2)
		{
			AccChargeCode code1 = charge1.ChargeCode;
			AccChargeCode code2 = charge2.ChargeCode;

			if (code1 == null)
			{
				return code2 == null ? 0 : 1;
			}
			else
			{
				return code2 == null ? -1 : StringComparer.OrdinalIgnoreCase.Compare(code1.AC_Code, code2.AC_Code);
			}
		}
		static string GetGroup(AccChargeCode chargeCode)
		{
			string group = null;

			if (chargeCode != null && chargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.ShippingDisbursements)
			{
				group = chargeCode.Lookups.ChargeSubGroupList.GetDescriptionFromCode(chargeCode.AC_ChargeSubGroup);
			}

			return string.IsNullOrEmpty(group) ? Res.GetString("9d7b6996-3558-41dc-a2a5-0929303b71e1", "Other") : group;
		}

		void AddNewLine(Charge previousCharge, GlbBranch[] branches, ZDecimal[] localAmounts, RefCurrency currency)
		{
			DocVoyageAccountDisbursementLine line = DocVoyageAccountDisbursementLine.New(GetGroup(previousCharge.ChargeCode), previousCharge.JR_Desc, Factory);
			ZDecimal total = 0m;

			for (int i = 0; i < branches.Length; i++)
			{
				total += localAmounts[i];
				line.Amounts.Add(DocVoyageAccountDisbursementAmount.New(branches[i].GB_Code, new Money(localAmounts[i], currency), Factory));
			}

			if (branches.Length > 1)
			{
				line.Amounts.Add(DocVoyageAccountDisbursementAmount.New(Res.GetString("ac08028f-3c2c-461e-abd3-63f9e763a8c9", "Total"), new Money(total, currency), Factory));
			}

			Add(line);
		}

		DocVoyageAccountDisbursementLine heading;

		#endregion
	}
}
