using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class GuaranteeAccessCodesUserSelectionObjectCollection : NonPersistentBusinessObjectCollection<GuaranteeAccessCodesUserSelectionObject>
	{
		public GuaranteeAccessCodesUserSelectionObjectCollection(NctsHeader nctsHeader) : base(Argument.NotNull(nctsHeader, nameof(nctsHeader)).Factory)
		{
			header = nctsHeader;
		}

		readonly NctsHeader header;

		public void PopulateElements()
		{
			RemoveAndDeleteAll();
			foreach (var guarantee in header.GetEffectiveGuarantees().Cast<NctsGuarantee>().Where(nctsGuarantee => nctsGuarantee.CusGuarantee != null))
			{
				Add(new GuaranteeAccessCodesUserSelectionObject(guarantee));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotSupportedException("Allow new is false so shouldn't get called");

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
