using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[CodeProperty(Schema.MB_Name), DescriptionProperty(Schema.MB_Description)]
	public class BMBoard : AutoBMBoard,
		IVisualBoardProvider,
		IDocManagerSupport,
		IBMBoard,
		ITemplateCopyable,
		ICustomisedLayoutSupportable,
		IAuditParent
	{
		public BMBoard(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region BusinessObject overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			var currentUser = GlbStaff.CurrentUser;
			if (currentUser != null)
			{
				MB_GS_NKStaffCode = currentUser.GS_Code;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return MB_Name; }
		}

		public override void Delete()
		{
			Sections.DeleteAll();
			CustomisedLayoutLinks.DeleteAll();
			foreach (var pivot in SlideshowPivots.ToList())
			{
				var slideshow = pivot.Slideshow;
				if (slideshow != null && !slideshow.IsDeleted && slideshow.BoardPivots.Count == 1 && slideshow.BoardPivots.Single().PK == pivot.PK)
				{
					slideshow.Delete();
				}
			}
			SlideshowPivots.DeleteAll();

			base.Delete();
		}

		public override void OnSaving()
		{
			var currentCompany = GlbCompany.CurrentCompany;
			if (currentCompany != null)
			{
				if (IsInDatabase && !MB_GC_CompanyInfo.OriginalValue.IsEmpty && currentCompany.PK != (ZGuid)MB_GC_CompanyInfo.OriginalValue)
				{
					ErrorReporter.ReportOnce("BMBoard.OnSaving", $"User {GlbStaff.CurrentUser?.PK} should not update non-global Board {PK} belonging to another Company {(ZGuid)MB_GC_CompanyInfo.OriginalValue}. (Current Company: {currentCompany.PK})");
				}
				else if (!MB_GC_Company.IsEmpty && currentCompany.PK != MB_GC_Company)
				{
					ErrorReporter.ReportOnce("BMBoard.OnSaving", $"Board {PK} should not belong to Company {MB_GC_Company}. (Current Company: {currentCompany.PK}, Current User: {GlbStaff.CurrentUser?.PK})");
				}
			}
			base.OnSaving();
		}

		#endregion

		#region Properties

		#region MB_FS_System

		[RelatedBusinessObject("System")]
		[List("Lookups.Systems")]
		public override ZGuid MB_FS_System
		{
			get { return base.MB_FS_System; }
			set
			{
				base.MB_FS_System = value;

				if (Sections.Count > 0 && !IsValidationSuspended)
				{
					foreach (var section in Sections)
					{
						section.Validation.ValidateMS_FC_Component();
					}
				}
			}
		}

		public virtual BMSystem System
		{
			get { return Factory.Load<BMSystem>(MB_FS_System); }
		}

		#endregion

		#region MB_IsPublished

		public override ZBool MB_IsPublished
		{
			get { return base.MB_IsPublished; }
			set
			{
				base.MB_IsPublished = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateMB_GG_ReleaseGroup();
					Validation.ValidateMB_GS_NKStaffCode();
				}
			}
		}

		#endregion

		#region IsGlobal

		public ZBool IsGlobal
		{
			get { return MB_GC_Company.IsEmpty; }
			set
			{
				if (value)
				{
					MB_GC_Company = ZGuid.Empty;
				}
				else
				{
					var currentCompany = GlbCompany.CurrentCompany;
					if (currentCompany != null)
					{
						MB_GC_Company = currentCompany.PK;
					}
				}
			}
		}

		#endregion

		#region MB_GG_ReleaseGroup

		[List("Lookups.SystemReleaseGroups")]
		public override ZGuid MB_GG_ReleaseGroup
		{
			get { return base.MB_GG_ReleaseGroup; }
			set
			{
				base.MB_GG_ReleaseGroup = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateMB_IsPublished();
					Validation.ValidateMB_GS_NKStaffCode();
				}
			}
		}

		#endregion

		#region MB_GS_NKStaffCode

		public override ZString MB_GS_NKStaffCode
		{
			get { return base.MB_GS_NKStaffCode; }
			set
			{
				base.MB_GS_NKStaffCode = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateMB_IsPublished();
					Validation.ValidateMB_GG_ReleaseGroup();
				}
			}
		}

		#endregion

		#endregion

		#region Related business objects

		[ChildEditable]
		public BMBoardSectionCollection Sections
		{
			get
			{
				if (sections == null)
				{
					sections = new BMBoardSectionCollection(this);
					RegisterEditableChildObject(sections);
				}
				return sections;
			}
		}

		BMBoardSectionCollection sections;

		[ChildEditable]
		public BMControlCustomisationLinkCollection CustomisedLayoutLinks
		{
			get
			{
				if (customisedLayoutLinks == null)
				{
					customisedLayoutLinks = new BMControlCustomisationLinkCollection(this);
					RegisterEditableChildObject(customisedLayoutLinks);
					BMControlCustomisationLink.AddBMControlCustomisationFetchHints(customisedLayoutLinks, Factory);
					((ICustomisedLayoutSupportable)this).AreCustomisedLayoutFetchHintsAdded = true;
				}

				return customisedLayoutLinks;
			}
		}

		BMControlCustomisationLinkCollection customisedLayoutLinks;

		[ChildEditable]
		public BMBoardSlideshowPivotCollection SlideshowPivots
		{
			get
			{
				if (slideshowPivots == null)
				{
					slideshowPivots = new BMBoardSlideshowPivotCollection(this);
					RegisterEditableChildObject(slideshowPivots);
				}

				return slideshowPivots;
			}
		}

		BMBoardSlideshowPivotCollection slideshowPivots;

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clone = (BMBoard)base.CloneInternal(args);

			this.CloneCustomisedLayoutLayoutLinks(clone);

			foreach (var section in this.Sections.ToArray())
			{
				clone.Sections.Add((BMBoardSection)section.Clone());
			}

			return clone;
		}

		#endregion

		#region IVisualBoardProvider Members

		string IVisualBoardProvider.Name
		{
			get { return MB_Name; }
		}

		string IVisualBoardProvider.Description
		{
			get { return MB_Description; }
		}

		IVisualBoardProvider IVisualBoardProvider.ReloadInFactory(BusinessObjectFactory factory)
		{
			return factory.Load<BMBoard>(PK);
		}

		IEnumerable<ISlideShowFrame> IVisualBoardProvider.Boards
		{
			get
			{
				yield return new SlideShowFrame(PK.ToGuid(), MB_Name, 0, Sections?.ToArray());
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

		string IVisualBoardProvider.OwnerStaffCode => MB_GS_NKStaffCode;

		ZGuid IVisualBoardProvider.OwnerGroupPK => MB_GG_ReleaseGroup;

		SecurityCheckpoint IVisualBoardProvider.EditCheckpoint => Env.Security.BMBoardEdit;

		ControllerID IVisualBoardProvider.ControllerID => ControllerIDs.BMBoard;

		string IVisualBoardProvider.HumanReadableShortcutName => HumanReadableShortcutName;

		ZBool IVisualBoardProvider.IsVisibleToCurrentCompany
		{
			get
			{
				if (MB_GC_Company.IsEmpty)
				{
					return true;
				}

				var currentCompany = GlbCompany.CurrentCompany;
				return currentCompany != null && MB_GC_Company == currentCompany.PK;
			}
		}

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.BMBoard)); }
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IBMBoard Members

		IBMSystem IBMBoard.System
		{
			get { return System; }
		}

		IEnumerable<IBMBoardSection> IBMBoard.Sections
		{
			get { return Sections; }
		}

		ZDateTime IBMBoard.CustomisationLastEditTimeUTC
		{
			get { return this.GetLatestCustomisationEditTimeUTC(); }
		}

		#endregion

		#region ITemplateCopyable Members

		public IBusiness TemplateCopy()
		{
			return this.Clone();
		}

		#endregion

		#region ICustomisedLayoutSupportable Members

		IBMControlCustomisationLinkCollection ICustomisedLayoutSupportable.CustomisedLayoutLinks
		{
			get { return CustomisedLayoutLinks; }
		}

		bool ICustomisedLayoutSupportable.AreCustomisedLayoutFetchHintsAdded { get; set; }

		#endregion

		#region IAuditParent Mambers

		public IEnumerable<AuditChildInfo> RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(BMBoardSectionSchema.MS_MB_Board, null);
			}
		}

		#endregion
	}
}
