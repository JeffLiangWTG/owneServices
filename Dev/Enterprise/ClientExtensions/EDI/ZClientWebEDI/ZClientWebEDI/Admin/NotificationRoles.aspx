<%@ Page Title="" Language="C#" MasterPageFile="~/CargoWiseIFrame.Master" AutoEventWireup="True" CodeBehind="NotificationRoles.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.NotificationRoles" EnableEventValidation="false" %>

<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>

<asp:Content ID="Header" ContentPlaceHolderID="Header" runat="server">
	<link type="text/css" rel="stylesheet" href="../BaseStyle.css" />
	<link type="text/css" rel="stylesheet" href="../StyleSheets/notificationroles.css" />

	<script type="text/javascript">

        var NrIsSelectingAllContacts = false;

        function NrSelectAllContacts(chkBox) {

            NrIsSelectingAllContacts = true;
            xState = chkBox.checked;

            try {
                elm = chkBox.form.elements;
                for (i = 0; i < elm.length; i++) {
                    if (elm[i].id.includes('SearchResultsDataGrid') && elm[i].id.includes('_chkbx_') && elm[i].type == "checkbox" && elm[i].id != chkBox.id && elm[i].checked != xState) {
                        elm[i].click();
                    }
                }
            }
            finally {
                NrIsSelectingAllContacts = false;
            }
        }

		function NrContactCellClick(td) {
			td.parentNode.firstElementChild.firstElementChild.firstElementChild.click();
		}

		function NrHighlightRow(chkBox) {
			chkBox.parentNode.parentNode.parentNode.className = chkBox.checked ? 'NrGridSelected' : '';
		}

		function NrClickButton(btn, msg) {
			btn.disabled = "disabled";
			btn.value = msg;
			__doPostBack(btn.getAttribute("name"), '');
			return false;
		}

		function NrClickBulkChkBox(chkBox) {
			NrHighlightRow(chkBox);
			chkBox.parentNode.parentNode.nextElementSibling.firstElementChild.firstElementChild.checked = false;
		}
	</script>
</asp:Content>

<asp:Content ID="Content" ContentPlaceHolderID="Content" runat="server">
	<div class="NrWrapper">
		<p id="Breadcrumb" runat="server">
			<a href="../../index.html" class="pagetrail" target="_parent">Home</a>
			<img src="../images/arrow2.gif" width="14" height="10" align="absmiddle" />
			<a href="../Default.aspx" class="pagetrail">My Account</a>
			<img src="../images/arrow2.gif" width="14" height="10" align="absmiddle" />
		</p>

		<h1>Notification Roles</h1>
		<hr size="1" />

		<br />

		<asp:Panel ID="TabContacts" runat="server" Visible="true" CssClass="NrTabContent">
			<div class="ContentSection">
				<div id="SearchControlHolder" runat="server"></div>
			</div>
		</asp:Panel>

		<asp:UpdatePanel ID="BulkUpdatePanel" runat="server" UpdateMode="Always">
			<ContentTemplate>
				<asp:Panel ID="BulkUpdateDiv" runat="server" CssClass="NrModalOff">
					<div class="NrModalContent">
						<edi:ZGrid ID="BulkUpdateGrid" runat="server" CssClass="NrGrid NrBulkUpdateGrid" BindTo="AdminGroupItems" DisableCollapsing="true"
							AllowAdd="false" AllowEdit="true" AllowDelete="false" AutoGenerateColumns="false"
							AllowPaging="false"
							DataKeyField="PK" HideButtonsToMenu="true" ShowMenu="false" ShowExportToExcelButton="false" ShowCustomizeColumnsButton="false" ShouldShowControl="false"
							OnItemDataBound="BulkUpdateGrid_ItemDataBound">
							<HeaderStyle CssClass="NrGridHeader" />
							<ItemStyle CssClass="NrGridItem" />
						</edi:ZGrid>
						<br />
                        <edi:ZRadioButtonList ID="UpdateModeRadioButton" runat="server" AutoPostBack="false" BindTo="BulkUpdateModeCollection"/>
						<div align="center" style="margin-top:10px;">
							<edi:ZButton ID="BulkUpdateCancel" runat="server" Text="Cancel" CssClass="NrButton" OnClick="BulkUpdateCancel_Click" />
							<edi:ZButton ID="BulkUpdateOK" runat="server" Text="Save changes" CssClass="NrButton" OnClientClick="NrClickButton(this, 'Saving...');" OnClick="BulkUpdateOK_Click" />
						</div>
					</div>
				</asp:Panel>
			</ContentTemplate>
		</asp:UpdatePanel>

		<br />

		<asp:UpdatePanel ID="ButtonsUpdatePanel" runat="server">
			<ContentTemplate>
				<div align="center">
					<edi:ZButton ID="ResetButton" runat="server" Text="Reset" CssClass="NrButton" OnClientClick="NrClickButton(this, 'Resetting...');" OnClick="ResetButton_Click" />
					<edi:ZButton ID="SaveButton" runat="server" Text="Save" CssClass="NrButton" OnClientClick="NrClickButton(this, 'Saving...');" OnClick="SaveButton_Click" />
				</div>
			</ContentTemplate>
		</asp:UpdatePanel>
	</div>
	<br />
</asp:Content>