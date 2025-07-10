using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.PAVE.Common.Interfaces;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessHeaderDescendantsStrategy : ILinkDescendantsStrategy<ProcessHeader>
	{
		public IEnumerable<ProcessHeader> GetChildren(ProcessHeader entity)
		{
			return entity.ChildHeaders;
		}

		public IEnumerable<ProcessHeader> GetParents(ProcessHeader entity)
		{
			return entity.LinkEntityParents.WhereNotNull();
		}

		public IEnumerable<ILinkEntity> GetChildren(ILinkEntity entity)
		{
			return GetChildren((ProcessHeader)entity);
		}

		public IEnumerable<ILinkEntity> GetParents(ILinkEntity entity)
		{
			return GetParents((ProcessHeader)entity);
		}

		public IEnumerable<ILink> FilterLinksByScope(IEnumerable<ILink> links)
		{
			return links;
		}
	}

	public class ProcessHeaderDescendantsStrategyForTemporaryTemplateLinks : ILinkDescendantsStrategy<ProcessHeader>
	{
		public ProcessHeaderDescendantsStrategyForTemporaryTemplateLinks(IEnumerable<TemporaryTemplateLink> tempLinks)
		{
			this.tempLinks = tempLinks;
		}
		readonly IEnumerable<TemporaryTemplateLink> tempLinks;

		public IEnumerable<ProcessHeader> GetChildren(ProcessHeader entity)
		{
			return TemporaryTemplateLink.GetChildHeadersFromTemporaryLinks(entity, tempLinks);
		}

		public IEnumerable<ProcessHeader> GetParents(ProcessHeader entity)
		{
			return TemporaryTemplateLink.GetParentHeadersFromTemporaryLinks(entity, tempLinks);
		}

		public IEnumerable<ILinkEntity> GetChildren(ILinkEntity entity)
		{
			return GetChildren((ProcessHeader)entity);
		}

		public IEnumerable<ILinkEntity> GetParents(ILinkEntity entity)
		{
			return GetParents((ProcessHeader)entity);
		}

		public IEnumerable<ILink> FilterLinksByScope(IEnumerable<ILink> links)
		{
			return links;
		}
	}

	public static class ProcessHeaderDescendantsStrategy_Extensions
	{
		public static bool IsApplicableDependencyToEntity(this ProcessHeaderLink link, ProcessHeader entity)
		{
			return link.IsApplicableDependencyToEntity(entity, new ProcessHeaderDescendantsStrategy());
		}

		public static IEnumerable<ProcessHeader> Postrequisites(this ProcessHeader header)
		{
			return header.Postrequisites(new ProcessHeaderDescendantsStrategy());
		}

		public static IEnumerable<ProcessHeader> Prerequisites(this ProcessHeader header)
		{
			return header.Prerequisites(new ProcessHeaderDescendantsStrategy());
		}

		public static IEnumerable<ProcessHeader> Descendants(this ProcessHeader header)
		{
			return header.Descendants(new ProcessHeaderDescendantsStrategy());
		}

		public static IEnumerable<ProcessHeader> Ancestors(this ProcessHeader header)
		{
			return header.Ancestors(new ProcessHeaderDescendantsStrategy());
		}

		public static bool IsPrerequisiteOf(this ProcessHeader header, ProcessHeader other)
		{
			return header.IsPrerequisiteOf(other, new ProcessHeaderDescendantsStrategy());
		}

		public static bool IsPostrequisiteOf(this ProcessHeader header, ProcessHeader other)
		{
			return header.IsPostrequisiteOf(other, new ProcessHeaderDescendantsStrategy());
		}
	}
}
