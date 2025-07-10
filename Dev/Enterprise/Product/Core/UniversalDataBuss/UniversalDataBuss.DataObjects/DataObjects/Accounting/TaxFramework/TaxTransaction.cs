using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting.TaxFramework
{
	[XsdSchema(Placement.Outer)]
	public partial class TaxTransaction : IDataObject
	{
		public TaxTransaction()
		{
		}

		public TaxTransaction(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		public ZInt? Link { get; set; }
		public CodeDescriptionPair TaxSuperType { get; set; }
		public CodeDescriptionPair10Char TaxSystem { get; set; }
		public CodeDescriptionPair30Char TaxConfiguration { get; set; }
		public CodeDescriptionPair10Char TaxAuthority { get; set; }
		[MaxLength(2)]
		public ZString? Ledger { get; set; } //ATT_Ledger
		public CodeDescriptionPair RealizationBasis { get; set; }
		public Branch Branch { get; set; }
		public Department Department { get; set; }
		public ZDate? PostDate { get; set; }
		public ZDate? TaxRealizedDate { get; set; }
		public ZDate? TaxDate { get; set; }
		public CodeDescriptionPair20Char ServiceCode { get; set; }
		public TaxID TaxID { get; set; }
		public TaxMessageID TaxMessageID { get; set; }
		public Currency OSCurrency { get; set; }
		public ZDecimal? OSTaxBase { get; set; }
		public ZDecimal? OSTaxAmount { get; set; }
		public ZDecimal? LocalTaxBase { get; set; }
		public ZDecimal? LocalTaxAmount { get; set; }
		public ZBool? IsCancelled { get; set; }
		public ZBool? IncludedInTransactionTotal { get; set; }
		public List<PostingJournalDetail> PostingJournalDetailCollection { get; private set; }
		public List<RealizationJournalReference> RealizationJournalReferenceCollection { get; private set; }
	}
}
