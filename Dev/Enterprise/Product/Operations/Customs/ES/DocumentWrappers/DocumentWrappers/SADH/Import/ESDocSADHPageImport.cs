using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.DocumentWrappers.Customs.EU;
using ESCusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;
using ESCusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.ES.Business.Declaration.CusEntryLine>;
using ESCusEntryLineFee = Enterprise.Customs.ES.Business.Declaration.CusEntryLineFee;
using ESUniversalReferenceConstants = Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH
{
	[AllowNoStaticNew]
	public class ESDocSADHPageImport : DocSADHPage
	{
		public static ESDocSADHPageImport New(BusinessObjectFactory factory, ESCusEntryLine entryLine1)
		{
			return new ESDocSADHPageImport(factory, ESDocSADHLineImport.New(entryLine1, factory), null, null, null);
		}

		public static ESDocSADHPageImport New(BusinessObjectFactory factory, ESCusEntryLineCollection entryLines, List<ESDocSADHLineImport> lines, int startFrom)
		{
			return new ESDocSADHPageImport(factory
				, GetElementSafe(lines, startFrom, factory)
				, GetElementSafe(lines, startFrom + 1, factory)
				, GetElementSafe(lines, startFrom + 2, factory)
				, entryLines
			);
		}

		ESDocSADHPageImport(BusinessObjectFactory factory, ESDocSADHLineImport line1, ESDocSADHLineImport line2, ESDocSADHLineImport line3, ESCusEntryLineCollection entryLines)
			: base(factory, line1, line2, line3)
		{
			EntryLines = entryLines;
		}
		public ESCusEntryLineCollection EntryLines { get; }
		public new ESDocSADHLineImport Line1 => (ESDocSADHLineImport)base.Line1;
		public new ESDocSADHLineImport Line2 => (ESDocSADHLineImport)base.Line2;
		public new ESDocSADHLineImport Line3 => (ESDocSADHLineImport)base.Line3;

		static ESDocSADHLineImport GetElementSafe(List<ESDocSADHLineImport> lines, int index, BusinessObjectFactory factory)
		{
			var line = index < lines.Count ? lines[index] : null;
			return line;
		}

		const string DecimalsFormat = "N2";

		const string BisConstant = "BIS";
		protected override ZString BISCaptionCore => BisConstant;

		protected override DocSADHLineTaxCollection Box47TaxesTotalsCore
		{
			get
			{
				List<NonPersistentFee> result = new List<NonPersistentFee>();
				if (EntryLines != null && IsLastPage(EntryLines))
				{
					foreach (ESCusEntryLine entryLine in EntryLines)
					{
						foreach (ESCusEntryLineFee feeLine in entryLine.Fees)
						{
							if (!result.Select(tax => tax.Type).Contains(feeLine.CF_ChargeType))
							{
								result.Add(new NonPersistentFee(feeLine, Factory));
							}
							else
							{
								result.Find(tax => tax.Type == feeLine.CF_ChargeType).AddAmountToTax(feeLine.CF_ChargeAmount);
							}
						}
					}
				}
				var lineTaxCollection = new ESDocSADHLineTaxCollectionImport(result.Cast<IESDocSADHLineTaxBoxSupporter>(), Factory);
				SortTaxes(lineTaxCollection);
				return lineTaxCollection;
			}
		}

		#region TaxOrder
		void SortTaxes(ESDocSADHLineTaxCollectionImport taxCollection)
		{
			taxCollection.Sort(new TaxSorter());
		}

		class TaxSorter : IComparer<DocSADHLineTax>
		{
			const string ChargeTypeD00 = "D00";
			const string ChargeTypeB20 = "B20";
			const string ChargeTypeD10 = "D10";
			const string ChargeType3IG = "3IG";
			public int Compare(DocSADHLineTax x, DocSADHLineTax y)
			{
				return taxCodes.IndexOf(x.G4_Type).CompareTo(taxCodes.IndexOf(y.G4_Type));
			}

			readonly IList<string> codes = new string[]
			{
				UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts,
				UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge,
				UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty,
				UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty,
				UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty,
				UniversalReferenceConstants.RefCusRateCodes.ProvisionalCountervailingDuty,
				UniversalReferenceConstants.RefCusRateCodes.DefunctUseA00InsteadCustomsDutyOnAgriculturalProducts,
				UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge,
				ChargeType3IG,
				ESUniversalReferenceConstants.RefCusRateCode.AIEM,
				ESUniversalReferenceConstants.RefCusRateCode.RetailerSurcharge,
				UniversalReferenceConstants.RefCusRateCodes.Vat,
				ChargeTypeD00, ChargeTypeB20, ChargeTypeD10,
				UniversalReferenceConstants.RefCusRateCodes.CompensatoryInterestVat
			};
			IList<string> taxCodes => codes;
		}
		#endregion

		protected override ZString Box47TotalAmountCore
		{
			get
			{
				var result = ZString.Empty;
				if (EntryLines != null && IsLastPage(EntryLines))
				{
					var totalAmount = ZDecimal.Zero;
					foreach (ESCusEntryLine entryLine in EntryLines)
					{
						ESDocSADHLineTaxCollectionImport lineTaxList = new ESDocSADHLineTaxCollectionImport(entryLine.GetESTaxBoxSupporterList(), Factory);
						totalAmount += lineTaxList.Count > 0 ? (ZDecimal)lineTaxList.Cast<ESDocSADHLineTaxImport>().Where(lineTax => lineTax.G4_MethodOfPayment != DeferredMoPCode).
							Sum(lineTax => ZDecimal.ParseSafe(lineTax.G4_Amount_InDeclarationCurrency, 0m))
							: ZDecimal.Zero;
					}
					result = totalAmount.ToString(DecimalsFormat);
				}
				return result;
			}
		}

		const string DeferredMoPCode = "D";
		protected override ZString Box47TotalMethodOfPaymentCore
		{
			get
			{
				var result = ZString.Empty;
				if (EntryLines != null && EntryLines.Any() && IsLastPage(EntryLines))
				{
					var randomLineTaxList = new ESDocSADHLineTaxCollectionImport(EntryLines.First().GetESTaxBoxSupporterList(), Factory).Cast<ESDocSADHLineTaxImport>();
					var nonDeferredMopTax = randomLineTaxList.FirstOrDefault(lineTax => lineTax.G4_MethodOfPayment != DeferredMoPCode);
					result = nonDeferredMopTax?.G4_MethodOfPayment ?? ZString.Empty;
				}
				return result;
			}
		}

		ZBool IsLastPage(ESCusEntryLineCollection entryLines) => Line2 == null || Line3 == null || Line3.EntryLine.PK == entryLines.Last().PK;
	}
}
