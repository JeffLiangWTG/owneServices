<%@ Page Language="C#" MasterPageFile="~/CargoWiseIFrame.Master" AutoEventWireup="True" CodeBehind="Reports.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.Reports" Title="Reports" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>

<asp:Content ID="Content2" ContentPlaceHolderID="Header" runat="server">
    <script type="text/javascript">
	    function ShowMenu(menu, img, page){
		    if(document.all(menu).style.display == 'none'){
			    document.all(menu).style.display = '';
			    document.images[img].src = '1.gif';
			    parent.frames['mainFrame'].location = page
		    }
		    else{
			    document.all(menu).style.display = 'none';
			    document.images[img].src = '1.gif';
		    }
	    }
    </script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="Content" runat="server">
     <div class="wrapper">
		<div class="Header" id="Breadcrumb" runat="server">
			<a href="../../index.html" class="pagetrail" target="_parent">Home</a>
			<img src="../images/arrow2.gif" width=14 height=10 align="absmiddle"> 
			<a href="../Default.aspx" class="pagetrail">My Account</a>
			<img src="../images/arrow2.gif" width=14 height=10 align="absmiddle">
		</div>
        <h1>Reports</h1>
        <hr size="2" />
        <br /><br />        

		    <div id="AuthorisedContent" runat="server">
			    <edi:ZGuidDropDownList ID="ReportsDropDownList"
								       runat="server" BindTo="ReportPK"
								       BindToList="WebReports"
								       DataTextField="SU_MenuName" 
								       DataValueField="PK" 
								       AutoPostBack="True" 
								       EmptyItemText="Please select a Report" 
								       OnSelectedIndexChanged="ReportsDropDownList_SelectedIndexChanged"
								       ShowEmptyItem="True">
			    </edi:ZGuidDropDownList>
			    <br />
			    <div id="FilterControlHolder" runat="server"></div>
		    </div>
		    <div id="UnauthorisedDiv" runat="server">
			    <edi:ztextlabel id="UnauthorisedLabel" runat="server"></edi:ztextlabel>
		    </div>

    </div>
</asp:Content>
