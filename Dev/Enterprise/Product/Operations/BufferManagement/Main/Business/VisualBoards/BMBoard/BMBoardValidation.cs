using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.BufferManagement.Business
{
	public class BMBoardValidation : AutoBMBoardValidation
	{
		public BMBoardValidation(AutoBMBoard parent)
			: base(parent)
		{
		}

		protected override void CheckMB_Name()
		{
			base.CheckMB_Name();
			MandatoryValidation.CheckEntered(Parent.MB_NameInfo);
			if (((BMBoard)Parent).System != null)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.MB_NameInfo, new BMBoardCollection(((BMBoard)Parent).System, new ZQuery()));
			}
		}

		protected override void CheckMB_IsPublished()
		{
			base.CheckMB_IsPublished();
			if ((!Parent.IsInDatabase || Parent.MB_IsPublishedInfo.HasChanges) && Parent.MB_IsPublished && !Env.Security.BMBoardPublish.IsAllowed)
			{
				Parent.MB_IsPublishedInfo.AddError(Res.GetString("c8e24fc8-9718-44df-bda8-e78a965be6fb", "You do not have permission to publish Visual Boards."));
			}
			else
			{
				CheckAppropriateAccessToBoardSpecified(Parent.MB_IsPublishedInfo);
			}
		}

		protected override void CheckMB_GG_ReleaseGroup()
		{
			base.CheckMB_GG_ReleaseGroup();
			CheckAppropriateAccessToBoardSpecified(Parent.MB_GG_ReleaseGroupInfo);
		}

		protected override void CheckMB_GS_NKStaffCode()
		{
			base.CheckMB_GS_NKStaffCode();
			CheckAppropriateAccessToBoardSpecified(Parent.MB_GS_NKStaffCodeInfo);
		}

		void CheckAppropriateAccessToBoardSpecified(ZPropertyInfo propertyInfo)
		{
			if (!Parent.MB_IsPublished && Parent.MB_GG_ReleaseGroup.IsEmpty && Parent.MB_GS_NKStaffCode.IsEmpty)
			{
				propertyInfo.AddError(Res.GetString("932334af-ad66-4575-bc66-30236924d69e", "This board must either be Published, have a Release Group specified, or an Owner specified."));
			}
		}
	}
}
