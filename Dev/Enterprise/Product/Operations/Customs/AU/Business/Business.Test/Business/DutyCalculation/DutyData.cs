using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	internal class DutyData : IDutyData
	{
		public DutyData()
		{
			fFactory = new BusinessObjectFactory();
			JobDeclaration declaration = fFactory.New<JobDeclaration>();
			fAddInfo = new AUAddInfo(declaration);
			fEffectiveDutyDate = new ZDateTime(2003, 11, 14);
			fDateOfValuation = new ZDateTime(2003, 11, 14);
			fPrice = new Money(0, JobDeclaration.GetLocalCurrency());
			fCustomsFactor = 1.0m;
			fTransportAndInsurance = Money.Empty;
			fCurrencyConverter = CurrencyConverter.New(fFactory, fDateOfValuation, Enterprise.ZArchitecture.Core.ExchangeRateType.Customs, 1);
			fIsNature20 = false;
		}

		public DutyData(
			ZDateTime dateOfValuation,
			ZDateTime effectiveDutyDate,
			RefCurrency currency1,
			RefCurrency currency2,
			RefCurrency currency3,
			ZDecimal customsFactor,

			ZString tariffNumber,
			ZString statCode,
			ZString treatmentCode,
			ZDecimal quantity,
			ZString unitOfQuantity,
			bool isNature20,
			Money price,
			Money transportAndInsurance,
			Money customsValue)
		{
			fDateOfValuation = dateOfValuation;
			fEffectiveDutyDate = effectiveDutyDate;
			fCurrency1 = currency1;
			fCurrency2 = currency2;
			fCurrency3 = currency3;
			fCustomsFactor = customsFactor;

			fTariffNumber = tariffNumber;
			fStatCode = statCode;
			fTreatmentCode = treatmentCode;
			fQuantity = quantity;
			fUnitOfQuantity = unitOfQuantity;
			fPrice = price;
			fIsNature20 = isNature20;
			fFactory = new BusinessObjectFactory();
			fCustomsValue = customsValue;
			fAddInfo = (Factory.New<JobComInvoiceHeader>()).AddInfo;
		}

		public BusinessObjectFactory Factory { get { return fFactory; } }
		public ZDecimal CustomsFactor { get { return fCustomsFactor; } }
		public ZDateTime DateOfValuation { get { return fDateOfValuation; } }
		public ZDateTime EffectiveDutyDate { get { return fEffectiveDutyDate; } }
		public AUAddInfo AddInfo { get { return fAddInfo; } }
		public ZString TariffNumber { get { return fTariffNumber.Replace(".", ""); } }
		public ZString StatCode { get { return fStatCode; } }
		public ZString TreatmentCode { get { return fTreatmentCode; } }
		public ZDecimal Quantity { get { return fQuantity; } }
		public ZString UnitOfQuantity { get { return fUnitOfQuantity; } }
		public ZBool IsNature20 { get { return fIsNature20; } }

		public Money Price { get { return fPrice; } }
		public Money CustomsValue { get { return fCustomsValue; } }
		public Money TransportAndInsurance
		{
			get { return fTransportAndInsurance; }
			set { fTransportAndInsurance = value; }
		}
		public CurrencyConverter CurrencyConverter
		{
			get { return fCurrencyConverter; }
			set { fCurrencyConverter = value; }
		}

		public RefCurrency Currency1 { get { return fCurrency1; } }
		public RefCurrency Currency2 { get { return fCurrency2; } }
		public RefCurrency Currency3 { get { return fCurrency3; } }

		#region Implementation

		public ZDateTime fDateOfValuation;
		public ZDateTime fEffectiveDutyDate;
		public RefCurrency fCurrency1;
		public RefCurrency fCurrency2;
		public RefCurrency fCurrency3;
		public ZDecimal fCustomsFactor;

		public ZString fTariffNumber;
		public ZString fStatCode;
		public ZString fTreatmentCode;
		public ZDecimal fQuantity;
		public ZString fUnitOfQuantity;
		public ZBool fIsNature20;

		public Money fPrice;
		Money fTransportAndInsurance;
		public Money fCustomsValue;
		CurrencyConverter fCurrencyConverter;
		public AUAddInfo fAddInfo;
		public BusinessObjectFactory fFactory;

		#endregion
	}
}
