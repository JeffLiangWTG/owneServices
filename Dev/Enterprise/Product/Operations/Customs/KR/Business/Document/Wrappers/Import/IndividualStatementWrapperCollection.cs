using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class IndividualStatementWrapperCollection : NonPersistentBusinessObjectCollection<IndividualStatementWrapper>
	{
		public IndividualStatementWrapperCollection(CusEntryHeader entry, BusinessObjectFactory factory) : base(factory)
		{
			if (entry == null)
			{
				throw new ArgumentNullException(nameof(entry), "Entry must need.");
			}
			PopulateElements(entry);
		}
		
		void PopulateElements(CusEntryHeader entry)
		{
			foreach (var statement in entry.IndividualStatements)
			{
				var wrapper = new IndividualStatementWrapper(statement);
				wrapper.Decorate(entry);
				Add(wrapper);
			}
		}
		protected override bool AllowNewCore => false;
		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotSupportedException();
	}
}
