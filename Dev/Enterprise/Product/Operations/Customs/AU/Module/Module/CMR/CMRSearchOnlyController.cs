using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public abstract class CMRSearchOnlyController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected CMRSearchOnlyController()
		{
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new NotSupportedException("HasAction is false for this module");
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get
			{
				return Env.Security.CustomsDeclarationEnquiry;
			}
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get
			{
				return Env.Security.CustomsDeclarationEnquiry;
			}
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get
			{
				return Env.Security.CustomsDeclarationEnquiry;
			}
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get
			{
				return Env.Security.CustomsDeclarationEnquiry;
			}
		}
	}
}
