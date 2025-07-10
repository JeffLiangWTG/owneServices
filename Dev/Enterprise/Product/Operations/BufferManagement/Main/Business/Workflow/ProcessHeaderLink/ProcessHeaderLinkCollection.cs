using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessHeaderLinkCollection : ActiveBusinessObjectCollection<ProcessHeaderLink>, IProcessHeaderLinkCollection
	{
		public ProcessHeaderLinkCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ProcessHeaderLinkCollection(ProcessHeader parent, SchemaGuidColumn directionColumn)
			: base(parent.Factory, parent, new ZQuery(), directionColumn)
		{
			this.processJobHeader = parent.JobHeader;
		}

		public ProcessHeaderLinkCollection(ProcessHeader parent, SchemaGuidColumn directionColumn, ZString linkType)
			: base(parent.Factory, parent, new ZQuery(ProcessHeaderLinkSchema.FP_LinkType, linkType), directionColumn)
		{
			this.processJobHeader = parent.JobHeader;
			this.linkType = linkType;
		}

		public ProcessHeaderLinkCollection(ProcessTaskTemplate taskTemplate)
			: base(taskTemplate.Factory, new AdhocCollectionRelationship(typeof(ProcessHeaderLink)))
		{
			this.template = taskTemplate;
			PopulateTheAdhocCollection(taskTemplate);
		}

		void PopulateTheAdhocCollection(ProcessTaskTemplate taskTemplate)
		{
			var headers = taskTemplate.ProcessHeaders.Cast<ProcessHeader>();
			var extraPks = new[] { ZGuid.Invalid, ZGuid.Empty };

			AddRange(ProcessHeaderLink.LoadLinksIntoFactoryOptimisingForUnsavedProcessHeaders(Factory, headers, extraPks));
		}

		readonly ProcessHeader workflow;
		readonly ProcessTaskTemplate template;
		readonly ProcessJobHeader processJobHeader;
		readonly ZString linkType;

		#region ActiveBusinessObjectCollection Overrides

		protected override void OnLoadedIntoCollectionCore(ProcessHeaderLink loadedObject)
		{
			base.OnLoadedIntoCollectionCore(loadedObject);

			loadedObject.ProcessJobHeader = processJobHeader;

			MaybeSetTemplate(loadedObject, processJobHeader, template);
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new ProcessHeaderLinkCollectionFetchStrategy(this);
		}

		class ProcessHeaderLinkCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
		{
			public ProcessHeaderLinkCollectionFetchStrategy(ProcessHeaderLinkCollection collection)
				: base(collection)
			{
			}

			protected new ProcessHeaderLinkCollection Collection => (ProcessHeaderLinkCollection)base.Collection;

			protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
			{
				base.FetchForViewCore(businessObjects, columns);

				var processHeaderLinks = businessObjects.OfType<ProcessHeaderLink>();
				var factory = Collection.Factory;

				if (columns.Any(x => x.ColumnName == BMConstants.ProcessHeaderLinkFilterControlColumnNames.FromJob || x.ColumnName == BMConstants.ProcessHeaderLinkFilterControlColumnNames.FromJobDescription))
				{
					processHeaderLinks.ForEach(link => factory.AddFetchHint(ProcessHeaderSchema.PK, link.FP_FH_HeaderFrom));
					processHeaderLinks.ForEach(link => link.HeaderFrom.AddDeepFetchHintForParentType());
				}

				if (columns.Any(x => x.ColumnName == BMConstants.ProcessHeaderLinkFilterControlColumnNames.ToJob || x.ColumnName == BMConstants.ProcessHeaderLinkFilterControlColumnNames.ToJobDescription))
				{
					processHeaderLinks.ForEach(link => factory.AddFetchHint(ProcessHeaderSchema.PK, link.FP_FH_HeaderTo));
					processHeaderLinks.ForEach(link => link.HeaderTo.AddDeepFetchHintForParentType());
				}
			}
		}

		protected override void SetDefaultsForNewElementCore(ProcessHeaderLink newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			SetDefaultsForNewElement(newElement, workflow, processJobHeader, linkType, template);
		}

		public static void SetDefaultsForNewElement(ProcessHeaderLink newElement, ProcessHeader parent, ProcessJobHeader jobHeader, ZString headerLinkType, ProcessTaskTemplate taskTemplate)
		{
			newElement.ProcessJobHeader = jobHeader;

			if (parent != null)
			{
				newElement.FP_FH_HeaderFrom = parent.PK;
			}

			MaybeSetTemplate(newElement, jobHeader, taskTemplate);

			if (!headerLinkType.IsEmpty)
			{
				newElement.FP_LinkType = headerLinkType;
			}
		}

		protected override bool AllowNew
		{
			get
			{
				var processHeader = workflow ?? processJobHeader;
				return processHeader == null || !processHeader.IsDeleted && processHeader.FH_P0_Template.IsValid;
			}
		}

		static void MaybeSetTemplate(ProcessHeaderLink link, ProcessJobHeader processJobHeader, ProcessTaskTemplate taskTemplate)
		{
			if (link.Template == null)
			{
				if (taskTemplate == null && processJobHeader != null)
				{
					link.Template = processJobHeader.Template;
				}
				else
				{
					link.Template = taskTemplate;
				}
			}
		}

		#endregion

		#region IProcessHeaderLinkCollection members

		IProcessHeaderLink IProcessHeaderLinkCollection.this[int index]
		{
			get { return base[index]; }
		}

		#endregion

		#region For Test
#if DEBUG

		public static ProcessHeaderLinkCollection Create_ForTest(ProcessHeader parent)
		{
			return new ProcessHeaderLinkCollection(parent);
		}

		ProcessHeaderLinkCollection(ProcessHeader parent)
			: base(parent.Factory, GetQueryForAllLinks_ForTest(parent))
		{
			workflow = parent;
			processJobHeader = parent.JobHeader;
		}

		static ZQuery GetQueryForAllLinks_ForTest(ProcessHeader parent)
		{
			return GetQueryFromProcessHeaderIDs_ForTest(parent.PK);
		}

		static ZQuery GetQueryFromProcessHeaderIDs_ForTest(params ZGuid[] headerPKs)
		{
			return new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderFrom, headerPKs)
				.AddToFilter(JoinCondition.Or, ProcessHeaderLinkSchema.FP_FH_HeaderTo, headerPKs);
		}

#endif
		#endregion
	}
}
