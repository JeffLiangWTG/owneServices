using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Res = Enterprise.Accounting.Business.Res;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	public class GLPresentationJournalCategoryValidation
	{
		public GLPresentationJournalCategoryValidation(GLPresentationJournalCategory parent)
		{
			this.Parent = parent;
		}

		readonly GLPresentationJournalCategory Parent;

		public virtual void ValidateCode()
		{
			MandatoryValidation.CheckEntered(Parent.CodeInfo, (IMultilingualString)ResString.GetMultilingualString("ab2841fe-d766-4d7e-a573-0dfc00b05881", "Code"));
			if (!Parent.CodeInfo.HasErrors() && Parent.ParentCollection != null && Parent.ParentCollection.ToArray<GLPresentationJournalCategory>().Any(x => x != Parent && x.Code.ToUpper() == Parent.Code.ToUpper()))
			{
				Parent.CodeInfo.AddError(Res.GetString("d205f3f4-2e20-4043-9ed8-32c4415c5eb7", "Code must be unique"));
			}
		}

		public virtual void ValidateDescription()
		{
			MandatoryValidation.CheckEntered(Parent.DescriptionInfo, (IMultilingualString)ResString.GetMultilingualString("3765ce49-c05c-4621-855c-c73b4da2a1a7", "Description"));
		}

		public virtual void ValidateIsActive()
		{
		}

		public virtual void ValidateUseForElimination()
		{
			if (Parent.Bool2 && Parent.ParentCollection != null && Parent.ParentCollection.ToArray<GLPresentationJournalCategory>().Any(x => x != Parent && x.Bool2))
			{
				Parent.Bool2Info.AddError(Res.GetString("4816B990-E855-45EE-9B3A-1050B5285BEC", "Only one Category can be used for Elimination"));
			}
		}

		public virtual void ValidateUseForClosing()
		{
			if (Parent.Bool3 && Parent.ParentCollection != null)
			{
				if (Parent.ParentCollection.ToArray<GLPresentationJournalCategory>().Any(x => x != Parent && x.Bool3))
				{
					Parent.Bool3Info.AddError(Res.GetString("7A482B54-95A1-4942-9824-8B506DCBBA30", "Only one Category can be used for Closing"));
				}
				if (Parent.Bool2)
				{
					Parent.Bool3Info.AddError(Res.GetString("F02F16F3-7A0A-4AD3-B7BB-C3FB315D8A62", "Category used for Elimination can not be used for Closing"));
				}
				if (Parent.Bool4)
				{
					Parent.Bool3Info.AddError(Res.GetString("5E585ECB-D8EE-410F-9923-20DB13D38BDF", "Category used for Opening can not be used for Closing"));
				}
			}
		}

		public virtual void ValidateUseForOpening()
		{
			if (Parent.Bool4 && Parent.ParentCollection != null)
			{
				if (Parent.ParentCollection.ToArray<GLPresentationJournalCategory>().Any(x => x != Parent && x.Bool4))
				{
					Parent.Bool4Info.AddError(Res.GetString("85732DD6-6B5B-4A7D-8D01-EA799497871B", "Only one Category can be used for Opening"));
				}
				if (Parent.Bool2)
				{
					Parent.Bool4Info.AddError(Res.GetString("D3326FD5-C043-4701-81CD-CFB2D5015CD5", "Category used for Elimination can not be used for Opening"));
				}
				if (Parent.Bool3)
				{
					Parent.Bool4Info.AddError(Res.GetString("3786313B-D6FF-4D73-AF11-D5DB4DFB34EC", "Category used for Closing can not be used for Opening"));
				}
			}
		}

		public virtual void ValidateParentCode()
		{
			if (!Parent.ParentCode.IsEmpty && Parent.ParentCollection != null)
			{
				if (!Parent.ParentCollection.ToArray<GLPresentationJournalCategory>().Any(x => x.Code != Parent.Code && x.Code == Parent.ParentCode))
				{
					Parent.ParentCodeInfo.AddError(Res.GetString("A596F94D-4098-42F6-A389-1A6C9B231FDB", "Code '{0}' is not valid", Parent.ParentCode));
				}

				var presentationJournalCategory = Parent.ParentCollection.ToArray<GLPresentationJournalCategory>().FirstOrDefault(x => x.Code != Parent.Code && x.Code == Parent.ParentCode && !x.ParentCode.IsEmpty);
				if (presentationJournalCategory != null)
				{
					Parent.ParentCodeInfo.AddError(Res.GetString("A3F2022B-2264-47A5-93A1-C097C8C820B3", "Code '{0}' is the child of '{1}' and cannot be setup as parent of '{2}'.", presentationJournalCategory.Code, presentationJournalCategory.ParentCode, Parent.Code));
				}
			}
		}
	}
}
