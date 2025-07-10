using static Enterprise.Integration.Customs.CA;

namespace Enterprise.Customs.CA.Business
{
	partial class ActiveCusEntryHeaderCollection : Customs.Business.ActiveCusEntryHeaderCollection, IActiveCusEntryHeaderCollection
	{
		protected new JobDeclaration declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		public new CusEntryHeader this[int index]
		{
			get { return (CusEntryHeader)base[index]; }
		}

		public new CusEntryHeader AddNew()
		{
			return (CusEntryHeader)base.AddNew();
		}

		ICACusEntryHeader IActiveCusEntryHeaderCollection.AddNew()
		{
			return AddNew();
		}
	}
}
