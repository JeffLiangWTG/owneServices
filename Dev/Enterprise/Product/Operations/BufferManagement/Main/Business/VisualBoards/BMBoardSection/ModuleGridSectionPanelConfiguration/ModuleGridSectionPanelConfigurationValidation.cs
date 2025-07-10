using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Business
{
	public class ModuleGridSectionPanelConfigurationValidation : ZValidation
	{
		public ModuleGridSectionPanelConfigurationValidation(ModuleGridSectionPanelConfiguration panelConfiguration)
			: base(panelConfiguration)
		{
			this.panelConfiguration = panelConfiguration;
		}

		readonly ModuleGridSectionPanelConfiguration panelConfiguration;

		#region Implementation

		public override void ValidateAll()
		{
			ValidateSequence();
			ValidateSectionNameOverride();
			ValidateFilterLayout();
		}

		public override Type AutoValidationType
		{
			get { return typeof(ModuleGridSectionPanelConfigurationValidation); }
		}

		public void ValidateSequence()
		{
			ValidateCalculatedProperty(panelConfiguration.SequenceInfo);
		}

		public void ValidateSectionNameOverride()
		{
			ValidateCalculatedProperty(panelConfiguration.SectionNameOverrideInfo);
		}

		public void ValidateFilterLayout()
		{
			ValidateCalculatedProperty(panelConfiguration.FilterLayoutInfo);
		}

		protected void CheckSequence()
		{
			CompareValidation.CheckWithinRange(panelConfiguration.SequenceInfo, 1, 100);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(panelConfiguration.SequenceInfo);
		}

		protected void CheckSectionNameOverride()
		{
			if (panelConfiguration.SectionNameIsOverridden && string.IsNullOrWhiteSpace(panelConfiguration.SectionNameOverride))
			{
				panelConfiguration.SectionNameOverrideInfo.AddError(Res.GetString("2A83A85F-E8E5-446F-A5A1-91E046498349", "Custom panel name cannot be empty when overriding panel name."));
			}
		}

		protected void CheckFilterLayout()
		{
			if (!panelConfiguration.FilterLayout.IsEmpty)
			{
				var layout = panelConfiguration.Factory.Load<StmModuleFilter>(panelConfiguration.FilterLayout);
				if (layout == null)
				{
					//layout can be null during unit testing when a wrong GUID is passed
					panelConfiguration.FilterLayoutInfo.AddError(Res.GetString("9E536510-0C79-4704-9352-D95EFD528504", "The layout cannot be loaded."));
					return;
				}
				var board = panelConfiguration.Section.Board;
				if (!layout.S9_IsPublished)
				{
					if (board != null && board.MB_IsPublished)
					{
						panelConfiguration.FilterLayoutInfo.AddError(Res.GetString("A4628E29-D1C3-4ECA-926B-C2A2974D8CA2", "This layout is not published, and therefore cannot be applied to a published visual board."));
						return;
					}
					else if (board != null && board.MB_GG_ReleaseGroup != null && !board.MB_GG_ReleaseGroup.IsEmpty)
					{
						panelConfiguration.FilterLayoutInfo.AddError(Res.GetString("C85D09B5-A0AA-4380-A36A-EF14E013F395", "This layout is not published, and therefore cannot be applied to a visual board associated with a release group."));
						return;
					}
					else
					{
						panelConfiguration.FilterLayoutInfo.AddWarning(Res.GetString("4EE1C22F-B748-4AA5-9FB0-99D42E73E37B", "This layout is not published, and will not be applied when other users open this visual board."));
					}
				}
				if (!layout.S9_GC.IsEmpty)
				{
					if (board != null && board.IsGlobal)
					{
						panelConfiguration.FilterLayoutInfo.AddError(Res.GetString("AF03B85B-19E9-4705-8BD2-A5897AE4AF6F", "This layout is not published for all companies, and therefore cannot be applied to a global visual board."));
					}
					else
					{
						var currentCompany = GlbCompany.CurrentCompany;
						if (currentCompany != null && layout.S9_GC != currentCompany.PK)
						{
							panelConfiguration.FilterLayoutInfo.AddError(Res.GetString("AB22D078-2249-4010-B749-48ED262CCC21", "This layout is not published for company {0}, and therefore cannot be applied to a company specific visual board.", currentCompany.CompanyName));
						}
					}
				}
			}
		}

		#endregion
	}
}
