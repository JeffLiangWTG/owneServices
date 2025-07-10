using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;

namespace Enterprise.BufferManagement.Business
{
	public class BMSystemWorkflowDeterminer : AutoBMSystemWorkflowDeterminer,
		IAuditParent
	{
		public BMSystemWorkflowDeterminer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region BusinessObject Overrides

		protected override bool SupportsCloneCore() => true;

		protected override bool ShouldUpdateNaturalKeyCacheWhenFSW_WorkflowTypeChanges(ZString newValue) => true;

		#endregion

		#region Properties

		[RelatedBusinessObject("System")]
		public override ZGuid FSW_FS_System
		{
			get { return base.FSW_FS_System; }
			set { base.FSW_FS_System = value; }
		}

		[List("Lookups.WorkflowTypes")]
		public override ZString FSW_WorkflowType
		{
			get { return base.FSW_WorkflowType; }
			set
			{
				base.FSW_WorkflowType = value;

				if (System != null)
				{
					foreach (var relatedWorkflowType in System.RelatedWorkflowTypes)
					{
						relatedWorkflowType.Validation.ValidateFSW_WorkflowType();
					}
				}
			}
		}

		#endregion

		#region New Properties

		[ResourceStringData("BMSystemWorkflowDeterminer|WorkflowTypeDescription", Caption = "Workflow Type Desc.", ShortCaption = "Description", FullDescription = "The workflow type that is managed by this Buffer Management System.")]
		public ZString WorkflowTypeDescription
		{
			get { return Lookups.WorkflowTypes.GetDescriptionFromCode(FSW_WorkflowType); }
		}

		#endregion

		#region Related Business Objects

		public BMSystem System => Factory.Load<BMSystem>(FSW_FS_System);

		#endregion

		#region IAuditParent Members

		public IEnumerable<AuditChildInfo> RelatedAuditChildren => Enumerable.Empty<AuditChildInfo>();

		#endregion
	}
}
