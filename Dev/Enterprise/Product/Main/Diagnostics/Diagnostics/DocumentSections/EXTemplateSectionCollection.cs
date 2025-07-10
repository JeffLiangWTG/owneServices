using System;
using CargoWise.EntityFramework;

namespace Enterprise.Diagnostics
{
	public class EXTemplateSectionCollection : NonPersistentBusinessObjectCollection<EXTemplateSection>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		protected override BusinessObject AddNewCore(Type bizoType)
		{
			throw new NotSupportedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
