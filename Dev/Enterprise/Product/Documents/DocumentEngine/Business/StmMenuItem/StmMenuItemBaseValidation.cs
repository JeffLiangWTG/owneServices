using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Business
{
	public class StmMenuItemBaseValidation : StmMenuItemValidation
	{
		public StmMenuItemBaseValidation(StmMenuItemBase validatee)
			: base(validatee)
		{
			Validatee = validatee;
		}
		readonly StmMenuItemBase Validatee;

		protected override void CheckSU_ContactType()
		{
			base.CheckSU_ContactType();

			if (Validatee.SU_ContactType.IsEmpty)
			{
				Validatee.SU_ContactTypeInfo.AddError(Res.GetString("462bc758-9a89-47c1-b786-8850483467d9", "You need to select a document group from the list."));
			}
			else if (!Validatee.ContactTypeList.ContainsCode(Validatee.SU_ContactType))
			{
				Validatee.SU_ContactTypeInfo.AddError(Res.GetString("775b33b5-84f3-4581-9e38-e076315ea17e", "You have entered an invalid code for document group. Please select one from the list."));
			}
		}

		protected override void CheckSU_AddressCategory()
		{
			base.CheckSU_AddressCategory();
			ListValidation.ErrorIfInvalidCode(Validatee.SU_AddressCategoryInfo, new OrgAddressCategory());
			if (Validatee.SU_AddressCategory == "")
			{
				if (!Validatee.SU_PreventAutoDelivery)
				{
					Validatee.SU_AddressCategoryInfo.AddError(Res.GetString("82d5bb6a-43a2-4ce1-9c4a-3673f37d5ac7", "Select an Address Category from the list"));
				}
			}
		}

		protected override void CheckSU_DraftOption()
		{
			base.CheckSU_DraftOption();
			ListValidation.ErrorIfInvalidCode(Validatee.SU_DraftOptionInfo, Validatee.SU_DraftOption_List, ResString.GetMultilingualString("42d0ae8a-e4db-4650-b294-a940f38efec5", "You need to select a valid draft option from the list"));
		}

		protected override void CheckSU_Hint()
		{
			base.CheckSU_Hint();

			if (Parent.HasChanges)
			{
				var filter = new ZQuery(Enterprise.ZArchitecture.Schema.StmMenuItemSchema.SU_BusinessContext, Parent.SU_BusinessContext);
				filter.AddToFilter(Enterprise.ZArchitecture.Schema.StmMenuItemSchema.SU_MenuName, Parent.SU_MenuName);
				filter.AddToFilter(Enterprise.ZArchitecture.Schema.StmMenuItemSchema.SU_MenuPath, Parent.SU_MenuPath);

				var collection = new StmMenuItemCollection(Parent.Factory, filter);
				collection.Load();

				var documents = collection.Cast<StmMenuItem>();

				if (documents.Count() > 1 && documents.Select(d => d.SU_Hint).Distinct().Count() < documents.Count())
				{
					Validatee.SU_HintInfo.AddError(Res.GetString("d241eea3-dfb8-4ed4-9ee1-5e582a4a3c07", "At least one other document has the same name and menu path. Please enter a unique description."));
				}
			}
		}

		protected override void CheckSU_DocumentDirection()
		{
			base.CheckSU_DocumentDirection();

			if (Validatee.SU_DocumentDirection.IsEmpty)
			{
				Validatee.SU_DocumentDirectionInfo.AddError(Res.GetString("d683eeb4-40e7-4bfc-bd0e-f12a51a8b31e", "You need to select a document direction from the list."));
			}
			else
			{
				try
				{
					if (!Validatee.SU_DocumentDirection.ContainsAnyChar("1234567890"))
					{
						DocumentDirection tryCast = (DocumentDirection)Enum.Parse(typeof(DocumentDirection), Validatee.SU_DocumentDirection);
					}
					else
					{
						throw new Exception();
					}
				}
				catch (Exception)
				{
					Validatee.SU_DocumentDirectionInfo.AddError(Res.GetString("1ea9e002-8efd-43bf-a3c4-a530de3576b2", "You have entered an invalid code for document direction. Please select one from the list."));
				}
			}
		}

		protected override void CheckSU_SignBy()
		{
			base.CheckSU_SignBy();

			MandatoryValidation.CheckEntered(Validatee.SU_SignByInfo, Res.GetString("661a0c14-6cfd-4ecf-a7ff-0c866e8197a4", "Sign By"));

			if (Parent.SU_SignByInfo.HasChanges)
			{
				ListValidation.ErrorIfInvalidCode(Validatee.SU_SignByInfo, Validatee.SignByList);
				CheckDocumentsAllowedForSigningByDOS();
				ErrorIfDOSModifiedByUnsupportedLoginCompany();
			}
		}

		void ErrorIfDOSModifiedByUnsupportedLoginCompany()
		{
			if (!DocumentsDataRegistry.Instance.EnableDocumentSigningService.Value)
			{
				if ((ZString)Parent.SU_SignByInfo.OriginalValue == DocumentsSignBy.DOS && Parent.SU_SignBy != DocumentsSignBy.DOS)
				{
					Validatee.SU_SignByInfo.AddError(Res.GetString("300a7d83-8f8a-4068-923e-4b0c3d5c47a2", "Modifying the DOS option while logged into this Company is not supported."));
				}
			}
		}

		void CheckDocumentsAllowedForSigningByDOS()
		{
			if (Validatee.SU_SignBy == DocumentsSignBy.DOS && !Env.CurrentUser.IsSupportUser)
			{
				if (!DocumentsDataRegistry.Instance.DocumentsAllowedForSigning.Value.IsDocumentsAllowedForSigning(Validatee.SU_MenuName, Validatee.SU_IsSystemDefined))
				{
					Validatee.SU_SignByInfo.AddError(Res.GetString("1FA2A6C7-843C-47B4-BBE7-0949B274D98B", "You cannot use the DOS service task for this document type."));
				}
			}
		}

		protected override void CheckSU_PreventAutoDelivery()
		{
			base.CheckSU_PreventAutoDelivery();
			ValidateSU_AddressCategory();
		}

		protected override void CheckSU_MenuType()
		{
			base.CheckSU_MenuType();

			MandatoryValidation.CheckEntered(Validatee.SU_MenuTypeInfo, Res.GetString("1bcdd94f-1ac0-4c9f-987c-1c3041454efe", "Menu Type"));
			ListValidation.ErrorIfInvalidCode(Validatee.SU_MenuTypeInfo, Validatee.MenuTypeList);
		}

		protected override void CheckSU_MenuName()
		{
			base.CheckSU_MenuName();

			MandatoryValidation.CheckEntered(Validatee.SU_MenuNameInfo, Res.GetString("46eb1560-5d54-49f9-a3a4-a758f0fd56af", "Menu Name"));
		}

		public void ValidateSU_Calc_IsWebSupportable()
		{
			ValidateCalculatedProperty(Validatee.SU_Calc_IsWebSupportableInfo);
		}

		protected virtual void CheckSU_Calc_IsWebSupportable()
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateSU_Calc_IsWebSupportable();
		}
	}
}
