<%@ Page Language="C#" MasterPageFile="~/CargoWiseIFrame.Master" AutoEventWireup="true" CodeBehind="Downloads.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.My_Account.Downloads" %>

<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350" %>
<asp:Content ID="Content2" ContentPlaceHolderID="Header" runat="server">
	<script type="text/javascript">
		function ShowMenu(menu, img, page) {
			if (document.all(menu).style.display == 'none') {
				document.all(menu).style.display = '';
				document.images[img].src = '1.gif';
				parent.frames['mainFrame'].location = page
			}
			else {
				document.all(menu).style.display = 'none';
				document.images[img].src = '1.gif';
			}
		}
	</script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="Content" runat="server">
	<div class="wrapper">

		<!-- content -->
		<table height="100%" cellspacing="0" cellpadding="0" width="700px" border="0">
			<tbody>
				<tr valign="top">
					<td bgcolor="white">
						<br />
						<p id="Breadcrumb" runat="server">
							<a class="pagetrail" href="http://www.cargowise.com/index.html">Home</a>
							<img height="10" src="../images/arrow2.gif" width="14" align="middle">
							<a class="pagetrail" href="../Default.aspx">My Account</a>
							<img height="10" src="../images/arrow2.gif" width="14" align="middle">
						</p>
						<h1>Server Downloads</h1>
						<hr style="height: 2px" />

						<!-- #BeginEditable "Body" -->
						<h2>Request Upgrades</h2>
						<p>
							You can request for an upgrade to be sent to your system.
               The list below shows the servers that belong to your organization. 
               The drop-down list contains the list of upgrades currently available.
						</p>
						<p>
							Click on 'Details' to view a list of all incidents that have been
               resolved in the selected version. 
               Click on 'Update Notes' to view a list of all update notes that have
               been published for the selected version. Click on 'Upgrade' to submit
               a request for the selected upgrade to be sent to your server.
						</p>

						<edi:ZTextLabel ID="Label2" font-name="Verdana" ForeColor="Green" Font-Size="10pt" runat="server" />

						<% 
							if (LicenceDatabases != null)
							{
						%>

						<asp:Repeater ID="Repeater1" runat="server" OnItemDataBound="R1_ItemDataBound">
							<HeaderTemplate>
								<table class="Resultstable" width="100%">
									<thead>
										<tr>
											<th>Server Code 
												<!--Uprgade Method-->
											</th>
											<th>Lic. Type</th>
											<th>Curr. Ring</th>
											<th colspan="2" width="80%">Current Version</th>
										</tr>
									</thead>
									<tr>
										<td colspan="5">
											<hr />
										</td>
									</tr>
							</HeaderTemplate>
							<ItemTemplate>
								<tr>
									<td>
										<edi:ZTextLabel ID="ServerCode" Text='<%# DataBinder.Eval(Container.DataItem, "LD_ServerCode") %>' runat="server" />
									</td>
									<td>
										<edi:ZTextLabel ID="LicenceType" Text='<%# DataBinder.Eval(Container.DataItem, "LD_LicenceType") %>' runat="server" />
									</td>
									<td>
										<edi:ZTextLabel ID="ReleaseRing" Text='<%# DataBinder.Eval(Container.DataItem, "LD_ReleaseRing") %>' runat="server" />
									</td>
									<td width="60%" colspan="2">
										<edi:ZTextLabel ID="CurrentVersion" Text='' runat="server" />
									</td>
								</tr>
								<tr>
									<td colspan="5" align="center">
										<asp:DropDownList ID="DropDownVersion" OnSelectedIndexChanged="cmbDropDownVersion_SelectedIndexChanged" runat="server" Width="87%" AutoPostBack="true"></asp:DropDownList>
									</td>
								</tr>
								<tr>
									<td colspan="5" align="center">
										<asp:HyperLink ID="LinkDetails" runat="server">Details</asp:HyperLink>&nbsp;
                                <asp:Button ID="RequestUpgrade" runat="server" CssClass="button" Text="Request Upgrade Package" Height="20px" Width="175px" />
									</td>
								</tr>
								<tr>
									<td colspan="5">
										<hr />
									</td>
								</tr>
							</ItemTemplate>
							<FooterTemplate>
								</table>
							</FooterTemplate>
						</asp:Repeater>

						<% } %>

						<div id="UpdateNotesLinksSection" runat="server">
							<h2>Update Notes</h2>
							<a class="pagetrail" href="https://wisetechacademy.com/explore/product-learning?product=cargowise&type=update+note">Find Update Notes for CargoWise on WiseTech Academy.</a>
						</div>

						<div id="StaticDownloadLinksSection" runat="server">
							<h2>Installation DVDs</h2>
							<p>
								The CargoWise Installation DVDs can be downloaded 
					from one of the following links. The ISO file is the actual DVD 
					image, which you can use to burn onto a DVD using standard DVD 
					burning software. Once the DVD is burnt, you can insert the DVD into 
					your computer to auto start the installation process.
							</p>
							<p>
								If you do not have a DVD burner, you can download the .zip file 
					and unzip the file with any zip compression tool. Then run 
					setup.exe to start the installation.
							</p>

							<b>CargoWise</b>
							<ul>
								<li><a id="CW1DvdIsoFileLink" runat="server" href="">CargoWise Installation - ISO</a></li>
								<li><a id="CW1DvdZipFileLink" runat="server" href="">CargoWise Installation - ZIP</a></li>
								<li><a id="CW1ExeFileLink" runat="server" href="">CargoWise Web Components - EXE</a></li>
							</ul>

							<b>BorderWise</b>
							<ul>
								<li><a href="https://myaccount-portal.cargowise.com/my-account/downloads/WiseTechMachineIdentificationService.msi">WiseTech Machine Identification Service</a></li>
							</ul>
						</div>

					</td>
				</tr>
			</tbody>
		</table>
		<!-- /content -->
	</div>
</asp:Content>
