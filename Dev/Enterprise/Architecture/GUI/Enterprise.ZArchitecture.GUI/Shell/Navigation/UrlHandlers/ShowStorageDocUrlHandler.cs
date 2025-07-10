using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.URLHandler;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture
{
	[ImmutableObject(true)]
	public class ShowStorageDocUrlHandler : UrlHandler, IShowEDocUrlHandler
	{
		public static ShowStorageDocUrlHandler Instance
		{
			get { return instance ?? (instance = new ShowStorageDocUrlHandler()); }
		}
		[ThreadStatic]
		static ShowStorageDocUrlHandler instance;

		protected override string ExpectedCommandText
		{
			get { return "ShowStorageDoc"; }
		}

		protected override bool HandleCore(QueryString queryString)
		{
			var form = ZForm.ActiveForm ?? ZApplication.GetOpenForms().OfType<KForm>().LastOrDefault();
			if (form == null)
			{
				var exceptionMessage = Res.GetString("25b0f651-3388-481c-a783-35d3a35f2f1f", "An active form could not be found to use for displaying the requested document. Please ensure {0} is open.", Enterprise.Core.Constants.ProductName);
				var exception = new EnterpriseUrlHandlerException(exceptionMessage);
				Globals.Message.ShowError(exceptionMessage);
				throw exception;
			}

			var viewer = ObjectFactory.Get<IStorageDocsViewer>();
			viewer.Initialise(form, null);

#if DEBUG
			if (Globals.IsTest)
			{
				lastViewerShownTestOnly = viewer;
			}
#endif

			var bizOPK = GetBusinessEntityPk(queryString);
			var documentPk = GetQueryStringPk(queryString, "StorageDocPK");

			if (bizOPK == Guid.Empty || documentPk == Guid.Empty)
			{
				ThrowInvalidUrlException();
			}

			var zForm = form as ZForm;
			var factory = zForm != null && zForm.BusinessEntity != null ? zForm.BusinessEntity.Factory : new BusinessObjectFactory();
			var docFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var docFactory = (IStorageMainForPK)docFactoryProvider.GetFactory(factory);
			var storageMainView = docFactory.GetStorageMain(bizOPK);
			var storageMain = storageMainView as IStorageMain;

			if (storageMain != null)
			{
				var storageDoc = storageMain.AllEDocs.GetFromUniqueKey(documentPk);
				if (storageDoc == null)
				{
					ShowDocumentNotFoundMessage();
				}
				else if (!VerifyUserSecurityRightsForStorageMainAccess(storageMain, storageDoc))
				{
					Globals.Message.Show(Res.GetString("39d51581-72a5-42c6-97f3-fa288fa2d476", "You do not have sufficient security rights to access this document"), Res.GetString("d3d869db-bdc5-4491-a851-b557007fc226", "Insufficient Access"), MessageBoxButtons.OK, DialogResult.None);
				}
				else
				{
					viewer.View(storageDoc, true);
				}

				return true;
			}
			else
			{
				if (storageMainView != null)
				{
					ErrorReporter.ReportOnce("The storage main view was found, but could not be cast to IStorageMain. Ensure that DbBackendDocumentFactory GetStorageMain is returning a StorageMain");
					return false;
				}
				else
				{
					ShowDocumentNotFoundMessage();
					return true;
				}
			}

			void ShowDocumentNotFoundMessage() => Globals.Message.Show(Res.GetString("858fc5ee-0a62-461f-85dd-dcfa81efc445", "The requested document could not be found. Please ensure the document has been saved and has not been deleted."), Res.GetString("0c1aad8e-76c0-439c-9141-28f0674acf6e", "Document not found"), MessageBoxButtons.OK, DialogResult.None);
		}

		protected bool VerifyUserSecurityRightsForStorageMainAccess(IStorageMain storageMain, IeDoc storageDoc)
		{
			Func<bool> isAllowedForStorageMain = () =>
			{
				var moduleFactory = ZModuleFactory.Instance;
				var docId = storageMain.DocumentOwnerModuleID;
				ModuleIdentifier id = null;

				if (docId == null || (id = moduleFactory.GetRegisteredIdentifierByName(docId.ToString())) == ModuleIDs.NotAssigned)
				{
					var message =
						(NoResString)"A module id could not be determined from the storage main. Please ensure ModuleID has been overriden in the derived AssemblyData class, the storage main has an SM_Type assigned, and the given type is mapped to AssemblyMetadata.xml"; // Column name used in error message, not key
					ErrorReporter.ReportOnce(message);
					ThrowInvalidUrlException();
				}

				using (var module = moduleFactory.Create(id))
				{
					return module.SecurityCheckpoint.IsAllowed;
				}
			};

			Func<ZString, ZString, SecurityCheckpoint, bool> nullOrValidOrGranted = (docCode, systemCode, checkpoint) => docCode.IsEmpty || docCode.Equals(systemCode) || checkpoint.IsAllowed;
			Func<bool> isAllowedForStorageDoc = () =>
			{
				return nullOrValidOrGranted(storageDoc.VisibleCompanyCode, EnvProxy.Instance.CurrentCompany.Code, Env.Security.ViewAllCompanySpecificDocuments)
					&& nullOrValidOrGranted(storageDoc.VisibleBranchCode, EnvProxy.Instance.CurrentBranch.Code, Env.Security.ViewAllBranchSpecificDocuments)
					&& nullOrValidOrGranted(storageDoc.VisibleDepartmentCode, EnvProxy.Instance.CurrentDepartment.Code, Env.Security.ViewAllDepartmentSpecificDocuments);
			};

			return isAllowedForStorageMain() && isAllowedForStorageDoc();
		}

		public string Create(IeDoc storageDoc)
			=> Create(((IStorageMain)storageDoc.ParentMain).ParentFK, storageDoc.UniqueKey);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Query string for Enterprise URL Service")]
		internal string Create(ZGuid storageMainParentFK, ZGuid storageDocPk)
		{
			var queryString = new QueryString();
			queryString.Add("Command", ExpectedCommandText);
			queryString.Add("BusinessEntityPK", storageMainParentFK.ToString());
			queryString.Add("StorageDocPK", storageDocPk.ToString());

			if (InstanceDetails.Current != null)
			{
				if(InstanceDetails.Current.Domain != null)
				{
					queryString.Add("Domain", InstanceDetails.Current.Domain);
					queryString.Add("Instance", InstanceDetails.Current.Instance);
				}

				if (InstanceDetails.ShouldAddDatabaseInfoToUrls)
				{
					queryString.Add("ServerName", InstanceDetails.Current.ServerName);
					queryString.Add("DatabaseName", InstanceDetails.Current.DatabaseName);
				}
			}
			queryString.Add("Hash", CreateQueryStringSecurityHash(queryString));

			return GetUrlFromQueryString(queryString);
		}

		public string CreateRtf(IeDoc storageDoc, string caption)
		{
			return ZMenuStrategyHelper.ShortcutCreator.FormatHyperLinkRtf(caption, Create(storageDoc));
		}

		protected internal override string[] GetQueryNamesSecuredBySecurityHash(QueryString queryString)
		{
			return new[] { "BusinessEntityPK", "StorageDocPK" };
		}

#if DEBUG

		[WTG.StaticAnalysis.Annotation.ThreadSafe] //Needs to not be thread static, so we can set and clear it from a different thread.
		static IStorageDocsViewer lastViewerShownTestOnly;

		internal IDisposable SetupStorageDocUrlHandlerForTest()
		{
			Action disposeLastViewerShown = () =>
			{
				if (lastViewerShownTestOnly != null)
				{
					lastViewerShownTestOnly.Dispose();
					lastViewerShownTestOnly = null;
				}
			};

			return new DisposableAction(disposeLastViewerShown, disposeLastViewerShown);
		}

#endif
	}
}
