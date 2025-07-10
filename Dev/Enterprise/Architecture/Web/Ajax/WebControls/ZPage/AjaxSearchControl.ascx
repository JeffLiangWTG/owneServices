<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Control Language="c#" AutoEventWireup="false" Codebehind="AjaxSearchControl.ascx.cs" Inherits="Enterprise.ZArchitecture.Web.GUI.WebControls.AjaxSearchControl" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<asp:UpdatePanel id="up" runat="server">
    <ContentTemplate>
        <div id="FilterControl" runat="server" class="ContentSection"/>
    </ContentTemplate>
</asp:UpdatePanel>
<div align="left" class="ContentSection">
    <asp:placeholder id = "Buttons" runat = "server"/><edi:ZGridLayoutControl id="GridLayoutControl" runat="server" RenderContentsOnly="true"/>	
</div>
<div runat="server" id="ResultsGridDiv" class="ContentSection">
	<edi:ZDataGrid id="SearchResultsDataGrid" runat="server"/>
</div>
