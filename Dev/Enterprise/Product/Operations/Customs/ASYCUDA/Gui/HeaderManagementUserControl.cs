using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Res = Enterprise.Customs.ASYCUDA.Gui.Res;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public partial class HeaderManagementUserControl : ZUserControl
	{
		public HeaderManagementUserControl()
		{
			InitializeComponent();
			InitialiseDecoupledManifestsGrid();
		}

		ManifestHeadersWrapper HeadersWrapper => CurrentDataItem as ManifestHeadersWrapper;
		bool IsManifestConsolDecouplingEnabled => AsycudaManifestHeader.IsManifestConsolDecouplingEnabled;

		void InitialiseDecoupledManifestsGrid()
		{
			this.SuspendLayout();

			var isManifestConsolDecouplingEnabled = IsManifestConsolDecouplingEnabled;
			if (isManifestConsolDecouplingEnabled)
			{
				zDropEditManifestToDelete.Visible = false;
				zButtonDeleteManifest.Visible = false;
				ManifestHeaderMainPanel.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(60);

				ManifestsGrid.HideButtonRow();
				ManifestsGrid.InnerGrid.AllowReadOnlyRowsToBeDeleted = true;
				ManifestsGrid.InnerGrid.RowsDeleting += OnManifestsGridRowsDeleting;
			}

			ManifestsGroupBox.Visible = isManifestConsolDecouplingEnabled;

			this.ResumeLayout(false);
			this.PerformLayout();
		}

		void CreateHeaderButton_Click(object sender, EventArgs e)
		{
			var wrapper = HeadersWrapper;
			if (wrapper != null)
			{
				wrapper.Validation.ValidateAll();

				var errors = wrapper.WR_CountryCodeInfo.GetMessageErrors()
					.Union(wrapper.WR_ManifestTypeInfo.GetMessageErrors())
					.Select(c => c.Message)
					.Distinct()
					.ToArray();

				if (wrapper.WR_CountryCode.IsEmpty)
				{
					errors = errors.Union(new[] { MandatoryValidation.YouHaveNotEnteredMessage(Res.GetString("481B7AC6-062D-45AA-BF51-E5A873FF2CF8", "New Manifest Country")) }).ToArray();
				}

				if (errors.Any())
				{
					var message = Res.GetString("E1FF95D0-1C03-4961-8725-22D1A3BEEB52",
						"There are errors that need to be corrected before this Manifest can be created.{0}{0}{1}",
						System.Environment.NewLine,
						string.Join(System.Environment.NewLine, errors));

					var caption = Res.GetString("0F7CD688-8E43-4250-9002-5C0BCEC4841D", "Create New Manifest");

					Globals.Message.ShowError(message, caption);
				}
				else
				{
					if (IsManifestConsolDecouplingEnabled)
					{
						CreateNewHeader(wrapper);
					}
					else
					{
						OnCreateNewHeader?.Invoke(sender, EventArgs.Empty);
					}
				}
			}
		}

		public event EventHandler OnCreateNewHeader;

		void CreateNewHeader(ManifestHeadersWrapper wrapper)
		{
			var countryCode = wrapper.WR_CountryCode;
			var manifestType = wrapper.WR_ManifestType;
			if (!countryCode.IsEmpty && !manifestType.IsEmpty)
			{
				var header = wrapper.Headers.GetHeader(countryCode, manifestType);
				if (header == null)
				{
					var consol = wrapper.Consol;
					if (!consol.CheckManifestHeaderHasBeenCreated(countryCode, manifestType))
					{
						if (consol.IsInDatabase && !consol.HasChanges)
						{
							header = CreateCountry(wrapper, consol, countryCode, manifestType);
						}
						else
						{
							Globals.Message.ShowError(
								Res.GetString("D58BE988-90F3-4057-9D98-27935C08969E", "The Consol needs to be saved before a new Manifest can be created."),
								CreateNewManifestCaption);
						}
					}
					else
					{
						Globals.Message.ShowError(
							Res.GetString("3D8A9666-3781-494C-9F0E-5137A9A31DB1", "Another user has already created a Manifest for {0}. Please close and re-open the Consol to see any newly added Manifest Countries", wrapper.CountryCodes.GetDescriptionFromCode(countryCode)),
							CreateNewManifestCaption);
					}
				}

				if (header != null)
				{
					ManifestsGrid.ShowEditForm(header);
				}
			}
		}

		AsycudaManifestHeader CreateCountry(ManifestHeadersWrapper wrapper, ForwardingConsol consol, ZString countryCode, ZString manifestType)
		{
			AsycudaManifestHeader result = null;

			var factory = consol.Factory;
			var consolPK = consol.PK;

			try
			{
				if (factory.LockChild(MutexIDs.AsycudaManJobBeingCreated, consolPK, countryCode))
				{
					if (!consol.CheckManifestHeaderHasBeenCreated(countryCode, manifestType))
					{
						result = wrapper.CreateCountry(countryCode, manifestType);
						factory.Save();
					}

					wrapper.WR_CountryCode = ZString.Empty;
					wrapper.WR_ManifestType = ZString.Empty;
				}
			}
			finally
			{
				factory.UnlockChild(MutexIDs.AsycudaManJobBeingCreated, consolPK, countryCode);
			}

			if (result == null)
			{
				Globals.Message.ShowError(
					Res.GetString("706596EA-6D48-4668-A700-266884EB9BD9", "{0} is already in the process of creating a Manifest for {1}.\r\nYou should be able to access the Manifest when the person has saved the record. Please try later.\r\n", factory.GetChildLockByInfo(MutexIDs.AsycudaManJobBeingCreated, consolPK, countryCode), wrapper.CountryCodes.GetDescriptionFromCode(countryCode)),
					CreateNewManifestCaption);
			}

			return result;
		}

		void DeleteManifestButton_Click(object sender, EventArgs e)
		{
			var wrapper = HeadersWrapper;
			var manifestToDelete = wrapper?.ManifestToDelete;
			if (manifestToDelete != null)
			{
				var submitted = manifestToDelete.HasManifestBeenSubmittedToCustomsIncludingChildren;
				if (submitted)
				{
					Globals.Message.ShowError(Res.GetString("11AB827E-8915-4230-85B1-B32B80D77D55", "This manifest cannot be deleted as it contains submitted messages."));
				}
				else if (Globals.Message.ShowConfirmation(Res.GetString("EE1FDD7B-EA87-4AFF-9163-DCD896ED2D3E", "Are you sure you would like to delete this manifest?"), "Delete Existing Manifest", "yes", MessageBoxIcon.Warning) == DialogResult.OK)
				{
					OnManifestDeleting?.Invoke(manifestToDelete, EventArgs.Empty);
					wrapper.DeleteManifest(manifestToDelete);
					wrapper.WR_KeywordCombination = ZString.Empty;
				}
			}
		}
		public event EventHandler OnManifestDeleting;

		void OnManifestsGridRowsDeleting(object sender, RowsDeletingEventArgs e)
		{
			if (!e.Cancel)
			{
				var manifests = e.Objects.Cast<AsycudaManifestHeader>();
				var jobsWithMessages = string.Join(", ", manifests.Where(x => x.HasManifestBeenSubmittedToCustomsIncludingChildren).Select(x => x.AMA_JobReference).OrderBy(x => x));
				if (jobsWithMessages.Length > 0)
				{
					e.Cancel = true;
					Globals.Message.ShowError(Res.GetString("23ECEB21-7C5E-4A2F-97E5-28EE0E4D2588", "Manifests with submitted messages cannot be deleted.\r\n {0}", jobsWithMessages));
				}
				else
				{
					var jobs = string.Join(", ", manifests.Select(x => x.AMA_JobReference).OrderBy(x => x));
					var msg = Res.GetString("4A671FA0-1CA6-42C7-A9D0-18706AAD75CD", "Are you sure you would like to delete\r\n {0}", jobs);

					e.Cancel = Globals.Message.ShowConfirmation(msg, "Delete Existing Manifest", "yes", MessageBoxIcon.Warning) != DialogResult.OK;
				}
			}
		}

		string CreateNewManifestCaption => Res.GetString("0FE56549-959D-4413-AD84-2CC6A720F893", "Create New Manifest");
	}
}
