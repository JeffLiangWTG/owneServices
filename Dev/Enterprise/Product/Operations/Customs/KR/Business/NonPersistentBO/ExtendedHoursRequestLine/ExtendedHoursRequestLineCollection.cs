using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class ExtendedHoursRequestLineCollection : NonPersistentBusinessObjectCollection<ExtendedHoursRequestLine>, IEmbeddedModulePopupCollection
	{
		public ExtendedHoursRequestLineCollection(ExtendedHoursRequestHeader parent) : base(parent.Factory)
		{
			Argument.NotNull(parent, nameof(parent));
			Parent = parent;
		}
		public ExtendedHoursRequestHeader Parent { get; }
		protected override BusinessObject CreateNonPersistentBusinessObject() => new ExtendedHoursRequestLine(Parent);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "Baseline")]
		ZDecimal ConvertToUSD(ZDateTime declarationDate, ZDecimal customsValueInKRW)
		{
			var result = ZDecimal.Zero;

			var company = Factory.Load<GlbCompany>(Parent.CompanyPK);
			var usdCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
			var krwCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.KoreaRepublicOf);
			var rateType = Parent.MessageType == ElectronicDocumentTypeList.Codes._5AC ? ExchangeRateType.CustomsSecondary : ExchangeRateType.Customs;

			if (company != null && usdCurrency != null && krwCurrency != null)
			{
				var currencyCurrencyConverter = new RefCurrencyCurrencyConverter(company, Factory, declarationDate, rateType, 0);
				return Math.Round(currencyCurrencyConverter.ConvertExact(new Money(customsValueInKRW, krwCurrency), usdCurrency).Amount);
			}
			return result;
		}

		public ExtendedHoursRequestLine AddNewLine(KREntryHeaderDetailsView moduleBO)
		{
			var line = AddNew();
			line.ReferenceNumber = moduleBO.KEH_EntryNum.SubstringSafe(0, 19);
			line.PackageCount = (ZInt)moduleBO.KEH_ExportPackQty;
			line.TotalWeight = moduleBO.KEH_TotalWeightInKG;
			line.SupplierName = moduleBO.KEH_SupplierName.SubstringSafe(0, 50);
			var date = moduleBO.KEH_EntryNumIssueDate == ZDateTime.Empty ? ZDateTime.Today : moduleBO.KEH_EntryNumIssueDate;
			line.CustomsValue = ConvertToUSD(date, moduleBO.KEH_TotalCustomsValueInKRW);
			return line;
		}

		BusinessObject IEmbeddedModulePopupCollection.AddNewLine(BusinessObject moduleBO) => AddNewLine((KREntryHeaderDetailsView)moduleBO);
	}
}
