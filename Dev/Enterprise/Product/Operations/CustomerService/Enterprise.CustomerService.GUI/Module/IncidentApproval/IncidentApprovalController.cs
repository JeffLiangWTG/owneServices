using System;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Modules;
using Enterprise.CustomerService.Business;
using Enterprise.CustomerService.GUI;
using Enterprise.eHubMessaging.Business;
using Enterprise.Environment;
using Enterprise.Messaging.Integration;
using Enterprise.Security;
using Enterprise.UserPortal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

#if !WINZOR
using Enterprise.ZArchitecture.Business;
#endif

namespace Enterprise.CustomerService.Module
{
	public partial class IncidentApprovalController : ZController, IServiceRequestController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.ServiceRequest; }
		}

#if WINZOR
		bool IsCWNext => true;
#else
		bool IsCWNext => CWNextFeatureHelper.IsCWNextEnabled();
#endif

		public override IZForm ShowNewForm()
		{
			var checkpoint = CheckPointForNew;
			if (!checkpoint.IsAllowed)
			{
				Globals.Message.ShowError(checkpoint.ErrorMessageForNotAllowed);
				return null;
			}

			var currentModuleId = GetModuleId(ParentForm);
			if (string.IsNullOrEmpty(currentModuleId))
			{
				var result = Globals.Message.Show(Res.GetString("d552af0d-5e8e-4717-987f-54ea74278ff3",
					@"Your eRequest will be handled more efficiently if you first navigate to the relevant module related to your query.

If your query is not related to a module, click 'OK' to continue. Otherwise click 'Cancel' and navigate there first."
					), Res.GetString("2efe0274-8db4-4aa4-9a62-4562232df63e", "Warning"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, DialogResult.Cancel);
				if (result == DialogResult.Cancel)
				{
					return null;
				}
			}
			var menuSectionCode = GetCustomerServiceMenuSectionCode(currentModuleId);
			var referenceId = ZGuid.NewZGuid().ToString();
			var isMainFormWithEmptyModuleId = ParentForm is IMainForm && string.IsNullOrWhiteSpace(currentModuleId);
			var shouldSendERequestDocument = ShouldSendERequestDocument && (IsCWNext || !isMainFormWithEmptyModuleId);
			var shouldAttachScreenshot = shouldSendERequestDocument
				&& (Globals.Message.Show(Res.GetString("abd28d62-6565-43a8-b8a3-32884df750da", "Would you like us to take a Screenshot and attach it to your incident?"),
					Res.GetString("301a36f4-c486-4f3c-9db2-ddb8c4f00c77", "Take Screenshot"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes);

			new UserPortalLauncher().GoToNewIncident(menuSectionCode, currentModuleId, referenceId);

			if (shouldSendERequestDocument)
			{
				SendERequestDocumentSafe(referenceId, ParentForm, shouldAttachScreenshot);
			}

			return null;
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(IncidentApproval); }
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			var incident = (IncidentApproval)sourceEntity;
			return !incident.HasBeenSent
				? base.ShowEditForm(sourceEntity)
				: ShowViewForm(sourceEntity);
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var incident = (IncidentApproval)businessEntity;
			return new IncidentApprovalForm(incident);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ServiceRequest; }
		}

		#region IServiceRequestController Members

		void IServiceRequestController.SetParentForm(Form parentForm)
		{
			this.ParentForm = parentForm;
		}

		Form ParentForm;

		#endregion

		public string GetModuleId(Form form)
		{
			var zForm = form as IZForm;
			if (zForm != null && zForm.ControllerID != null)
			{
				var controller = ZControllerFactory.Create(zForm.ControllerID);
				if (controller.ModuleID != null)
				{
					return controller.ModuleID.ID.ToString();
				}
			}
			else
			{
				var mainForm = form as IMainForm;
				if (mainForm != null)
				{
					var currentModule = mainForm.CurrentModule;
					if (currentModule != null)
					{
						return (currentModule as IMainFormModule)?.ModuleTreeID ?? currentModule.ID;
					}
				}
			}

			return string.Empty;
		}

		#region GetCustomerServiceMenuSectionCode

		public string GetCustomerServiceMenuSectionCode(string moduleTreeId)
		{
			return GetCustomerServiceMenuSectionCode(moduleTreeId, ParentForm);
		}

		public string GetCustomerServiceMenuSectionCode(string moduleTreeId, Form form)
		{
			var codeOverridable = form as ICustomerServiceMenuSectionCodeOverridable;
			if (codeOverridable != null)
			{
				return codeOverridable.SectionCode;
			}

			var searchResult = GetModuleTree().FindByModuleTreeID(moduleTreeId, IncludeHiddenModule);
			foreach (var section in searchResult.Select(x => x.ParentSection).Where(x => x != null))
			{
				if (!string.IsNullOrEmpty(section.CustomerServiceMenuSectionCode) && section.Subcategory != ModuleTreeLoaderConstant.Category.Jump)
				{
					return section.CustomerServiceMenuSectionCode;
				}
			}

			if (form is IMainForm)
			{
				return ModuleTreeCustomerServiceMenuSectionList.Codes.System;
			}

			return MandatoryCustomerServiceMenuSectionList.Codes.Other;
		}

		protected virtual bool IncludeHiddenModule => false;

		protected virtual ModuleTree GetModuleTree()
		{
			return ModuleTree.Tree;
		}

		#endregion

		#region Security checkpoints

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.IncidentApprovalView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.IncidentApprovalModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.IncidentApprovalNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.IncidentApprovalDelete; }
		}

		#endregion Security checkpoints

		#region ERequestDocument

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer exception message does not need translation")]
		const string SendRequestDeveloperExceptionMessage = "eRequest Document Creation Failed";

#if !WINZOR
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		static void SendERequestDocumentSafe(string referenceId, Form requestRaisingForm, bool shouldAttachScreenshot)
		{
			try
			{
				var factory = new BusinessObjectFactory();
				var eRequestDoc = new Xsd.ERequestDocument();
				eRequestDoc.ReferenceId = referenceId;

				var timestamp = ZDateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
				if (requestRaisingForm != null && !requestRaisingForm.IsDisposed && shouldAttachScreenshot)
				{
					AttachScreenShot(requestRaisingForm, eRequestDoc, timestamp);
				}

				createAndSaveSystemReport(factory, eRequestDoc, timestamp);
			}
			catch (Exception ex)
			{
				ExceptionReporter.Instance.ReportDeveloperException("dec5abce-d955-4dd4-b94d-b8b1be1f3c87", SendRequestDeveloperExceptionMessage, ex);
			}
		}

		static void AttachScreenShot(Form form, Xsd.ERequestDocument eRequestDoc, string timestamp)
		{
			var screenShot = CreateAttachment(eRequestDoc, timestamp);

			using var screenShotImage = ZScreenShotGrabber.Capture(form);
			if (screenShotImage != null)
			{
				using (var memoryStream = new MemoryStream())
				{
					screenShotImage.Save(memoryStream, ImageFormat.Png);
					screenShot.Data = memoryStream.ToArray();
				}
			}
		}
#endif
		static Xsd.ERequestDocumentAttachment CreateAttachment(Xsd.ERequestDocument eRequestDoc, string timestamp)
		{
			var screenShot = eRequestDoc.Attachments.AddNew();
			screenShot.IsPublished = true;
			screenShot.IsPublishedSpecified = true;
			screenShot.FileName = $"ScreenShot_{timestamp}.png";
			return screenShot;
		}

		static void createAndSaveSystemReport(BusinessObjectFactory factory, Xsd.ERequestDocument eRequestDoc, string timestamp)
		{
			AttachSystemReport(eRequestDoc, timestamp);

			SystemMessage.CreateSecureInterchange(factory, SystemMessageList.Descriptions.ERequestDocument, ZXmlSerializer.New(typeof(Xsd.ERequestDocument)), eRequestDoc);
			factory.Save();
		}

		static void AttachSystemReport(Xsd.ERequestDocument eRequestDoc, string timestamp)
		{
			var systemReport = eRequestDoc.Attachments.AddNew();
			systemReport.FileName = $"SystemReport_{timestamp}.zip";

			using (var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(new ExceptionReportBuilder(new ExceptionReportArgs(null, null, null, null)).GenerateReport())))
			using (var zipStream = new MemoryStream())
			{
				var creator = new ZipCreator();
				creator.ZipStream($"SystemReport_{timestamp}.xml", contentStream, zipStream);
				systemReport.Data = zipStream.ToArray();
			}
		}

		public bool ShouldSendERequestDocument { get; set; } = true;

		#endregion
	}
}
