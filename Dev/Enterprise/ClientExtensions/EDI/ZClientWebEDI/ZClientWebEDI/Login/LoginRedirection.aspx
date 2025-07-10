<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Page Language="c#" EnableViewStateMac="false" CodeBehind="LoginRedirection.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.ZClientWebCargoWiseEDI.LoginRedirection" %>
<!DOCTYPE html>
<html class="login" xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>CargoWise | My Account > Home > Login Redirection</title>
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
					<h1>User Account Login</h1>
					<p>
						User Account changes have occurred which affect the way you login into My Account.
						<br />
						The following user accounts are no longer accessible.
					</p>
					<div class="table-wrapper">
						<table>
							<thead>
							<tr>
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
							<edi:zrepeater id="DeactivatedContactsRepeater" runat="server" bindto="ContactsForDeactivation">
								<HeaderTemplate>
								</HeaderTemplate>
								<ItemTemplate>
									<tr>
										<td>
											<edi:ZTextLabel id="DeactivatedCompanyCode" runat="server" Text='<%# Eval("OrganisationCode") %>'></edi:ZTextLabel> 
										</td>
										<td>
											<edi:ZTextLabel id="DeactivatedCompanyName" runat="server" Text='<%# Eval("WorkingAddressCompanyName") %>'></edi:ZTextLabel> 
										</td>
										<td>
											<edi:ZTextLabel id="DeactivatedEmail" runat="server" Text='<%# Eval("OC_Email") %>'></edi:ZTextLabel> 
										</td>
										<td style="text-align: center">
											<edi:ZTextLabel id="DeactivatedPrimaryWorkplace" runat="server" Text='<%# Eval("OC_IsPrimaryContact") %>'></edi:ZTextLabel>
										</td>
									</tr>
								</ItemTemplate>
								<FooterTemplate>
								</FooterTemplate>
							</edi:zrepeater>
							</tbody>
						</table>
					</div>
					<div id="login-contact-lists">
						<p>
							But don't worry, you still have access to My Account using the following accounts.
							<br />
							Select an account you wish to login as then Continue.
						</p>
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
											Linked System(s)
										</th>
										<th>
											Primary Workplace
										</th>
									</tr>
								</thead>
								<tbody>
								<edi:zrepeater id="LoginContactRepeater" runat="server" bindto="LoginContacts">
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
												<edi:ZTextLabel id="CompanyName" runat="server" Text='<%# Eval("OrganisationName") %>'></edi:ZTextLabel> 
											</td>
											<td>
												<edi:ZTextLabel id="Email" runat="server" Text='<%# Eval("Email") %>'></edi:ZTextLabel> 
											</td>
											<td>
												<edi:ZTextLabel id="LinkedSystems" runat="server" Text='<%# Eval("LinkedSystems") %>'></edi:ZTextLabel>
											</td>
											<td style="text-align: center">
												<edi:ZTextLabel id="PrimaryWorkplace" runat="server" Text='<%# Eval("PrimaryWorkplace") %>'></edi:ZTextLabel>
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
						<asp:button ID="ContinueButton" runat="server" text="Continue" onclick="ContinueButton_Click"></asp:button>
					</div>
					<br />
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
