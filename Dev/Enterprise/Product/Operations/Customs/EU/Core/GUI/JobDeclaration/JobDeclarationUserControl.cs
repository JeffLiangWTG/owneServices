using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class EUJobDeclarationUserControl : BaseCustomsDeclarationUserControl
	{
		public EUJobDeclarationUserControl()
		{
			InitializeComponent();
			Controls.Remove(base.ImporterOrganisationControl);
			Controls.Remove(base.SupplierOrganisationControl);

#if DEBUG
			TypeDescriptor.AddAttributes(ZG_AgreedPlaceCodeDropEdit, new SuppressControlRequiresTextBasherAttribute());
#endif
		}

		JobDeclaration EUJobDeclaration => (JobDeclaration)base.JobDeclaration;

		const string IsVisibleForBindingString = "IsVisibleForBinding";

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			GatewayDropEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);
			AircraftRegistrationNumberTextBox.DataBindings.RemoveBinding(IsVisibleForBindingString);

			if (dataSource != null)
			{
				GatewayDropEdit.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, Business.Declaration.JobDeclaration.Schema.ZG_Gateway + "Visible")); //Code Sniffer was failing without using the constant.
				AircraftRegistrationNumberTextBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, dataSource, "AircraftRegistrationNumberVisible", false, DataSourceUpdateMode.Never));
			}
		}

		protected virtual int DeclarationTypeMaxLength => 7;

		protected override void SetFolioNumberVisible()
		{
			FolioNumberTextBox.Visible = false;
		}

		protected override bool IsVoyageFlightNoVisible => JobDeclaration.IsAir || JobDeclaration.IsSea;

		protected override void Declaration_MultipleKeyToUseChanged(object sender, EventArgs e)
		{
			RefreshCaption(FinalDestinationFindBox);
			RefreshCaption(OriginFindBox);
			RefreshCaption(ExportDeclarationNumberBoundTextBox);
		}

		protected void RefreshCaption(Control control)
		{
			if (control != null && !control.IsDisposed)
			{
				control.GetExtension<ILabelCaptionRenderer>().Refresh();
			}
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();

			SetTransportIdVisibleAndLabel();
			SetTransportNationalityVisible();
			SetBox18TransportIDVisible();
			SetBox18TransportNationalityVisible();

			var isImport = JobDeclaration.IsImport;
			SpecificCircumstanceDropEdit.Visible = !isImport;
			JE_IATALoadPortCodeFindBox.Visible = isImport && JobDeclaration.IsAir;
			if (CustomsOfficesUserControl.HostedControl is CustomsOfficesUserControl customsOfficesUserControl)
			{
				customsOfficesUserControl.HandleDeclarationControlVisibilityChanged();
			}
			BadgeCodeDropEdit.Visible = false;

			OrganizationExportUserControl.Visible = !isImport;
			OrganizationImportUserControl.Visible = isImport;

			IsHighValueOvrdCheckBox.Visible = ShowIsHighValueOvrdCheckBox(isImport);
			InlandTransportCodeDropEdit.Visible = isImport && EUJobDeclaration.Configuration.IsUCC6(JobDeclaration);
		}

		public virtual bool ShowIsHighValueOvrdCheckBox(bool isImport) => isImport && EUJobDeclaration.DV1DetailsSupport;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			CustomsOfficesUserControl.UserControlType = GetCustomsOfficesUserControlType();
			OrganizationImportUserControl.UserControlType = GetOrganizationImportUserControlType();
			OrganizationExportUserControl.UserControlType = GetOrganizationExportUserControlType();
		}

		void SetTransportIdVisibleAndLabel()
		{
			DepartureTransportIDTextBox.Visible = GetTransportIDVisible();
			if (DepartureTransportIDTextBox.Visible)
			{
				DepartureTransportIDTextBox.GetExtension<ILabelCaptionRenderer>().Caption = EUJobDeclaration.TransportIDLabel;
			}
		}

		protected virtual bool GetTransportIDVisible()
		{
			return !JobDeclaration.IsAir && !JobDeclaration.IsSea;
		}

		void SetBox18TransportIDVisible()
		{
			Box18TransportIDTextBox.Visible = GetBox18TransportIDVisible();
		}

		protected virtual bool GetBox18TransportIDVisible()
		{
			return true;
		}

		void SetTransportNationalityVisible()
		{
			TransportNationalityFindBox.Visible = GetTransportNationalityVisible();
		}

		protected virtual bool GetTransportNationalityVisible()
		{
			return true;
		}

		void SetBox18TransportNationalityVisible()
		{
			Box18TrNationalityFindBox.Visible = GetBox18TransportNationalityVisible();
		}

		protected virtual bool GetBox18TransportNationalityVisible()
		{
			return true;
		}

		protected override bool JE_ContainerModeBoundDropDownEditVisible => true;

		protected override void SetContainerCountAndNoOfPieces()
		{
			JE_ContainerCountCalcEdit.Visible = false;
			JE_TotalNoOfPiecesBoundCalcEdit.Visible = false;
		}

		protected virtual Type GetCustomsOfficesUserControlType() => typeof(CustomsOfficesUserControl);

		protected virtual Type GetOrganizationImportUserControlType() => typeof(ImportOrganizationUserControl);

		protected virtual Type GetOrganizationExportUserControlType() => typeof(ExportOrganizationUserControl);

		protected virtual ZDocAddressControl GetSupplierDocAddressControl()
		{
			return new ZDocAddressControl();
		}

		protected virtual ZDocAddressControl GetImporterDocAddressControl()
		{
			return new ZDocAddressControl();
		}
	}
}
