using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.Business
{
	public class DeclarationRelatedImportLicenseEntryCollection : CustomsGenPivotCollection<DeclarationRelatedImportLicenseEntryGenPivot, JobDeclaration, CusEntryInstruction>
	{
		public DeclarationRelatedImportLicenseEntryCollection(JobDeclaration master)
			: base(master)
		{
		}

		protected override string RelationType
		{
			get { return GenPivotTypeDecider.Types.JobDecRelatedImportLicenseEntryGenPivot; }
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		public IEnumerable<CusEntryInstruction> EntryInstructions => this.Cast<DeclarationRelatedImportLicenseEntryGenPivot>().Select(x => x.EntryInstruction);
	}
}
