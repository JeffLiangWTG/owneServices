using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.CA.Business
{
	class IM2AdjustmentsDocumentWrapper : AdjustmentsDocumentWrapper
	{
		public IM2AdjustmentsDocumentWrapper(JobDeclaration declaration)
			: base(declaration)
		{
			this.declaration = declaration;
		}
		readonly JobDeclaration declaration;

		public JobDeclaration PreviousJob
		{
			get { return declaration.PreviousJob; }
		}

		protected override void CheckDeclarationType(JobDeclaration declaration)
		{
			if (!declaration.IsIM2)
			{
				throw new InvalidOperationException("Declaration must be a Copy IM2 Adjustments.");
			}
		}

		protected override AdjustmentsDocHeader CreateNewHeader(JobDeclaration declaration = null)
		{
			if (declaration != null)
			{
				return new IM2AdjustmentsDocHeader(declaration);
			}
			else
			{
				return new IM2AdjustmentsDocHeader();
			}
		}

		protected override AdjustmentsDocFooter CreateNewFooter(JobDeclaration declaration = null, IEnumerable<AdjustmentsDocPage> pages = null)
		{
			if (declaration != null)
			{
				return new IM2AdjustmentsDocFooter(declaration, pages);
			}
			else
			{
				return new IM2AdjustmentsDocFooter();
			}
		}

		protected override AdjustmentsDocPage CreateNewPage()
		{
			return new IM2AdjustmentsDocPage();
		}

		protected override IEnumerable<AdjustmentsDocPage> GetDocumentPages()
		{
			var result = new List<AdjustmentsDocPage>();
			var emptyHeader = CreateNewHeader();
			var emptyFooter = CreateNewFooter();
			var tempPage = CreateNewPage();
			var im2Pages = tempPage.GetPages(null, declaration);
			foreach (var page in im2Pages)
			{
				page.Header = emptyHeader;
				page.Footer = emptyFooter;
				result.Add(page);
			}

			if (result.Any())
			{
				result[0].Header = CreateNewHeader(declaration);
				result[result.Count - 1].Footer = CreateNewFooter(declaration, im2Pages);
			}
			return result;
		}
	}
}
