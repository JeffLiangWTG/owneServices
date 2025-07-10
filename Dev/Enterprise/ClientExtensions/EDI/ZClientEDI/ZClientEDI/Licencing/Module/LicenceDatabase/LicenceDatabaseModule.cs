using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Module;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ZClientEDI.Business.Licencing;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Module
{
	public class LicenceDatabaseModule : ZFilterGridModule, IImportCollectionInfoProvider, IOperationalActionSupportable
	{
		public LicenceDatabaseModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID
		{
			get { return Modules.ClientModuleRegistration.LicenceDatabase; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(Modules.ClientControllerRegistration.LicenceDatabase);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new LicenceDatabaseFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new LicenceDatabaseNonDependentCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new LicenceDatabaseFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Organisation; }
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewStandardMenuItems())
			{
				new ZMenuItem("Send Manual System Shutdown Date", SendNewSystem_Click),
				new ZMenuItem("Request Staff List", RequestStaffList_Click)
			};

			return result.ToArray();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			result.Add(new ZMenuItem("Import STL Customer Settings", ImportStlCustomerSettings));
			result.Add(new ZMenuItem("Set Enterprise ID/Code", MoveDatabasesToNewEnterprise));
			return result.ToArray();
		}

		void SendNewSystem_Click(object sender, EventArgs args)
		{
			var action = new LicenceDatabaseSendSystemExpiryAction(SelectedLicenceDatabases);
			action.SendNewSystemShutdownDate();
		}

		void RequestStaffList_Click(object sender, EventArgs args)
		{
			var sbSuccess = new StringBuilder();
			var sbSentRequests = new StringBuilder();
			foreach (var database in SelectedLicenceDatabases)
			{
				if (!WasRequestStaffListSentRecently(database))
				{
					ClientStaffReportRequest.Send(database);
					sbSuccess.AppendLine($"{database.LD_ServerCode} - {database.LD_Product} - {database.LicenceCodeForSystemMessage}");
				}
				else
				{
					sbSentRequests.AppendLine($"{database.LD_ServerCode} - {database.LD_Product} - {database.LicenceCodeForSystemMessage}");
				}
			}

			ShowRequestStaffListMessage(sbSuccess, sbSentRequests);
		}

		bool WasRequestStaffListSentRecently(LicenceDatabase database)
		{
			var alreadySent = false;
			if (!string.IsNullOrEmpty(database.LicenceCodeForSystemMessage))
			{
				var query = new ZQuery();
				query.AddToFilter(EDIInterchangeSchema.EI_To, database.LicenceCodeForSystemMessage);
				query.AddToFilter(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcNow.AddMinutes(-3));

				var sql = "IsNull(dbo.CLRUncompressAsString(" + EDIInterchangeSchema.EI_BodyData.Name + "), '') like @Value";
				query.AddFilterAndZSQLParameterCollection(sql, new ZSqlParameterCollection(ZSqlParameter.New("@Value", "%StaffReportRequest%", EDIInterchangeSchema.EI_BodyData)));

				alreadySent = Factory.ExistsInDatabase(EDIInterchangeSchema.Constants.TableName, query);
			}
			return alreadySent;
		}

		void ShowRequestStaffListMessage(StringBuilder sbSuccess, StringBuilder sbSentRequests)
		{
			var message = string.Empty;
			if (sbSuccess.Length > 0)
			{
				message = "A request for Staff List has been sent to the licence(s) below:\r\n" + sbSuccess.ToString();
			}
			if (sbSentRequests.Length > 0)
			{
				if (!string.IsNullOrEmpty(message))
				{
					message += "\r\n";
				}
				message += "A request for Staff List for the licence(s) below was recently sent, please wait and try again later:\r\n" + sbSentRequests.ToString();
			}
			if (!string.IsNullOrEmpty(message))
			{
				Globals.Message.Show(message);
			}
		}

		LicenceDatabase[] SelectedLicenceDatabases
		{
			get
			{
				List<LicenceDatabase> result = new List<LicenceDatabase>();
				foreach (BusinessObject o in LicenceDatabaseEmbeddedControl.FilteredGrid.SelectedElements)
				{
					LicenceDatabase ld = o as LicenceDatabase;
					if (ld != null)
					{
						result.Add(ld);
					}
				}
				return result.ToArray();
			}
		}

		LicenceDatabaseFilterControl LicenceDatabaseEmbeddedControl
		{
			get { return (LicenceDatabaseFilterControl)EmbeddedControl; }
		}

		string IImportCollectionInfoProvider.ContextKey
		{
			get { return "LicenceDatabaseModuleImportWizard"; }
		}

		IImportCollectionInfo IImportCollectionInfoProvider.ImportCollectionInfo
		{
			get
			{
				var collection = new LicenceDatabaseFlattenedCollection(Factory);
				return new LicenceDatabaseImportInfo(collection);
			}
		}

		public OperationalActionSupporter OperationalActionSupporter => new LicenceDatabaseOpAccSupporter();

		protected override void RunImportDataWizard()
		{
			var importInfoProvider = (IImportCollectionInfoProvider)this;
			var importInfo = importInfoProvider.ImportCollectionInfo;
			var importFactory = new BusinessObjectFactory();
			var processor = new LicenceDatabaseFlattenedDataTransferProcessor(new LicenceDatabaseNonDependentCollection(importFactory), importInfo);

			bool isCancelled = false;
			var form = new MultistepDataImportWizardForm(importInfo, importInfoProvider.ContextKey, new DataTransferProcessor[] { processor }, ID.Description.ToString());
			form.Cancelled += (s, e) => { processor.Rollback(); isCancelled = true; };
			form.Imported += (s, e) => { DisplayResult(processor, isCancelled); };
			form.Show();
		}

		void DisplayResult(LicenceDatabaseFlattenedDataTransferProcessor processor, bool isCancelled)
		{
			string mesgs;
			string caption;

			if (isCancelled)
			{
				mesgs = Res.GetString("AAFF011F-9F5E-45B6-8B44-FCC384BA952D", "No database was created.");
				caption = Res.GetString("909079C2-B858-4803-BB32-FC0DD16094B0", "Import canceled");
			}
			else
			{
				mesgs = Res.GetString("DE2E2190-5396-4911-A37E-287F06D85CE5", "Databases to Import = {0}", processor.HeadersToCreate) + "\r\n";
				mesgs += processor.Log;
				mesgs += "\r\n" + Res.GetString("DE2F2C4A-CB5A-4736-9719-A0FD6EFDE689", "TOTAL: Databases created = {0}, Databases excluded = {1}",
					processor.HeadersCreated, processor.HeadersExcluded) + "\r\n";

				caption = Res.GetString("7DC5103F-5CF3-4E3F-9259-3EFF7582BAF0", "Import completed");
			}

			using (ZMessageBox notification = new ZMessageBox(mesgs, caption, MessageBoxButtons.OK, MessageBoxIcon.Information))
			{
				notification.ShowDialog();
			}
		}

		void ImportStlCustomerSettings(object sender, EventArgs arg)
		{
			var checkpoint = EDISecurityCheckpoints.OrgLicenceBilling;
			if (!checkpoint.IsAllowed)
			{
				checkpoint.ShowError();
				return;
			}

			new EdiLicenceSettingImporter().Import();
		}

		void MoveDatabasesToNewEnterprise(object sender, EventArgs arg)
		{
			var checkpoint = EDISecurityCheckpoints.OrgLicenceRemoveLicenceDatabase;
			if (!checkpoint.IsAllowed)
			{
				checkpoint.ShowError();
				return;
			}

			var databases = SelectedLicenceDatabases;

			if (!databases.Any())
			{
				Globals.Message.ShowError("Please select at least one database.");
				return;
			}

			var bizOForPopup = new MoveDatabasesToNewEnterpriseBizObj(new BusinessObjectFactory(), databases.Select(x => x.PK).ToArray(), new MoveDatabasesToNewEnterpriseLogger());
			ZFormModaliser.ShowDialogAndDispose(new MoveDatabasesToNewEnterprisePopupForm(bizOForPopup));
		}
	}

	internal sealed class LicenceDatabaseOpAccSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext => BusinessContext.LicenceDatabase;
		public override SecurityCheckpoint CustomizationSecurityCheckpoint => EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails;
		public override Type RootType => typeof(LicenceDatabase);
		public override SecurityCheckpoint RunSecurityCheckpoint => EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails;
	}
}
