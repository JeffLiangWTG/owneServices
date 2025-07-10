using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.Business
{
	public interface ILoopDetectable
	{
		ZString LinkType { get; }
		ProcessHeader HeaderFrom { get; }
		ProcessHeader HeaderTo { get; }
	}

	public class TemporaryTemplateLink : ILoopDetectable, ILink
	{
		#region Interface properties

		public ZString LinkType { get; set; }
		public ProcessHeader HeaderFrom { get; set; }
		public ProcessHeader HeaderTo { get; set; }

		public Guid PK => Guid.Empty;

		public ILinkEntity Owner => null;

		public ILinkEntity From => HeaderFrom;

		public ILinkEntity To => HeaderTo;

		public bool IsBufferedAttachment => false;

		#endregion

		#region Helper methods

		public static IEnumerable<ProcessHeader> GetChildHeadersFromTemporaryLinks(ProcessHeader header, IEnumerable<TemporaryTemplateLink> tempLinks)
		{
			foreach (var childLink in tempLinks.Where(l => l.LinkType == ProcessHeaderLinkTypeList.Codes.ParentChild && l.HeaderTo != null && l.HeaderTo.PK == header.PK))
			{
				yield return childLink.HeaderFrom;
			}
		}

		public static IEnumerable<ProcessHeader> GetParentHeadersFromTemporaryLinks(ProcessHeader header, IEnumerable<TemporaryTemplateLink> tempLinks)
		{
			var jobHeader = header.JobHeader;
			var tempLink = GetParentLinks(tempLinks, header).FirstOrDefault(l => l.HeaderTo != null && IsChildWorkflowWithinJobOf(tempLinks, l.HeaderTo, header));

			if (tempLink != null)
			{
				yield return tempLink.HeaderTo;
			}
			else if (jobHeader != null)
			{
				yield return jobHeader;
			}
		}

		static IEnumerable<TemporaryTemplateLink> GetParentLinks(IEnumerable<TemporaryTemplateLink> tempLinks, ProcessHeader header)
		{
			foreach (var parent in tempLinks.Where(l => l.LinkType == ProcessHeaderLinkTypeList.Codes.ParentChild && l.HeaderFrom != null && l.HeaderFrom.PK == header.PK))
			{
				yield return parent;
			}
		}

		static bool IsChildWorkflowWithinJobOf(IEnumerable<TemporaryTemplateLink> tempLinks, ProcessHeader parentHeader, ProcessHeader header)
		{
			if (header.IsInSameJob(parentHeader) && parentHeader.PK != header.PK)
			{
				return GetParentLinks(tempLinks, header).Any(l => l.HeaderTo != null && l.HeaderTo.PK == parentHeader.PK && l.HeaderTo.IsWorkflow);
			}
			else
			{
				return false;
			}
		}

		public static IEnumerable<ILoopDetectable> GetPrerequisiteLinksFromTemporaryLinks(ProcessHeader header, IEnumerable<TemporaryTemplateLink> tempLinks)
		{
			return tempLinks.Where(l => l.LinkType == ProcessHeaderLinkTypeList.Codes.Dependency && l.HeaderTo != null && l.HeaderTo.PK == header.PK);
		}

		#endregion
	}
}
