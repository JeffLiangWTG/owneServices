using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.DataTransfer.GLJournals
{
	public class GLJournalLineSubAccountForADAW : NonPersistentBusinessObject , IGLJournalLineSubAccount
	{
		public GLJournalLineSubAccountForADAW(BusinessObjectFactory factory) : base(factory)
		{
		}

		public abstract class Schema
		{
			public const string PK = "IAL1_PK";
		}

		[BusinessObjectTestExclude]
		public ZString SubAccountType { get; set; }
		public ZPropertyInfo SubAccountTypeInfo
		{
			get { return GetZPropertyInfo(nameof(SubAccountType)); }
		}

		[BusinessObjectTestExclude]
		public ZString SubAccountValue { get; set; }
		public ZPropertyInfo SubAccountValueInfo
		{
			get { return GetZPropertyInfo(nameof(SubAccountValue)); }
		}
	}

	interface IGLJournalLineSubAccount
	{
		ZString SubAccountType { get; set; }
		ZString SubAccountValue { get; set; }
	}
}
