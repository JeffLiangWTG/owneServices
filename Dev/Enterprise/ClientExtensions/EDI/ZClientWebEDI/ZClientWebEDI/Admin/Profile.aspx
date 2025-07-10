<%@ Page Title="" Language="C#" MasterPageFile="~/CargoWiseIFrame.Master" AutoEventWireup="True" CodeBehind="Profile.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.Profile" EnableEventValidation="false" %>
<%@ Register TagPrefix="edi" Tagname="UserAccountRelationship" src="UserAccountRelationshipControl.ascx" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>

<asp:Content ID="Header" ContentPlaceHolderID="Header" runat="server">
	<link type="text/css" rel="stylesheet" href="../BaseStyle.css" />
	<link type="text/css" rel="stylesheet" href="../StyleSheets/profile.css" />

	<script type="text/javascript" src="../Scripts/jquery-3.6.0.min.js"></script>
	<script type="text/javascript">
		jQuery.noConflict();

		jQuery(function () {
			jQuery("#SaveEmailButton").click(function (e) {
				var email = jQuery("#PersonalEmailTextBox").val();
				if (email == "") {
					alert("The personal email address cannot be empty.");
					e.preventDefault();
					return;
				}

				if (email.length > 254) {
					alert("The length of personal email address exceeds the max length 254.");
					e.preventDefault();
					return;
				}

				var originalPersonalEmail = jQuery("#RegisteredPersonalEmail").val();
				if (originalPersonalEmail == email) {
					alert("You have already registered this personal email.");
					e.preventDefault();
					return;
				}

				var emailPattern = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
				if (!emailPattern.test(email)) {
					alert("The personal email address is invalid.");
					e.preventDefault();
				}
			});
		});
	</script>
</asp:Content>

<asp:Content ID="Content" ContentPlaceHolderID="Content" runat="server">
	<div class="wrapper">
		<p id="Breadcrumb" runat="server">
			<a href="/index.html" class="pagetrail" target="_parent">Home</a>
			<img src="../images/arrow2.gif" width=14 height=10 align="absmiddle"> 
			<a href="../Default.aspx" class="pagetrail">My Account</a>
			<img src="../images/arrow2.gif" width=14 height=10 align="absmiddle">
		</p>

		<h1>Account Information</h1>
		<hr size="2" />

		<br />
		<div ID="TabContainer" class="CiTab" runat="server">
			<asp:Button ID="ButtonPersonalEmail" Enabled="false" CssClass="CiTabButton" runat="server" OnClick="SwitchPage_Click" Text="Contact Information" />
			<asp:Button ID="ButtonUserAccounts" CssClass="CiTabButton" runat="server" OnClick="SwitchPage_Click" Text="Related Accounts" />
		</div>

		<asp:Panel ID="TabPersonalEmail" ClientIDMode="Static" CssClass="CiTabContent" Visible="True" runat="server">
			<div class="ProfileContent">
				<table>
					<tr>
						<td class="BodyText rightAlign" style="width: 150px;">
							Full Name:
						</td>
						<td width="250" class="BoldBodyText">
							<edi:ZTextLabel runat="server" ID="UserNameLabel"></edi:ZTextLabel>
						</td>
					</tr>
					<tr>
						<td class="BodyText rightAlign">
							Primary Workplace:
						</td>
						<td class="BoldBodyText">
							<edi:ZTextLabel runat="server" ID="CompanyNameLabel">></edi:ZTextLabel>
						</td>
					</tr>
					<tr>
						<td class="BodyText rightAlign">
							Work Email:
						</td>
						<td class="BoldBodyText">
							<edi:ZTextLabel runat="server" ID="EmailLabel" ClientIDMode="Static"></edi:ZTextLabel>
						</td>
					</tr>
					<tr>
						<td class="BodyText rightAlign">
							Personal Recovery Email:
						</td>
						<td class="BoldBodyText">
							<edi:ZTextLabel runat="server" ID="PersonalEmailLabel"></edi:ZTextLabel>
							<asp:HiddenField runat="server" ID="RegisteredPersonalEmail" ClientIDMode="Static"/>
							<edi:ZTextBox runat="server" ID="PersonalEmailTextBox" ClientIDMode="Static" class="emailTextBox"/>
						</td>
					</tr>
					<tr>
						<td colspan="2">
							<edi:ZTextLabel runat="server" ID="MessageLabel" class="ErrorMessage" />
						</td>
					</tr>
					<tr>
						<td></td>
						<td>
							<edi:ZButton ID="RegisterPersonalEmailButton" runat="server" ClientIDMode="Static" Text="Register Personal Email" CssClass="ProfileButton" OnClick="RegisterPersonalEmailButton_OnClick" />
							<edi:ZButton ID="ChangePersonalEmailButton" runat="server" Text="Change Personal Email" CssClass="ProfileButton" OnClick="ChangePersonalEmailButton_OnClick" />
							<edi:ZButton ID="SaveEmailButton" runat="server" Text="Save Email" CssClass="ProfileButton" ClientIDMode="Static" OnClick="SaveEmailButton_OnClick" />
						</td>
					</tr>
				</table>
			</div>
			<div class="footer">
				<span id="footer" runat="server" ClientIDMode="Static"></span>
			</div>
		</asp:Panel>
		<asp:Panel ID="TabUserAccounts" CssClass="CiTabContent" Visible="False" runat="server">
			<edi:UserAccountRelationship id="UserAccountRelationship" runat="server" />
		</asp:Panel>

		<asp:Panel ID="ConfirmationDiv" runat="server" CssClass="CiModalOff">
			<div class="CiModalContent">
				<p>This record has been modified.</p>
				<p>Would you like to save the changes?</p>
				<br/>
				<div align="center">
					<edi:ZButton ID="ConfirmationYes" runat="server" Text="Yes" CssClass="ProfileButton" OnClick="ConfirmationYes_Click" />
					<edi:ZButton ID="ConfirmationNo" runat="server" Text="No" CssClass="ProfileButton" OnClick="ConfirmationNo_Click" />
					<edi:ZButton ID="ConfirmationCancel" runat="server" Text="Cancel" CssClass="ProfileButton" OnClick="ConfirmationCancel_Click" />
				</div>
			</div>
		</asp:Panel>
	</div>
	<br />
	<script type="text/javascript" src="../Scripts/buttontoggler.js"></script>
	<script>
		setupToggleButtonAvailabilityEventListener(document.getElementById('SaveEmailButton'), [document.getElementById('PersonalEmailTextBox')]);
	</script>
</asp:Content>
