<%@ Page Language="C#" MasterPageFile="~/CargoWiseIFrame.Master" AutoEventWireup="true" CodeBehind="Notification.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.Notification" %>

<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350" %>

<asp:Content ContentPlaceHolderID="Header" runat="server">
	<style>
		.CompanyLogoDiv {
			margin: 20px 0px 40px 30px !important;
		}

		.notification-base {
			font-size: 1.4rem;
			border-radius: 5px;
			min-height: 100px;
			padding: 10px;
			display: flex;
			justify-content: center;
			align-items: center;
			font-weight:600
		}

		.notification-success {
			border-color: #00795d;
			background-color: #e6f4f0;
			color: #00382b;
		}

		.notification-warning {
			border-color: #716d28;
			background-color: #f6f3c7;
			color: #333112;
		}

		.notification-error {
			border-color: #d50047;
			background-color: #fef6f9;
			color: #670022;
		}

		.notification-info {
			border-color: #d50047;
			background-color: #fdedf2;
			color: #670022;
		}
	</style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="Content" runat="server">
	<link type="text/css" rel="stylesheet" href="../StyleSheets/bootstrap.min.css" />
	<link type="text/css" rel="stylesheet" href="../BaseStyle.css" />
	<link type="text/css" rel="stylesheet" href="../StyleSheets/monthpicker.css" />
	<link type="text/css" rel="stylesheet" href="../StyleSheets/nprogress.css" />

	<script type="text/javascript" src="../Scripts/angular-1.8.2.min.js"></script>
	<script type="text/javascript" src="../Scripts/jquery-3.6.0.min.js"></script>
	<script type="text/javascript" src="../Scripts/monthpicker.js"></script>
	<script type="text/javascript" src="../Scripts/nprogress.js"></script>
	<script type="text/javascript" src="../Scripts/bootstrap.bundle.min.js"></script>

	<div class="container">
		<div class="row">
			<div class="col-md-2 CompanyLogo CompanyLogoDiv"></div>
		</div>
		<div class="row justify-content-center">
			<div class="col-md-8">
				<div class="shadow <%= GetAlertClass(Type) %> notification-base mt-3" role="alert">
					<%= Message %>
				</div>
			</div>
		</div>
	</div>
</asp:Content>
