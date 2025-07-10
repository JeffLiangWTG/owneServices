using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Business
{
	public class JobComInvoiceLineLookups : Customs.Business.JobComInvoiceLineLookups
	{
		public JobComInvoiceLineLookups(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

		public override CodeDescriptionPairList CustomsUQList => Factory.GetCachedValue<CustomsUQList>();

		public CodeDescriptionPairList PreferenceList => ReferenceDataProvider.GetPreferenceList(Factory);

		public CodeDescriptionPairList OriginCertifierList => ReferenceDataProvider.GetOriginCertifierList(Factory);

		public ZZRefCusCodeListCombinedCollection DutyReductionExemptionRefundCodeCollection => Parent.IsImport ? ReferenceDataProvider.GetDutyExemptionCode(Factory) : ReferenceDataProvider.GetDutyExemptionRefundCodeList(Factory);

		public CodeDescriptionPairList CertificateOfOriginCertifierList
		{
			get
			{
				var preference = Parent.JI_Calc_Preference;
				return preference.IsEmpty ? [] : ReferenceDataProvider.GetCertificateOfOriginCertifierList(Factory, preference, Parent.EntryInstruction?.CEI_Style ?? ZString.Empty);
			}
		}

		public ICodeDescriptionPairList ConcessionOrderList
		{
			get
			{
				var tariff = Parent.UniversalTariff;
				var primaryPreference = Parent.JI_PrimaryPreference;

				return tariff != null && !primaryPreference.IsEmpty
					? ReferenceDataProvider.GetConcessionOrderList(Factory, tariff.PK, primaryPreference)
					: new CodeDescriptionPairList();
			}
		}

		protected override Customs.Business.OrgSupplierPartCollection GetNewPartCollection()
		{
			var declaration = InvoiceLine.Declaration;
			return declaration != null ? new OrgSupplierPartCollection(Factory, InvoiceLine, InvoiceLine.Supplier, InvoiceLine.Importer, InvoiceLine.InvoiceHeader?.IsExport ?? ZBool.False) : null;
		}

		public ICodeDescriptionPairList TradeControlOrderAppendixList => ReferenceDataProvider.GetTradeControlOrderAppendixList(Factory, InvoiceLine.IsExport);

		public FEFTAArticle48List FEFTAArticle48List => Factory.GetCachedValue<FEFTAArticle48List>();

		public CodeDescriptionPairList ConsumptionTaxExemptionIDList => ReferenceDataProvider.GetExportConsumptionTaxExemptionCode(Factory);

		public CodeDescriptionPairList NACCSCodeList
		{
			get
			{
				var invoiceLine = Parent;
				var code = invoiceLine.CalculatedNACCSCode;

				var codeLists = Factory.GetCachedValue($"JP-NACCSCodeList-{invoiceLine.IsExport}-{code}", () =>
				{
					CodeDescriptionPairList result;
					if (invoiceLine.IsExport)
					{
						result = new ExportNACCSCodeList();
					}
					else
					{
						result = new ImportNACCSCodeList();
					}
					if (!string.IsNullOrEmpty(code))
					{
						result.AddPair(code);
					}
					return result;
				});

				return codeLists;
			}
		}

		public CodeDescriptionPairList StorageTypeList
		{
			get
			{
				CodeDescriptionPairList result = null;
				var invoiceLine = Parent;
				var instruction = invoiceLine.EntryInstruction;
				if (invoiceLine.IsImport && instruction != null)
				{
					switch (instruction.CEI_Style)
					{
						case JPImportDeclarationTypeList.Codes.A:
							result = Factory.GetCachedValue<StorageTypeListWhenDeclarationTypeIsA>();
							break;
						case JPImportDeclarationTypeList.Codes.G:
							result = Factory.GetCachedValue<StorageTypeListWhenDeclarationTypeIsG>();
							break;
						default:
							if (invoiceLine.Declaration.TransportMode == TransportTypeList.Codes.Sea)
							{
								result = Factory.GetCachedValue<StorageTypeListWhenTransportModeIsSea>();
							}
							break;
					}
				}
				return result ?? new CodeDescriptionPairList();
			}
		}
	}
}
