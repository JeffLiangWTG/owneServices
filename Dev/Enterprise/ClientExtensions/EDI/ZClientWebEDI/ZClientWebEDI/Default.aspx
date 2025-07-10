<%@ Page Language="C#" MasterPageFile="~/CargoWiseIFrame.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.Default" Title="Untitled Page" %>
<asp:Content ID="Content1" ContentPlaceHolderID="Header" runat="server" />
<asp:Content ID="Content2" ContentPlaceHolderID="Content" runat="server">
    <div class="wrapper">
		<p id="Breadcrumb" runat="server">
			<a href="../index.html" target="_top" class="pagetrail">Home</a> <img src="images/arrow2.gif" width="14" height="10" align="absmiddle" />
		</p>
		<h1>My Account</h1>
		<hr/>

        <div class="homeButtons clearfix">
            <asp:HyperLink id="eLearningLink" runat="server" CssClass="eLearningLink" NavigateUrl="http://www.cargowise.com/eLearning.aspx" ToolTip="ediEnterprise Wise Learning" target="_parent"><span></span>ediENTERPRISE<br />WISE LEARNING</asp:HyperLink>
            <asp:HyperLink id="sapphireELearningLink" runat="server" CssClass="sapphireELearningLink" NavigateUrl="http://www.cargowise.com/TLXeLearning.aspx" ToolTip="Sapphire Wise Learning" target="_parent"><span></span>Sapphire<br />Wise Learning</asp:HyperLink>
            <asp:HyperLink id="odysseyELearningLink" runat="server" CssClass="odysseyELearningLink" NavigateUrl="http://www.cargowise.com/ODYeLearning.aspx" ToolTip="Odyssey Wise Learning" target="_parent"><span></span>Odyssey<br />Wise Learning</asp:HyperLink>
            <asp:HyperLink id="UpdatenotesLink" runat="server" CssClass="updatenotesLink" NavigateUrl="ReleaseNotes/ReleaseNotes.aspx" ToolTip="Update Notes"><span></span>Update Notes</asp:HyperLink>
            <asp:HyperLink ID="DownloadsLink" runat="server" CssClass="downloadsLink" NavigateUrl="my-account/downloads.aspx" ToolTip="Server Downloads"><span></span>Server Downloads</asp:HyperLink>
            <asp:HyperLink id="AccreditationLink" runat="server" CssClass="learningcenterLink" ToolTip="Learning Archive" target="_parent"><span></span>Learning Archive</asp:HyperLink>
            <asp:HyperLink ID="ReportsLink" runat="server" CssClass="reportsLink" NavigateUrl="Reports/Reports.aspx" ToolTip="Reports"><span></span>Reports</asp:HyperLink>
			<asp:HyperLink ID="WebSecurityLink" runat="server" CssClass="webSecurityLink" NavigateUrl="Admin/WebSecurity.aspx" ToolTip="Web Security"><span></span>Web Security</asp:HyperLink>
            <asp:HyperLink ID="NotificationRolesLink" runat="server" CssClass="notificationRolesLink" NavigateUrl="Admin/NotificationRoles.aspx" ToolTip="Notification Roles"><span></span>Notification Roles</asp:HyperLink>
            <asp:HyperLink id="ChangepasswordLink" runat="server" CssClass="changepasswordLink" NavigateUrl="Admin/ChangePassword.aspx" ToolTip="Change Password"><span></span>Change Password</asp:HyperLink>
        </div>
    </div>
</asp:Content>
