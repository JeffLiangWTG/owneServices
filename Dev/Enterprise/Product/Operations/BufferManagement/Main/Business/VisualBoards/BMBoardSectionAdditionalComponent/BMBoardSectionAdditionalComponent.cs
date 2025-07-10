using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;

namespace Enterprise.BufferManagement.Business
{
	public class BMBoardSectionAdditionalComponent : AutoBMBoardSectionAdditionalComponent, IAuditParent
	{
		public BMBoardSectionAdditionalComponent(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region BSA_FC_Component

		[List("Lookups.Components")]
		[RelatedBusinessObject("Component")]
		public override ZGuid BSA_FC_Component
		{
			get { return base.BSA_FC_Component; }
			set { base.BSA_FC_Component = value; }
		}

		#endregion

		#region BSA_MS_Section

		[RelatedBusinessObject("Section")]
		public override ZGuid BSA_MS_Section
		{
			get { return base.BSA_MS_Section; }
			set { base.BSA_MS_Section = value; }
		}

		#endregion

		#endregion

		#region Related Business Objects

		public BMComponent Component
		{
			get { return Factory.Load<BMComponent>(BSA_FC_Component); }
		}

		public BMBoardSection Section
		{
			get { return Factory.Load<BMBoardSection>(BSA_MS_Section); }
		}

		#endregion

		#region IAuditParent Mambers

		public IEnumerable<AuditChildInfo> RelatedAuditChildren => Enumerable.Empty<AuditChildInfo>();

		#endregion
	}
}
