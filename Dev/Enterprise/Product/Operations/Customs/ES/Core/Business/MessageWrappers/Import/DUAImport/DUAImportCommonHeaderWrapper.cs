using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DUAImportCommonHeaderWrapper : IDUAImportCommonHeader
	{
		public DUAImportCommonHeaderWrapper(CusEntryHeader cusEntryHeader)
		{
			entryHeader = Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
			declaration = entryHeader.Declaration;
			entryInstruction = entryHeader.EntryInstruction;
		}
		protected readonly CusEntryHeader entryHeader;
		protected readonly JobDeclaration declaration;
		protected readonly CusEntryInstruction entryInstruction;

		public ZString CustomsOfficeOfDestination => declaration.JE_CustomsOffice.Right(4);

		public ZString ShipmentType => declaration.JE_MessageSubType;

		public ZString Procedure => ProcedureCore;

		protected virtual ZString ProcedureCore => entryInstruction.CEI_SubStyle;

		public ZInt TotalLinesNum => entryHeader.MergedLines.Count;

		public ZInt TotalPackagesNum => entryHeader.PackagesCount;

		public ZString CommercialReference => declaration.JE_OwnerRef;

		public IImportImporterProvider Importer => CachedValueHelper.GetValue(ref importer, () => ImportImporterWrapper.New(declaration.ImporterDocumentaryAddress, declaration));
		CachedValue<IImportImporterProvider> importer;

		public IImportDeclarantPartyIdProvider Declarant => CachedValueHelper.GetValue(ref declarant, () => ImportDeclarantPartyIdWrapper.New(declaration));
		CachedValue<IImportDeclarantPartyIdProvider> declarant;

		public ZString DeclarationEmail => declaration.DeclEmailAddr;

		public ZString OtherEmail => declaration.ZG_OtherEmailAddr;

		public ZString OriginCountry => declaration.JE_GoodsOrigin;

		public ZBool IsContainerised => entryHeader.IsContainerised();

		public ZString CurrencyCode => entryHeader.RandomHeader.JZ_RX_NKInvoice_Currency;

		const string GoodsLocationPrefix = "ES00";

		public ZString GoodsLocation
		{
			get
			{
				var result = entryInstruction.GoodsLocation.Address.AuthorisationNumber;

				if (!result.IsEmpty && result.Length <= 10)
				{
					result = GoodsLocationPrefix + result;
				}

				return result;
			}
		}

		public ZDecimal TotalTributesAmount
		{
			get
			{
				if (totalTributesAmount == null)
				{
					totalTributesAmount = new CachedProperty<ZDecimal>(entryHeader.Factory, () =>
					{
						return entryHeader.GetTotalAmountToDeclare();
					});
				}
				return totalTributesAmount.Value;
			}
		}
		CachedProperty<ZDecimal> totalTributesAmount;

		public ZString PaymentMode
		{
			get
			{
				var invoiceLine = (JobComInvoiceLine)entryHeader.RandomEntryLine.RandomLine;
				return invoiceLine.ZG_MethodOfPayment;
			}
		}

		public ZString ClearanceGuarantee
		{
			get
			{
				if (clearanceGuarantee == null)
				{
					clearanceGuarantee = new CachedProperty<ZString>(entryHeader.Factory, () =>
					{
						return declaration.Guarantees.GetReferenceForType(entryHeader.CH_CEI_Instruction, "A");
					});
				}
				return clearanceGuarantee.Value;
			}
		}
		CachedProperty<ZString> clearanceGuarantee;

		public ZString PendenciesGuarantee
		{
			get
			{
				if (pendenciesGuarantee == null)
				{
					pendenciesGuarantee = new CachedProperty<ZString>(entryHeader.Factory, () =>
					{
						return declaration.Guarantees.GetReferenceForType(entryHeader.CH_CEI_Instruction, "P");
					});
				}
				return pendenciesGuarantee.Value;
			}
		}
		CachedProperty<ZString> pendenciesGuarantee;

		public IReadOnlyCollection<ZString> GRNGuarantees
		{
			get
			{
				if (grnGuarantees == null)
				{
					grnGuarantees = declaration.Guarantees.GetGRNReferencesForCharacter(entryHeader.CH_CEI_Instruction, 'A').AsReadOnly();
				}
				return grnGuarantees;
			}
		}
		IReadOnlyCollection<ZString> grnGuarantees;

		public ZString PaymentModeCan
		{
			get
			{
				var paymentModeCan = ZString.Empty;
				if (declaration.DestinationStateIsCanaryIsland)
				{
					var invoiceLine = (JobComInvoiceLine)entryHeader.RandomEntryLine.RandomLine;
					paymentModeCan = invoiceLine.ZG_MethodOfPayment2;
				}
				return paymentModeCan;
			}
		}

		public ZString ClearanceGuaranteeCan
		{
			get
			{
				if (clearanceGuaranteeCan == null)
				{
					clearanceGuaranteeCan = new CachedProperty<ZString>(entryHeader.Factory, () =>
					{
						return declaration.Guarantees.GetReferenceForType(entryHeader.CH_CEI_Instruction, "C");
					});
				}
				return clearanceGuaranteeCan.Value;
			}
		}
		CachedProperty<ZString> clearanceGuaranteeCan;

		public ZString PendenciesGuaranteeCan
		{
			get
			{
				if (pendenciesGuaranteeCan == null)
				{
					pendenciesGuaranteeCan = new CachedProperty<ZString>(entryHeader.Factory, () =>
					{
						return declaration.Guarantees.GetReferenceForType(entryHeader.CH_CEI_Instruction, "T");
					});
				}
				return pendenciesGuaranteeCan.Value;
			}
		}
		CachedProperty<ZString> pendenciesGuaranteeCan;

		public IReadOnlyCollection<ZString> GRNGuaranteesCan
		{
			get
			{
				if (grnGuaranteesCan == null)
				{
					grnGuaranteesCan = declaration.Guarantees.GetGRNReferencesForCharacter(entryHeader.CH_CEI_Instruction, 'C').AsReadOnly();
				}
				return grnGuaranteesCan;
			}
		}
		IReadOnlyCollection<ZString> grnGuaranteesCan;
	}
}
