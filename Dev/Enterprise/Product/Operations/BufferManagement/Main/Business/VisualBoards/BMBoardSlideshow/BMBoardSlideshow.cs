using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[CodeProperty(BMBoardSlideshow.Schema.MD_Name), DescriptionProperty(BMBoardSlideshow.Schema.MD_Name)]
	public class BMBoardSlideshow : AutoBMBoardSlideshow,
		IBMBoardSlideshow,
		IAuditParent
	{
		public BMBoardSlideshow(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Business Objects

		[ChildEditable]
		public BMBoardSlideshowPivotCollection BoardPivots
		{
			get
			{
				if (boardPivots == null)
				{
					boardPivots = new BMBoardSlideshowPivotCollection(this);
					boardPivots.ApplySort(BMBoardSlideshowPivotSchema.MC_Sequence.Name, ListSortDirection.Ascending);
					RegisterEditableChildObject(boardPivots);
				}

				return boardPivots;
			}
		}

		BMBoardSlideshowPivotCollection boardPivots;

		#endregion

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			MD_IsPublished = true;
		}

		public override void Delete()
		{
			base.Delete();
			BoardPivots.DeleteAll();
		}

		protected override ZString HumanReadableNameCore
		{
			get { return MD_Name; }
		}

		#endregion

		#region IVisualBoardProvider Members

		string IVisualBoardProvider.Name
		{
			get { return MD_Name; }
		}

		string IVisualBoardProvider.Description
		{
			get { return MD_Name; }
		}

		string IVisualBoardProvider.HumanReadableShortcutName => HumanReadableShortcutName;

		IVisualBoardProvider IVisualBoardProvider.ReloadInFactory(BusinessObjectFactory factory)
		{
			return factory.Load<BMBoardSlideshow>(PK);
		}

		IEnumerable<ISlideShowFrame> IVisualBoardProvider.Boards
		{
			get
			{
				foreach (var boardPivot in BoardPivots)
				{
					yield return new SlideShowFrame(boardPivot.Board.PK.ToGuid(), boardPivot.Board.MB_Name, boardPivot.MC_DurationInSeconds, boardPivot.Board.Sections?.ToArray());
				}
			}
		}

		Guid IVisualBoardProvider.PK
		{
			get { return PK.ToGuid(); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int IVisualBoardProvider.StartingRefreshIntervalInMinutes
		{
			get { return BMSRegistry.Instance.DefaultBoardRefreshIntervalMinutes.Value; }
		}

		string IVisualBoardProvider.OwnerStaffCode => MD_GS_NKStaffCode;

		ZGuid IVisualBoardProvider.OwnerGroupPK => MD_GG_ReleaseGroup;

		SecurityCheckpoint IVisualBoardProvider.EditCheckpoint => Env.Security.BMSlideShowsEdit;

		ControllerID IVisualBoardProvider.ControllerID => ControllerIDs.BMBoardSlideshow;

		ZBool IVisualBoardProvider.IsVisibleToCurrentCompany => true;

		#endregion

		#region Helper Methods

		public static IEnumerable<BMComponent> GetAllComponentsAcrossMultipleBoards(IEnumerable<BMBoard> boards)
		{
			AddFetchHintsForSectionsAcrossMultipleBoards(boards);

			var sections = boards.SelectMany(board => board.Sections)
				.WhereNotNull()
				.Where(section => section.SectionConfiguration != null);

			AddFetchHintsForComponentsAcrossMultipleSections(sections);

			var relationships = sections.SelectMany(section => section.SectionConfiguration.AdditionalComponents.Select(component => component.Component))
				.OfType<ComponentRelationship>()
				.Where(relationship => relationship != null && relationship.FC_IsActive);

			ComponentRelationship.AddComponentFetchHints(relationships);

			foreach (var relationship in relationships)
			{
				relationship.Factory.AddFetchHint(BMComponentSchema.Instance,
					new ZQuery(BMComponentSchema.FC_FC_ParentComponent, relationship.RelatedComponentLinks.Select(link => link.FL_FC_ComponentTo)));
			}

			return sections.SelectMany(section => section.AllComponents).Distinct();
		}

		static void AddFetchHintsForSectionsAcrossMultipleBoards(IEnumerable<BMBoard> boards)
		{
			foreach (var board in boards)
			{
				board.Factory.AddFetchHint(BMBoardSectionSchema.Instance, new ZQuery(BMBoardSectionSchema.MS_MB_Board, board.PK));
			}
		}

		static void AddFetchHintsForComponentsAcrossMultipleSections(IEnumerable<BMBoardSection> sections)
		{
			foreach (var section in sections)
			{
				section.Factory.AddFetchHint(BMBoardSectionAdditionalComponentSchema.Instance, new ZQuery(BMBoardSectionAdditionalComponentSchema.BSA_MS_Section, section.PK));
			}

			foreach (var section in sections)
			{
				section.Factory.AddFetchHint(BMComponentSchema.Instance, new ZQuery(BMComponentSchema.PK, section.MS_FC_Component));
				section.Factory.AddFetchHint(BMComponentSchema.Instance, new ZQuery(BMComponentSchema.FC_FC_ParentComponent, section.MS_FC_Component));

				section.Factory.AddFetchHint(BMComponentSchema.Instance,
					new ZQuery(BMComponentSchema.PK, section.SectionConfiguration.AdditionalComponents.Select(component => component.BSA_FC_Component)));
				section.Factory.AddFetchHint(BMComponentSchema.Instance,
					new ZQuery(BMComponentSchema.FC_FC_ParentComponent, section.SectionConfiguration.AdditionalComponents.Select(component => component.BSA_FC_Component)));
			}
		}

		#endregion

		#region IAuditParent Mambers

		public IEnumerable<AuditChildInfo> RelatedAuditChildren => Enumerable.Empty<AuditChildInfo>();

		#endregion
	}
}
