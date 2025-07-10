//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccStatementLookups
//
//    This class should be used for overriding collections in AutoAccStatementLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Auto
{
	public class AccStatementLookups : AutoAccStatementLookups
	{
		public AccStatementLookups(AutoAccStatement parent)
			: base(parent)
		{
		}

		public AccGLHeaderCollection GLAccounts
		{
			get { return new AccGLHeaderCollection(Factory); }
		}

		public CodeDescriptionPairList AS_Type_List
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(ReceiptTypes.Cheque, Res.GetString("1ee8f7f4-056c-4fd2-af8b-1b3d090dfaac", "Check"));
				list.AddPair(ReceiptTypes.CreditCard, Res.GetString("330ec421-31cf-4d1e-a343-325fda289457", "Credit Card"));
				list.AddPair(ReceiptTypes.DirectCredit, Res.GetString("89655fd8-d734-4a80-a15b-b8a3d6c96277", "Direct Credit"));
				list.AddPair(ReceiptTypes.DirectDebit, Res.GetString("263cdf48-e677-4999-8455-f6f381ea821e", "Direct Debit"));
				list.AddPair(ReceiptTypes.EFT, Res.GetString("a59d16af-640d-4e32-bbf4-bfe49d3afee3", "Electronic Funds Transfer"));
				list.AddPair(ReceiptTypes.ScheduledEFT, ResString.GetMultilingualString("54869BF2-4A1D-405A-9520-A48B2D40D5C1", "Scheduled EFT"));
				list.AddPair(ReceiptTypes.CollectionRequest, ResString.GetMultilingualString("E8F790ED-B5B6-40E6-9D94-730DABD76EE2", "Collection Request"));
				list.AddPair(TransactionTypes.ReceiptBatch, Res.GetString("e3865e7b-446e-491c-82c0-6faa59c16950", "Receipt Batch"));
				list.AddPair(TransactionTypes.Transfer, Res.GetString("0d944732-8f7f-4353-81c0-7e665b335de5", "Transfer"));
				list.AddRange(BankChargeTypes_List);

				return list;
			}
		}

		public CodeDescriptionPairList BankChargeTypes_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.BankChargeTypes); }
		}

		public CodeDescriptionPairList AS_DebitCredit_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.DebitCredit); }
		}
	}
}