using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	/// <summary>
	/// Top Level entity for Project, Customer Service Ticket, and Work Item.
	/// </summary>
	[XsdSchema("UniversalActivity.xsd"), RootElement("UniversalActivity")]
	public partial class Activity : TopLevelDataObject,
		ICustomizedFieldContainer,
		IOrganizationAddressCollectionParent,
		ITaskSetCollectionParent,
		IMilestoneCollectionParent,
		IJobCostingData
	{
		public Activity()
		{
		}

		public Activity(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		[NamespaceDependent(UniversalXmlInfo.Namespace_2011_11, typeof(_2011_11.DataContext))]
		[NamespaceDependent(UniversalXmlInfo.Namespace_2012_11, typeof(_2012_11.DataContext))]
		[ReferenceProperty]
		public override IDataContextDataObject DataContext { get; set; }

		[MaxLength(100), Mandatory]
		public ZString? Summary { get; set; }
		[MaxLength(UniversalXmlInfo.MaxStringLength), AllowLineControlWhiteSpace]
		public ZString? Description { get; set; }

		public CodeDescriptionPair Status { get; set; }
		public CodeDescriptionPair SelectionCriterion1 { get; set; }
		public CodeDescriptionPair SelectionCriterion2 { get; set; }
		public CodeDescriptionPair SelectionCriterion3 { get; set; }
		public CodeDescriptionPair SelectionCriterion4 { get; set; }
		public CodeDescriptionPair SelectionCriterion5 { get; set; }
		public CodeDescriptionPair5Char Location { get; set; }

		public Branch Branch { get; set; }
		public Department Department { get; set; }
		public Company Company { get; set; }
		public Staff ProjectManager { get; set; }
		public Staff CreatedBy { get; set; }
		public Conversation Conversation { get; set; }

		public List<OrganizationAddress> OrganizationAddressCollection { get; private set; }
		public List<CustomizedField> CustomizedFieldCollection { get; private set; }
		public List<Activity> RelatedActivityCollection { get; private set; }
		public DataObjectList<Note> NoteCollection { get; private set; }
		public List<TaskSet> TaskSetCollection { get; private set; }
		public List<Milestone> MilestoneCollection { get; private set; }

		public JobCosting JobCosting { get; set; }
	}
}
