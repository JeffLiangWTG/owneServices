using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class AddInfoJobComInvoiceLineLookups : CAAddInfoLookups
	{
		public AddInfoJobComInvoiceLineLookups(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new AddInfoJobComInvoiceLine Parent
		{
			get { return (AddInfoJobComInvoiceLine)base.Parent; }
		}

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					invoiceLine = Parent.Parent;
				}
				return invoiceLine;
			}
		}
		JobComInvoiceLine invoiceLine;

		#region DefaultOrigins

		public virtual RefCountryCollection DefaultOrigins
		{
			get { return new RefCountryCollection(Factory); }
		}

		#endregion

		public CodeDescriptionPairList StatesOfExport
		{
			get { return Factory.GetCachedValue<USStatesList>(); }
		}

		public CodeDescriptionPairList StatesOfOrigin
		{
			get { return StatesOfOriginBase(InvoiceLine.IsExport, InvoiceLine.JI_CountryOfOrigin); }
		}

		public new CodeDescriptionPairList TreatmentCodes
		{
			get
			{
				var effectiveCountryOfOrigin = InvoiceLine.EffectiveCountryOfOrigin;
				var effectiveCountryOfExport = InvoiceLine.EffectiveCountryOfExport;
				var effectiveDutyDate = InvoiceLine.Declaration?.EffectiveDutyDate ?? ZDateTime.Empty;
				var tradeZone = InvoiceLine.InvoiceHeader?.CA_TradeZone ?? ZString.Empty;

				return Factory.GetCachedValue(string.Format("JobComInvoiceLine|TreatmentCodes|{0}|{1}|{2}|{3:dd-MMM-yy}|{4}",
					effectiveCountryOfOrigin,
					effectiveCountryOfExport,
					InvoiceLine.JI_Tariff,
					effectiveDutyDate,
					tradeZone),
					() =>
				{
					var result = new CodeDescriptionPairList(LookupsHelper.TreatmentCodesByOriginAndExport(Factory, effectiveCountryOfOrigin, effectiveCountryOfExport, tradeZone, effectiveDutyDate));
					if (InvoiceLine.Declaration != null && !InvoiceLine.JI_Tariff.IsEmpty)
					{
						foreach (var pair in result.ToArray())
						{
							if (CACRate.Load(InvoiceLine.JI_Tariff, pair.Code, effectiveDutyDate, InvoiceLine.Factory) == null)
							{
								result.Remove(pair);
							}
						}
					}
					return result;
				});
			}
		}

		public RefCountryCollection CFIACountryOfSourceList
		{
			get { return new RefCountryCollection(Factory); }
		}

		public CodeDescriptionPairList CFIAStateOfSourceList
		{
			get { return StatesOfOriginBase(InvoiceLine.IsExport, InvoiceLine.CA_CFIACountryOfSource); }
		}

		public CodeDescriptionPairList RemissionTypeList
		{
			get
			{
				var isCADEnabled = Parent.Declaration?.IsCADEnabled ?? false;
				return Factory.GetCachedValue(string.Format("RemissionTypeList_IsCADEnabled{0}", isCADEnabled), () =>
				{
					var result = new RemissionTypeList();
					if (isCADEnabled)
					{
						result.RemoveCode(CA.Business.RemissionTypeList.Codes.AppealsCaseNumber);
						result.RemoveCode(CA.Business.RemissionTypeList.Codes.Case);
						result.RemoveCode(CA.Business.RemissionTypeList.Codes.ComplianceCaseNumber);
						result.RemoveCode(CA.Business.RemissionTypeList.Codes.Ruling);
					}
					return result;
				});
			}
		}
	}
}
