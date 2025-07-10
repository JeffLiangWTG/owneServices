using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.GUI
{
	public partial class EntryInstructionAuthorisationsUserControl : ZUserControl
	{
		public EntryInstructionAuthorisationsUserControl()
		{
			InitializeComponent();
			if (CusAuthorisationHeaderProvider.EnableAdHoc)
			{
				AuthorisationsGrid.Hotkeys.RegisterHotKey(Keys.F5, RunTemporaryAuthorisationController);
			}

			ApplyVisibilityOnControl();

			AuthorisationsGrid.AllowOutsideOfParent();
		}

		void RunTemporaryAuthorisationController()
		{
			if (ActiveControl.Name == CusAuthorizationUsage.Schema.AGC_CPH_Authorization)
			{
				if (Env.Security.FlagAuthorisationAdHoc.IsAllowed)
				{
					var authorizationUsage = (CusAuthorizationUsage)AuthorisationsGrid.GetCurrent();
					if ((authorizationUsage.AGC_Number.IsEmpty || authorizationUsage.RelatedAuthorisationHeader == null) && authorizationUsage.AuthorisationHeader == null)
					{
						var tempAuthorisation = authorizationUsage.CreateTemporaryAuthorisationFromUsage();
						ShowTemporaryAuthorisationForm(tempAuthorisation);

						if (tempAuthorisation.IsInDatabase)
						{
							AuthorisationsGrid.ListManager.AddNew();
							authorizationUsage = (CusAuthorizationUsage)AuthorisationsGrid.GetCurrent();
							authorizationUsage.CopyFromTemporaryAuthorization(tempAuthorisation);
							RefreshAuthorization();
							AuthorisationsGrid.ListManager.EndCurrentEdit();
						}
					}
				}
				else
				{
					Env.Security.FlagAuthorisationAdHoc.ShowError();
				}
			}
		}

		protected virtual void ShowTemporaryAuthorisationForm(CusAuthorisationHeader tempAuthorisation)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.CusAuthorisations);
			var form = ((ZControllerInternals)controller).GetForm(tempAuthorisation);
			ZFormModaliser.ShowDialogAndDispose((Form)form);
		}

		void RefreshAuthorization()
		{
			ActiveControl = AuthorizationZGuidFindBox;
		}

		TextBox AuthorizationZGuidFindBox => authorizationZGuidFindBox ?? (authorizationZGuidFindBox = AuthorisationsGrid.Controls.OfType<TextBox>().ToArray().FirstOrDefault(x => x.Name == CusAuthorizationUsage.Schema.AGC_CPH_Authorization));
		TextBox authorizationZGuidFindBox;

		public new JobDeclaration CurrentDataItem => (JobDeclaration)base.CurrentDataItem;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			AuthorisationsGrid.SetAvailability(CusAuthorisationHeaderProvider.ShowCustomsCode && (CurrentDataItem?.IsUCC6 ?? false), nameof(CusAuthorizationUsage.CustomsCode));
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			UnHookDeclarationEvents();
			base.OnCurrentDataItemChanging(e);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			HookDeclarationEvents();
		}

		void HookDeclarationEvents()
		{
			if (CurrentDataItem is JobDeclaration declaration)
			{
				declaration.MultipleKeyToUseChanged += Declaration_MultipleKeyToUseChanged;
			}
			Declaration_MultipleKeyToUseChanged(this, null);
		}

		void UnHookDeclarationEvents()
		{
			if (CurrentDataItem is JobDeclaration declaration)
			{
				declaration.MultipleKeyToUseChanged -= Declaration_MultipleKeyToUseChanged;
			}
		}

		void Declaration_MultipleKeyToUseChanged(object sender, EventArgs e)
		{
			AuthorisationsGrid.Extensions.Get<CargoWise.Windows.UI.IAutomaticLabelExtension>().Refresh();
		}

		void ApplyVisibilityOnControl()
		{
			AuthorisationsGrid.GetColumnStyle(AutoCusAuthorizationUsage.Schema.AGC_CPH_Authorization).IsVisible = CusAuthorisationHeaderProvider.EnableAdHoc;
			AuthorisationsGrid.GetColumnStyle("ReferenceNumberOf_RelatedAuthorisationHeaderIgnoringReferenceNumber").IsVisible = CusAuthorisationHeaderProvider.ShowRelatedAuthorisationWithoutReference;
		}

		Customs.Business.CusAuthorisationHeaderProvider CusAuthorisationHeaderProvider => cusAuthorisationHeaderProvider ?? (cusAuthorisationHeaderProvider = Customs.Business.CusAuthorisationHeaderProvider.GetByCountryCode(GlbCompany.CurrentCompany?.GC_RN_NKCountryCode ?? ZString.Empty));
		Customs.Business.CusAuthorisationHeaderProvider cusAuthorisationHeaderProvider;
	}
}
