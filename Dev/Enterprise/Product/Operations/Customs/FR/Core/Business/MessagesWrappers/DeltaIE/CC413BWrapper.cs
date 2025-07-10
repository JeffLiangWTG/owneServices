using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessageSending;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class CC413BWrapper : ICC413B
	{
		CC413BWrapper(DeltaIEJobDeclarationMessageSendingObject sendingObject, bool isForOperationalAction = false)
		{
			this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
			this.entryHeader = Argument.NotNull(sendingObject.Header, nameof(entryHeader));
			this.jobDeclaration = Argument.NotNull(sendingObject.Declaration, nameof(jobDeclaration));
			this.isForOperationalAction = isForOperationalAction;
		}

		readonly JobDeclaration jobDeclaration;
		readonly CusEntryHeader entryHeader;
		readonly DeltaIEJobDeclarationMessageSendingObject sendingObject;
		readonly bool isForOperationalAction;

		public static CC413BWrapper New(DeltaIEJobDeclarationMessageSendingObject sendingObject, bool isForOperationalAction = false) => sendingObject == null ? null : new CC413BWrapper(sendingObject, isForOperationalAction);

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

		public IDeclarant Declarant => declarant ?? (declarant = isForOperationalAction ? DeclarantWrapper.New(true) : DeclarantWrapper.New(jobDeclaration));
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

		public ICC413BCciOperation ImportOperation => importOperation ?? (importOperation = CC413BImportOperationWrapper.New(entryHeader));
		ICC413BCciOperation importOperation;

		public IPersonPayingCustomsDuty PersonPayingCustomsDuty => personPayingCustomsDuty ?? (personPayingCustomsDuty = PersonPayingCustomsDutyWrapper.New(jobDeclaration));
		IPersonPayingCustomsDuty personPayingCustomsDuty;

		public IPersonProvidingGuarantee PersonProvidingAGuarantee => personProvidingAGuarantee ?? (personProvidingAGuarantee = PersonProvidingGuaranteeWrapper.New());
		IPersonProvidingGuarantee personProvidingAGuarantee;

		public IRepresentative Representative => representative ?? (representative = RepresentativeWrapper.New(jobDeclaration));
		IRepresentative representative;

		public ISco SupervisingCustomsOffice => supervisingCustomsOffice ?? (supervisingCustomsOffice = SupervisingCustomsOfficeWrapper.New(jobDeclaration));
		ISco supervisingCustomsOffice;

		public ICollection<ICustomsOfficesOfDischarge> CustomsOfficesOfDischarge => customsOfficesOfDischarge ?? (customsOfficesOfDischarge = GetCustomsOfficesOfDischarge());
		ICollection<ICustomsOfficesOfDischarge> customsOfficesOfDischarge;

		ICollection<ICustomsOfficesOfDischarge> GetCustomsOfficesOfDischarge()
		{
			var result = new Collection<ICustomsOfficesOfDischarge>();
			jobDeclaration.CustomsOffices?.Cast<EU.Business.EuOfficeCode>()?.Where(x => x.CY_Code == FrOfficeCodesTypes.Codes.OfficeOfDischarge).ForEach(x => result.Add(CustomsOfficesOfDischargeWrapper.New(x)));
			return result;
		}

		public IRequest Request => request ?? (request = RequestWrapper.New(sendingObject));
		IRequest request;

		public bool IsSimplified => entryHeader.EntryInstruction.IsSimplified;
	}
}
