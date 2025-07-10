<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Page Language="c#" EnableViewStateMac="false" CodeBehind="RegisterMasterPassword.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.ZClientWebCargoWiseEDI.RegisterMasterPassword" %>
<!DOCTYPE html>
<html class="login" xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>CargoWise | My Account > Home > Register Master Password</title>
	<meta name="viewport" content="width=device-width, initial-scale=1.0" />
	<script>
		if (!Element.prototype.matches) Element.prototype.matches = Element.prototype.msMatchesSelector;
		if (!Element.prototype.closest) Element.prototype.closest = function (selector) {
			var el = this;
			while (el) {
				if (el.matches(selector)) {
					return el;
				}
				el = el.parentElement;
			}
		};

		function updateContactListRowSelected() {
			var radios = document.querySelectorAll("#login-contact-lists input[type=radio]");
			for (var r = 0; r < radios.length; r++) {
				radios[r].checked ? radios[r].closest("tr").classList.add("selected") : radios[r].closest("tr").classList.remove("selected");
			}
		}

		document.addEventListener("DOMContentLoaded", function () {
			var radios = document.querySelectorAll("#login-contact-lists input[type=radio]");
			for (var r = 0; r < radios.length; r++) {
				radios[r].addEventListener("change", updateContactListRowSelected);
				radios[r].closest("tr").addEventListener("click", (function (radio) {
					return function () {
						var radios = document.querySelectorAll("#login-contact-lists input[type=radio]");
						for (var r = 0; r < radios.length; r++) {
							radios[r].checked = false;
						}
						radio.checked = true;
						updateContactListRowSelected();
					};
				})(radios[r]));
			}
			updateContactListRowSelected();
		});
	</script>
</head>
<body id="DefaultBody" runat="server">
<form id="Form1" runat="server">
	<div id="login" class="login-redirection" runat="server">
		<div id="login-company-logo-container">
			<img id="login-company-logo" alt="CargoWise" src="/myaccount/Images/cargowise-logo.svg" />
		</div>
		<div id="login-content">
			<div id="contact-lists">
				<div id="ContactsBox" runat="server">
					<h1>Register Master Password</h1>
					<p>
						User Account changes have occurred which affect the way you login into My Account. Your accounts will now be available under a single password.
						<br />
						<br />
						You can set this password through one of the following options:
						<br />
					</p>
					<div class="login-row">
						<edi:ZTextLabel id="ChooseExistingPasswordMessageLabel" runat="server">1. Use one of the passwords from your existing accounts:</edi:ZTextLabel><br/><br/>
					</div>
					<div id="login-contact-lists">
						<div class="table-wrapper">
							<table>
								<thead>
									<tr>
										<th></th>
										<th>
											Company Code
										</th>
										<th>
											Company Name
										</th>
										<th>
											Email
										</th>
										<th>
											Primary Workplace
										</th>
									</tr>
								</thead>
								<tbody>
								<edi:zrepeater id="PasswordSourceContactRepeater" runat="server" bindto="PasswordHoldingContacts">
									<HeaderTemplate>
									</HeaderTemplate>
									<ItemTemplate>
										<tr>
											<td>
												<edi:ZRadioButton id="Checked" Enabled="true" runat="server" GroupName="LoginCheckBoxes"></edi:ZRadioButton>
											</td>
											<td>
												<edi:ZTextLabel id="CompanyCode" runat="server" Text='<%# Eval("OrganisationCode") %>'></edi:ZTextLabel> 
											</td>
											<td>
												<edi:ZTextLabel id="CompanyName" runat="server" Text='<%# Eval("WorkingAddressCompanyName") %>'></edi:ZTextLabel> 
											</td>
											<td>
												<edi:ZTextLabel id="Email" runat="server" Text='<%# Eval("OC_Email") %>'></edi:ZTextLabel> 
											</td>
											<td style="text-align: center">
												<edi:ZTextLabel id="PrimaryWorkplace" runat="server" Text='<%# Eval("OC_IsPrimaryContact") %>'></edi:ZTextLabel>
											</td>
										</tr>

									</ItemTemplate>
									<FooterTemplate>
									</FooterTemplate>
								</edi:zrepeater>
								</tbody>
							</table>
						</div>
					</div>
					<br />
					<br />
					<div class="login-row">
						<asp:button ID="UseExistingButton" runat="server" text="Use Existing Password" onclick="UseExistingButton_Click"></asp:button>
					</div>
					<br />
					<div class="login-row">
						<edi:ZTextLabel id="SetNewPasswordMessageLabel" runat="server">2. Set a new password:</edi:ZTextLabel><br/><br/>
						<asp:button ID="SetPasswordButton" runat="server" text="Set New Password" onclick="SetNewPasswordButton_Click"></asp:button>
					</div>
					<p>
						NOTE: If you would like to disconnect any of the accounts shown above, visit the Account Information page once you have logged in.
					</p>
				</div>
				<edi:ZTextLabel ID="ErrorMessage" runat="server" cssclass="ErrorMessage" />
			</div>
		</div>
		<div id="login-footer">
			<strong>&copy; <edi:ZTextLabel id="CopyrightYear" runat="server"></edi:ZTextLabel> WiseTech Global</strong>
			<a target="_blank" href="https://www.wisetechglobal.com/legal#privacypolicy">Privacy Policy</a>
			<a target="_blank" href="https://www.wisetechglobal.com/legal#cookies">Cookie Usage</a>
			<a target="_blank" href="https://www.wisetechglobal.com/legal#termsofuse">Terms of Use</a>
		</div>
	</div>
</form>
</body>
</html>
