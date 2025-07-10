using System;
using System.Collections.Generic;

namespace Enterprise.Dash.Integration
{
	public class DashEDocsDetails
	{
		public Guid DocId { get; set; }

		public Guid? DocMainId { get; set; }

		public Guid RelatedEntityId { get; set; }

		public string RelatedEntityTypeCode { get; set; }

		public Guid RelatedBranchId { get; set; }

		public Guid RelatedDepartmentId { get; set; }

		public List<string> DocumentChanges { get; set; }
	}
}
