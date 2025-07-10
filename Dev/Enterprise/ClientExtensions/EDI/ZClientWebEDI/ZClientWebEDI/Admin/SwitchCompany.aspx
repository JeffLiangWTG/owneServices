<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SwitchCompany.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.SwitchCompany" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
	<title>Switch Company</title>
</head>
<body id="DefaultBody" runat="server">
	<script type="text/javascript" src="../Scripts/myaccountRememberMe.js"></script>
	<script>
		function handleClientClick(button, actionType) {
			saveCompanyCode(button.innerText);
			return true;
		}
	</script>
	<form id="form1" runat="server">
		<div id="OuterContentPane" runat="server">
			<div class="wrapper">
				<h1>Switch Company</h1>
				<hr size="2" />

				<div class="ContentSection">
					You are currently logged in with the <edi:ZTextLabel ID="CurrentCompanyLabel" runat="server" CssClass="DetailsItem" BindTo="SiteUser.LoggedInOrgContact.WorkingAddressCompanyName"></edi:ZTextLabel>  company context.<br>
					<br>
					Select a company below to switch context.
					<p></p>
					<div id="contact-lists">
						<edi:zdatagrid id="RelatedContactsDataGrid" runat="server" CssClass="SwitchCompanyTable" AutoGenerateColumns="False"
									BindTo="SiteUser.ContactRelatedAccounts">
							<PagerStyle Mode="NumericPages"></PagerStyle>
							<ItemStyle CssClass="DetailsCell"></ItemStyle>
							<HeaderStyle CssClass="SwitchCompanyHeader"></HeaderStyle>
						</edi:zdatagrid>
					</div>
				</div>
				<p></p>
			</div>
		</div>
	</form>
</body>
</html>
