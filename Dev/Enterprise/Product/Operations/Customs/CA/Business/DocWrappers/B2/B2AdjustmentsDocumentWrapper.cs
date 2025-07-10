using System;
using System.Collections.Generic;

namespace Enterprise.Customs.CA.Business
{
	class B2AdjustmentsDocumentWrapper : AdjustmentsDocumentWrapper
	{
		public B2AdjustmentsDocumentWrapper(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override void CheckDeclarationType(JobDeclaration declaration)
		{
			if (!declaration.IsB2Adjustments)
			{
				throw new InvalidOperationException("Declaration must be a Manual B2 Adjustments");
			}
		}

		protected override AdjustmentsDocHeader CreateNewHeader(JobDeclaration declaration = null)
		{
			if (declaration != null)
			{
				return new B2AdjustmentsDocHeader(declaration);
			}
			else
			{
				return new B2AdjustmentsDocHeader();
			}
		}

		protected override AdjustmentsDocFooter CreateNewFooter(JobDeclaration declaration = null, IEnumerable<AdjustmentsDocPage> pages = null)
		{
			if (declaration != null)
			{
				return new B2AdjustmentsDocFooter(declaration);
			}
			else
			{
				return new B2AdjustmentsDocFooter();
			}
		}

		protected override AdjustmentsDocPage CreateNewPage()
		{
			return new B2AdjustmentsDocPage();
		}
	}
}
