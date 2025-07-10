//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoArchiveEDocsValidation
//
//    This class should be used for overriding validation in AutoArchiveEDocsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.DocumentScanning.Business
{
	public class ArchiveEDocsValidation : AutoArchiveEDocsValidation
	{
		public ArchiveEDocsValidation(AutoArchiveEDocs parent)
			: base(parent)
		{
		}

		protected override void CheckSA_IncludeConsignee()
		{
			base.CheckSA_IncludeConsignee();
			ValidateSA_IncludeConsignor();
			if (!IsConsigneeOrConsignorSelected)
			{
				Parent.SA_IncludeConsigneeInfo.AddError(Res.GetString("fae85188-7ab2-4bb4-bf14-0df25daefdd1", "You must select whether this organization is Consignor or Consignee to find matching records."));
			}
		}

		protected override void CheckSA_IncludeConsignor()
		{
			base.CheckSA_IncludeConsignor();
			ValidateSA_IncludeConsignee();
			if (!IsConsigneeOrConsignorSelected)
			{
				Parent.SA_IncludeConsignorInfo.AddError(Res.GetString("fae85188-7ab2-4bb4-bf14-0df25daefdd1", "You must select whether this organization is Consignor or Consignee to find matching records."));
			}
		}

		bool IsConsigneeOrConsignorSelected
		{
			get { return Parent.SA_IncludeConsignor || Parent.SA_IncludeConsignee; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateIsSearchTypeSelected();
		}

		public void ValidateIsSearchTypeSelected()
		{
			this.ValidateCalculatedProperty((Parent as ArchiveEDocsManager).IsSearchTypeSelectedInfo);
		}

		static string AtLeastOneSearchTypeHasToBeSelected
		{
			get { return Res.GetString("a8ce0785-33f2-4a8c-8666-2a7ecffbb3c3", "At least one Search Type has to be selected"); }
		}

		protected void CheckIsSearchTypeSelected()
		{
			ArchiveEDocsManager manager = Parent as ArchiveEDocsManager;

			if (!manager.IsSearchTypeSelected)
			{
				manager.IsSearchTypeSelectedInfo.AddError(AtLeastOneSearchTypeHasToBeSelected);

				foreach (SearchType searchType in manager.SearchTypes)
				{
					searchType.IsFilterOnInfo.AddError(AtLeastOneSearchTypeHasToBeSelected);
				}
			}
			else
			{
				foreach (SearchType searchType in manager.SearchTypes)
				{
					searchType.IsFilterOnInfo.ClearAllNotifications();
				}
			}
		}
	}
}
