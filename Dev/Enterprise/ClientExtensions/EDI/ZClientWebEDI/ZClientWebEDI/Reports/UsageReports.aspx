<%@ Page Language="C#" MasterPageFile="~/CargoWiseIFrame.Master" AutoEventWireup="true" CodeBehind="UsageReports.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.UsageReports" Title="Usage Reports" %>

<asp:Content ID="Content1" ContentPlaceHolderID="Content" runat="server">
	
	<link type="text/css" rel="stylesheet" href="../StyleSheets/bootstrap.min.css" />
	<link type="text/css" rel="stylesheet" href="../BaseStyle.css" />
	<link type="text/css" rel="stylesheet" href="../StyleSheets/monthpicker.css" />
	<link type="text/css" rel="stylesheet" href="../StyleSheets/nprogress.css" />

	<script type="text/javascript" src="../Scripts/angular-1.8.2.min.js"></script>
	<script type="text/javascript" src="../Scripts/jquery-3.6.0.min.js"></script>
	<script type="text/javascript" src="../Scripts/monthpicker.js"></script>
	<script type="text/javascript" src="../Scripts/nprogress.js"></script>
	<script type="text/javascript" src="../Scripts/bootstrap.bundle.min.js"></script>

	<script type="text/javascript">
		if (typeof JSON.stringify !== "function") {
			JSON.stringify = function (value, replacer, space) { return StandardBuiltInJSON.stringify(value, replacer, space); };
		}
		if (typeof JSON.parse !== "function") {
			JSON.parse = function (text, reviver) { return StandardBuiltInJSON.parse(text, reviver); };
		}

		var app = angular.module('myApp', []);
		var httpBodyData = { UserSession : '<%=UserSession%>', Product : '<%=Product%>' };

		app.controller('ReportCtrl', function ReportCtrl($scope, $http) {
			$scope.orgUsages = [];
			NProgress.configure({ parent: '#progressbar' });
			$scope.loadOrgUsages = function (year, month) {
				$scope.loading = true;
				NProgress.start();
				var httpRequest = $http({
					method: 'POST',
					url: '../api/UsageReport/' + year + '/' + month + '/',
					data: httpBodyData,
					headers: { 'Content-Type' : 'application/json' }
				}).then(function (response) {                    
					NProgress.done();
					$scope.orgUsages = response.data;
					window.setTimeout(function () {
						$scope.$apply(function () {
							$scope.loading = false;
						});
					}, 350);
					window.setTimeout(function () {
						$scope.$apply(function () {
							DetermineDropDirection();
						});
					}, 500);
				},function (error){
					console.log(error, 'can not get data.');
				});
            };

            $scope.GetStlCombinedUsageReport = GetStlCombinedUsageReport;
			$scope.ClickOnPdf = function (url) { return url == '#' ? alert('This report is too large for PDF format. Please use csv format instead.') : window.location.assign(url); };
			$scope.GetPdfClass = function (url) { return url == '#' ? 'no-pdf' : ''; };
        });

        $body = $("body");
        $(document).on({
            ajaxStart: function () { $body.addClass("loading"); },
            ajaxStop: function () { $body.removeClass("loading"); }
        });
		
		function GetPreviousMonth()
		{
			//Convert zero-based current month to one-based previous month
			var pageDate = new Date();
			var year = pageDate.getFullYear();
			var month = pageDate.getMonth();
			if (month == 0) {
				month = 12;
				year -= 1;
			}

			return { month: month, year: year };
		}

		$(document).ready(function () {
			var defaultSelectedMonth = GetPreviousMonth();
			LoadUsageLinks(defaultSelectedMonth);
		});

		$(function () {
			var defaultSelectedMonth = GetPreviousMonth();
			var defaultYear = defaultSelectedMonth["year"];
			var defaultMonth = defaultSelectedMonth["month"];

			var yearRange = (2010 - defaultYear).toString();
			if (defaultYear == 2010)
				yearRange = "-" + yearRange;
			if (defaultMonth == 12) {
				yearRange = yearRange + "~1";
			}
			else {
				yearRange = yearRange + "~0";
			}
			$("#month_picker").monthpicker({
				elements: [
					{
						tpl: "month",
						opt: { value: defaultMonth }
					},
					{
						tpl: "year",
						opt: {
							range: yearRange,
							value: defaultYear
						}
					}
				],
				onChanged: LoadUsageLinks
			});
		});

		function LoadUsageLinks(data, $e) {
			var scope = angular.element(document.getElementById("linkTable")).scope();
			scope.$apply(function () {
				scope.loadOrgUsages(data["year"], data["month"]);
			});
		}

		function DetermineDropDirection() {
			$(".dropdown-menu").each(function () {
				$(this).css({
					visibility: "hidden",
					display: "block"
				});

				$(this).parent().removeClass("dropup");

				if ($(this).offset().top + $(this).outerHeight() > $("#linkTable").offset().top + $("#linkTable").innerHeight()
					&& $(this).outerHeight() < $(this).offset().top - $("#linkTable").offset().top) {
					$(this).parent().addClass("dropup");
				}

				$(this).css({
					visibility: "",
					display: ""
				});
			});
		}

        function GetStlCombinedUsageReport(database, yyyyMM)
        {
			$.ajax(
				{
					type: 'POST',
					url: '../api/UsageReport/GetStlCombinedUsageReport/' + database + '/' + yyyyMM + '/',
					data: JSON.stringify(httpBodyData),
					contentType: "application/json",
					dataType: 'text',
					success: function (status)
					{
						if (status == '"OK"')
						{
							alert('Report is being generated.\nYou will receive a notification email with download link later.');
						}
						else
						{
							alert('System Error. Please try again later.\n' + status);
						}
					}
				})
                .fail(
                function (jqXHR, textStatus, err)
                {
                    alert('System Error. Please try again later.\n' + err);
                });
        }

	</script>

	<style>
		body {
			line-height: initial;
			margin: initial;
		}

		h1 {
			line-height: inherit;
			margin-top: 10px;
			margin-bottom: 10px;
		}

		h2 {
			margin-left: 10px;
			margin-top: 20px;
			font-weight: bold;
		}

		h3 {
			font-size: 13px;
			margin: 5px 0 5px 0;
			line-height: 15px;
			font-weight: bold;
		}

		hr {
			border-top: 1px solid #ccc;
		}

		a {
			text-decoration: underline;
		}

		p, ul {
			margin: initial;
		}

		label {
			font-weight: normal;
		}

		table {
			border-collapse: separate;
			border-spacing: 1px;
		}

		.usageBlock {
			border-bottom: 1px solid #eee;
			padding: 8px 15px 8px 15px;
		}

		.usageBlockTopBorder {
			border-bottom: 1px solid #eee;
		}

		.billingType {
			font-size: 9px;
			background-color: #ccc;
			color: #fff;
			padding: 2px 4px 2px 4px;
			margin-left: 15px;
			font-weight: normal;
			vertical-align: middle;
		}

		#month_picker { margin-top:10px; }

			#month_picker ul ul {
				border: 1px solid #ccc;
			}

			#month_picker ul li a {
				font-size: 11px;
				padding: 2px 0;
			}
			
		#progressbar { height:50px; }

		#linkTable {
			min-height: 600px;
			max-height: 600px;
			padding-bottom: 20px;
			overflow-y: scroll;
		}
			#linkTable table {
				table-layout: fixed;
				width: 660px;
				overflow-y: scroll;
			}
			
			#linkTable td {
				padding: 2px 0;
				vertical-align: top;
			}
				#linkTable td.col1 {
					width: 480px;
				}
				#linkTable td.col2 {
					width: 40px;
				}
				#linkTable td.col3 {
					width: 80px;
				}
				#linkTable td.heading {
					width: 480px;
					font-weight: bold;
					font-style: italic;
					line-height: 14px;
				}

			#linkTable a {
				font-size: 11px;
				font-weight: bold;
				text-decoration: none;
			}

			
		.dropdown-menu {
			min-width: 140px;
			line-height: normal;
		}
			.dropdown-menu .dropdown-header {
				font-weight: bold;
				color: #ddd;
				padding: 1px 8px;
				font-size: 11px;
			}
			.dropdown-menu .divider {
				margin: 3px 0px 5px 0px;
			}
			.dropdown-menu li a {
				padding: 1px 8px;
				min-width: 45px;
			}
			.dropdown-menu li.ng-scope
			{
				float: left;
				position: relative;
			}
			.dropdown-menu .no-pdf {
				color: #b0b0b0;
			}

		.dropup .caret
		{
			border-top: 4px dashed;
			border-bottom: 0;
		}

        .modal {
            display:    none;
            position:   fixed;
            z-index:    1000;
            top:        0;
            left:       0;
            height:     100%;
            width:      100%;
            background: rgba( 255, 255, 255, .8 ) 
                        50% 50% 
                        no-repeat;
            text-align: center;
            vertical-align: middle;
        }

        body.loading {
            overflow: hidden;   
        }

        body.loading .modal {
          display: flex;
          align-items: center;
          justify-content: center;
        }

	</style>

	<div class="wrapper">
		<p id="Breadcrumb" runat="server">
			<a href="../../index.html" class="pagetrail" target="_parent">Home</a>
			<img src="../images/arrow2.gif" width="14" height="10" align="absmiddle" />
			<a href="../Default.aspx" class="pagetrail">My Account</a>
			<img src="../images/arrow2.gif" width="14" height="10" align="absmiddle" />
		</p>

		<h1>Usage Reports</h1>
		<hr size="1" />

		<div class="MonthPicker" id="month_picker"></div>
		
		<div ng-app="myApp">
			<div id="linkTable" ng-controller="ReportCtrl">
				<div id="progressbar" data-ng-show="loading"></div>
				<div data-ng-hide="loading">
					<h2>{{orgUsages.PeriodStart}}</h2>

					<div ng-if="orgUsages.StlUsages.length > 0 || orgUsages.OdplUsages.length > 0" class="usageBlockTopBorder"></div>

					<div ng-repeat="stlUsage in orgUsages.StlUsages" class="usageBlock">
						<h3>Server {{stlUsage.DatabaseServerCode}}<span class="billingType">STL</span></h3>
						<table>
							<tbody>
								<tr ng-repeat="usage in stlUsage.UsageData">
									<td class="col1" ng-style="{'padding-left': 10 * usage.IndentLevel + 'px'}" ng-class="{'heading': usage.ReportLinks.length == 0}">{{usage.Description}}</td>
									<td class="col2">
										<div class="dropdown" ng-if="usage.ReportLinks.length > 0">
											<a href="#" data-bs-toggle="dropdown" class="dropdown-toggle">PDF <b class="caret"></b></a>
											<ul class="dropdown-menu">
												<li class="dropdown-header">Company</li>
												<li role="separator" class="divider"></li>
												<li ng-repeat="link in usage.ReportLinks" ng-if="link.PdfLinkUrl"><a href='#' ng-click='ClickOnPdf(link.PdfLinkUrl)' ng-class='GetPdfClass(link.PdfLinkUrl)'>{{link.CompanyCode}}</a></li>
											</ul>
										</div>
									</td>
									<td class="col3">
										<div class="dropdown" ng-if="usage.ReportLinks.length > 0">
											<a href="#" data-bs-toggle="dropdown" class="dropdown-toggle">CSV <b class="caret"></b></a>
											<ul class="dropdown-menu" style="left:-65px;">
												<li class="dropdown-header">Company</li>
												<li role="separator" class="divider"></li>
												<li ng-repeat="link in usage.ReportLinks" ng-if="link.CsvLinkUrl"><a href="{{link.CsvLinkUrl}}">{{link.CompanyCode}}</a></li>
											</ul>
										</div>
									</td>
								</tr>
								<tr><td class="col1"></td><td class="col2"></td><td class="col3"></td></tr>
                                <tr>
									<td class="col1" style="padding-left:0px" class="heading">Combined Detail Report</td>
									<td class="col2"></td>
									<td class="col3">
										<div class="dropdown" ng-if="true">
                                            <a href='#' ng-click="GetStlCombinedUsageReport(stlUsage.DatabasePK, orgUsages.PeriodStartYYYYMM);" class="dropdown-toggle">CSV</a>
										</div>
									</td>
								</tr>
							</tbody>
						</table>
					</div>

					<div ng-repeat="odplUsage in orgUsages.OdplUsages" class="usageBlock">
						<h3>{{odplUsage.OrgName}}<span class="billingType">ODPL</span></h3>
						<table>
							<tbody>
								<tr ng-repeat="usage in odplUsage.UsageData">
									<td class="col1">{{usage.Description}}</td>
									<td class="col2">
										<div class="dropdown" ng-if="usage.ReportLinks.length > 0">
											<a href="#" data-bs-toggle="dropdown" class="dropdown-toggle">PDF <b class="caret"></b></a>
											<ul class="dropdown-menu">
												<li class="dropdown-header">Server - Company</li>
												<li role="separator" class="divider"></li>
												<li ng-repeat="link in usage.ReportLinks" ng-if="link.PdfLinkUrl"><a href='#' ng-click='ClickOnPdf(link.PdfLinkUrl)' ng-class='GetPdfClass(link.PdfLinkUrl)'>{{link.ServerCode}} - {{link.CompanyCode}}</a></li>
											</ul>
										</div>
									</td>
									<td class="col3">
										<div class="dropdown" ng-if="usage.ReportLinks.length > 0">
											<a href="#" data-bs-toggle="dropdown" class="dropdown-toggle">CSV <b class="caret"></b></a>
											<ul class="dropdown-menu" style="left:-65px;">
												<li class="dropdown-header">Server - Company</li>
												<li role="separator" class="divider"></li>
												<li ng-repeat="link in usage.ReportLinks" ng-if="link.CsvLinkUrl"><a href="{{link.CsvLinkUrl}}">{{link.ServerCode}} - {{link.CompanyCode}}</a></li>
											</ul>
										</div>
									</td>
								</tr>
							</tbody>
						</table>
					</div>

					<div ng-repeat="bwUsage in orgUsages.BorderWiseUsages" class="usageBlock">
						<h3>{{bwUsage.OrgName}}<span class="billingType">BorderWise</span></h3>
						<table>
							<tbody>
								<tr ng-repeat="usage in bwUsage.UsageData">
									<td class="col1">{{usage.Description}}</td>
									<td class="col2">
										<div>
                                            <a href="{{usage.ReportLinks[0].PdfLinkUrl}}">PDF</a>
										</div>
									</td>
									<td class="col3">
										<div>
                                            <a href="{{usage.ReportLinks[0].CsvLinkUrl}}">CSV</a>
										</div>
									</td>
								</tr>
							</tbody>
						</table>
					</div>

					<span ng-if="orgUsages.StlUsages.length == 0 && orgUsages.OdplUsages.length == 0 && orgUsages.BorderWiseUsages.length == 0" style="margin-left:15px;">
						No usage data.
					</span>
				</div>
			</div>
		</div>
	</div>
	
	<script type="text/javascript">
		$("#linkTable").scroll(DetermineDropDirection);
	</script>

    <div class="modal">Loading...</div>

</asp:Content>
