using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.eNett;
using Enterprise.Accounting.DataTransfer.com.enett991;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class ComPayRegisteredOrganisationsControl : ZUserControl
	{
		public ComPayRegisteredOrganisationsControl()
		{
			InitializeComponent();
			Factory = new BusinessObjectFactory();
			dataSource = new ComPayRegisteredOrganisationDataSource(Factory);
			SetDataBinding(dataSource, "");
			ContextMenu menu = ComPayGrid.ContextMenu.GetContextMenu();
			menu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("103f7534-a4c1-4d0a-88f9-e483f085eff2", "Allocate Organization"), new EventHandler(ComPayGrid_DoubleClick)));
		}

		readonly BusinessObjectFactory Factory;
		ComPayRegisteredOrganisationDataSource dataSource;

		#region Implementation

		ZGrid ComPayGrid;
		ZGroupBox DescriptionGroupBox;
		ZButton FindButton;

		#endregion

		#region System stuff

		readonly System.ComponentModel.Container components;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		void FindButton_Click(object sender, EventArgs e)
		{
			dataSource.ComPayRegisteredOrganisations.RemoveAndDeleteAll();
			if (!AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode.IsEmpty && AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode.IsNumbersOnlyOrEmpty)
			{
				bool result = false;
				Response_GetClientList[] response = null;
				IeNettWebServiceClient eNettService = ObjectFactory.Get<IeNettWebServiceClient>();
				eNettService.Url = AccountingConfigurationRegistry.Instance.ENettWebServiceLocation.Value;
				try
				{
					response = eNettService.DisplayClientList("CARGOWISE",
						AccountingConfigurationRegistry.Instance.ENettIntegratorKey.Value,
						Convert.ToInt32(AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode), "");
					result = true;
				}
				catch (WebException) { }
				catch (SocketException) { }
				catch (InvalidOperationException) { }

				if (!result)
				{
					Globals.Message.ShowError(Res.GetString("623d255e-aafa-4e4e-afa4-c4758ddf7f83", "There was a problem connecting to the ComPay web service. Please try again later."));
				}
				else
				{
					DynamicBusinessObjectCollection headers = new DynamicBusinessObjectCollection(Factory);
					ZSqlParameterCollection @params = new ZSqlParameterCollection();
					@params.Add(ZSqlParameter.New("@AUCode", Core.Constants.CountryCodes.Australia, RefCountrySchema.RN_Code));
					@params.Add(ZSqlParameter.New("@ENECode", OrgCusCode.CodeTypes.eNettRegistrationNumber, OrgCusCodeSchema.OK_CodeType));

					headers.Load("Select " + OrgHeaderSchema.Constants.OH_Code + ", " + OrgCusCodeSchema.Constants.OK_CustomsRegNo + " From " +
					OrgCusCodeSchema.Constants.SqlSchemaName + "." + OrgCusCodeSchema.Constants.TableName + " INNER JOIN " + OrgHeaderSchema.Constants.SqlSchemaName + "." + OrgHeaderSchema.Constants.TableName + " ON " +
					OrgCusCodeSchema.Constants.OK_OH + " = " + OrgHeaderSchema.Constants.PK + " AND " + OrgCusCodeSchema.Constants.OK_RN_NKCodeCountry + " = @AUCode " +
					" WHERE " + OrgCusCodeSchema.Constants.OK_CodeType + " = @ENECode", @params);

					foreach (Response_GetClientList client in response)
					{
						if (client.ECN != 0)
						{
							ComPayRegisteredOrganisation line = dataSource.ComPayRegisteredOrganisations.AddNew();
							line.ClientName = client.ClientName;
							line.ABN = client.ABN;
							line.ECN = client.ECN;
							line.RegistrationDate = client.RegistrationDate;
							line.Address1 = client.Address1;
							line.Address2 = client.Address2;
							line.Suburb = client.Suburb;
							line.State = client.State;
							line.Postcode = client.Postcode;
							line.Country = client.Country;
							line.Phone = client.Phone;
							line.Fax = client.Fax;
							line.TerminalCode = client.TerminalCode;
							line.RelatedOrganisations = ZString.Empty;
							foreach (DynamicBusinessObject header in headers)
							{
								if ((ZString)header[OrgCusCodeSchema.Constants.OK_CustomsRegNo] == client.ECN.ToString())
								{
									line.RelatedOrganisations += ", " + (ZString)header[OrgHeaderSchema.Constants.OH_Code];
								}
							}
							line.RelatedOrganisations = line.RelatedOrganisations.TrimStart(", ".ToCharArray());
						}
					}
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("7CFAF945-2956-459a-AE6F-111BCD7C427A", "Registration Code is invalid or empty!"));
			}
		}

		void ComPayGrid_DoubleClick(object sender, EventArgs e)
		{
			if (ComPayGrid.SelectedElements != null && ComPayGrid.SelectedElements.Length == 1)
			{
				AllocateToOrganisationForm form = new AllocateToOrganisationForm((ComPayRegisteredOrganisation)ComPayGrid.SelectedElements[0]);
				form.Show();
			}
		}
	}
}

