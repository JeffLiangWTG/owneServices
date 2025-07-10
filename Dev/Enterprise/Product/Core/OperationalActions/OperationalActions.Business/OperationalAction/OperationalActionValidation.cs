using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.OperationalActions.Business
{
	public class OperationalActionValidation : StmMenuItemValidation
	{
		public OperationalActionValidation(OperationalAction parent)
			: base(parent) { }

		protected override void CheckSU_IsSystemDefined()
		{
			base.CheckSU_IsSystemDefined();
			ValidateSU_Calc_IsPublished();
		}

		protected override void CheckSU_MenuName()
		{
			base.CheckSU_MenuName();
			MandatoryValidation.CheckEntered(Parent.SU_MenuNameInfo);
			if (((IBusinessObjectInternals)Parent).ParentCollections.Length > 0)
			{
				foreach (OperationalAction sibling in ((IBusinessObjectInternals)Parent).ParentCollections[0])
				{
					if (sibling != Parent)
					{
						if (StringComparer.OrdinalIgnoreCase.Equals(Parent.SU_MenuNameMultilingual, sibling.SU_MenuNameMultilingual) &&
							MenuPathComparer.Equals(Parent.SU_MenuPath, sibling.SU_MenuPath))
						{
							Parent.SU_MenuNameInfo.AddError(Res.GetString("{8677D7F5-D203-4fb6-825D-CEA198A17F0D}", "The Menu Name and path has been duplicated and must be unique."));
						}
					}
				}
			}
		}

		protected override void CheckSU_MenuPath()
		{
			base.CheckSU_MenuPath();
			TranslatableDataFieldAttribute.Validate(Parent.SU_MenuPathInfo);
		}

		protected override void CheckSU_SE_NKDocumentEvent()
		{
			base.CheckSU_SE_NKDocumentEvent();
			ListValidation.ErrorIfInvalidCode(Parent.SU_SE_NKDocumentEventInfo, Parent.Lookups.Events);
		}

		public void ValidateSU_Calc_IsPublished()
		{
			ValidateCalculatedProperty(Parent.SU_Calc_IsPublishedInfo);
		}

		protected void CheckSU_Calc_IsPublished()
		{
			if (!Parent.SU_Calc_IsPublished && Parent.SU_IsSystemDefined)
			{
				Parent.SU_Calc_IsPublishedInfo.AddError(Res.GetString("{1DBBD0F5-0CC1-4f52-BC8D-1CDABC3FC099}", "System-defined Operational Actions must be published."));
			}
		}

		protected override void CheckSU_FilterList()
		{
			base.CheckSU_FilterList();

			try
			{
				IFilterExpression expression = FilterParser.Parse(Parent.SU_FilterList);

				foreach (string constraint in expression.GetConstraints())
				{
					if (EnvironmentFilterProvider.Instance.GetConstraint(constraint) == null)
					{
						Parent.SU_FilterListInfo.AddError(Res.GetString("{32696055-97AD-4900-A5D9-C30CC3E1851F}", "'{0}' is not a recognized constraint", constraint));
					}
				}
			}
			catch (ParseException ex)
			{
				Parent.SU_FilterListInfo.AddError(ex.Message);
			}
		}

		public void ValidateRow()
		{
			Parent.ClearRowNotifications();
			CheckRow();
		}

		protected void CheckRow()
		{
			if (IsMeaninglessAction(Parent))
			{
				if (Parent.Context.Supporter == null || Parent.Context.SupportsDocuments)
				{
					Parent.AddRowError(Res.GetString("{8685389D-77DD-4009-A542-6D7B19D7BEE9}", "This action is not meaningful. Please add a document, defined process, field or event."));
				}
				else
				{
					Parent.AddRowError(Res.GetString("{CBCED1E2-5B89-40d8-819A-5725E7E72386}", "This action is not meaningful. Please add a defined process, field or event."));
				}
			}
		}

		#region Implementation

		new OperationalAction Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (OperationalAction)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateSU_Calc_IsPublished();
			ValidateRow();
		}

		static bool IsMeaninglessAction(OperationalAction action)
		{
			return action.SU_SE_NKDocumentEvent.IsEmpty
				&& action.DocumentPivots.Count == 0
				&& action.MethodDescriptors.Count == 0
				&& action.FieldDescriptors.Count == 0;
		}

		#endregion
	}
}
