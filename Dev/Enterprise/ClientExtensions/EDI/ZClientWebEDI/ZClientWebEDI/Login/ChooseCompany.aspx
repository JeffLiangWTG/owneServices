<%@ Page language="c#" Codebehind="ChooseCompany.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.ZClientWebCargoWiseEDI.ChooseCompany" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<!DOCTYPE html>
<html class="login" xmlns="http://www.w3.org/1999/xhtml">
<HEAD>
	<title>CargoWise | My Account > Home > Choose Company</title>
	<meta name="viewport" content="width=device-width, initial-scale=1.0" />
	<script type="text/javascript">
		function ResizeParentIFrame()
		{
			if (window.parent.document.getElementById('contentFrame'))
			{
				window.parent.document.getElementById('contentFrame').height = document.body.scrollHeight;
			}
		}
	</script>
</HEAD>
<body>
	<!-- Google Tag Manager -->
	<noscript><iframe src="//www.googletagmanager.com/ns.html?id=GTM-PWS25L"
	height="0" width="0" style="display:none;visibility:hidden"></iframe></noscript>
	<script>(function (w, d, s, l, i) {
	w[l] = w[l] || []; w[l].push({
	'gtm.start':
	new Date().getTime(), event: 'gtm.js'
	}); var f = d.getElementsByTagName(s)[0],
	j = d.createElement(s), dl = l != 'dataLayer' ? '&l=' + l : ''; j.async = true; j.src =
	'//www.googletagmanager.com/gtm.js?id=' + i + dl; f.parentNode.insertBefore(j, f);
	})(window, document, 'script', 'dataLayer', 'GTM-PWS25L');</script>
	<!-- End Google Tag Manager -->

	<script type="text/javascript" src="../Scripts/myaccountRememberMe.js"></script>	
	<script>
		if (top.location != self.location) {
			top.location = self.location.href;
		}

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
				const companyCode = radios[r].closest("span").getAttribute("orgCode");
				radios[r].addEventListener("change", updateContactListRowSelected);
				radios[r].closest("tr").addEventListener("click", (function (radio) {
					return function () {
						var radios = document.querySelectorAll("#login-contact-lists input[type=radio]");
						for (var r = 0; r < radios.length; r++) {
							radios[r].checked = false;
						}

						saveCompanyCode(radio.closest("span").getAttribute("orgCode")); 
						radio.checked = true;
						updateContactListRowSelected();
					};
				})(radios[r]));

				if (companyCode && companyCode === getCompanyCode()) {
					radios[r].closest("tr").click();
				}
			}
			updateContactListRowSelected();
		});
	</script>
	<div id="login-whole-page">
		<div id="login-page-left-div">
			<form id="LoginForm" runat="server">
				<!-- Theme Container -->
				<div id="login" runat="server">
					<div id="login-content">
						<div id="login-company-logo-container">
							<img id="login-company-logo" alt="CargoWise" src="../Images/blue-cargowise-logo.svg" />
						</div>
						<h1>Login to My Account</h1>
						<div id="contact-lists" class="choose-company-table">
							<div id="login-contact-lists">
								<div id="LoginContactsDiv" class="table-wrapper" runat="server">

									<edi:ZTextLabel id="Message" runat="server" CssClass="ErrorMessage" />

									<p id="select-org-label">
										Select your organization
									</p>
									<table>
										<tbody>
										<edi:zrepeater id="LoginContactRepeater" runat="server" bindto="LoginContacts">
											<HeaderTemplate>
											</HeaderTemplate>
											<ItemTemplate>
												<tr>
													<td>
														<div id="blue-radio-button"></div>
														<edi:ZRadioButton id="Checked" Enabled="true" runat="server" orgCode='<%# Eval("OrganisationCode") %>' GroupName="LoginCheckBoxes"></edi:ZRadioButton>
													</td>
													<td>
														<edi:ZTextLabel id="CompanyCode" runat="server" value='<%# Eval("OrganisationCode") %>'><%# Eval("OrganisationCode") %> - <%# Eval("WorkingAddressCompanyName") %></edi:ZTextLabel>
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
						</div>
						<div id="login-button-panel-div">
							<asp:HyperLink ID="LoginLink1" runat="server" font-size="X-Large" NavigateUrl="Login.aspx" CssClass="link-button back-button">Back</asp:HyperLink>
							<asp:Button id="SignInButton" runat="server" Text="Login" onclick="SignInButton_Click"></asp:Button>
						</div>
					</div>
				</div>
				<div id="login-footer">
					<strong>&copy; <edi:ZTextLabel id="CopyrightYear" runat="server"></edi:ZTextLabel> WiseTech Global</strong>
					<a target="_blank" href="https://www.wisetechglobal.com/legal#privacypolicy" style="text-decoration-line: underline">Privacy</a>
					<a target="_blank" href="https://www.wisetechglobal.com/legal#termsofuse" style="text-decoration-line: underline">Terms</a>
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
