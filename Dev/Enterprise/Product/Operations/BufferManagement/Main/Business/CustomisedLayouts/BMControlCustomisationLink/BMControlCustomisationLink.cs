using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[DependentBusinessObject(typeof(BMControlCustomisation), "ControlUsages")]
	public class BMControlCustomisationLink : AutoBMControlCustomisationLink, IBMControlCustomisationLink, IAuditParent
	{
		public BMControlCustomisationLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region BusinessObject Overrides

		protected override bool SupportsCloneCore()
		{
			return true;
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			FML_ControlType = CustomisedControlTypeList.Codes.DetailedCard;
		}

#endif

		#endregion

		#region Properties

		[List("Lookups.CustomisedLayouts")]
		[RelatedBusinessObject("CustomisedLayout")]
		public override ZGuid FML_FM_ControlCustomisation
		{
			get { return base.FML_FM_ControlCustomisation; }
			set
			{
				base.FML_FM_ControlCustomisation = value;

				var customisation = CustomisedLayout;
				FML_ControlType = customisation != null ? customisation.FM_ControlType : ZString.Empty; // TODO: FML_ControlType can probably be removed...
			}
		}

		[List("Lookups.JobTypes")]
		public override ZString FML_JobType
		{
			get { return base.FML_JobType; }
			set { base.FML_JobType = value; }
		}

		#endregion

		#region New Properties

		[ResourceStringData("BMControlCustomisationLink.UsageDescription", Caption = "Used By", FullDescription = "The place this customized layout is used.")]
		public ZString UsageDescription
		{
			get
			{
				var parent = Factory.Load(FML_ParentTableCode, FML_ParentId);

				var system = parent as BMSystem;
				if (system != null)
				{
					return Res.GetString("58d94e37-bbe1-40d1-8eac-17dc934ee9a5", "System '{0}'", system.FS_Name);
				}

				var group = parent as GlbGroup;
				if (group != null)
				{
					return Res.GetString("9608cf8c-30d2-411a-98d8-5af0cdaecbd2", "Release Group '{0}'", group.GG_Desc);
				}

				var board = parent as BMBoard;
				if (board != null)
				{
					return Res.GetString("0f28f11b-eacd-4b9e-a233-c536aeea4810", "Visual Board '{0}'", board.MB_Name);
				}

				var section = parent as BMBoardSection;
				if (section != null)
				{
					return Res.GetString("f0ded830-7120-48cf-b1c9-9a85e42fe4e2", "Section '{0}' on Visual Board '{1}'", section.Component.FC_Name, section.Board.MB_Name);
				}

				return ZString.Empty;
			}
		}

		#endregion

		#region Related Business Objects

		public BMControlCustomisation CustomisedLayout
		{
			get { return Factory.Load<BMControlCustomisation>(FML_FM_ControlCustomisation); }
		}

		public BMControlCustomisation CustomisedLayoutIncludingBlob
		{
			get
			{
				ZQuery query = new ZQuery(BMControlCustomisationSchema.PK, FML_FM_ControlCustomisation);
				query.IncludeBlob(BMControlCustomisationSchema.FM_LayoutData);
				return Factory.LoadTop1<BMControlCustomisation>(query);
			}
		}

		public static void AddBMControlCustomisationFetchHints(IEnumerable<BMControlCustomisationLink> links, BusinessObjectFactory factory)
		{
			foreach (var link in links)
			{
				var query = new ZQuery(BMControlCustomisationSchema.PK, link.FML_FM_ControlCustomisation);
				query.IncludeBlob(BMControlCustomisationSchema.FM_LayoutData);
				factory.AddFetchHint(BMControlCustomisationSchema.Instance, query);
			}
		}
		#endregion

		#region Load/create

		public static BMControlCustomisationLink GetForParent(BusinessObject parent, string controlType)
		{
			var query = new ZQuery(BMControlCustomisationLinkSchema.FML_ParentId, parent.PK);
			query.AddToFilter(BMControlCustomisationLinkSchema.FML_ControlType, controlType);

			var result = parent.Factory.LoadTop1<BMControlCustomisationLink>(query);
			if (result != null)
			{
				parent.RegisterEditableChildObject(result);
			}

			return result;
		}

		public static BMControlCustomisationLink CreateForParent(BusinessObject parent, BMControlCustomisation customisation)
		{
			if (customisation.FM_ControlType.IsEmpty)
			{
				throw new InvalidOperationException("Cannot create a pivot for a BMControlCustomisation with no FM_ControlType.");
			}

			var link = parent.Factory.New<BMControlCustomisationLink>();
			link.FML_ParentId = parent.PK;
			link.FML_ParentTableCode = parent.TablePrefix;
			link.FML_FM_ControlCustomisation = customisation.PK;
			link.FML_ControlType = customisation.FM_ControlType;

			parent.RegisterEditableChildObject(link);

			return link;
		}

		#endregion

		#region IAuditParent Mambers

		public IEnumerable<AuditChildInfo> RelatedAuditChildren => Enumerable.Empty<AuditChildInfo>();

		#endregion
	}
}
