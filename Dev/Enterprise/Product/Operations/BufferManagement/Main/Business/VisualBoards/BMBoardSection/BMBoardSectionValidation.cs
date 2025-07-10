using System;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public class BMBoardSectionValidation : AutoBMBoardSectionValidation
	{
		public BMBoardSectionValidation(AutoBMBoardSection parent)
			: base(parent)
		{
		}

		new BMBoardSection Parent
		{
			get { return (BMBoardSection)base.Parent; }
		}

		protected override void CheckMS_FC_Component()
		{
			base.CheckMS_FC_Component();
			if (Parent.MS_SectionType == BMConstants.ComponentSectionType)
			{
				MandatoryValidation.CheckEntered(Parent.MS_FC_ComponentInfo);
				ListValidation.ErrorIfInvalidPK(Parent.MS_FC_ComponentInfo);
			}

			if (Parent.Component != null && Parent.SectionConfiguration != null && Parent.Component.FC_Type != BMComponentTypeList.Codes.Bucket && Parent.Component.FC_Type != BMComponentTypeList.Codes.Buffer)
			{
				Parent.MS_FC_ComponentInfo.AddError(Res.GetString("d0def5db-2aaf-4adf-a8eb-e78f6a223d1d", "Cannot add a component type other than Buffer or Bucket to a Buffer Board."));
			}
		}

		protected override void CheckMS_SectionType()
		{
			base.CheckMS_SectionType();

			MandatoryValidation.CheckEntered(Parent.MS_SectionTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.MS_SectionTypeInfo);

			if (AreSectionTypesSelected(BMConstants.MENTSectionType) && !BMSRegistry.Instance.EnableMENTSections.Value)
			{
				Parent.MS_SectionTypeInfo.AddError(Res.GetString("68D5E0EC-607A-4366-A01A-48A9DE797895", "MENT sections are no longer supported."));
			}

			if (AreSectionTypesSelected(BMConstants.WEBSectionType))
			{
				Parent.MS_SectionTypeInfo.AddError(Res.GetString("53740565-790A-4952-BC76-DFBCACD03D6C", "WEB sections are no longer supported."));
			}

			if (!BMSRegistryProvider.IsPlanningManagementEnabled)
			{
				if (!BMSRegistryProvider.IsBufferManagementWorkflowModeOrBetterEnabled && AreSectionTypesSelected(BMConstants.MENTSectionType))
				{
					Parent.MS_SectionTypeInfo.AddError(Res.GetString("5dc9fe06-234e-44b8-80ad-e89f52911699",
						"The selected section type is only available when the registry item [{0}] is set to '{1}' or '{2}'.",
						GetRegistryPath(),
						CombineCodeAndDescription(WorkflowManagementModes.Codes.IncludesBufferManagement, WorkflowManagementModes.Descriptions.IncludesBufferManagement),
						CombineCodeAndDescription(WorkflowManagementModes.Codes.PlanningManagement, WorkflowManagementModes.Descriptions.PlanningManagement)));
				}
				else if (!AreSectionTypesSelected(BMConstants.ComponentSectionType, BMConstants.ModuleGridSectionType, BMConstants.MENTSectionType))
				{
					Parent.MS_SectionTypeInfo.AddError(Res.GetString("61583f31-b686-45c7-9ef6-a2a633148349",
						"The selected section type is only available when the registry item [{0}] is set to '{1}'.",
						GetRegistryPath(),
						CombineCodeAndDescription(WorkflowManagementModes.Codes.PlanningManagement, WorkflowManagementModes.Descriptions.PlanningManagement)));
				}
			}

			bool AreSectionTypesSelected(params string[] sectionTypes)
			{
				return sectionTypes.Contains(Parent.MS_SectionType.ToString(), StringComparer.InvariantCultureIgnoreCase);
			}

			string GetRegistryPath()
			{
				return BMSRegistry.Instance.WorkflowManagementMode.GetLocation();
			}

			string CombineCodeAndDescription(string code, string description)
			{
				return FormattableString.Invariant($"{code} - {description}");
			}
		}

		public void ValidateRow()
		{
			ValidateCalculatedProperty(Parent.RowInfo);
		}

		protected void CheckRow()
		{
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.RowInfo, 0);
			CheckUniqueCoords(Parent.RowInfo, Parent.Column, Parent.Row);
			CheckIntersection(Parent.RowInfo, Parent.Column, Parent.Row, Parent.ColSpan, Parent.RowSpan);
			ValidateColumn();
		}

		public void ValidateColumn()
		{
			ValidateCalculatedProperty(Parent.ColumnInfo);
		}

		protected void CheckColumn()
		{
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.ColumnInfo, 0);
			CheckUniqueCoords(Parent.ColumnInfo, Parent.Column, Parent.Row);
			CheckIntersection(Parent.ColumnInfo, Parent.Column, Parent.Row, Parent.ColSpan, Parent.RowSpan);
			ValidateRow();
		}

		public void ValidateColSpan()
		{
			ValidateCalculatedProperty(Parent.ColSpanInfo);
		}

		protected void CheckColSpan()
		{
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.ColSpanInfo, 1);
			CheckIntersection(Parent.ColSpanInfo, Parent.Column, Parent.Row, Parent.ColSpan, Parent.RowSpan);
		}

		public void ValidateRowSpan()
		{
			ValidateCalculatedProperty(Parent.RowSpanInfo);
		}

		protected void CheckRowSpan()
		{
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.RowSpanInfo, 1);
			CheckIntersection(Parent.RowSpanInfo, Parent.Column, Parent.Row, Parent.ColSpan, Parent.RowSpan);
		}

		public void ValidateRowHeightPercent()
		{
			ValidateCalculatedProperty(Parent.RowHeightPercentInfo);
		}

		protected void CheckRowHeightPercent()
		{
			CompareValidation.CheckWithinRange(Parent.RowHeightPercentInfo, 0, 100);
		}

		public void ValidateColWidthPercent()
		{
			ValidateCalculatedProperty(Parent.ColWidthPercentInfo);
		}

		protected void CheckColWidthPercent()
		{
			CompareValidation.CheckWithinRange(Parent.ColWidthPercentInfo, 0, 100);
		}

		public void ValidateBackgroundColor()
		{
			ValidateCalculatedProperty(Parent.BackgroundColorInfo);
		}

		protected void CheckBackgroundColor()
		{
			ListValidation.ErrorIfInvalidCode(Parent.BackgroundColorInfo, Parent.Lookups.ColorList);

			if (Parent.Configuration is BMComponentSectionConfiguration config)
			{
				if (Parent.BackgroundColor == Color.Transparent.Name)
				{
					Parent.BackgroundColorInfo.AddError(Res.GetString("0b21eaac-e7f3-4bf2-a5b1-d19281450526", "Buffers and buckets cannot have transparent backgrounds"));
				}
				else if (!(config.IsBuffer && config.ShowZones))
				{
					CompareValidation.CheckNotEqual(Parent.BackgroundColorInfo, Parent.ForegroundColorInfo);
				}
			}
		}

		public void ValidateForegroundColor()
		{
			ValidateCalculatedProperty(Parent.ForegroundColorInfo);
		}

		protected void CheckForegroundColor()
		{
			ListValidation.ErrorIfInvalidCode(Parent.ForegroundColorInfo, Parent.Lookups.ColorList);
			if (Parent.Configuration is BMComponentSectionConfiguration && (!Parent.SectionConfiguration.IsBuffer || !Parent.SectionConfiguration.ShowZones))
			{
				CompareValidation.CheckNotEqual(Parent.ForegroundColorInfo, Parent.BackgroundColorInfo);
			}
		}

		void CheckUniqueCoords(ZPropertyInfo info, ZInt col, ZInt row)
		{
			if (Parent.Board != null && Parent.Board.Sections.Cast<BMBoardSection>().Except(new[] { Parent }).Any(s => s.Column == col && s.Row == row))
			{
				info.AddError(Res.GetString("6a33bd83-4c80-435d-8a82-47cdeb027295", "Column and Row combinations must be unique."));
			}
		}

		void CheckIntersection(ZPropertyInfo info, ZInt col, ZInt row, ZInt colSpan, ZInt rowSpan)
		{
			if (Parent.Board != null && Parent.Board.Sections.Cast<BMBoardSection>().Except(new[] { Parent }).Any(s =>
				s.Column < (uint)(int)col + (uint)(int)colSpan && (uint)(int)s.Column + (uint)(int)s.ColSpan > col
				&& s.Row < (uint)(int)row + (uint)(int)rowSpan && (uint)(int)s.Row + (uint)(int)s.RowSpan > row))
			{
				info.AddError(Res.GetString("134de61a-9858-4593-8c3e-1657267e5496", "Sections should not be intersecting."));
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateColumn();
			ValidateRow();
			ValidateColSpan();
			ValidateRowSpan();
			ValidateColWidthPercent();
			ValidateRowHeightPercent();
			ValidateBackgroundColor();
			ValidateForegroundColor();
		}
	}
}
