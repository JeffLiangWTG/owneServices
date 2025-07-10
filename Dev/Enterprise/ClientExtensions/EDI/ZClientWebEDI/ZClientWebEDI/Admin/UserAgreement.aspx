<%@ Page Title="" Language="C#" MasterPageFile="~/CargoWiseIFrame.Master" AutoEventWireup="True" CodeBehind="UserAgreement.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.UserAgreement" EnableEventValidation="false" %>

<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>

<asp:Content ID="Header" ContentPlaceHolderID="Header" runat="server">
</asp:Content>

<asp:Content ID="Content" ContentPlaceHolderID="Content" runat="server">
	<link type="text/css" rel="stylesheet" href="../StyleSheets/bootstrap.min.css" />
	<link type="text/css" rel="stylesheet" href="../BaseStyle.css" />
	<link type="text/css" rel="stylesheet" href="../StyleSheets/monthpicker.css" />
	<link type="text/css" rel="stylesheet" href="../StyleSheets/nprogress.css" />

	<script type="text/javascript" src="../Scripts/angular-1.8.2.min.js"></script>
	<script type="text/javascript" src="../Scripts/jquery-3.6.0.min.js"></script>
	<script type="text/javascript" src="../Scripts/monthpicker.js"></script>
	<script type="text/javascript" src="../Scripts/nprogress.js"></script>
	<script type="text/javascript" src="../Scripts/bootstrap.bundle.min.js"></script>

	<style>
		.CompanyLogoDiv {
			margin: 20px 0px 40px 30px !important;
		}

		.textBoxLabel {
			font-size: 1.2rem;
			font-weight: 500;
		}

		.GroupPanel {
			border: 1px solid #ccc;
			margin-bottom: 5px;
			padding: 10px;
		}

		.TextBox {
			height: 40px;
			font-size: 1.2rem;
			margin-bottom: 5px;
		}

		.TextBox.readonly {
			background-color: #e9ecef;
			border-color: #ced4da;
			color: #6c757d;
		}

		.formRow {
			margin-bottom: 4px;
		}

		.AgreementBox {
			margin-top: 10px;
			height: 500px;
			overflow: auto;
			font-size: 1.2rem;
		}

		.GroupCaption {
			font-weight: 700;
			font-size: 1.5rem;
		}

		.ButtonBase {
			width: 140px;
			height: 40px;
			border-radius: 3px;
			margin-left: 5px;
			font-size: 1.2rem;
			font-weight: 600;
		}

		.SystemsPanel {
			max-height: 300px;
			overflow: auto;
		}
	</style>


	<div class="container">
		<div class="row">
			<div class="col-md-2 CompanyLogo CompanyLogoDiv"></div>
		</div>
		<div class="row">
			<div class="col-md-1"></div>
			<h1 class="col-md-6" style="font-size: 2.2rem; font-weight: 600"><%= CurrentAgreementTitle %></h1>
		</div>
		<div class="row justify-content-center">
			<div class="col-md-10 GroupPanel AgreementBox lh-sm">
				<asp:PlaceHolder ID="AgreementContent" runat="server" />
			</div>
		</div>
		<div class="row justify-content-center">
			<div class="col-md-10 GroupPanel" runat="server" id ="DocumentPanel">
				<h3 class="GroupCaption">Documents</h3>
				<div>
					<table class="table">
						<thead>
							<tr class="table-light">
								<th>Document Description</th>
								<th>File Type</th>
							</tr>
						</thead>
						<tbody>
							<edi:ZRepeater ID="EDocList" runat="server" BindTo="EDocs" OnItemDataBound="EDocListRepeater_ItemDataBound">
								<HeaderTemplate>
								</HeaderTemplate>
								<ItemTemplate>
									<tr>
										<td>
											<asp:HyperLink ID="EDocLink" runat="server" Text='<%# Eval("SC_Desc") %>'/>
										</td>
										<td>
											<label><%# Eval("SC_DataType") %></label>
										</td>
									</tr>
								</ItemTemplate>
								<FooterTemplate>
								</FooterTemplate>
							</edi:ZRepeater>
						</tbody>
					</table>
				</div>
			</div>
			<div class="col-md-10 GroupPanel">
				<h3 class="GroupCaption">Your CargoWise systems</h3>
				<div>
					<table class="table">
						<thead>
							<tr class="table-light">
								<th>Server Code</th>
								<th>Lic. Type</th>
								<th>Ring</th>
								<th>Current Version</th>
							</tr>
						</thead>
						<tbody>
							<edi:ZRepeater ID="DatabaseList" runat="server" BindTo="Databases">
								<HeaderTemplate>
								</HeaderTemplate>
								<ItemTemplate>
									<tr>
										<td>
											<label><%# Eval("LD_ServerCode") %></label>
										</td>
										<td>
											<label><%# Eval("LD_LicenceType") %> - <%# Eval("LD_LicenceTypeDescription") %></label>
										</td>
										<td>
											<label><%# Eval("LD_ReleaseRing") %> - <%# Eval("ReleaseRingDescription") %></label>
										</td>
										<td>
											<label><%# Eval("CurrentVersion.VersionNumber") %></label>
										</td>
									</tr>
								</ItemTemplate>
								<FooterTemplate>
								</FooterTemplate>
							</edi:ZRepeater>
						</tbody>
					</table>
				</div>
			</div>
		</div>
		<div class="row justify-content-center">
			<div class="col-md-10 GroupPanel">
				<h3 class="GroupCaption">Declaration</h3>
				<div style="font-weight: 500; font-size: 1.2rem; margin-bottom: 3px">I declare that I am an authorized officer of <%= CompanyName %>, and I have the authority to enter this CargoWise Next agreement by completing my details below</div>
				<div class="container">
					<div class="row formRow">
						<label class="textBoxLabel">Full Name</label>
						<div class="col-md-6 align-content-lg-stretch">
							<edi:ZTextBox CssClass="w-100 TextBox" ID="FullNameTextBox" runat="server" />
							<asp:RequiredFieldValidator ID="FullNameRequired" runat="server" ControlToValidate="FullNameTextBox" ErrorMessage="Full Name is required." CssClass="text-danger" Display="Dynamic" />
						</div>
					</div>
					<div class="row formRow">
						<label class="textBoxLabel">Job Title</label>
						<div class="col-md-6">
							<edi:ZTextBox CssClass="w-100 TextBox" ID="JobTitleTextBox" runat="server" />
							<asp:RequiredFieldValidator ID="JobTitleRequired" runat="server" ControlToValidate="JobTitleTextBox" ErrorMessage="Job Title is required." CssClass="text-danger" Display="Dynamic" />
						</div>
					</div>
					<div class="row formRow">
						<div class="col-md-12">
							<label class="textBoxLabel">Email Address</label>
							<div class="row">
								<div class="col-md-6">
									<edi:ZTextBox CssClass="w-100 TextBox" ID="EmailAddressTextBox" runat="server" />
								</div>
								<div class="col-md-4 row" runat="server" id="VerifyPanel">
									<label class="col-md-6" style="display: flex; justify-content: left; align-items: center; font-size: 1.1rem; color: #00795d; font-weight: 600;">Email verified</label>
								</div>
							</div>
							<asp:RequiredFieldValidator ID="EmailRequired" runat="server" ControlToValidate="EmailAddressTextBox" ErrorMessage="Email Address is required." CssClass="text-danger" Display="Dynamic" />
							<asp:RegularExpressionValidator ID="EmailFormatValidator" runat="server" ControlToValidate="EmailAddressTextBox" ErrorMessage="Invalid email format." CssClass="text-danger" Display="Dynamic" ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$" />
						</div>

					</div>
				</div>
			</div>
		</div>
		<div class="row justify-content-center">
			<label class="col-md-10" style="font-weight: 700; margin: 6px" runat="server"><%= PageFooter %></label>
		</div>

		<div class="row justify-content-end" style="margin-bottom: 10px">
			<div class="col-md-6 d-flex flex-row-reverse">
				<edi:ZButton CssClass="ButtonBase btn-primary active" ID="VerifyEmailButton" runat="server" OnClick="VerifyEmailButton_Click" Text="Verify My Email" CausesValidation="true" />
				<edi:ZButton CssClass="ButtonBase btn-primary active" ID="AcceptButton" runat="server" OnClick="AcceptButton_Click" Text="Accept" CausesValidation="true" />
				<edi:ZButton CssClass="ButtonBase" ID="CancelButton" runat="server" OnClick="CancelButton_Click" Text="Cancel" />
			</div>
			<div class="col-md-1"></div>
		</div>
	</div>
</asp:Content>
