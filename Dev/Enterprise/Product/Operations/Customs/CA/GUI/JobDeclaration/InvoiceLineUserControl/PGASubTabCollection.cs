using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public class PGASubTabCollection : IDisposable
	{
		readonly string agencyCode;
		readonly TabCollection tabCollection;

		protected PGAProgramRequirementCollection pgaProgramRequirements;
		IPGAHeader pgaProgramRequirementsHeader;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public PGASubTabCollection(ZTabControl tabControl, ZGrid relatedGrid, string agencyCode, CodeDescriptionPairList programCodeList, Func<string, ZUserControl> createSubTabUserControl)
		{
			this.agencyCode = agencyCode;
			this.tabCollection = new TabCollection(tabControl, relatedGrid);

			foreach (var code in programCodeList.GetAllCodes())
			{
				tabCollection.Add(
					() => pgaProgramRequirements != null && pgaProgramRequirements.IsDeclared(code),
					programCodeList.GetDescriptionFromCode(code),
					() => createSubTabUserControl(code),
					() => pgaProgramRequirements?.PGARequirement?.PGAHeader,
					() => ""
				);
			}

			tabCollection.UpdateTabs();
		}

		public void Update(IPGAHeader pgaHeader)
		{
			PGARequirement pgaRequirement = pgaHeader?.Parent?.PGARequirements?.PGARequirement(agencyCode);
			PGAProgramRequirementCollection requirements = pgaRequirement?.ProgramCodeRequirements;

			if (pgaProgramRequirementsHeader == pgaHeader && requirements == pgaProgramRequirements)
			{
				return;
			}

			UnHookChangeEvents(pgaProgramRequirements);
			pgaProgramRequirementsHeader = pgaHeader;
			pgaProgramRequirements = requirements;
			HookChangeEvents(pgaProgramRequirements);
			tabCollection.UpdateTabs();
		}

		void HookChangeEvents(PGAProgramRequirementCollection programRequirementCollection)
		{
			if (programRequirementCollection == null)
			{
				return;
			}
			programRequirementCollection.CountChanged += ProgramRequirementCollectionOnCountChanged;
			foreach (PGAProgramRequirement pgaRequirement in programRequirementCollection)
			{
				pgaRequirement.IndicatorInfo.ValueChanged += OnIndicatorInfoValueChanged;
			}
		}

		void ProgramRequirementCollectionOnCountChanged(object sender, CollectionCountChangedEventArgs args)
		{
			var programRequirement = args.BizObject as PGAProgramRequirement;
			if (programRequirement != null)
			{
				if (args.ItemRemoved)
				{
					programRequirement.IndicatorInfo.ValueChanged -= OnIndicatorInfoValueChanged;
				}
				if (args.ItemAdded)
				{
					// The 'programRequirement.IndicatorInfo.ValueChanged' event should not have OnIndicatorInfoValueChanged handler.
					// But if it is there for some strange reason, lets remove it first to keep only one handler.
					programRequirement.IndicatorInfo.ValueChanged -= OnIndicatorInfoValueChanged;
					programRequirement.IndicatorInfo.ValueChanged += OnIndicatorInfoValueChanged;
				}
			}
		}

		void UnHookChangeEvents(PGAProgramRequirementCollection programRequirementCollection)
		{
			if (programRequirementCollection == null)
			{
				return;
			}
			programRequirementCollection.CountChanged -= ProgramRequirementCollectionOnCountChanged;
			foreach (PGAProgramRequirement pgaRequirement in programRequirementCollection)
			{
				pgaRequirement.IndicatorInfo.ValueChanged -= OnIndicatorInfoValueChanged;
			}
		}

		void OnIndicatorInfoValueChanged(object obj, EventArgs args)
		{
			tabCollection.UpdateTabs();
		}

		public void Dispose()
		{
			if (pgaProgramRequirements != null)
			{
				UnHookChangeEvents(pgaProgramRequirements);
			}
			pgaProgramRequirements = null;
		}
	}
}
