using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class CC415BWrapper : ICC415B
	{
		CC415BWrapper(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			this.jobDeclaration = Argument.NotNull(entryHeader.Declaration, nameof(jobDeclaration));
		}

		readonly JobDeclaration jobDeclaration;
		readonly CusEntryHeader entryHeader;

		public static CC415BWrapper New(CusEntryHeader entryHeader) => entryHeader == null ? null : new CC415BWrapper(entryHeader);

		public ICollection<IAuthorisation> Authorisation => authorisation ?? (authorisation = GetAuthorisation());
		ICollection<IAuthorisation> authorisation;

		ICollection<IAuthorisation> GetAuthorisation()
		{
			var result = new Collection<IAuthorisation>();
			entryHeader.EntryInstruction.CusAuthorizationUsages.Cast<CusAuthorizationUsage>().ForEach(authorizationUsage => result.Add(AuthorisationWrapper.New(authorizationUsage)));
			return result;
		}

		public ICurrencyExchange CurrencyExchange => currencyExchange ?? (currencyExchange = new CurrencyExchangeWrapper());
		ICurrencyExchange currencyExchange;

		public IPco CustomsOfficeOfPresentation => customsOfficeOfPresentation ?? (customsOfficeOfPresentation = CustomsOfficeOfPresentationWrapper.New(jobDeclaration));
		IPco customsOfficeOfPresentation;

		public IDeclarant Declarant => declarant ?? (declarant = DeclarantWrapper.New(jobDeclaration));
		IDeclarant declarant;

		public ICollection<IDeferredPayment> DeferredPayment => deferredPayment ?? (deferredPayment = GetDeferredPayment());
		ICollection<IDeferredPayment> deferredPayment;

		ICollection<IDeferredPayment> GetDeferredPayment()
		{
			var result = new Collection<IDeferredPayment>
			{
				DeferredPaymentWrapper.New(jobDeclaration)
			};
			return result;
		}

		public ICollection<IGoodsShipment> GoodsShipment => goodsShipment ?? (goodsShipment = GetGoodsShipment());
		ICollection<IGoodsShipment> goodsShipment;

		ICollection<IGoodsShipment> GetGoodsShipment()
		{
			var result = new Collection<IGoodsShipment>();
			result.Add(GoodsShipmentWrapperFor415And413.New(entryHeader));
			return result;
		}

		public ICollection<IGuarantee> Guarantee => GetGuarantee();

		ICollection<IGuarantee> GetGuarantee()
		{
			var result = new Collection<IGuarantee>();
			entryHeader.EntryInstruction?.Guarantees.Cast<GuaranteeForEntryInstruction>().ForEach(guarantee => result.Add(GuaranteeWrapper.New(guarantee)));
			if (jobDeclaration.IsUCC6 && DeferredPayment.Any(x => !string.IsNullOrEmpty(x.DeferredPayment)))
			{
				result.Add(UCC6GuaranteeWrapper.New(DeferredPayment.SingleOrDefault(x => !string.IsNullOrEmpty(x.DeferredPayment)).DeferredPayment));
			}
			return result;
		}

		public IImporter Importer => importer ?? (importer = ImporterWrapper.New(jobDeclaration.Importer, jobDeclaration.ImporterDocumentaryAddress?.Address));
		IImporter importer;

		public ICC415BCciOperation ImportOperation => importOperation ?? (importOperation = CC415BImportOperationWrapper.New(entryHeader));
		ICC415BCciOperation importOperation;

		public IPersonPayingCustomsDuty PersonPayingCustomsDuty => personPayingCustomsDuty ?? (personPayingCustomsDuty = PersonPayingCustomsDutyWrapper.New(jobDeclaration));
		IPersonPayingCustomsDuty personPayingCustomsDuty;

		public IPersonProvidingGuarantee PersonProvidingAGuarantee => personProvidingAGuarantee ?? (personProvidingAGuarantee = PersonProvidingGuaranteeWrapper.New());
		IPersonProvidingGuarantee personProvidingAGuarantee;

		public IRepresentative Representative => representative ?? (representative = RepresentativeWrapper.New(jobDeclaration));
		IRepresentative representative;

		public ISco SupervisingCustomsOffice => supervisingCustomsOffice ?? (supervisingCustomsOffice = SupervisingCustomsOfficeWrapper.New(jobDeclaration));
		ISco supervisingCustomsOffice;

		public ICustomsOfficesOfDischarge CustomsOfficesOfDischarge => customsOfficesOfDischarge ?? (customsOfficesOfDischarge = GetCustomsOfficesOfDischarge());
		ICustomsOfficesOfDischarge customsOfficesOfDischarge;

		ICustomsOfficesOfDischarge GetCustomsOfficesOfDischarge()
		{
			EuOfficeCode officeCode = jobDeclaration.CustomsOffices?.Cast<EuOfficeCode>()?.FirstOrDefault(x => x.CY_Code == FrOfficeCodesTypes.Codes.OfficeOfDischarge);
			return officeCode == null ? null : CustomsOfficesOfDischargeWrapper.New(officeCode);
		}

		public bool IsSimplified => entryHeader.EntryInstruction.IsSimplified;
	}
}
