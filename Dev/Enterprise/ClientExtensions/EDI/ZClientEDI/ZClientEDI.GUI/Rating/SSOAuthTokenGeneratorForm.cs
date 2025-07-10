using System;
using System.Linq;
using AuthenticationService.Client.Models;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using ZClientEDI.Business.Rating;

namespace ZClientEDI.GUI.Rating
{
	public partial class SSOAuthTokenGeneratorForm : ZChildForm
	{
		readonly Func<string> generateSupportLoginToken;
		readonly Action<string> doClipboard;

		readonly IAuthTokenProvider authTokenProvider;

		public SSOAuthTokenGeneratorForm(Func<string> generateSupportLoginToken, Action<string> doClipboard, IAuthTokenProvider authTokenProvider)
		{
			InitializeComponent();

			this.generateSupportLoginToken = generateSupportLoginToken;
			this.doClipboard = doClipboard;
			this.authTokenProvider = authTokenProvider;

			var currentUser = GlbStaff.CurrentUser;
			var currentCompany = GlbCompany.CurrentCompany;

			var vm = new SubmitAuthTokenInputs
			{
				EnterpriseCode = "HYE",
				ServerCode = "HYE",
				UserCode = currentUser.GS_Code,
				UserFullName = currentUser.GS_FullName,
				UserEmail = currentUser.GS_EmailAddress,
				CompanyCode = currentCompany.GC_Code,
				CompanyName = currentCompany.GC_Name,
				Roles = new ZBoolDescriptionPairList()
			};

			vm.Roles.Add(new ZBoolDescriptionPair(WTGRoles.CargoSphereRateAdmin, false));
			vm.Roles.Add(new ZBoolDescriptionPair(WTGRoles.CargoSphereStandardUser, false));
			vm.Roles.Add(new ZBoolDescriptionPair(WTGRoles.CargoguideStandardUser, false));
			vm.Roles.Add(new ZBoolDescriptionPair(WTGRoles.CargoguideAutoCostingUser, false));

			SetDataBinding(vm, "");
		}

		SubmitAuthTokenInputs ViewModel => (SubmitAuthTokenInputs)BindingSource.Current;

		void GenerateButton_Click(object sender, EventArgs e)
		{
			ValidateAll(CargoWise.EntityFramework.ValidationType.Full);
			if (ViewModel.HasErrors)
			{
				return;
			}

			var supportLoginToken = generateSupportLoginToken();
			if (supportLoginToken == null)
			{
				return;
			}

			GenerateButton.Enabled = false;

			try
			{
				var correlationId = Guid.NewGuid().ToString();
				var (token, validation) = authTokenProvider.GetToken(correlationId, 30, MapInputsToApiPayload(ViewModel, supportLoginToken));

				if (token != null)
				{
					TokenResultTextBox.Text = token;

					doClipboard(token);
				}
				else
				{
					TokenResultTextBox.Text = validation;
				}
			}
			catch (Exception ex)
			{
				TokenResultTextBox.Text = $"{ex.Message}\n\n{ex.StackTrace}";
			}
			finally
			{
				GenerateButton.Enabled = true;
			}
		}

		static Action<LoginInfo> MapInputsToApiPayload(SubmitAuthTokenInputs inputs, string supportLoginToken)
		{
			return (payload) =>
			{
				payload.Type = LoginType.CW1Support;
				payload.UserCode = inputs.UserCode;
				payload.UserFullName = inputs.UserFullName;
				payload.UserEmail = inputs.UserEmail;
				payload.CompanyCode = inputs.CompanyCode;
				payload.CompanyName = inputs.CompanyName;
				payload.ServerCode = inputs.ServerCode;
				payload.DatabaseNumber = inputs.DatabaseNumber;
				payload.EnterpriseCode = inputs.EnterpriseCode;
				payload.Roles = MapRoles(inputs.Roles);
				payload.Password = supportLoginToken;
			};
		}

		/// <summary>
		/// Map roles to role names; ensure support role is always added
		/// </summary>
		static string[] MapRoles(ZBoolDescriptionPairList roles)
		{
			var support = new[] { WTGRoles.Support };
			var selectedRoleNames = roles.Where(r => r.Value).Select(r => r.Description.ToString());
			return support.Concat(selectedRoleNames).ToArray();
		}
	}
}
