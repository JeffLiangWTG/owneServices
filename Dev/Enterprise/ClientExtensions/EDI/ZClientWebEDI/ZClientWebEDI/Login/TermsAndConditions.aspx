<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TermsAndConditions.aspx.cs" Inherits="Enterprise.ZClientWebCargoWiseEDI.TermsAndConditions" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Frameset//EN" "http://www.w3.org/TR/html4/frameset.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Terms And Conditions</title>
	<script type="text/javascript" src="../Scripts/jquery-3.6.0.min.js"></script>
	<script type="text/javascript" src="../Scripts/blockUI.js"></script>
	<script type="text/javascript">
		jQuery.noConflict();	//Resolve $ alias conflict

		function ResizeParentIFrame() {
			if (window.parent.document.getElementById('contentFrame')) {
				window.parent.document.getElementById('contentFrame').height = document.body.scrollHeight;
			}
		}
	</script>
	<style type="text/css">
		.AcknowledgeWrapper	{ margin-left: 230px; margin-right: 30px; }
		.AcknowledgeOuterDiv { margin-top: 10px; height: 320px; overflow:auto; border: 1px solid #ccc; }
		.AcknowledgeInnerDiv { padding: 10px; }
		.AcknowledgeButtonDiv { text-align:right; margin-top: 15px; }	
		.AcknowledgeCheckBox label { padding-left: 10px; }
		.PrintButton { float: left; margin-top: 15px; }
		.ImportantNotice { color: #990033; font-weight: bold; margin-right: 10px; }
		.BlockMsg h1 { color: #fff; }
	</style>
</head>
<body onload="ResizeParentIFrame();">
	<form id="form1" runat="server" method="post">
		<div id="OuterContentPane" runat="server">
			<div class="AcknowledgeWrapper" id="AcknowledgeWrapper" runat="server">
				<h1>My Account Terms and Conditions</h1>
				<hr/>
				<edi:ZTextLabel id="ContentChangedMessage" runat="server" text="The terms and conditions of the agreement have been updated. As such, please read through and accept the updated agreement." visible="false" cssclass="ImportantNotice"/>
				<div class="AcknowledgeOuterDiv">
					<div id="WebContractContentHolder" runat="server" class="AcknowledgeInnerDiv"></div>
				</div>
				<div style="margin-top: 5px;">
					<div id="AgreementNoteDiv" runat="server" style="margin: 10px 0px 10px 0px;">
						Please tick the check box below if you are authorized to agree on behalf of your company.<br />
						If you are not authorized, please follow the instruction below:<br />
						1) Close this page<br />
						2) Ask an authorized person in your company to register for My Account<br />
						3) When login detail received, the authorized person login and agree the terms and conditions<br />
						4) Then you can login with your username and password
					</div>					
					<asp:CheckBox id="AuthorisedUserCheckBox" runat="server" CssClass="AcknowledgeCheckBox"></asp:CheckBox>
				</div>
				<asp:Button id="PrintButton" runat="server" CssClass="PrintButton" text="Print Agreement" />
				<div class="AcknowledgeButtonDiv">
					<span class="ImportantNotice">Please read the entire agreement and scroll to the end before accepting</span>
					<asp:Button id="AcceptButton" runat="server" text="Accept" onclick="AcceptButton_Click" />
					&nbsp;
					<asp:Button id="DeclineButton" runat="server" text="Don't Accept" onclick="DeclineButton_Click" />
				</div>
			</div>
			<edi:ZTextLabel id="ErrorMessage" runat="server" CssClass="ErrorMessage" />
		</div>
	</form>

	<!-- Script -->
	<script type="text/javascript">
		var readEntireAgreement = false;

		jQuery(".AcknowledgeOuterDiv").scroll(function () {
			var outerDiv = jQuery(this);
			var innerDiv = jQuery(">.AcknowledgeInnerDiv", jQuery(this));
			var ScrollMod = 1;
			if (outerDiv.offset().top < innerDiv.outerHeight()) {
				ScrollMod = -1;
			}
			if (Math.round((ScrollMod * innerDiv.offset().top) + outerDiv.height() + outerDiv.offset().top + 5) >= innerDiv.outerHeight() && Math.abs(innerDiv.offset().top) != 0) {
				jQuery(this).unbind("scroll");
				readEntireAgreement = true;
			}
		});

		jQuery(".PrintButton").click(function () {
			printArea = document.createElement('iframe');
			jQuery(printArea).attr({ style: 'border:0;position:absolute;width:0px;height:0px;left:0px;top:0px;' });
			document.body.appendChild(printArea);
			printArea.doc = printArea.contentWindow.document;

			printArea.doc.open();
			printArea.doc.write(jQuery("#OuterContentPane #WebContractContentHolder").html());
			printArea.doc.close();
			printArea.contentWindow.focus();
			printArea.contentWindow.print();
			return false;
			document.body.removeChild(printArea);
		});

		jQuery("#AcceptButton").click(function () {
			if (!readEntireAgreement) {
				alert("Please read the entire agreement and scroll to the end before accepting.");
				return false;
			} else if (!jQuery("#AuthorisedUserCheckBox").is(':checked')) {
				alert("Please tick the check box if you are authorized to agree on behalf of your company.");
				return false;
			} else {
				jQuery.blockUI({
					message: '<h1>Please wait...</h1>',
					css: {
						border: 'none',
						padding: '15px',
						width: '40%',
						left: '30%',
						backgroundColor: '#000',
						'-webkit-border-radius': '10px',
						'-moz-border-radius': '10px',
						opacity: 0.6,
						color: '#fff'
					},
					overlayCSS: {
						backgroundColor: '#fff'
					},
					blockMsgClass: 'BlockMsg'
				});
				return true;
			}
		});
	</script>
	<!-- Script -->
	
</body>
</html>
