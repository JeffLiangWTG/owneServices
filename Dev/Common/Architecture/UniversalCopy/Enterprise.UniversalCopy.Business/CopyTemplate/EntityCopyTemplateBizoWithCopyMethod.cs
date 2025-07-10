using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.UniversalCopy.Business
{
	public abstract class EntityCopyTemplateBizoWithCopyMethod : EntityCopyTemplateBizo
	{
		protected EntityCopyTemplateBizoWithCopyMethod(WrappedCopyTemplateNode copyTemplateNode, CopyTemplateNode rootNode, EntityCopyTemplateBizo parent)
			: base(copyTemplateNode, rootNode)
		{
			Parent = parent;
		}

		public EntityCopyTemplateBizo Parent { get; private set; }

		#region Copy Method

		[List("CopyMethods")]
		[MaxLength(3)]
		public ZString CopyMethod
		{
			get { return CopyMethodCore; }
			set
			{
				if (CopyMethodCore != value)
				{
					CheckMaximumLength(CopyMethodInfo, value);

					CopyMethodCore = value;

					if (!NeedsChildrenForCopy() && HasNotifications())
					{
						RunPreSaveValidation();
					}

					HasChanges = true;
					CopyMethodInfo.RefreshBinding();
					CopyMethodDescriptionInfo.RefreshBinding();
				}
			}
		}

		protected abstract ZString CopyMethodCore { get; set; }

		public ZPropertyInfo CopyMethodInfo
		{
			get { return GetZPropertyInfo(nameof(CopyMethod)); }
		}

		public CodeDescriptionPairList CopyMethods
		{
			get { return copyMethods ?? (copyMethods = GetCopyMethodsCore()); }
		}
		CodeDescriptionPairList copyMethods;

		protected abstract CodeDescriptionPairList GetCopyMethodsCore();

#if DEBUG
		internal void ResetCopyMethodsForTest()
		{
			copyMethods = null;
		}
#endif

		#endregion

		#region Copy Method Description

		public override string GetCopyActionDescription()
		{
			return CopyMethodDescription;
		}

#if DEBUG
		[BusinessObjectTestExclude]
#endif
		[List("CopyMethodDescriptions")]
		public ZString CopyMethodDescription
		{
			get { return CopyMethods.GetDescriptionFromCode(CopyMethod); }
			set
			{
				CopyMethod = CopyMethods.GetCodeFromDescription(value);
				CopyMethodDescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CopyMethodDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(CopyMethodDescription)); }
		}

		public CodeDescriptionPairList CopyMethodDescriptions
		{
			get
			{
				if (copyMethodDescriptions == null)
				{
					copyMethodDescriptions = new CodeDescriptionPairList();
					foreach (CodeDescriptionPair copyMethod in CopyMethods)
					{
						copyMethodDescriptions.AddPair(copyMethod.MultilingualDescription);
					}
				}
				return copyMethodDescriptions;
			}
		}
		CodeDescriptionPairList copyMethodDescriptions;

		#endregion

		#region Validation

		protected override CopyTemplateNodeBizoValidation GetNewValidation()
		{
			return new EntityCopyTemplateBizoWithCopyMethodValidation(this);
		}

		#endregion
	}

	#region Validation Class

	class EntityCopyTemplateBizoWithCopyMethodValidation : CopyTemplateNodeBizoValidation
	{
		public EntityCopyTemplateBizoWithCopyMethodValidation(EntityCopyTemplateBizoWithCopyMethod parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			inValidateAll = true;
			try
			{
				ValidateCopyMethod();
				ValidateCopyMethodDescription();
			}
			finally
			{
				inValidateAll = false;
			}
		}
		bool inValidateAll;

		protected new EntityCopyTemplateBizoWithCopyMethod Parent
		{
			get { return (EntityCopyTemplateBizoWithCopyMethod)base.Parent; }
		}

		#region Properties Validation

		#region CopyMethod

		public void ValidateCopyMethod()
		{
			ZValidationInternals.Validate(Parent.CopyMethodInfo,
				() =>
				{
					CheckCopyMethod();
					if (inValidateAll || hadCheckHasSomethingToCopyError)
					{
						CheckHasSomethingToCopy();
					}
				});
		}

		protected virtual void CheckCopyMethod()
		{
			ListValidation.ErrorIfInvalidCode(Parent.CopyMethodInfo);

			if (!Parent.CopyMethod.IsEmpty)
			{
				var parentParent = Parent.Parent as ICopyable;
				if (parentParent != null && parentParent.CopyMethod.IsEmpty)
				{
					Parent.CopyMethodInfo.AddWarning(
						Res.GetString("c9d56b7c-ea44-46c6-a4b6-fa2ddb43c171", "To copy this {0} please select a copy method on its parent {1}", Parent.Name, Parent.Parent.Name));
				}
			}
			else if (Parent.CopyTemplateNode.IsMandatory)
			{
				Parent.CopyMethodInfo.AddError(Res.GetString("AE88A0FF-7D60-4515-9786-59447788D697", "Related entity {0} is mandatory to be copied.", Parent.Name));
			}
		}

		void CheckHasSomethingToCopy()
		{
			if (NeedCheckCopySelection &&
				!Parent.ChildNodes.Cast<EntityCopyTemplateBizo>().Any(nodeBizo => nodeBizo.CopyTemplateNode.HasData()) &&
				!Parent.PropertyNodes.Cast<PropertyCopyTemplateBizo>().Any(nodeBizo => nodeBizo.CopyTemplateNode.HasData()))
			{
				Parent.CopyMethodInfo.AddError(Res.GetString("e61a8953-b22e-49ec-9c22-5d3c38c19607", "There is nothing selected on {0} to copy.", Parent.Name));
				hadCheckHasSomethingToCopyError = true;
			}
		}

		bool hadCheckHasSomethingToCopyError;

		protected virtual bool NeedCheckCopySelection
		{
			get { return false; }
		}

		public void ValidateCopyMethodDescription()
		{
			ZValidationInternals.Validate(Parent.CopyMethodDescriptionInfo, CheckCopyMethodDescription);
		}

		void CheckCopyMethodDescription()
		{
			ValidateCopyMethod();

			if (Parent.CopyMethodInfo.HasNotifications())
			{
				Parent.CopyMethodDescriptionInfo.AddAllNotificationsFrom(Parent.CopyMethodInfo);
			}
		}

		#endregion

		#endregion
	}

	#endregion
}
