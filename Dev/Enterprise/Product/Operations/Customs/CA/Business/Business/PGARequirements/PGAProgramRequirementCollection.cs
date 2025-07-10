using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class PGAProgramRequirementCollection : NonPersistentBusinessObjectCollection<PGAProgramRequirement>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public PGAProgramRequirementCollection(PGARequirement pgaRequirement, Func<IPGAProgramRequirementProvider> getProgramRequirementProvider)
			: base(pgaRequirement.Factory)
		{
			this.pgaRequirement = pgaRequirement;
			this.getProgramRequirementProvider = getProgramRequirementProvider;
		}
		readonly PGARequirement pgaRequirement;
		readonly Func<IPGAProgramRequirementProvider> getProgramRequirementProvider;

		public PGARequirement PGARequirement
		{
			get { return pgaRequirement; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new InvalidOperationException();
		}

		internal void Populate()
		{
			RemoveAndDeleteAll();

			var programRequirementProvider = getProgramRequirementProvider();
			if (programRequirementProvider != null)
			{
				var programCodesList = programRequirementProvider.GetProgramCodesList();
				foreach (CodeDescriptionPair program in programCodesList)
				{
					var programCode = program.Code;
					Add(new PGAProgramRequirement(pgaRequirement, programCode, program.Description, getProgramRequirementProvider));
				}
			}
		}

		public bool IsDeclared(string programCode)
		{
			return this.Cast<PGAProgramRequirement>().Any(x => x.ProgramCode == programCode && x.Indicator == YesNoList.Codes.Yes);
		}
	}
}
