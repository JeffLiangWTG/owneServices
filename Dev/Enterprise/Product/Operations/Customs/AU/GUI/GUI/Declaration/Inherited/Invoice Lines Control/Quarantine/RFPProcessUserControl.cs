using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class RFPProcessUserControl : ZUserControl
	{
		public RFPProcessUserControl()
		{
			InitializeComponent();
			Errata54Enabled = UniversalReferenceHelper.Errata54Enabled();
		}

		bool Errata54Enabled { get; }

		public void SetProcessingAndTreatmentGridsVisibility(bool isNEXDOCSActive)
		{
			defaultColumnsForGrid = null;

			if (!Errata54Enabled || isNEXDOCSActive)
			{
				ProcessingGrid.RemoveFromAvailableColumns(
					QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentDuration,
					QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentDurationUQ,
					QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentTemperature,
					QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentTemperatureUQ,
					QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentConcentration,
					QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentConcentrationUQ);
				TreatmentActiveIngredientGrid.Visible = false;
				TreatmentActiveIngredientGroupBox.Visible = false;
			}
			else
			{
				ProcessingGrid.AddToAvailableColumns(
					QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentDuration,
					QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentDurationUQ,
					QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentTemperature,
					QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentTemperatureUQ,
					QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentConcentration,
					QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentConcentrationUQ);
				TreatmentActiveIngredientGrid.Visible = true;
				TreatmentActiveIngredientGroupBox.Visible = true;
			}

			if (!isNEXDOCSActive)
			{
				ProcessingGrid.RemoveFromAvailableColumns(QuarantineExDocEstablishmentAndTime.Schema.EE_EstablishmentIndicator);
			}
			else
			{
				ProcessingGrid.AddToAvailableColumns(QuarantineExDocEstablishmentAndTime.Schema.EE_EstablishmentIndicator);
			}

			if (isNEXDOCSActive)
			{
				ProcessingGrid.AddToAvailableColumns(QuarantineExDocEstablishmentAndTime.Schema.EE_EstablishmentPostedStatusDescription);
				EnableRemoveEntry();
			}
			else
			{
				ProcessingGrid.RemoveFromAvailableColumns(QuarantineExDocEstablishmentAndTime.Schema.EE_EstablishmentPostedStatusDescription);
				DisableRemoveEntry();
			}

			ProcessingGrid.ReOrderColumns(DefaultColumnsForGrid.ToArray());
		}

		#region RemoveEntry

		internal ZMenuItem removeEntryMenuItem;
		ZString removeEntryMenuItemCaption => ResString.GetMultilingualString("Customs.AU.GUI.RFPProcessUserControl.RemoveEntryMenuItemCaption", "&Remove Entry");

		void EnableRemoveEntry()
		{
			if (removeEntryMenuItem == null)
			{
				removeEntryMenuItem = new ZMenuItem(removeEntryMenuItemCaption, OnRemoveEntryClick);
				var deleteItemIndex = ProcessingGrid.DeleteMenuItem.Index;
				ProcessingGrid.ContextMenu.MenuItems.Add(deleteItemIndex + 1, removeEntryMenuItem);
				ProcessingGrid.ContextMenu.Popup += OnContextMenuPopup;

				ProcessingGrid.ColourDeciding += OnProcessingGridColourDeciding;
			}
		}

		void DisableRemoveEntry()
		{
			if (removeEntryMenuItem != null)
			{
				ProcessingGrid.ColourDeciding -= OnProcessingGridColourDeciding;

				ProcessingGrid.ContextMenu.Popup -= OnContextMenuPopup;
				ProcessingGrid.ContextMenu.MenuItems.Remove(removeEntryMenuItem);
				removeEntryMenuItem.Dispose();
				removeEntryMenuItem = null;
			}
		}

		void OnRemoveEntryClick(object sender, EventArgs e)
		{
			if (ZGridExtensions.GetCurrent(ProcessingGrid) is QuarantineExDocEstablishmentAndTime process && process.IsLodged)
			{
				process.EE_EstablishmentPostedStatus = NEXDOCEstablishmentPostedStatus.Codes.DeletePending;
			}
		}

		void OnContextMenuPopup(object sender, EventArgs e)
		{
			removeEntryMenuItem.Enabled = (ZGridExtensions.GetCurrent(ProcessingGrid) as QuarantineExDocEstablishmentAndTime)?.IsLodged ?? false;
		}

		void OnProcessingGridColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			if (e.ObjectAtRow is QuarantineExDocEstablishmentAndTime process && process.IsDeletePending)
			{
				e.Colour = System.Drawing.Color.DarkGray;
			}
		}

		#endregion

		List<string> DefaultColumnsForGrid
		{
			get
			{
				if (defaultColumnsForGrid == null)
				{
					defaultColumnsForGrid = new List<string>();

					defaultColumnsForGrid.Add(QuarantineExDocEstablishmentAndTime.Schema.EE_ProcessingType);
					defaultColumnsForGrid.Add(QuarantineExDocEstablishmentAndTime.Schema.EE_EstablishmentPostedStatusDescription);
					defaultColumnsForGrid.Add(QuarantineExDocEstablishmentAndTime.Schema.EE_E2_Address);
					defaultColumnsForGrid.Add(QuarantineExDocEstablishmentAndTime.Schema.EE_AuthorisationEstablishmentID);
					defaultColumnsForGrid.Add(QuarantineExDocEstablishmentAndTime.Schema.EE_EstablishmentIndicator);
					defaultColumnsForGrid.Add(QuarantineExDocEstablishmentAndTime.Schema.EE_StartDate);
					defaultColumnsForGrid.Add(QuarantineExDocEstablishmentAndTime.Schema.EE_EndDate);
					defaultColumnsForGrid.Add(QuarantineExDocEstablishmentAndTime.Schema.EE_Depuration);
					defaultColumnsForGrid.Add(QuarantineExDocEstablishmentAndTime.Schema.EE_HarvestArea);
					defaultColumnsForGrid.Add(QuarantineExDocEstablishmentAndTime.Schema.EE_InspectionRequestedDate);
					defaultColumnsForGrid.Add(QuarantineExDocEstablishmentAndTime.Schema.EE_LeaseNumber);
					defaultColumnsForGrid.Add(QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentCode);
					defaultColumnsForGrid.Add(QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentDuration);
					defaultColumnsForGrid.Add(QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentDurationUQ);
					defaultColumnsForGrid.Add(QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentTemperature);
					defaultColumnsForGrid.Add(QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentTemperatureUQ);
					defaultColumnsForGrid.Add(QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentConcentration);
					defaultColumnsForGrid.Add(QuarantineExDocEstablishmentAndTime.Schema.EE_TreatmentConcentrationUQ);
				}

				return defaultColumnsForGrid;
			}
		}
		List<string> defaultColumnsForGrid;
	}
}
