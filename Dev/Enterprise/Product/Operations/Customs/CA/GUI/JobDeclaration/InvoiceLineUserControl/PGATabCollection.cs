using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public class PGATabCollection : IDisposable
	{
		readonly TabCollection tabCollection;

		protected PGARequirementCollection pgaRequirements;
		bool visible = true;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public PGATabCollection(ZTabControl tabControl, ZGrid relatedGrid, bool isOnInvoiceLine)
		{
			tabCollection = new TabCollection(tabControl, relatedGrid);

			// If isOnInvoiceLine is true, then parent is JobComInvoiceLine.
			// If isOnInvoiceLine is false, then parent is CusClassPartPivot or CusClassification.

			AddPGAHeaderTab(PGACodes.Codes.CFIA, () => new CFIAUserControl(isOnInvoiceLine));
			AddPGAHeaderTab(PGACodes.Codes.HC, () => new HCUserControl(isOnInvoiceLine));
			AddPGAHeaderTab(PGACodes.Codes.PHAC, () => new PHACUserControl(isOnInvoiceLine));
			AddPGAHeaderTab(PGACodes.Codes.TC, () => new TCUserControl(isOnInvoiceLine));
			AddPGAHeaderTab(PGACodes.Codes.ECCC, () => new ECCCUserControl(isOnInvoiceLine));
			AddPGAHeaderTab(PGACodes.Codes.NRCan, () => new NRCanUserControl(isOnInvoiceLine));
			AddPGAHeaderTab(PGACodes.Codes.DFO, () => new DFOUserControl(isOnInvoiceLine));
			AddPGAHeaderTab(PGACodes.Codes.CNSC, () => new CNSCUserControl(isOnInvoiceLine));
			AddPGAHeaderTab(PGACodes.Codes.GAC, () => new GACUserControl());

			tabCollection.UpdateTabs();
		}

		void AddPGAHeaderTab(string agencyCode, Func<ZUserControl> createContent)
		{
			tabCollection.Add(
				() => visible && RequirementsHasPGAProgramCodesDeclared(agencyCode),
				agencyCode,
				createContent,
				() => pgaRequirements?.PGARequirement(agencyCode)?.PGAHeader,
				null
			);
		}

		ZBool RequirementsHasPGAProgramCodesDeclared(ZString programCode)
		{
			return pgaRequirements != null && pgaRequirements.HasPGAProgramCodesDeclared(programCode);
		}

		public bool Visible
		{
			get { return visible; }
			set
			{
				visible = value;
				tabCollection.UpdateTabs();
			}
		}

		public void Update(PGARequirementCollection requirements)
		{
			if (requirements == pgaRequirements && !(requirements?.PGATabCollectionNeedToBeRefreshed ?? false))
			{
				// Since nothing has changed, there is nothing to update.
				// Changes within the requirements collection will be tracked by event handlers.
				// Changes outside the requirements collection should not be used to define visibility (consider using PGATabCollection.Visible property instead).
				return;
			}
			UnHookChangeEvents(pgaRequirements);
			pgaRequirements = requirements;
			HookChangeEvents(pgaRequirements);
			tabCollection.UpdateTabs();
			MarkPGATabCollectionDoesNotNeedToBeRefreshed(pgaRequirements);
		}

		void HookChangeEvents(PGARequirementCollection pgaRequirementCollection)
		{
			if (pgaRequirementCollection == null)
			{
				return;
			}
			foreach (PGARequirement pgaRequirement in pgaRequirementCollection)
			{
				pgaRequirement.IndicatorInfo.ValueChanged += OnIndicatorInfoValueChanged;
				HookChangeEvents(pgaRequirement.ProgramCodeRequirements);
			}
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

		void UnHookChangeEvents(PGARequirementCollection pgaRequirementCollection)
		{
			if (pgaRequirementCollection == null)
			{
				return;
			}
			foreach (PGARequirement pgaRequirement in pgaRequirementCollection)
			{
				pgaRequirement.IndicatorInfo.ValueChanged -= OnIndicatorInfoValueChanged;
				UnHookChangeEvents(pgaRequirement.ProgramCodeRequirements);
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

		void MarkPGATabCollectionDoesNotNeedToBeRefreshed(PGARequirementCollection pgaRequirementCollection)
		{
			if (pgaRequirementCollection == null)
			{
				return;
			}
			pgaRequirementCollection.PGATabCollectionNeedToBeRefreshed = false;
		}

		public void Dispose()
		{
			if (pgaRequirements != null)
			{
				UnHookChangeEvents(pgaRequirements);
			}
			pgaRequirements = null;
		}
	}
}
