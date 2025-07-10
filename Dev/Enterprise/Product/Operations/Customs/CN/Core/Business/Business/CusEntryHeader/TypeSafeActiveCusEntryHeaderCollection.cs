namespace Enterprise.Customs.CN.Business
{
	partial class ActiveCusEntryHeaderCollection : Customs.Business.ActiveCusEntryHeaderCollection
	{
		protected new JobDeclaration declaration => (JobDeclaration)base.declaration;

		public new CusEntryHeader this[int index] => (CusEntryHeader)base[index];

		public new CusEntryHeader AddNew() => (CusEntryHeader)base.AddNew();
	}
}
