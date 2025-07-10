using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business.DocumentWrappers
{
	public class SummaryCusDecDocumentWrapper : NonPersistentBusinessObject
		, IBODocDataProvider
		, IDocumentWrapper
	{
		public SummaryCusDecDocumentWrapper(CusEntryHeader cusEntryHeader)
		{
			EntryHeader = Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
			Declaration = cusEntryHeader.Declaration;
		}

		public CusEntryHeader EntryHeader { get; }

		public JobDeclaration Declaration { get; }

		public ZInt TotalLineCount => EntryHeader.MergedLines.Count;

		public ZString DeclarationType => EntryInstruction?.CEI_Style ?? ZString.Empty;

		public ZString OfficeOfExit => EntryInstruction?.ASY_PortOfExit ?? ZString.Empty;

		CusEntryInstruction EntryInstruction => entryInstruction ?? (entryInstruction = EntryHeader.EntryInstruction);
		CusEntryInstruction entryInstruction;

		#region Declartaion Fields

		public ZString CustomsOfficeName => Declaration.Lookups.CustomsOfficeList.GetDescriptionFromCode(Declaration.JE_CustomsOffice);

		public ZString ExporterOrganizationCode => Declaration.Supplier?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.SupplierCode, GlbCompany.CurrentCompany.Country.Code) ?? ZString.Empty;

		public ZString ImporterOrganizationCode => Declaration.Importer?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CustomsClientCode, GlbCompany.CurrentCompany.Country.Code) ?? ZString.Empty;

		public ZString OriginCE => Declaration.JE_RL_NKOrigin.SubstringSafe(0, 2);

		public ZString DestinationCD => Declaration.JE_RL_NKFinalDestination.SubstringSafe(0, 2);

		public ZString VessalCountryOfReg => Declaration.Vessel?.RV_RN_NKCountryOfReg ?? ZString.Empty;

		public ZString PortOfArrival => Declaration.PortOfArrival?.RL_PortName ?? ZString.Empty;

		public ZString PortOfLoading => Declaration.PortOfLoading?.RL_PortName ?? ZString.Empty;

		public ZString DutyPayerName => DutyPayer?.OH_FullName ?? ZString.Empty;

		public ZString DutyPayerAddress => DutyPayer?.MainAddress?.AddressAsASingleLineWithoutCompanyName ?? ZString.Empty;

		OrgHeader DutyPayer => dutyPayer ?? (dutyPayer = Declaration.DutyPayer);
		OrgHeader dutyPayer;

		public ZString DeclarantName => Declarant?.OH_FullName ?? ZString.Empty;

		public ZString DeclarantAddress => Declarant?.MainAddress?.AddressAsASingleLineWithoutCompanyName ?? ZString.Empty;

		OrgHeader Declarant => declarant ?? (declarant = Declaration.DeclarantAddress?.Header);
		OrgHeader declarant;

		#endregion

		#region Invoice Fields

		public ZString IncoTerm => FirstInvoice?.JZ_IncoTerm ?? ZString.Empty;

		public ZString IncoTermDec => FirstInvoice?.Lookups.JZ_IncoTerm_List.GetDescriptionFromCode(IncoTerm) ?? ZString.Empty;

		public ZString DeliveryTerms => Invoices.Count() > 1 ? string.Format(CultureInfo.InvariantCulture, Res.GetString("E865125B-80A7-408F-9966-B55EA699B4E8", "MULTIPLE INVOICES")) : string.Join(" ", new[] { IncoTerm, IncoTermDec });

		public ZString InvoiceCurrency => FirstInvoice?.JZ_RX_NKInvoice_Currency ?? ZString.Empty;

		public ZString InvoiceNo => ZString.Join(",", Invoices.Where(x => !x.JZ_InvoiceNumber.IsEmpty).Select(x => x.JZ_InvoiceNumber).ToArray());

		public ZDecimal InvoiceCurrExRate => FirstInvoice?.JZ_InvoiceCurrExRate ?? ZDecimal.Zero;

		public ZDecimal TotalInvoiceAmount => Invoices.Sum(x => x.JZ_InvoiceAmount);

		public JobComInvoiceHeader FirstInvoice => Invoices.FirstOrDefault();

		IEnumerable<JobComInvoiceHeader> Invoices => invoices ?? (invoices = EntryHeader.InvoiceHeaders());
		JobComInvoiceHeader[] invoices;

		#endregion

		#region Collections

		public BusinessObjectCollectionWrapper<CusEntryLineDocumentWrapper> EntryLines => fEntryLines ?? (fEntryLines = new BusinessObjectCollectionWrapper<CusEntryLineDocumentWrapper>(EntryHeader.MergedLines.Cast<CusEntryLine>().Select(x => new CusEntryLineDocumentWrapper(x))));
		BusinessObjectCollectionWrapper<CusEntryLineDocumentWrapper> fEntryLines;

		#endregion

		#region IBODocDataProvider

		DocWrapperCopyInfo IBODocDataProvider.AdditionalCopyInfo => BasicBODocDataProvider.AdditionalCopyInfo;

		BusinessObject IBODocDataProvider.BusinessObjectToLogAgainst => EntryHeader;

		BusinessObject IBODocDataProvider.ParentBusinessObject => BasicBODocDataProvider.ParentBusinessObject;

		void IBODocDataProvider.SetDocWrapperContext(Dictionary<string, object> constants) => BasicBODocDataProvider.SetDocWrapperContext(constants);

		string IBODocDataProvider.ToString() => EntryHeader.HumanReadableName;

		ZString IBODocDataProvider.GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue) => BasicBODocDataProvider.GetDocDataValue(docDataIdentifier, formatStringForFallbackValue);

		IZType IBODocDataProvider.GetCustomField(string fieldName, string typeName) => BasicBODocDataProvider.GetCustomField(fieldName, typeName);

		string IBODocDataProvider.GetCustomFieldCodeDescription(string fieldName, string typeName) => BasicBODocDataProvider.GetCustomFieldCodeDescription(fieldName, typeName);

		ZDateTime IBODocDataProvider.GetEventLastDateTime(string eventCode) => BasicBODocDataProvider.GetEventLastDateTime(eventCode);

		string[] IBODocDataProvider.ImageNamesToRemove => BasicBODocDataProvider.ImageNamesToRemove;

		IBODocDataProvider BasicBODocDataProvider => basicBODocDataProvider ?? (basicBODocDataProvider = BODocDataProvider.GetDefault(this));
		IBODocDataProvider basicBODocDataProvider;

		#endregion
	}
}
