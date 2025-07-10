<%@ Page Title="" Language="C#" MasterPageFile="~/CargoWiseIFrame.Master" AutoEventWireup="True" CodeBehind="WebSecurity.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.WebSecurity" EnableEventValidation="false" %>

<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>

<asp:Content ID="Header" ContentPlaceHolderID="Header" runat="server">
	<link type="text/css" rel="stylesheet" href="../BaseStyle.css" />
	<link type="text/css" rel="stylesheet" href="../StyleSheets/websecurity.css" />

	<script type="text/javascript">

		var WsIsSelectingAllContacts = false;

		function WsSelectAllContacts(chkBox) {

			WsIsSelectingAllContacts = true;
			xState = chkBox.checked;

			try {
				elm = chkBox.form.elements;
				for (i = 0; i < elm.length; i++) {
					if (elm[i].id.includes('SearchResultsDataGrid') && elm[i].type == "checkbox" && elm[i].id != chkBox.id && elm[i].checked != xState) {
						elm[i].click();
					}
				}
			}
			finally {
				WsIsSelectingAllContacts = false;
				WsClickContactChkBox(chkBox);
			}
		}
		function WsClickContactChkBox(chkBox) {
			if (!WsIsSelectingAllContacts) {

				var selectedCount = 0;
				var firstChkFound = false;
				elm = chkBox.form.elements;
				for (i = 0; i < elm.length; i++) {
					if (elm[i].id.includes('SearchResultsDataGrid') && elm[i].type == "checkbox") {
						if (!firstChkFound) {
							firstChkFound = true;
						}
						else if (elm[i].checked) {
							selectedCount++;
						}
					}
				}

				var grid = document.getElementById("WsContactSecurityGrid");
				var tb = document.getElementById("WsContactMultiSelectionCaption");

				if (selectedCount == 0) {
					tb.style.display = "none";
					if (grid) grid.style.display = "none";
				}
				else if (selectedCount > 1) {
					tb.style.display = "block";
					if (grid) grid.style.display = "none";
					tb.innerHTML = selectedCount.toString() + " contacts selected.";
				}
				else {
					tb.innerHTML = "";
					tb.style.display = "block";
					if (grid) grid.style.display = "none";
					__doPostBack('ContactSecurityGridUpdatePanel', null);
				}
			}
		}

		function WsContactCellClick(td) {
			td.parentNode.firstElementChild.firstElementChild.firstElementChild.click();
		}

		function WsHighlightRow(chkBox) {
			chkBox.parentNode.parentNode.parentNode.className = chkBox.checked ? 'WsGridSelected' : '';
		}

		function WsClickButton(btn, msg) {
			btn.disabled = "disabled";
			btn.value = msg;
			__doPostBack(btn.getAttribute("name"), '');
			return false;
		}

		function WsClickBulkChkBox(chkBox) {
			WsHighlightRow(chkBox);
			chkBox.parentNode.parentNode.nextElementSibling.firstElementChild.firstElementChild.checked = false;
		}
	</script>
</asp:Content>

<asp:Content ID="Content" ContentPlaceHolderID="Content" runat="server">
	<div class="WsWrapper">
		<p id="Breadcrumb" runat="server">
			<a href="../../index.html" class="pagetrail" target="_parent">Home</a>
			<img src="../images/arrow2.gif" width="14" height="10" align="absmiddle" />
			<a href="../Default.aspx" class="pagetrail">My Account</a>
			<img src="../images/arrow2.gif" width="14" height="10" align="absmiddle" />
		</p>

		<h1>Web Security</h1>
		<hr size="1" />

		<br />

		<div class="WsTab">
			<asp:Button ID="ButtonContacts" Enabled="false" CssClass="WsTabButton" runat="server" OnClick="SwitchPage_Click" Text="Contact Security" />
			<asp:Button ID="ButtonSecurityDefaults" CssClass="WsTabButton" runat="server" OnClick="SwitchPage_Click" Text="Default Security" />
		</div>

		<asp:Panel ID="TabSecurityDefaults" runat="server" Visible="false" CssClass="WsTabContent">
			<div class="ContentSection">
				<table>
					<tr>
						<td>
							<edi:ZTextLabel ID="SecurityProfileFilterLabel" runat="server">Profile:</edi:ZTextLabel>
							<edi:ZDropDownList ID="SecurityProfileFilter" runat="server" BindTo="SecurityProfile" BindToList="SecurityProfileList"
								AutoPostBack="true" Width="250px" OnSelectedIndexChanged="SecurityProfileFilter_SelectedIndexChanged">
							</edi:ZDropDownList>
						</td>
					</tr>
					<tr>
						<td>
							<edi:ZGrid ID="SecurityGrid" runat="server" CssClass="WsGrid" BindTo="SecurityRights" DisableCollapsing="true"
								AllowAdd="false" AllowEdit="true" AllowDelete="false" AutoGenerateColumns="false"
								AllowPaging="false"
								DataKeyField="PK" HideButtonsToMenu="true" ShowMenu="false" ShowExportToExcelButton="false" ShowCustomizeColumnsButton="false" ShouldShowControl="false"
								OnItemDataBound="SecurityGrid_ItemDataBound">
								<HeaderStyle CssClass="WsGridHeader" />
								<ItemStyle CssClass="WsGridItem" />
							</edi:ZGrid>
						</td>
					</tr>
				</table>
				<br />
				*  These security preferences are applied to all new staff members only.
				<br />
				* Users can also apply this default profile to the selected contacts through the 'Contact Security' page.
			</div>
		</asp:Panel>

		<asp:Panel ID="TabContacts" runat="server" Visible="true" CssClass="WsTabContent">
			<div class="ContentSection">
				<div id="SearchControlHolder" runat="server"></div>
			</div>
		</asp:Panel>

		<asp:UpdatePanel ID="BulkUpdatePanel" runat="server" UpdateMode="Always">
			<ContentTemplate>
				<asp:Panel ID="BulkUpdateDiv" runat="server" CssClass="WsModalOff">
					<div class="WsModalContent">
						<edi:ZTextLabel runat="server">Profile:</edi:ZTextLabel>
						<edi:ZDropDownList ID="BulkUpdateProfile" runat="server" BindTo="BulkUpdateProfile.ProfileName" BindToList="SecurityProfileList"
							AutoPostBack="true" Width="250px" OnSelectedIndexChanged="BulkUpdateProfile_SelectedIndexChanged">
						</edi:ZDropDownList>
						<br />
						<edi:ZGrid ID="BulkUpdateSecurityGrid" runat="server" CssClass="WsGrid WsBulkUpdateGrid" BindTo="BulkUpdateProfile.Items" DisableCollapsing="true"
							AllowAdd="false" AllowEdit="true" AllowDelete="false" AutoGenerateColumns="false"
							AllowPaging="false"
							DataKeyField="PK" HideButtonsToMenu="true" ShowMenu="false" ShowExportToExcelButton="false" ShowCustomizeColumnsButton="false" ShouldShowControl="false"
							OnItemDataBound="BulkUpdateSecurityGrid_ItemDataBound">
							<HeaderStyle CssClass="WsGridHeader" />
							<ItemStyle CssClass="WsGridItem" />
						</edi:ZGrid>
						<br />
                        <edi:ZRadioButtonList ID="UpdateModeRadioButton" runat="server" AutoPostBack="true" BindTo="BulkUpdateModeCollection"/>
						<div align="center" style="margin-top:10px;">
							<edi:ZButton ID="BulkUpdateCancel" runat="server" Text="Cancel" CssClass="WsButton" OnClick="BulkUpdateCancel_Click" />
							<edi:ZButton ID="BulkUpdateOK" runat="server" Text="Save changes" CssClass="WsButton" OnClientClick="WsClickButton(this, 'Saving...');" OnClick="BulkUpdateOK_Click" />
						</div>
					</div>
				</asp:Panel>
				<asp:Panel ID="ErrorConfirmationDiv" runat="server" CssClass="WsModalOff">
					<div class="WsModalContent">
						<p>While you have been working with this page, another user has made changes.</p>
						<p>The system cannot automatically merge your changes because there are conflicts with critical fields.</p>
						<p>Please click OK to reset the page.</p>
						<br/>
						<div align="center">
							<edi:ZButton ID="ErrorConfirmationOK" runat="server" Text="OK" CssClass="WsButton" OnClientClick="WsClickButton(this, 'Resetting...');" OnClick="ResetButton_Click" />
						</div>
					</div>
				</asp:Panel>
			</ContentTemplate>
		</asp:UpdatePanel>

		<asp:Panel ID="ConfirmationDiv" runat="server" CssClass="WsModalOff">
			<div class="WsModalContent">
				<p>This record has been modified.</p>
				<p>Would you like to save the changes?</p>
				<br/>
				<div align="center">
					<edi:ZButton ID="ConfirmationYes" runat="server" Text="Yes" CssClass="WsButton" OnClientClick="WsClickButton(this, 'Saving...');" OnClick="ConfirmationYes_Click" />
					<edi:ZButton ID="ConfirmationNo" runat="server" Text="No" CssClass="WsButton" OnClientClick="WsClickButton(this, 'Resetting...');" OnClick="ConfirmationNo_Click" />
					<edi:ZButton ID="ConfirmationCancel" runat="server" Text="Cancel" CssClass="WsButton" OnClick="ConfirmationCancel_Click" />
				</div>
			</div>
		</asp:Panel>

		<br />

		<asp:UpdatePanel ID="ButtonsUpdatePanel" runat="server">
			<ContentTemplate>
				<div align="center">
					<edi:ZButton ID="ResetButton" runat="server" Text="Reset" CssClass="WsButton" OnClientClick="WsClickButton(this, 'Resetting...');" OnClick="ResetButton_Click" />
					<edi:ZButton ID="SaveButton" runat="server" Text="Save" CssClass="WsButton" OnClientClick="WsClickButton(this, 'Saving...');" OnClick="SaveButton_Click" />
				</div>
			</ContentTemplate>
		</asp:UpdatePanel>
	</div>
	<br />
</asp:Content>