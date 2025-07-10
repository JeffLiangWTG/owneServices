<%@ Page Language="c#" AutoEventWireup="True" EnableViewState="True" CodeBehind="LoginV2.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.LoginV2" %>

<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350" %>

<!DOCTYPE html>
<html class="login" xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>CargoWise | My Account > Login</title>
	<meta name="viewport" content="width=device-width, initial-scale=1.0" />
</head>
<body runat="server">
	<script type="text/javascript" src="../Scripts/jquery-3.6.0.min.js"></script>
	<script type="text/javascript" src="../Scripts/myaccountRememberMe.js"></script>	
	<script>
		if (top.location != self.location) {
			top.location = self.location.href;
		}

		const currentTextBox = '<%= PageStatus == PageStatusList.Password? PasswordTextBox.ClientID: LoginNameTextBox.ClientID %>';

		function forgetUser() {
			fetch('../api/oidc/forget-user', {
				method: 'POST',
				credentials: 'same-origin'
			})
				.then(response => {
					if (response.ok) {
						clearUserNameAndCompanyName();
						document.getElementById('LoginNameTextBox').value = '';
						document.getElementById('CompanyCodeTextBox').value = '';
						document.getElementById('ForgetUser').style.display = 'none';
						toggleButton();
					}
				})
				.catch(error => {
					console.error('Error while clearing cookie:', error);
				});
		}

		function toggleForgetUserLinkVisibility() {
			var emailUsername = document.getElementById('LoginNameTextBox');
			var organisationCode = document.getElementById('CompanyCodeTextBox');
			var forgetUserLink = document.querySelector('.ForgetUser');

			if (emailUsername && organisationCode && forgetUserLink) {
				if (emailUsername.value.trim() !== '' || organisationCode.value.trim() !== '') {
					forgetUserLink.style.display = 'inline';
				} else {
					forgetUserLink.style.display = 'none';
				}
			}
		}

		function showCompanyCode() {
			const companyCodeDiv = document.getElementById("CompanyCodeDiv");
			const showCompanyCodeLabelDiv = document.getElementById("ShowCompanyCodeLabelDiv");
			if (companyCodeDiv.style.display === "none") {
				companyCodeDiv.style.display = "block";
				showCompanyCodeLabelDiv.style.display = "none";
			}
		}

		function restoreUserNameAndCompanyName() {
			const loginNameTextBox = document.getElementById("LoginNameTextBox");
			const companyCodeTextBox = document.getElementById("CompanyCodeTextBox");
			const loginName = getUserName();
			const companyCode = getCompanyCode();

			if (loginName !== '' && loginNameTextBox && loginNameTextBox.value === '') {
				loginNameTextBox.value = loginName;
			}

			if (companyCode !== '' && companyCodeTextBox && companyCodeTextBox.value === '') {
				companyCodeTextBox.value = companyCode;
				showCompanyCode();
			}

			toggleForgetUserLinkVisibility();
		}

		function saveUserNameAndCompanyName() {
			const loginName = document.getElementById("LoginNameTextBox");
			const companyCode = document.getElementById("CompanyCodeTextBox");
			if (loginName) {
				saveUserName(loginName.value);
			}

			if (companyCode) {
				saveCompanyCode(companyCode.value);
			}

			return true;
		}

		function toggleButton() {
			var textBox = document.getElementById(currentTextBox);
			var signinBtn = document.getElementById('<%= SigninBtn.ClientID %>');

			if (textBox.value.trim() !== "") {
				signinBtn.disabled = false;
			} else {
				signinBtn.disabled = true;
			}
		}

		$(document).ready(function () {
			restoreUserNameAndCompanyName();
			toggleButton();
			document.getElementById(currentTextBox).addEventListener('input', (event) => {
				toggleButton();
			});
		});
	</script>

	<div id="login-whole-page">
		<div id="login-page-left-div">
			<form id="LoginForm" runat="server" method="post" class="login-form">
				<!-- Theme Container -->
				<div id="login">
					<div id="login-page">
						<div id="login-content">

							<div id="login-company-logo-container">
								<img id="login-company-logo" alt="CargoWise" src="../Images/blue-cargowise-logo.svg" />
							</div>

							<h1>Login to My Account</h1>
							<asp:PlaceHolder runat="server">
								<% if (PageStatus != PageStatusList.Password) { %>
								<asp:Label class="LoginLabel" runat="server">Email address</asp:Label>
								<edi:ZTextBox ID="LoginNameTextBox" runat="server" BindTo="UserName" MaxLength="40" TabIndex="1"></edi:ZTextBox>

								<div id="block-separator"></div>

								<div id="ShowCompanyCodeLabelDiv" runat="server">
									<label id="ShowCompanyCodeLabel">Specify Organization code (optional)</label>
								</div>

								<div id="CompanyCodeDiv" runat="server" style="display: none">
									<label class="LoginLabel">Organization code (optional)</label>
									<edi:ztextbox id="CompanyCodeTextBox" runat="server" BindTo="CompanyCode" MaxLength="12" Placeholder="Optional" TabIndex="3"></edi:ztextbox>
								</div>

								<span class="ForgetUser">
									<asp:HyperLink runat="server" onclick="forgetUser()" ID="ForgetUser">Forget saved user</asp:HyperLink>
								</span><br />
								<% } else { %>
								<edi:ZTextLabel class="EnteredUserInfo" ID="LoginNameTextLabel" runat="server"></edi:ZTextLabel>
									<% if (!string.IsNullOrEmpty(CompanyCodeTextLabel.Text)) { %>
										<asp:Label class="LoginLabel"  runat="server">Company code</asp:Label>
									<% } %>
								<edi:ZTextLabel class="EnteredUserInfo" ID="CompanyCodeTextLabel" runat="server"></edi:ZTextLabel>
								<asp:Label class="LoginLabel"  runat="server">Password</asp:Label>
								<edi:ZTextBox ID="PasswordTextBox" TextMode="password" runat="server" BindTo="Password" TabIndex="3"></edi:ZTextBox>
								<span class="ForgotPassword">
									<asp:HyperLink ID="ForgotPasswordSpan" runat="server" NavigateUrl="../Login/RetrieveLogin.aspx">Forgot password?</asp:HyperLink>
								</span><br />
								<% } %>
							<edi:ZTextLabel ID="Message" runat="server" CssClass="ErrorMessage" />
							</asp:PlaceHolder>
							<div id="login-button-panel-div">
								<asp:PlaceHolder runat="server">
									<% if (PageStatus == PageStatusList.Password) { %>
										<asp:HyperLink ID="LoginLink1" runat="server" font-size="X-Large" NavigateUrl="Login.aspx" CssClass="link-button back-button">Back</asp:HyperLink>
									<% } %>
								</asp:PlaceHolder>
								<asp:Button ID="SigninBtn" runat="server" Text="Next" OnClientClick="saveUserNameAndCompanyName()" OnClick="SigninBtn_Click"></asp:Button>
							</div>
						</div>
					</div>
				</div>
					<div id="login-footer">
						<strong>&copy; <edi:ZTextLabel id="CopyrightYear" runat="server"></edi:ZTextLabel> WiseTech Global</strong>
						<a target="_blank" href="https://www.wisetechglobal.com/legal#privacypolicy" style="text-decoration-line: underline" >Privacy</a>
						<a target="_blank" href="https://www.wisetechglobal.com/legal#termsofuse" style="text-decoration-line: underline" >Terms</a>
					</div>
				<!-- End Theme Container -->
			</form>
		</div>
		<div id="login-supply-chain-image-container">
			<img id="supply-chain-image" alt="Enabling and empowering the world's supply chains" src="../Images/supply-chain-without-whitespace.svg"/>			
		</div>
	</div>
</body>
</html>
