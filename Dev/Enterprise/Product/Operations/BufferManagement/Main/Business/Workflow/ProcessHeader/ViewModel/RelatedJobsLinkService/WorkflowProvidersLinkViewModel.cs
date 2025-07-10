using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.BufferManagement.Business
{
	public sealed class WorkflowProvidersLinkViewModel : NonPersistentBusinessObject, IObsoleteValidation
	{
		public WorkflowProvidersLinkViewModel(ProcessJobHeader firstJobHeader, ProcessJobHeader secondJobHeader)
			: base(firstJobHeader.Factory)
		{
			this.firstJobHeader = firstJobHeader;
			this.secondJobHeader = secondJobHeader;
		}

		public WorkflowProvidersLinkViewModel(ProcessJobHeader firstJobHeader, ProcessJobHeader secondJobHeader, IEnumerable<IProcessTaskTemplate> headerLinkSourceTemplates)
			: this(firstJobHeader, secondJobHeader)
		{
			this.headerLinkSourceTemplates = headerLinkSourceTemplates;
		}

		readonly ProcessJobHeader firstJobHeader;
		readonly ProcessJobHeader secondJobHeader;
		readonly IEnumerable<IProcessTaskTemplate> headerLinkSourceTemplates;

		#region Related Business Objects

		[ChildEditable]
		public ProposedProcessHeaderLinkCollection ProposedProcessHeaderLinks
		{
			get
			{
				if (proposedProcessHeaderLinks == null)
				{
					proposedProcessHeaderLinks = new ProposedProcessHeaderLinkCollection(firstJobHeader, secondJobHeader, headerLinkSourceTemplates);
					RegisterEditableChildObject(proposedProcessHeaderLinks);
				}

				return proposedProcessHeaderLinks;
			}
		}

		ProposedProcessHeaderLinkCollection proposedProcessHeaderLinks;

		#endregion

		#region NonPersistentBusinessObject Overrides

		protected override void AddToFactoryCache()
		{
		}

		#endregion

		#region ViewModel Behaviour

		public void CreateRealLinksFromProposed()
		{
			if (HasErrors)
			{
				throw new InvalidOperationException("Cannot create links when in an invalid state");
			}

			foreach (ProposedProcessHeaderLink proposedLink in ProposedProcessHeaderLinks)
			{
				proposedLink.EnsureRealLinkExists();
			}
		}

		#endregion
	}
}
