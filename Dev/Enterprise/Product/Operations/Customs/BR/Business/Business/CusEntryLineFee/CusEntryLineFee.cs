using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public partial class CusEntryLineFee : AutoCusEntryLineFee
	{
		public CusEntryLineFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override bool ShouldDeleteIfChargeAmountIsZero => CF_BaseValue.IsEmpty && CF_Rate.IsEmpty;

		public ZString ChargeTypeDescription => Lookups.ChargeTypeList.GetDescriptionFromCode(CF_ChargeType);

		protected override void ResetDataCore()
		{
			CF_BaseValue = 0;
			CF_MethodOfCalculation = ZString.Empty;
		}

		public bool IsEmpty => CF_ChargeAmount.IsEmpty && CF_BaseValue.IsEmpty && CF_Rate.IsEmpty;

		public bool IsQuantityPerUnit => CF_MethodOfCalculation == SpecialCaseTaxTypeList.Codes.QuantityPerUnit;

		protected override int CF_BaseValueDecimalPlacesCore => 4;

		public override ZDecimal CF_BaseValue
		{
			get => Utilities.Round(base.CF_BaseValue, CF_BaseValueDecimalPlaces);
			set => base.CF_BaseValue = value;
		}

		[ResourceStringData("Enterprise.Customs.BR.Business.CusEntryLineFee|FormalEntryLineNumbers", Caption = "Entry Line Numbers", FullDescription = "Entry Line Numbers linked to SISCOMEX Usage Fee.")]
		public ZString FormalEntryLineNumbers => Factory.GetCached(ref cachedFormalEntryLineNumbers, GetFormalEntryLineNumbers);
		CachedProperty<ZString> cachedFormalEntryLineNumbers;

		ZString GetFormalEntryLineNumbers() => EntryLine == null ? string.Empty : string.Join(", ", EntryLine.InvoiceLines.Select(s => s.CusEntryLine?.CL_LineNumber.ToString()).WhereNotNull().Distinct().OrderBy(x => x));
	}
}
