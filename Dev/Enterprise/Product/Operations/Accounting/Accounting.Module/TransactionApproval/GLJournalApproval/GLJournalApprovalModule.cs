using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.Module.TransactionApproval
{
	public class GLJournalApprovalModule : TransactionApprovalModule<GLJournal, GLJournalApprovalRequest, GLJournalApprovalRequestDetails>
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GLJournalApproval; }
		}

		public override bool AllowEdit
		{
			get { return true; }
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new GLJournalApprovalFilterBusinessObject();
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			var journal = selectedBusinessObject as GLJournal;
			if (journal != null)
			{
				return ZControllerFactory.Create(ControllerIDs.GLJournalLinkedToApproval);
			}

			return ZControllerFactory.Create(ControllerIDs.GLJournalApproval);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new GLJournalApprovalFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new GLJournalApprovalRequestCollection(Factory);
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.GLJournalApproval; }
		}

		protected override TransactionApprovalBulk<GLJournal, GLJournalApprovalRequest, GLJournalApprovalRequestDetails> GetNewApprovalBulk(BusinessObjectFactory factory, ISecurityOverrideProvider interactiveSecurityOverrideProvider, params GLJournalApprovalRequest[] approvalRequests)
		{
			var securityOverrideProvider = AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.Value ? interactiveSecurityOverrideProvider : new DefaultAccessSecurityProvider();
			return new GLJournalApprovalBulk(factory, securityOverrideProvider, approvalRequests);
		}

		protected override ZForm GetNewApprovalBulkForm(TransactionApprovalBulk<GLJournal, GLJournalApprovalRequest, GLJournalApprovalRequestDetails> bizo, TransactionApprovalFormModes actionMode)
		{
			return new GLJournalApprovalBulkForm((GLJournalApprovalBulk)bizo, actionMode);
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());

			menuItems.Add(new ZMenuItem(ResString.GetMultilingualString("4F6DA938-3C27-4852-9AAD-6251A827F3BB", "&Post"), HandlePost));

			return menuItems.ToArray();
		}

		protected override IModuleDecisionProvider CreateDefaultModuleDecisionProvider()
		{
			return new DecisionProvider(this);
		}

		protected override IZForm ShowEditForm(BusinessObject selectedBusinessObject)
		{
			IZForm result = null;
			var newFactory = new BusinessObjectFactory();
			newFactory.SetContext(GLJournalApprovalRequest.Context.Editing);
			var selectedRequestInNewFactory = newFactory.Load<GLJournalApprovalRequest>(selectedBusinessObject.PK);
			if (selectedRequestInNewFactory != null)
			{
				if (selectedRequestInNewFactory.XP_ApprovalStatus != Constants.GenApprovalRequestApprovalStatus.Requested)
				{
					Globals.Message.ShowInformation(Res.GetString("2643C6BE-A215-4C28-AC24-DD5FB9141C62", "Can't Edit request ({0}) - Only requests with status 'Requested' can be edited.", selectedRequestInNewFactory.ReferenceID));
				}
				else
				{
					var journalToEdit = selectedRequestInNewFactory.GetLinkedJournal().journal;
					if (journalToEdit == null)
					{
						Globals.Message.ShowError(Res.GetString("828AEC52-0399-401C-9F33-04215784AE9F", "A transaction can't be found for request ({0}).", selectedRequestInNewFactory.ReferenceID));
					}
					else
					{
						var controller = GetNewController(journalToEdit);
#if DEBUG
						SetLastControllerForTest(controller);
#endif
						result = controller.ShowEditForm(journalToEdit);
					}
				}
			}

			return result;
		}

		void HandlePost(object sender, EventArgs e)
		{
			var securityCheckPoint = Env.Security.GLJournalApprovalPost;
			if (!securityCheckPoint.IsAllowed)
			{
				securityCheckPoint.ShowError();
			}
			else
			{
				ZGuid[] selectedRequestPKs = Grid.SelectedElements.Select(x => x.PK).ToArray();
				if (!selectedRequestPKs.Any())
				{
					Globals.Message.ShowInformation(Res.GetString("7e2a8d3e-2ce5-4ea0-ba11-ef7b3337bad3", "Please select a record in the grid."));
				}
				else
				{
					var errorMessages = new ZStringBuilder();
					var journalsToPost = new List<GLJournal>();
					foreach (var selectedRequestPK in selectedRequestPKs)
					{
						var newFactory = new BusinessObjectFactory();
						newFactory.SetContext(GLJournalApprovalRequest.Context.Posting);
						var request = newFactory.Load<GLJournalApprovalRequest>(selectedRequestPK);
						if (request.XP_ApprovalStatus != Constants.GenApprovalRequestApprovalStatus.Approved)
						{
							errorMessages.AppendLine(Res.GetString("AC38634A-82A0-4977-A4B7-5FEAE916E2B7", "Request ({0}) - Only requests with status 'Approved' can be posted/reversed.", request.ReferenceID));
						}
						else
						{
							var journalToPost = request.GetLinkedJournal().journal;
							if (journalToPost == null)
							{
								errorMessages.AppendLine(Res.GetString("58f9f2de-6810-40da-b8b4-3251d6415157", "A transaction can't be found for request ({0}).", request.ReferenceID));
							}
							else
							{
								try
								{
									journalToPost.RelinkEDocsFromRequestToNewJournal(request);
									journalsToPost.Add(journalToPost);
								}
								catch (ExternalStorageException)
								{
									errorMessages.AppendLine(AllocateErrorMessage.LastExternalStorageExceptionMessage);
								}
							}
						}
					}

					if (errorMessages.Length > 0)
					{
						var messageToShow = new ZStringBuilder(Res.GetString("CDDA017E-619D-4D3C-8C08-5932EED48474", "The following request(s) cannot be posted/reversed."));
						messageToShow.AppendLine();
						messageToShow.Append(errorMessages);
						Globals.Message.ShowInformation(messageToShow.ToString());
					}

					if (journalsToPost.Count > 0)
					{
						foreach (GLJournal journal in journalsToPost)
						{
							var controller = GetNewController(journal);
#if DEBUG
							SetLastControllerForTest(controller);
#endif
							IZForm result = controller.ShowEditForm(journal);
						}
					}
				}
			}
		}

#if DEBUG
		public ZController LastController_ForTestOnly;

		void SetLastControllerForTest(ZController controller)
		{
			if (Globals.IsTest)
			{
				LastController_ForTestOnly = controller;
			}
		}
#endif
	}
}
