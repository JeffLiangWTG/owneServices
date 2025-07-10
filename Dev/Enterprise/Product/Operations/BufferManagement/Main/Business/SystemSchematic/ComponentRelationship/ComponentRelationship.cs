using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[PreventDelete(false)]
	public class ComponentRelationship : BMComponent, IDocManagerSupport
	{
		public ComponentRelationship(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Fetch Hints

		public void AddFetchHints()
		{
			Factory.AddFetchHint(BMComponentSchema.Instance,
				new ZQuery(BMComponentSchema.PK, RelatedComponentLinks.Select(link => link.FL_FC_ComponentTo)));

			Factory.AddFetchHint(BMSystemSchema.Instance,
				new ZQuery(BMSystemSchema.PK, RelatedComponentLinks.Select(link => link.ComponentToSystemPK)));
		}

		public static void AddComponentFetchHints(IEnumerable<ComponentRelationship> relationships)
		{
			foreach (var relationship in relationships)
			{
				relationship.Factory.AddFetchHint(BMComponentLinkSchema.Instance,
						new ZQuery(BMComponentLinkSchema.FL_FC_ComponentFrom, relationship.PK));
			}

			foreach (var relationship in relationships)
			{
				relationship.Factory.AddFetchHint(BMComponentSchema.Instance,
					new ZQuery(BMComponentSchema.PK, relationship.RelatedComponentLinks.Select(link => link.FL_FC_ComponentTo)));
			}
		}

		#endregion

		#region Properties

		#region FC_Type

		[BusinessObjectTestExclude]
		public override ZString FC_Type
		{
			get { return base.FC_Type; }
			set
			{
				if (value != BMComponentTypeList.Codes.ComponentRelationship)
				{
					ErrorReporter.ReportOnce("Attempted to set a different type on a component relationship. This makes no sense. SAD!");
				}

				base.FC_Type = value;
			}
		}

		#endregion

		#region FC_FS_System

		[BusinessObjectTestExclude]
		public override ZGuid FC_FS_System
		{
			get { return base.FC_FS_System; }
			set
			{
				if (!value.IsEmpty)
				{
					ErrorReporter.ReportOnce("Attempted to set a system on a component relationship. This makes no sense. SAD!");
				}

				base.FC_FS_System = value;
			}
		}

		#endregion

		#region FC_IsActive

		[ResourceStringData("ComponentRelationship.IsActive", Caption = "Is Active", FullDescription = "Indicates whether this component relationship is available for use.")]
		public override ZBool FC_IsActive { get => base.FC_IsActive; set => base.FC_IsActive = value; }

		#endregion

		#region FC_Name

		[ResourceStringData("ComponentRelationship.Name", FullDescription = "Relationship Name")]
		public override ZString FC_Name { get => base.FC_Name; set => base.FC_Name = value; }

		#endregion

		#endregion

		#region New Properties

		[ChildEditable(true)]
		public ComponentRelationshipLinkDependentCollection RelatedComponentLinks
		{
			get
			{
				if (relatedComponentLinks == null)
				{
					relatedComponentLinks = new ComponentRelationshipLinkDependentCollection(this, BMComponentLinkSchema.FL_FC_ComponentFrom);
					RegisterEditableChildObject(relatedComponentLinks);
				}
				return relatedComponentLinks;
			}
		}

		protected ComponentRelationshipLinkDependentCollection relatedComponentLinks;

		#endregion

		#region BusinessObject Overrides

		public override void Delete()
		{
			RelatedComponentLinks.DeleteAll();

			base.Delete();
		}

		protected override ZString HumanReadableNameCore => Res.GetString("ea5aaae4-39a2-4e2d-bac8-1de2cec95de9", "Component Relationship");

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			FC_Type = BMComponentTypeList.Codes.ComponentRelationship;
		}

		protected override BMComponentValidation GetNewValidation()
		{
			return new ComponentRelationshipValidation(this);
		}

		#endregion

		#region Test Data

#if DEBUG

		protected override void AddSystemForTest()
		{
		}

#endif

		#endregion

		#region IDocManagerSupport

		DocManagerInfo IDocManagerSupport.DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.ComponentRelationship));
		DocManagerInfo docManagerInfo;

		#endregion

	}
}
