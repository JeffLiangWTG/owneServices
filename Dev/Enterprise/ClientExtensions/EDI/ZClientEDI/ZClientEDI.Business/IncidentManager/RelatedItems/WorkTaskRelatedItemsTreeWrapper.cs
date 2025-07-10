using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class WorkTaskRelatedItemsTreeWrapper : NonPersistentBusinessObject
	{
		public IWorkTaskTreeNode RelatedItem { get; }
		public readonly WorkTaskRelatedItemsTreeModel TreeModel;
		public readonly IEnumerable<WorkTaskRelatedItemsTreeWrapper> Children;

		#region WorkTaskRelatedItemsTreeWrapper override

		public ZString Group => BizoCode + " " + RelatedItem.Number;

		public ZString StatusDescription => RelatedItem.StatusDescription;

		public ZString Type => RelatedItem.Type;

		public ZString Description => Regex.Replace(RelatedItem.ItemDescription, @"\t|\n|\r", "");

		public ZString OrganisationCode => RelatedItem.ClientCode;

		public ZString OrganisationName => RelatedItem.ClientName;

		public ZDateTime AgreedDeliveryDate => RelatedItem.AgreedDeliveryDate;

		public ZString CurrentTaskStatus => RelatedItem.CurrentTaskStatus;

		public ZString CurrentTaskDescription => RelatedItem.CurrentTaskDescription;

		public ZString CurrentTaskAssigned => RelatedItem.CurrentTaskAssigned;

		public ZString CurrentTaskCapabilityCodeDescription => RelatedItem.CurrentTaskCapabilityCodeDescription;

		public ZString SelectionCriterion1Code => RelatedItem.SelectionCriterion1Code;

		public ZString SelectionCriterion2Code => RelatedItem.SelectionCriterion2Code;

		public ZString SelectionCriterion3Code => RelatedItem.SelectionCriterion3Code;

		public ZString SelectionCriterion4Code => RelatedItem.SelectionCriterion4Code;

		public ZString SelectionCriterion5Code => RelatedItem.SelectionCriterion5Code;

		public ZBool IsClosedOrCancelled => RelatedItem.IsClosedOrCancelled;

		#endregion

		ZString BizoCode
		{
			get
			{
				if (string.IsNullOrEmpty(bizoCode))
				{
					bizoCode = GetBizoCode(RelatedItem as BusinessObject);
				}
				return bizoCode;
			}
		}
		ZString bizoCode;

		public bool IsChild { get; set; }

		public bool IsTopIncident { get; set; }

		public WorkTaskRelatedItemsTreeWrapper(WorkTaskRelatedItemsTreeModel treeModel, IWorkTaskTreeNode relatedItem, IEnumerable<WorkTaskRelatedItemsTreeWrapper> children, bool isChild, bool isTopIncident = false)
				: base(treeModel.Factory)
		{
			TreeModel = treeModel;
			Children = children;
			RelatedItem = relatedItem;
			IsChild = isChild;
			IsTopIncident = isTopIncident;
		}

		ZString GetBizoCode(BusinessObject bizo)
		{
			var result = "UNK";
			if (bizo != null)
			{
				switch (bizo.TableName)
				{
					case "SupportIncident":
					case "IncidentMain":
						result = "INC";
						break;
					case "WorkItem":
						result = "WKI";
						break;
					case "WorkProject":
						result = "WKP";
						break;
					case "HelpErrorLog":
						result = "ISS";
						break;
					case "OrgSalesCall":
						result = "CMM";
						break;
					case "OrgOpportunity":
						result = "OPP";
						break;
					case "IncidentManagementGroup":
						result = "ING";
						break;
				}
			}
			return result;
		}
	}
}
