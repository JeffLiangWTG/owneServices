using System.Collections;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class PreviousDocumentLookups : Customs.Business.CusSupportingInfoLookups
	{
		public PreviousDocumentLookups(PreviousDocument parent)
			: base(parent)
		{
		}

		public new PreviousDocument Parent => (PreviousDocument)base.Parent;

		public override ICollection CodeList
		{
			get
			{
				var declaration = Parent.Parent?.Declaration;
				if (declaration?.IsImport ?? false)
				{
					if (declaration.IsImportSiscomex)
					{
						return Factory.GetCachedValue<ImportSiscomexPreviousDocumentList>();
					}
					else
					{
						return Factory.GetCachedValue<ImportPreviousDocumentList>();
					}
				}
				else
				{
					return Factory.GetCachedValue<ExportPreviousDocumentList>();
				}
			}
		}
	}
}
