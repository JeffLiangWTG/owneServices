<%@ page language="C#" autoeventwireup="true" codebehind="ResetMasterPassword.aspx.cs" inherits="Enterprise.ZClientWebCargoWiseEDI.ResetMasterPassword" %>
<%@ Register TagPrefix="cc1" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350" %>
<%@ Register TagPrefix="edi" Tagname="AccountVerification" src="../Login/AccountVerificationControl.ascx"%>
<%@ Register TagPrefix="edi" Tagname="PasswordChangeRequirements" src="PasswordChangeRequirementsControl.ascx" %>
<!DOCTYPE html>
<html class="login" xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
	<title>CargoWise | My Account > Reset Master Password</title>
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
			var radios = document.querySelectorAll("#contact-lists input[type=radio]");
			for (var r = 0; r < radios.length; r++) {
				radios[r].checked ? radios[r].closest("tr").classList.add("selected") : radios[r].closest("tr").classList.remove("selected");
			}
		}

		document.addEventListener("DOMContentLoaded", function () {
			var radios = document.querySelectorAll("#contact-lists input[type=radio]");
			for (var r = 0; r < radios.length; r++) {
				radios[r].addEventListener("change", updateContactListRowSelected);
				radios[r].closest("tr").addEventListener("click", (function (radio) {
					return function () {
						if (!radio.checked) {
							var radios = document.querySelectorAll("#contact-lists input[type=radio]");
							for (var r = 0; r < radios.length; r++) {
								radios[r].checked = false;
							}
							radio.checked = true;
							updateContactListRowSelected();
							__doPostBack();
						}
					};
				})(radios[r]));
			}
			updateContactListRowSelected();
		});
	</script>
</head>
<style type="text/css">
	body { counter-reset: AutoNumCounter; }
	.AutoNum:before {
		content: counter(AutoNumCounter) ". ";
		counter-increment: AutoNumCounter;
	}

	#contact-lists table.reset-password-table tr td {
		background-color: transparent;
	}

	#contact-lists table.reset-password-table tr:hover {
		background-color: #ecf1ff;
	}

	#contact-lists table.reset-password-table tr.selected {
		background-color: #dae2fe;
	}
</style>
<body>
	<div id="login-whole-page">
		<div id="login-page-left-div">
			<form id="LoginForm" runat="server">
				<div id="login" class="set-master-password">
					<div id="login-content">

						<div id="login-company-logo-container">
							<img id="login-company-logo" alt="CargoWise" src="../Images/blue-cargowise-logo.svg" />
						</div>
					
						<div class="login-row">
							<h1>MyAccount Password</h1>
							<div id="HeadingMessageDiv" runat="server">
								<div>
									<cc1:ZTextLabel id="PasswordExpiredMessageLabel" runat="server" Text="Your password has expired due to an enforced password rotation policy. Please enter a new password." Visible="False"></cc1:ZTextLabel>
								</div>
								<div>
									<cc1:ZTextLabel id="ResetMasterPasswordHeadingLabel" runat="server" Text="Please select the Person Account you would like to set the new password for." Visible="False"></cc1:ZTextLabel>
								</div>
							</div>
						</div>

						<div id="contact-lists" class="choose-company-table">
							<div id="ContactsBox" class="table-wrapper" runat="server">
								<table class="reset-password-table">
									<thead>
									<tr>
										<th class="set-password-table-header"></th>
										<th class="set-password-table-header">
											Person
										</th>
										<th class="set-password-table-header">
											Related Account(s)
										</th>
									</tr>
									</thead>
									<tbody>
									<cc1:zrepeater id="LoginContactsRepeater" runat="server" bindto="PersonsForBinding">
										<HeaderTemplate>
										</HeaderTemplate>
										<ItemTemplate>
											<tr>
												<td>
													<cc1:ZRadioButton id="Checked" Enabled="true" runat="server" GroupName="LoginCheckBoxes" AutoPostBack="True" OnCheckedChanged="CheckBox_OnCheckedChanged"></cc1:ZRadioButton>
												</td>
												<td>
													<cc1:ZTextLabel id="Name" runat="server" Text='<%# Eval("Name") %>'></cc1:ZTextLabel> 
												</td>
												<td>
													<cc1:ZTextLabelNoEncode id="RelatedAccounts" runat="server" Text='<%# Eval("RelatedAccounts") %>'></cc1:ZTextLabelNoEncode> 
												</td>
											</tr>
										</ItemTemplate>
										<FooterTemplate>
										</FooterTemplate>
									</cc1:zrepeater>
									</tbody>
								</table>
							</div>
						</div>

						<div id="PasswordDiv" runat="server">
							<div class="login-row">
								<asp:textbox id="NewPassword" runat="server" TextMode="Password" Placeholder="New password" OnKeyUp="return checkPasswordRequirements(document.getElementById('NewPassword').value);"></asp:textbox>
							</div>

							<div style="margin:15px;"></div>
							
							<div class="login-row">
								<asp:textbox id="NewPasswordConfirm" runat="server" TextMode="Password" Placeholder="Confirm new password"></asp:textbox>
							</div>

							<div style="margin:15px;"></div>

							<div class="login-row password-requirements">
								<edi:PasswordChangeRequirements id="passwordChangeRequirements" runat="server" />
							</div>

							<div style="margin:15px;"></div>

							<div class="login-row">
								<asp:Button id="Update" runat="server" Text="Set Password" onclick="Update_Click"></asp:Button>
							</div>
							<div class="login-row" style="display:none;">
								<asp:Hyperlink id="BackLink" runat="server" CssClass="link-button master-password-back-button">Back</asp:Hyperlink>
							</div>

							<cc1:ZTextLabel id="PasswordChangeMessage" runat="server" cssclass="ErrorMessage"></cc1:ZTextLabel>
							<p>
								Go <u><asp:HyperLink ID="GoBackLoginLink" runat="server" NavigateUrl="../Login/Login.aspx">back</asp:HyperLink></u> to the login page.
							</p>
						</div>
						<div runat="server" id="LoginOptionsDiv" class="login-options" visible="false">
							<edi:AccountVerification id="AccountVerification" runat="server" />
						</div>
					</div>
				</div>
				<div id="login-footer">
					<strong>&copy; <cc1:ZTextLabel id="CopyrightYear" runat="server"></cc1:ZTextLabel> WiseTech Global</strong>
					<a target="_blank" href="https://www.wisetechglobal.com/legal#privacypolicy"><u>Privacy</u></a>
					<a target="_blank" href="https://www.wisetechglobal.com/legal#termsofuse"><u>Terms</u></a>
				</div>
			</form>
		</div>
		<div id="login-supply-chain-image-container">
			<img id="supply-chain-image" alt="Enabling and empowering the world's supply chains" src="../Images/supply-chain-without-whitespace.svg"/>			
		</div>
	</div>
	<script type="text/javascript" src="../Scripts/buttontoggler.js"></script>
	<script type="text/javascript" src="../Scripts/passwordChangeMouseRightClickPaste.js"></script>
	<script>
		setupToggleButtonAvailabilityEventListener(document.getElementById('Update'), [document.getElementById('NewPassword'), document.getElementById('NewPasswordConfirm')]);
	</script>
</body>
</html>
