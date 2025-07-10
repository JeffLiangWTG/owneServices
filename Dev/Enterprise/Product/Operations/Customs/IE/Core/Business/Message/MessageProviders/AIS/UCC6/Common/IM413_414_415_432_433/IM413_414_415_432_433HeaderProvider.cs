using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM413_414_415_432_433HeaderProvider : EntryHeaderMessageProvider
	{
		public IM413_414_415_432_433HeaderProvider(AISMessageSendingAction sendingAction) : base(sendingAction.EntryHeader)
		{
			SendingAction = sendingAction;
		}

		protected AISMessageSendingAction SendingAction { get; }

		public IReadOnlyCollection<IAuthorisation> Authorisations => authorisations ?? (authorisations = GetEntryInstructionAuthorization());
		IReadOnlyCollection<IAuthorisation> authorisations;

		public string CustomsOfficeOfPresentation => declaration.PresentationCustomsOffice;

		public string SupervisingCustomsOffice => declaration.SupervisingCustomsOffice;

		public string CustomsOfficeLodgement => declaration.JE_CustomsOffice;

		public IImporter Importer => CachedValueHelper.GetValue(ref importerCached, () => ImporterProvider.New(declaration.ImporterDocumentaryAddress?.Address));
		CachedValue<ImporterProvider> importerCached;

		public IMDeclarant Declarant => CachedValueHelper.GetValue(ref declarantCached, () => MDeclarantProvider.New(declaration.Declarant));
		CachedValue<IMDeclarant> declarantCached;

		public string PersonProvidingAGuaranteeID => personProvidingAGuaranteeID ?? (personProvidingAGuaranteeID = declaration.DefermentPartyDocAddress.GetEuIdentificationNumber());
		string personProvidingAGuaranteeID;

		public string PersonPayingCustomsDutyID => personPayingCustomsDutyID ?? (personPayingCustomsDutyID = declaration.DutyPayer.GetEORI());
		string personPayingCustomsDutyID;

		public IMRepresentative Representative => CachedValueHelper.GetValue(ref representativeCached, () => new MRepresentativeProvider(declaration));
		CachedValue<IMRepresentative> representativeCached;

		public IReadOnlyCollection<IGuarantee> Guarantees => guaranteesCache ?? (guaranteesCache = GetEntryInstructionGuaranteesGrouped());
		IReadOnlyCollection<IGuarantee> guaranteesCache;

		public string CurrencyExchange => Core.Constants.CurrencyCodes.EuropeanUnion;

		public IReadOnlyCollection<IDeferredPayment> DeferredPayments => deferredPayments ?? (deferredPayments = new[] { new DeferredPaymentProvider(declaration) });
		IReadOnlyCollection<IDeferredPayment> deferredPayments;

		public IReadOnlyCollection<IGoodsShipment> GoodsShipments => goodsShipments ?? (goodsShipments = new[] { new GoodsShipmentProvider(1, entryHeaderWrapper) });
		IReadOnlyCollection<IGoodsShipment> goodsShipments;

		public IFallbackProcedure FallbackProcedure => CachedValueHelper.GetValue(ref fallbackProcedureCached, () => FallbackProcedureProvider.New(SendingAction));
		CachedValue<IFallbackProcedure> fallbackProcedureCached;

		IReadOnlyCollection<IAuthorisation> GetEntryInstructionAuthorization()
		{
			var usages = new List<IAuthorisation>();
			foreach (var instruction in declaration.CustomsEntryInstructions)
			{
				usages.AddRange(instruction.CusAuthorizationUsages.Select(x => new EntryInstructionAuthorizationProvider(x)));
			}

			return usages;
		}

		public IReadOnlyCollection<IGuarantee> GetEntryInstructionGuaranteesGrouped()
		{
			var guarantees = new List<IGuarantee>();
			ZShort seqNum = 1;

			foreach (var instruction in declaration.CustomsEntryInstructions)
			{
				var groupedGuarantees = instruction.Guarantees.Cast<GuaranteeForEntryInstruction>().GroupBy(x => x.PW_BondType);
				foreach (var groupedGuarantee in groupedGuarantees)
				{
					guarantees.Add(new GuaranteeProvider(groupedGuarantee.ToList(), groupedGuarantee.Key, seqNum));
					seqNum++;
				}
			}
			return guarantees;
		}
	}
}
