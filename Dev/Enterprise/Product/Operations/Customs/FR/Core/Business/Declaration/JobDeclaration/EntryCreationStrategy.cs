using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Registry;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class EntryCreationStrategy : EU.Business.Declaration.EntryCreationStrategy
	{
		public EntryCreationStrategy(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override void AfterCreateOrGetEntryHeader(Customs.Business.CusEntryHeader entryHeader, BaseJobComInvoiceLine baseInvoiceLine)
		{
			base.AfterCreateOrGetEntryHeader(entryHeader, baseInvoiceLine);

			var frCusEntryHeader = (CusEntryHeader)entryHeader;
			if (!frCusEntryHeader.IsInDatabase && string.IsNullOrEmpty(frCusEntryHeader.CH_TriggeringPointForValidation))
			{
				frCusEntryHeader.CH_TriggeringPointForValidation = GetDefaultCH_TriggeringPointForValidation(frCusEntryHeader);
			}
		}

		ZString GetDefaultCH_TriggeringPointForValidation(CusEntryHeader entryHeader)
		{
			var registryProperty = FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.Value;
			return entryHeader.IsImport ? registryProperty.ImportTriggerPoint : registryProperty.ExportTriggerPoint;
		}

		protected override MergeKey GetKeyForHeaderCore(BaseJobComInvoiceLine invoiceLine)
		{
			var result = base.GetKeyForHeaderCore(invoiceLine);
			var line = (JobComInvoiceLine)invoiceLine;
			result.Add(line.InvoiceHeader.ZG_AgreedPlaceCode);
			return result;
		}

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine invoiceLine)
		{
			var line = (JobComInvoiceLine)invoiceLine;
			var result = base.GetKeyForLine(invoiceLine);

			result.Add(line.JI_TariffBypassCode);
			result.Add(line.JI_TariffBypassReason);
		
			ProcessPreviousDocumentForLineMergeKey(result, line);
			ProcessPreviousInbondMovementForLineMergeKey(result, line);

			if (line.Taxes.Any())
			{
				result.Add(line.Taxes[0].JLT_MethodOfCalculation);
			}

			result.Add(line.SelectedPackType);

			var declaration = line.Declaration;
			if (declaration.IsDeltaC)
			{
				if (declaration.IsImport)
				{
					result.Remove(line.InvoiceHeader.JZ_OH_Supplier);
				}
				else if (declaration.IsExport)
				{
					result.Remove(line.InvoiceHeader.JZ_OH_Buyer);
				}
			}

			result.Add(line.ZG_CountryOfDispatch);

			return result;
		}
		protected override bool DocumentForMerge(EU.Business.Declaration.MultiLineAddInfos.SupportingDocument doc)
		{
			return doc.CSI_Code != "N380";
		}

		void ProcessPreviousDocumentForLineMergeKey(MergeKey mergeKey, JobComInvoiceLine line)
		{
			List<BusinessObject> lineDocument = new List<BusinessObject>();
			var doc = line.PreviousISTDocument;
			if (doc != null)
			{
				lineDocument.Add(doc);
			}
			AddResults(mergeKey, lineDocument, GetPreviousDocumentForLineMergeKeys());
		}

		protected string[] GetPreviousDocumentForLineMergeKeys()
		{
			return new string[]
			{
				PreviousDocument.Schema.CSI_ReferenceNumber,
				PreviousDocument.Schema.CSI_LineNo
			};
		}

		void ProcessPreviousInbondMovementForLineMergeKey(MergeKey mergeKey, JobComInvoiceLine line)
		{
			List<BusinessObject> lineDocument = new List<BusinessObject>();
			var doc = line.PreviousDocuments.Cast<PreviousDocument>().FirstOrDefault(x => x.CSI_Code == PreviousDocumentCodeList.Codes.IM);
			if (doc != null)
			{
				lineDocument.Add(doc);
			}
			AddResults(mergeKey, lineDocument, GetPreviousInbondMovementForLineMergeKeys());
		}

		string[] GetPreviousInbondMovementForLineMergeKeys()
		{
			return new[] { PreviousDocument.Schema.CSI_ReferenceNumber };
		}

		protected override string[] GetAdditionalInfoKeysCore() => new string[3] { AdditionalInfo.Schema.CSI_SubType, AdditionalInfo.Schema.CSI_Code, AdditionalInfo.Schema.CSI_ReferenceNumber };
	}
}
