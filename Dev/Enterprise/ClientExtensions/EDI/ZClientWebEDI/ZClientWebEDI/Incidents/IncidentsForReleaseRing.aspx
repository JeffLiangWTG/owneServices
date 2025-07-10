<%@ Page Language="C#" MasterPageFile="~/CargoWiseIFrame.Master" AutoEventWireup="true" CodeBehind="IncidentsForReleaseRing.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.IncidentsForReleaseRing" %>

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
		<p class="gray" id="Breadcrumb" runat="server">
			<a href="../../index.html" class="pagetrail" target="_parent">Home</a>
			<img alt="arrow" src="../images/arrow2.gif" width="14" height="10" align="absmiddle"> 
			<a href="../Default.aspx" class="pagetrail">My Account</a>
			<img src="../images/arrow2.gif" width="14" height="10" align="absmiddle">
		</p> 
        <!-- title -->    
        <h1>Customer Service Incidents
        
        </h1>
        <hr size="2"/>
        <!-- /title --> 
        <br /><br />
       
       <h2> 
       <asp:Label id="LabelHeader" runat="server" Text=""></asp:Label><br/>
       </h2>
       
        <!-- #BeginEditable "Body" -->
	    <div id="OuterContentPane" runat="server">
		    <div id="SearchControlHolder" runat="server"></div>
	    </div>
		<!-- #EndEditable -->
		<!--/CMS-TXT-->
    </div>
</asp:Content>