<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls"
	Assembly="Enterprise.ZArchitecture.Web.GUI" %>

<%@ Page Language="c#" MasterPageFile="~/CargoWiseIFrame.Master" Codebehind="IncidentDetails.aspx.cs"
	AutoEventWireup="True" Inherits="Enterprise.ZClientWebCargoWiseEDI.IncidentDetails" %>

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
		<p id="Breadcrumb" runat="server">
			<a href="../../index.html" class="pagetrail" target="_parent">Home</a>
			<img src="../images/arrow2.gif" width=14 height=10 align="absmiddle">
			<a href="../Default.aspx" class="pagetrail>My Account</a>
			<img src="../images/arrow2.gif" width=14 height=10 align="absmiddle">
			<a href="Incidents.aspx" class="pagetrail">Customer Service</a>
			<img src="../images/arrow2.gif" width=14 height=10 align="absmiddle">
		</p>
		<!-- title -->
		<h1>Customer Service Incidents</h1>
		<hr size="2">
		<br /><br />
		<!-- /title -->
		<!-- #BeginEditable "Body" -->
		<div style="width: 100%; overflow: visible">
			<div id="IncidentContents" runat="server" class="ContentSection">
				<table class="ResultsTable" width="100%" border="0">
					<tr valign="top">
						<td colspan="2"><asp:Label ID="Label8" runat="server" CssClass="DetailsItem">Incident Num: </asp:Label><edi:ZTextLabel
							ID="Zdatetimelabel3" runat="server" BindTo="IM_IncidentNumber"></edi:ZTextLabel><br />
						</td>
					</tr>
					<tr>
						<td colspan="2"><asp:Label ID="Label2" runat="server" CssClass="DetailsItem">Client Organization: </asp:Label><edi:ZTextLabel
							ID="Ztextlabel1" runat="server" BindTo="Client.OH_FullName"></edi:ZTextLabel></td>
					</tr>
					<tr>
						<td colspan="2">&nbsp;</td>
					</tr>
					<tr valign="top">
						<td width="50%">
							<asp:Label ID="Label11" runat="server" CssClass="DetailsItem">Client Contact: </asp:Label><edi:ZTextLabel
								ID="Ztextlabel2" runat="server" BindTo="Contact.OC_ContactName"></edi:ZTextLabel><br />
						</td>
						<td width="50%">
							<asp:Label ID="DateAddedLabel" runat="server" CssClass="DetailsItem">Date Added: </asp:Label><edi:ZDateTimeLabel
								ID="DateAdded" runat="server" BindTo="IM_SystemCreateTimeUtc"></edi:ZDateTimeLabel><br />
						</td>
					</tr>
					<tr valign="top">
						<td width="50%"><asp:Label ID="Label9" runat="server" CssClass="DetailsItem">Product: </asp:Label><edi:ZCodeLookupLabel
								ID="Ztextlabel9" runat="server" BindTo="IM_Product" BindToList="Lookups.ProductList"
								DisplayStyle="DescriptionOnly"></edi:ZCodeLookupLabel><br />
						</td>
						<td width="50%">
							<asp:Label ID="Label7" runat="server" CssClass="DetailsItem">Date Closed: </asp:Label><edi:ZDateTimeLabel
								ID="Zdatetimelabel2" runat="server" BindTo="IM_CloseDate"></edi:ZDateTimeLabel><br />
						</td>
					</tr>
					<tr valign="top">
						<td width="50%">
							<asp:Label ID="Label5" runat="server" CssClass="DetailsItem">Status: </asp:Label><edi:ZTextLabel
								ID="Ztextlabel5" runat="server" BindTo="IM_StatusDescription"></edi:ZTextLabel>
						</td>
						<td width="50%">
							<asp:Label ID="Label6" runat="server" CssClass="DetailsItem">Disposition: </asp:Label><edi:ZTextLabel
								ID="Ztextlabel6" runat="server" BindTo="IM_ResolutionCodeDescription"></edi:ZTextLabel>
						</td>
					</tr>
					<tr valign="top">
						<td width="50%">
							<asp:Label ID="StageLabel" runat="server" CssClass="DetailsItem">Stage: </asp:Label><edi:ZTextLabel
								ID="Stage" runat="server" BindTo="Stage"></edi:ZTextLabel>
						</td>
						<td width="50%">&nbsp;</td>
					</tr>
					<tr>
						<td colspan="2">&nbsp;</td>
					</tr>
					<tr valign="top">
						<td colspan="2"><asp:Label ID="Label3" runat="server" CssClass="DetailsItem">Module: </asp:Label><edi:ZCodeLookupLabel
							ID="Ztextlabel3" runat="server" BindTo="IM_Module" BindToList="Lookups.ModuleList"
							DisplayStyle="DescriptionOnly"></edi:ZCodeLookupLabel></td>
					</tr>
					<tr>
						<td colspan="2"><asp:Label ID="Label4" runat="server" CssClass="DetailsItem">Criticality: </asp:Label><edi:ZTextLabel
							ID="PriorityDescription" runat="server" BindTo="PriorityDescription"></edi:ZTextLabel></td>
					</tr>
					<tr>
						<td colspan="2">
							<div class="ContentSection">
								<asp:Label ID="DescriptionLabel" runat="server" CssClass="DetailsItem">Description: </asp:Label><br />
								<edi:ZTextLabel ID="Description" runat="server" BindTo="IM_Description"></edi:ZTextLabel></div>
							<div class="ContentSection">
								<asp:Label ID="Label1" runat="server" CssClass="DetailsItem">Problem Details: </asp:Label><br />
								<edi:ZTextLabel ID="Ztextlabel7" runat="server" BindTo="DetailNoteText"></edi:ZTextLabel>
							</div>
							<div class="ContentSection">
								<asp:Label ID="ResolutionCommentLabel" runat="server" CssClass="DetailsItem">Resolution Comment: </asp:Label><br />
								<edi:ZTextLabel ID="ResolutionComment" runat="server" BindTo="ResolutionComment"></edi:ZTextLabel>
							</div>
						</td>
					</tr>
				</table>
			</div>
			<div id="NotFoundError" runat="server" class="ContentSection">
				<edi:ZTextLabel ID="IncidentNotFoundLabel" runat="server"></edi:ZTextLabel>
			</div>
			</div>
		<!-- #EndEditable -->
		<!--/CMS-TXT-->
	</div>
</asp:Content>
