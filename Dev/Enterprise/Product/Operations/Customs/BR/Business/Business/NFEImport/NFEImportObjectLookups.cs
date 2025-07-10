using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class NFEImportObjectLookups : ZLookups
	{
		public NFEImportObjectLookups(NFEImportObject parent)
			: base(parent)
		{
		}

		NFEImportObject NfeImportObject
		{
			get { return (NFEImportObject)Parent; }
		}

		public CodeDescriptionPairList IncoTermList
		{
			get { return Factory.GetCachedValue<BRIncoTermList>(); }
		}

		public RefCurrencyCollection CurrencyList
		{
			get { return new RefCurrencyCollection(Factory); }
		}

		public IBusinessObjectCollection InvoiceHeaderList => NfeImportObject?.Declaration?.Invoices
															  ?? (IBusinessObjectCollection)new ActiveBusinessObjectCollection<JobComInvoiceHeader>(Factory, ZQuery.NoResultQuery);

		public SubsetCusEntryInstructionCollection EntryInstructionList
		{
			get
			{
				SubsetCusEntryInstructionCollection result = null;
				var declaration = NfeImportObject.Declaration;
				if (declaration?.IsPersistent ?? false)
				{
					result = new SubsetCusEntryInstructionCollection(declaration.CustomsEntryInstructions, x => x.CEI_LegalDocument == LegalDocumentList.Codes.ElectronicLogisticInvoice);
				}
				return result;
			}
		}
	}
}
