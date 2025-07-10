using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.GUI.WarehouseExtensions;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.ES.Business.ESConstants;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.GUI;

public partial class MessageUserControl : EU.GUI.MessageUserControl
{
	public MessageUserControl()
	{
		InitialSetup();
	}

	public MessageUserControl(JobDeclaration jobDeclaration) : base(jobDeclaration)
	{
		InitialSetup();
	}

	void InitialSetup()
	{
		InitializeComponent();
		SetUpEntryLinesMessagesTabControlPages();
		SetUpEntryHeaderColumns();
		SetUpEntryLineGridColumns();
	}

	protected override IPanelLayoutProvider GetNewEntryDetailsPanelLayout() => new EntryDetailsLayouts();

	protected override ZBool DynamicLayoutApplied => ZBool.True;

	MenuItem GenerateExitControlMenuItem { get; set; }
	MenuItem UpdateCSVClearanceMenuItem { get; set; }
	MenuItem RequestEffectiveDepCertMenuItem { get; set; }
	MenuItem PUERequestsMenuItem { get; set; }
	MenuItem ROHSRAEEMenuItem { get; set; }
	MenuItem COMMenuItem { get; set; }
	MenuItem ECOMenuItem { get; set; }
	MenuItem CreateSupplementaryFromSimplifiedMenuItem { get; set; }

	void SetUpEntryLinesMessagesTabControlPages()
	{
		var newEntryLine = (ZTabPage)Controls.Find("NewEntryDetailsTabPage", true).FirstOrDefault();
		var expectedTabPagesInOrder = new Dictionary<ZInt, ZTabPage>()
		{
			{ 0, newEntryLine },
			{ 1, EntryLinesTabPage },
			{ 2, AnnexTabPage },
			{ 3, MessageTabPage },
		}.ToImmutableDictionary();

		OrderTabPages(EntryLinesMessagesTabControl, expectedTabPagesInOrder);
	}

	void OrderTabPages(ZTabControl tabControl, IReadOnlyDictionary<ZInt, ZTabPage> expectedTabPagesInOrder)
	{
		Argument.NotNull(tabControl, nameof(tabControl));
		Argument.NotNull(expectedTabPagesInOrder, nameof(expectedTabPagesInOrder));

		var allTabPages = tabControl.AllTabPages.Cast<ZTabPage>();
		var unknownTabPages = allTabPages.Except(expectedTabPagesInOrder.Values).ToArray();
		var tabPagesVisibilityDictionary = allTabPages.ToDictionary(x => x, x => x.TabVisible);

		tabControl.TabPages.Clear();
		foreach (var expectedTabPage in expectedTabPagesInOrder.Values)
		{
			if (allTabPages.Contains(expectedTabPage))
			{
				tabControl.TabPages.Add(expectedTabPage);
			}
		}
		tabControl.TabPages.AddRange(unknownTabPages);

		foreach (var tabPage in allTabPages)
		{
			tabPage.TabVisible = tabPagesVisibilityDictionary[tabPage];
		}
	}

	void SetUpEntryHeaderColumns()
	{
		var issueDateColumnStyle = EntriesBoundGrid.GetColumnStyle("CusEntryNumber+CE_IssueDate");
		if (issueDateColumnStyle != null)
		{
			EntriesBoundGrid.ColumnStyles.Remove(issueDateColumnStyle);
		}
		EntriesBoundGrid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
		{
			new ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("A19A586F-AF33-4FE0-A6B9-58672FFBAAB9", "Acceptance Date"),
				ColumnName = CusEntryHeader.Schema.MovementReferenceNumberIssueDate,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Long
			},
			new ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("521E8487-F886-4964-87C0-5307B2AE169E", "Circuit"),
				ColumnName = CusEntryHeader.Schema.FormattedCircuit,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60),
			},
			new ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("5E708D4B-22A7-4A66-BD26-7B904E190747", "Circuit Can"),
				ColumnName = CusEntryHeader.Schema.FormattedCircuitCan,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60),
			},
			new ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("AD1D605C-F278-4599-9BCD-C0A728ACA2B5", "CSV Clearance"),
				ColumnName = CusEntryHeader.Schema.CSVClearance,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			},
		});

		EntriesBoundGrid.ContextMenu.MenuItems.Add(Res.GetString("50402c1b-63ed-42a5-ac1a-04b2e0172932", "Reset Canceled Entry"), ResetCancelledEntry);

		EntriesBoundGrid.ContextMenu.MenuItems.Add(Res.GetString("137261e8-2200-49df-8855-7add6b354edb", "Check for Inbox Notifications"), RequestInboxNotifications);

		GenerateExitControlMenuItem = EntriesBoundGrid.ContextMenu.MenuItems.Add(Res.GetString("e302ccef-791b-46c2-a7d2-b19beed8949a", "Generate Exit Control"), GenerateExitControlFromEntries);

		UpdateCSVClearanceMenuItem = EntriesBoundGrid.ContextMenu.MenuItems.Add(Res.GetString("e36166e9-9beb-4706-9801-b96bd674efe7", "Update CSV Clearance"), UpdateCSVClearanceClick);

		EntriesBoundGrid.ContextMenu.MenuItems.Add(Res.GetString("906DD80F-7D2E-4E55-A0F9-FCE99605A44A", "View on Customs Website"), ViewOnCustomsWebsite);

		EntriesBoundGrid.ContextMenu.MenuItems.Add(Res.GetString("1548C1B9-1A33-456A-9731-20ED48236180", "Re-enable Annexes Messages"), ReenableAnnexesMessagesClick);

		CreateSupplementaryFromSimplifiedMenuItem = EntriesBoundGrid.ContextMenu.MenuItems.Add(Res.GetString("82B41375-B537-4C0C-AF55-E6ACA95F6226", "Create Supplementary Entry from Simplified Entry"));
		CreateSupplementaryFromSimplifiedMenuItem.MenuItems.Add(Res.GetString("D7174884-2DEF-4193-9069-FF9F0FC408EF", "New Related Declaration"), NewRelatedDeclaration);
		CreateSupplementaryFromSimplifiedMenuItem.MenuItems.Add(Res.GetString("3A3F6A31-8309-4B3D-B266-27FBD8DA8663", "New Entry and Instruction"), NewEntryAndInstruction);

		RequestEffectiveDepCertMenuItem = EntriesBoundGrid.ContextMenu.MenuItems.Add(Res.GetString("D3EC1E4B-CA3C-4F2F-B51F-769D04E5B656", "Request Certificate of Effective Departure"), RequestEffectiveDepCertClick);

		PUERequestsMenuItem = EntriesBoundGrid.ContextMenu.MenuItems.Add(Res.GetString("D0E3B0E5-B103-40AD-B4D7-D1AD542E35FF", "PUE Requests"));
		PUERequestsMenuItem.MenuItems.Add(Res.GetString("82805E49-C519-446F-B228-8A72E593862C", "Send Annex Documents"), SendAnnexDocumentsPUE);
		PUERequestsMenuItem.MenuItems.Add(Res.GetString("E5626626-881E-45C7-BF1D-EA2E8E385916", "Send Message"), SendMessagePUE);

		ROHSRAEEMenuItem = PUERequestsMenuItem.MenuItems.Add(Res.GetString("72133D96-763B-4E53-A2E8-78BBEAECD570", "ROHS-RAEE"));
		ROHSRAEEMenuItem.MenuItems.Add(Res.GetString("E4E44476-E843-48E3-A858-D631CBB48855", "Request Certificate"), RequestCertificateROHSRAEE);
		ROHSRAEEMenuItem.MenuItems.Add(Res.GetString("2BEAAA79-C712-4AC1-8E8A-9AE7CCFA9D8A", "Send Additional Data"), SendAdditionalDataROHSRAEE);
		ROHSRAEEMenuItem.MenuItems.Add(Res.GetString("5856789C-37D6-465F-BFE9-0CB0BF18B1ED", "Query Existing Certificates"), QueryExistingCertificatesROHSRAEE);

		COMMenuItem = PUERequestsMenuItem.MenuItems.Add(Res.GetString("895199D0-6B9E-4DB2-B504-951AE0DEBF2E", "COM"));
		COMMenuItem.MenuItems.Add(Res.GetString("C5050FD7-37A7-4645-99A7-ACB5C02B0F6F", "Request Certificate"), RequestCertificateCOM);
		COMMenuItem.MenuItems.Add(Res.GetString("7DE61EB3-F1C5-4171-8947-9D860BD26D2B", "Send Additional Data"), SendAdditionalDataCOM);
		COMMenuItem.MenuItems.Add(Res.GetString("BED17A69-5E54-4080-89A5-16D950ACE461", "Query Existing Certificates"), QueryExistingCertificatesCOM);

		ECOMenuItem = PUERequestsMenuItem.MenuItems.Add(Res.GetString("DDE9F0C7-8286-432E-814F-E9786DA551A0", "ECO"));
		ECOMenuItem.MenuItems.Add(Res.GetString("253CB8D9-AD75-4DDC-8CBD-BF6B866DFD35", "Request Certificate"), RequestCertificateECO);
		ECOMenuItem.MenuItems.Add(Res.GetString("A97752D9-350C-4EEC-8616-F762BC800FC8", "Send Additional Data"), SendAdditionalDataECO);
		ECOMenuItem.MenuItems.Add(Res.GetString("CEA7E17F-3939-4639-9981-48C2389D62A2", "Query Existing Certificates"), QueryExistingCertificatesECO);
	}

	protected override void HandleDeclarationControlVisibilityChangedCore()
	{
		base.HandleDeclarationControlVisibilityChangedCore();

		if (GenerateExitControlMenuItem != null)
		{
			GenerateExitControlMenuItem.Visible = JobDeclaration.IsExport && ObjectFactory.Get<Integration.Customs.EUExitControl.IExitControlCustomsDataRegistry>().IsExitControlPluginEnabledForCurrentCompany;
		}

		if (UpdateCSVClearanceMenuItem != null)
		{
			UpdateCSVClearanceMenuItem.Visible = JobDeclaration.IsExport || JobDeclaration.IsImport;
		}

		if (RequestEffectiveDepCertMenuItem != null)
		{
			RequestEffectiveDepCertMenuItem.Visible = JobDeclaration.IsExport;
		}

		if (PUERequestsMenuItem != null)
		{
			PUERequestsMenuItem.Visible = JobDeclaration.IsImport;
		}

		if (CreateSupplementaryFromSimplifiedMenuItem != null)
		{
			SetCreateSupplementaryFromSimplifiedMenuItemVisibility();
		}
	}

	protected bool SaveIfRequiredAndConfirmedByUser()
	{
		var okToContinue = true;
		var declaration = CurrentDataItem;
		var topLevelBusinessObject = (BusinessObject)declaration.Shipment ?? declaration;
		if (topLevelBusinessObject.HasChanges)
		{
			var confirmedToSave = Globals.Message.Show(
									Res.GetString("22486BAD-3002-48B1-983C-0FD2318BEC92", "The Job has not yet been saved. Do you want to save and proceed?"),
									Res.GetString("6E4A2482-69A3-4F86-BEFA-64DEB79EDF73", "Save Job"),
									MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.Yes;

			okToContinue = confirmedToSave && (this.FireSaveButton() == ContinueWithSave.Yes);
		}

		return okToContinue;
	}

	void ResetCancelledEntry(object sender, EventArgs ev)
	{
		int entries = 0;

		if (EntriesBoundGrid.SelectedElements.Length == 0)
		{
			Globals.Message.Show(SelectRowFirstMessage);
		}
		else if (EntriesBoundGrid.SelectedElements.Any(x => ((CusEntryHeader)x).CH_EntryStatus != EntryStatusCodes.Cancelled && ((CusEntryHeader)x).CH_EntryStatus != EntryStatusCodes.Invalidated))
		{
			Globals.Message.Show(Res.GetString("208e0036-8e4c-4806-a279-3de2f557539c", "Please select only Canceled or Invalidated Entries"));
		}
		else
		{
			if (SaveIfRequiredAndConfirmedByUser())
			{
				var hasConfirmed = false;
				foreach (CusEntryHeader entryHeader in EntriesBoundGrid.SelectedElements)
				{
					if (hasConfirmed || entryHeader.Declaration.MessageInitiator.ShowUserConfirmation(Res.GetString("67c4571b-a144-4a9a-853a-b3a7b6ccc571", "Are you sure you want to reset this Canceled Entry and delete the sending data?"),
						(NoResString)"Reset Cancelled Entry", Res.GetString("94aae28b-599c-4c32-a29b-e00b712f9299", "If you are absolutely sure you want to reset this Canceled Entry, please type: "),
						Res.GetString("e41c3d44-826c-4583-bf40-b0cfe1785ab5", "yes")))
					{
						ResetAndSaveCancelledEntry(entryHeader);
						entries++;
						hasConfirmed = true;
					}
				}
				if (entries == 1)
				{
					Globals.Message.Show(Res.GetString("9c1c6036-4277-4a48-8f8b-18660b056d33", "One Entry was reset and sending data deleted"));
				}
				else
				{
					Globals.Message.Show(Res.GetString("7c5376bc-53ed-4556-9a11-5b8a09a8cf2f", "{0} Entries were reset and sending data deleted", entries.ToString(Culture.Current)));
				}
			}
		}
	}

	void ResetAndSaveCancelledEntry(CusEntryHeader entryHeader)
	{
		try
		{
			var factory = new BusinessObjectFactory();
			var newFactoryEntryHeader = factory.Load<CusEntryHeader>(entryHeader.PK);
			newFactoryEntryHeader.ResetCancelledEntry();
			factory.Save();
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
	}

	void RequestInboxNotifications(object sender, EventArgs ev)
	{
		if (EntriesBoundGrid.SelectedElements.Length == 0)
		{
			Globals.Message.Show(SelectRowFirstMessage);
		}
		else
		{
			if (SaveIfRequiredAndConfirmedByUser())
			{
				bool continueWithSend = CheckBrokerBeforeSending(out var messageSendingObject, out var declaration);

				if (continueWithSend)
				{
					int messagesCreated = 0;

					var messagesTypesToSend = new List<ZString>();
					foreach (CusEntryHeader entryHeader in EntriesBoundGrid.SelectedElements)
					{
						var messageTypesList = GetInboxRequestMessageTypesForEntry(entryHeader);

						foreach (var messageType in messageTypesList)
						{
							CreateAndSaveInboxRequestMessage(entryHeader, messageType, ((ICertificateProvider)messageSendingObject).CertificateName);
							messagesCreated++;
							if (!messagesTypesToSend.Contains(messageType))
							{
								messagesTypesToSend.Add(messageType);
							}
						}
					}

					if (messagesTypesToSend.Any())
					{
						CreateAndSaveInboxListMessages(messagesTypesToSend.ToArray());
					}

					if (messagesCreated == 1)
					{
						Globals.Message.Show(ResString.GetMultilingualString("574B3830-9154-4DBB-8D78-54DD545296FE", "1 In-box Notification request created"));
					}
					else
					{
						Globals.Message.Show(ResString.GetMultilingualString("917C114B-0A2D-4E7C-AB4D-7CE24E9EE5EC", "{0} In-box Notification requests created", messagesCreated.ToString(Culture.Current)));
					}
				}
			}
		}
	}

	ZString[] GetInboxRequestMessageTypesForEntry(CusEntryHeader entryHeader)
	{
		var messageTypes = new List<ZString>();

		if (entryHeader.IsExportUCC6)
		{
			messageTypes.AddRange(entryHeader.GetInboxRequestMessageTypesForAES());
		}
		else if (entryHeader.CH_EntryStatus == EntryStatusCodes.PreDeclarationAccepted)
		{
			if (entryHeader.IsImport)
			{
				messageTypes.Add(entryHeader.IsH2Style ? DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2 : DeclarationMessageTypeList.Codes.InBoxNotificationForImport);
			}
			else
			{
				messageTypes.Add(DeclarationMessageTypeList.Codes.InBoxNotificationForExport);
			}
		}
		return messageTypes.ToArray();
	}

	void CreateAndSaveInboxRequestMessage(CusEntryHeader entryHeader, ZString messageType, ZString certName)
	{
		try
		{
			var factory = new BusinessObjectFactory();
			MessageRequest.CreateEDIMessageForInboxRequest(factory, entryHeader, entryHeader.PK, entryHeader.TablePrefix, entryHeader.MovementReferenceNumber, entryHeader.CH_BGMReference, messageType, certName);
			factory.Save();
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
	}

	void CreateAndSaveInboxListMessages(ZString[] messagesTypesToSend)
	{
		try
		{
			var factory = new BusinessObjectFactory();
			var inboxListRequestSender = new InboxListRequestSender(null);
			inboxListRequestSender.CreateListPollingEDIMessages(factory, specificTypes: messagesTypesToSend);
			factory.Save();
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
	}

	bool CheckBrokerBeforeSending(out MessageSendingObject messageSendingObject, out JobDeclaration declaration, bool shouldEditMessagePopUpResponse = true)
	{
		messageSendingObject = null;
		bool continueWithSend = false;
		declaration = (JobDeclaration)CurrentDataItem;
		var broker = declaration.CusAgent;
		if (broker == null || CertificateHasMessageErrors(declaration))
		{
			Globals.Message.ShowError(ResString.GetMultilingualString("2204F761-C002-4779-A246-154EFE4AC7BA", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options."));
		}
		else
		{
			messageSendingObject = new MessageSendingObject(declaration, broker);

			if (shouldEditMessagePopUpResponse)
			{
				messageSendingObject.ShouldEditMessage = MessageEditHelper.GetShouldEditMessagePopUpResponse(DialogResult.No);
			}

			continueWithSend = true;
		}
		return continueWithSend;

		ZBool CertificateHasMessageErrors(JobDeclaration dec)
		{
			dec.Validation.ValidateJE_CustomsProfile();
			return dec.JE_CustomsProfileInfo.HasMessageErrors();
		}
	}

	void GenerateExitControlFromEntries(object sender, EventArgs ev)
	{
		if (EntriesBoundGrid.SelectedElements.Length == 0)
		{
			Globals.Message.Show(SelectRowFirstMessage);
		}
		else
		{
			if (SaveIfRequiredAndConfirmedByUser())
			{
				var movementsAddedOrUpdated = 0;
				var nonUpdatableEntriesMessage = ZString.Empty;

				PopulateExitControlData populateDataEDI = null;
				PopulateExitControlData populateDataAES = null;

				var declaration = (JobDeclaration)CurrentDataItem;
				var factory = new BusinessObjectFactory();
				var newFactoryDeclaration = factory.Load<JobDeclaration>(declaration.PK);

				(var acceptedEntriesEDI, var acceptedEntriesAES) = GetAcceptedEntries();
				if (!acceptedEntriesEDI.IsNullOrEmpty())
				{
					populateDataEDI = AddOrUpdateMovementsEDI(newFactoryDeclaration, acceptedEntriesEDI);
				}
				if (!acceptedEntriesAES.IsNullOrEmpty())
				{
					populateDataAES = AddOrUpdateMovementsAES(newFactoryDeclaration, acceptedEntriesAES);
				}

				if (populateDataEDI != null || populateDataAES != null)
				{
					try
					{
						factory.Save();
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ZExceptionReporting.HandleSaveException(ex);
					}

					movementsAddedOrUpdated += populateDataEDI?.ObjectsAddedOrUpdated ?? ZInt.Zero;
					movementsAddedOrUpdated += populateDataAES?.ObjectsAddedOrUpdated ?? ZInt.Zero;

					if (!populateDataEDI?.NonUpdatableEntriesMRNs.IsNullOrEmpty() ?? false)
					{
						nonUpdatableEntriesMessage += GetPopUpTextForNonUpdatableMRNs(populateDataEDI.NonUpdatableEntriesMRNs) + LineBreakForMessage;
					}

					if (!populateDataAES?.ErrorMessages.IsNullOrEmpty() ?? false)
					{
						nonUpdatableEntriesMessage += string.Join(LineBreakForMessage, populateDataAES.ErrorMessages) + LineBreakForMessage;
					}

					if (!populateDataAES?.NonUpdatableEntriesMRNs.IsNullOrEmpty() ?? false)
					{
						nonUpdatableEntriesMessage += GetPopUpTextForNonUpdatableMRNs(populateDataAES.NonUpdatableEntriesMRNs) + LineBreakForMessage;
					}
				}

				var successfulMessage = GetMessageSendSuccessful(movementsAddedOrUpdated);
				Globals.Message.Show(nonUpdatableEntriesMessage.IsEmpty ? (string)successfulMessage : nonUpdatableEntriesMessage + successfulMessage);
			}
		}
	}

	(IEnumerable<CusEntryHeader> ediEntries, IEnumerable<CusEntryHeader> aesEntries) GetAcceptedEntries()
	{
		IEnumerable<CusEntryHeader> acceptedEntriesEDI = null;
		IEnumerable<CusEntryHeader> acceptedEntriesAES = null;

		var selectedEntries = EntriesBoundGrid.SelectedElements.Cast<CusEntryHeader>();
		if (selectedEntries.Any(x => x.IsAcceptedExport))
		{
			acceptedEntriesEDI = selectedEntries.Where(x => x.IsAcceptedExport && x.IsExportNoUCC6);
			acceptedEntriesAES = selectedEntries.Where(x => x.IsAcceptedExport && x.IsExportUCC6);
		}

		return (acceptedEntriesEDI, acceptedEntriesAES);
	}

	PopulateExitControlData AddOrUpdateMovementsEDI(JobDeclaration declaration, IEnumerable<CusEntryHeader> acceptedEntries)
	{
		var populateData = new PopulateExitControlData();
		IEnumerable<CusEntryHeader> updatableEntries = null;
		IEnumerable<CusEntryHeader> entriesToAddOrUpdate = null;

		var exitHeader = declaration.GetOrCreateExitControlHeader();
		var details = exitHeader.CusExitDetails;

		if (details.Count > 0)
		{
			(updatableEntries, populateData.NonUpdatableEntriesMRNs) = RemoveNonUpdatableEntries(declaration, acceptedEntries, details);

			entriesToAddOrUpdate = GetConfirmationToUpdateEntries(declaration, updatableEntries, details);
		}
		else
		{
			entriesToAddOrUpdate = acceptedEntries;
		}

		populateData.ObjectsAddedOrUpdated = declaration.GenerateExitControlFromEntries(declaration.GetMRNFromEntries(entriesToAddOrUpdate), exitHeader);

		return populateData;
	}

	(IEnumerable<CusEntryHeader>, IEnumerable<ZString>) RemoveNonUpdatableEntries(JobDeclaration declaration, IEnumerable<CusEntryHeader> acceptedEntries, CusExitDetailCollection details)
	{
		var nonUpdatableEntries = declaration.GetEntriesWithMRNInAcceptedExitDetails(acceptedEntries, details);
		var updatableEntries = acceptedEntries.Except(nonUpdatableEntries);
		return (updatableEntries, declaration.GetMRNFromEntries(nonUpdatableEntries));
	}

	IEnumerable<CusEntryHeader> GetConfirmationToUpdateEntries(JobDeclaration declaration, IEnumerable<CusEntryHeader> acceptedEntries, CusExitDetailCollection details)
	{
		var updatableEntries = declaration.GetEntriesWithMRNInNotAcceptedExitDetails(acceptedEntries, details);
		var entriesToAddOrUpdate = acceptedEntries.Except(updatableEntries).ToList();

		foreach (var entry in updatableEntries)
		{
			if (GetConfirmationToOverwriteExitDetail(entry.MovementReferenceNumber))
			{
				entriesToAddOrUpdate.Add(entry);
			}
		}
		return entriesToAddOrUpdate;
	}

	PopulateExitControlData AddOrUpdateMovementsAES(JobDeclaration declaration, IEnumerable<CusEntryHeader> acceptedEntries)
	{
		var populateExitControlManager = ObjectFactory.Get<IPopulateExitControlManager>();
		return populateExitControlManager.AddOrUpdateExitControl(declaration, acceptedEntries, GetConfirmationToOverwriteExitDetail);
	}

	bool GetConfirmationToOverwriteExitDetail(ZString mrnCode)
	{
		return Globals.Message.Show(
					Res.GetString("2A8D4951-2A07-44C4-B449-9B4143F14ABC", "A movement for MRN {0} already exists in Exit Control list. Do you want to overwrite it?", mrnCode),
					Res.GetString("EE896A62-5D50-4E87-8FD4-CCE64A2BEF20", "Overwrite Movement"),
					MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.Yes;
	}

	ZString GetPopUpTextForNonUpdatableMRNs(IEnumerable<ZString> mrnCodes)
	{
		var message = ZString.Empty;
		if (mrnCodes.Any())
		{
			ZString mrnsString = string.Join(", ", mrnCodes.ToArray());
			var numMRNCodes = mrnCodes.Count();
			message = numMRNCodes == 1
							? ResString.GetMultilingualString("2ED2FFC5-0918-46D4-9BAD-7EF2CA27B3E9", "No movements have been generated for MRN {0} because it is sent or accepted", mrnsString)
							: ResString.GetMultilingualString("312E1D7C-69B2-4963-B586-7E29CD19EE3F", "No movements have been generated for MRN {0} because they are sent or accepted", mrnsString);
		}
		return message;
	}

	ZString GetMessageSendSuccessful(int movementsAddedOrUpdated) => ResString.GetMultilingualString("0A79C935-8FC7-45A9-AB2E-F03135C41417", "{0} EAL(s) created successfully", movementsAddedOrUpdated.ToString(Culture.Current));

	void UpdateCSVClearanceClick(object sender, EventArgs ev)
	{
		var selectedElements = EntriesBoundGrid.SelectedElements;
		if (selectedElements.Length != 1)
		{
			Globals.Message.Show(Res.GetString("1c60c360-9a6c-4926-81d2-79aa8110b189", "Please select a single row first"));
		}
		else
		{
			var entryHeaderSelected = (CusEntryHeader)selectedElements[0];
			var isImportT2LPOUS2 = entryHeaderSelected.IsImport && entryHeaderSelected.IsT2L && entryHeaderSelected.ZG_POUSVersion > 1;
			var mrnCodeIsEmpty = (isImportT2LPOUS2 && entryHeaderSelected.T2CMovementReferenceNumber.IsEmpty)
								|| (!entryHeaderSelected.IsT2C && !isImportT2LPOUS2 && entryHeaderSelected.MovementReferenceNumber.IsEmpty)
								|| (entryHeaderSelected.IsT2C && entryHeaderSelected.T2CMovementReferenceNumber.IsEmpty);
			if (mrnCodeIsEmpty || entryHeaderSelected.EntryInstruction == null
				|| (entryHeaderSelected.IsImport && entryHeaderSelected.IsT2L && entryHeaderSelected.ZG_POUSVersion <= 1))
			{
				Globals.Message.Show(Res.GetString("3adf9289-cb16-4f36-87f7-c0b3b5a99f27", "Please select only an entry with MRN"));
			}
			else if (SaveIfRequiredAndConfirmedByUser())
			{
				bool continueWithSend = CheckBrokerBeforeSending(out var messageSendingObject, out var declaration, false);
				if (continueWithSend)
				{
					UpdateCsvClearanceWithUserInput(entryHeaderSelected, messageSendingObject, declaration);
				}
			}
		}
	}

	void UpdateCsvClearanceWithUserInput(CusEntryHeader entryHeaderSelected, MessageSendingObject messageSendingObject, JobDeclaration declaration)
	{
		var csvCodeInfo = CsvCodeInfo.LoadNew(entryHeaderSelected);
		using (var updateCSVForm = GetUpdateCSVClearanceForm(csvCodeInfo))
		{
			if (ZFormModaliser.ShowDialogWithoutDispose(updateCSVForm) == DialogResult.OK)
			{
				csvCodeInfo.Validation.ValidateAll();

				if (!csvCodeInfo.CsvCodeFromUserInfo.HasErrors()
					&& !csvCodeInfo.ClearanceDateFromUserInfo.HasErrors()
					&& !csvCodeInfo.SecondaryCsvCodeFromUserInfo.HasErrors()
					&& !csvCodeInfo.ThirdCsvCodeFromUserInfo.HasErrors())
				{
					UpdateCSVClearanceAndTriggerDocRequests(entryHeaderSelected, csvCodeInfo, messageSendingObject, declaration);
				}
				else
				{
					Globals.Message.Show(Res.GetString("05EEC3E0-8BDA-42CA-89B3-A6103C8C62EA", "Nothing was updated because there were errors"));
				}
			}
		}
	}

	void UpdateCSVClearanceAndTriggerDocRequests(CusEntryHeader entryHeader, CsvCodeInfo csvCodeInfo, MessageSendingObject messageSendingObject, JobDeclaration declaration)
	{
		var factory = new BusinessObjectFactory();
		var newFactoryEntryHeader = factory.Load<CusEntryHeader>(entryHeader.PK);
		var newFactoryDeclaration = factory.Load<JobDeclaration>(declaration.PK);

		var oldCSVClearance = newFactoryEntryHeader.CSVClearance;
		newFactoryEntryHeader.SetClearanceDate(csvCodeInfo.ClearanceDateFromUser);
		newFactoryEntryHeader.SetCSVClearance(csvCodeInfo.CsvCodeFromUser);
		UpdateSecondaryCSVClearance(newFactoryEntryHeader, csvCodeInfo.SecondaryCsvCodeFromUser);
		newFactoryEntryHeader.SetCSVExitCertificate(csvCodeInfo.ThirdCsvCodeFromUser);

		var messagesSentCount = SendExportDocumentRequestToCustoms(((ICertificateProvider)messageSendingObject).CertificateName, newFactoryEntryHeader, newFactoryDeclaration, oldCSVClearance);

		try
		{
			factory.Save();
			ShowCsvUpdateAndDocCaptureRequestSendingSuccessMessage(messagesSentCount);
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
	}

	void UpdateSecondaryCSVClearance(CusEntryHeader entryHeader, ZString csvCodeFromUser)
	{
		if (!entryHeader.IsT2L && !entryHeader.IsExsSubStyle && !entryHeader.IsT2C) 
		{
			if (entryHeader.IsExport)
			{
				entryHeader.SetT2LClearance(csvCodeFromUser);
			}
			else if (entryHeader.IsImport)
			{
				entryHeader.SetCSVImportCertificate(csvCodeFromUser);
			}
		}
	}

	int SendExportDocumentRequestToCustoms(ZString certificateName, CusEntryHeader entryHeader, JobDeclaration declaration, ZString oldCSVClearance)
	{
		int messagesSentCount = ZInt.Zero;
		var isImportT2LPOUS2 = declaration.IsImport && entryHeader.IsT2L && entryHeader.ZG_POUSVersion > 1;
		var mrnCode = !entryHeader.MovementReferenceNumber.IsEmpty && !entryHeader.IsT2C && !isImportT2LPOUS2 ? entryHeader.MovementReferenceNumber : entryHeader.T2CMovementReferenceNumber;
		if (!mrnCode.IsEmpty && !certificateName.IsEmpty)
		{
			if (declaration.IsExport && entryHeader.IsExsSubStyle)
			{
				var fileNamesDict = EDocHelper.FileNamesDictEXS(entryHeader.MovementReferenceNumber, oldCSVClearance);
				EDocHelper.ChangeExistingEDocsFileNames(entryHeader, fileNamesDict, entryHeader);
				var exsDocRequest = new EXSDocumentRequest(entryHeader, certificateName);
				messagesSentCount += exsDocRequest.RequestMissingDocuments();
			}
			else if (entryHeader.IsExportNoUCC6 && !entryHeader.IsT2L)
			{
				var fileNamesDict = EDocHelper.FileNamesDictExport(entryHeader.MovementReferenceNumber, oldCSVClearance);
				EDocHelper.ChangeExistingEDocsFileNames(entryHeader, fileNamesDict, entryHeader);
				var exportDocRequest = new ExportDocumentRequest(entryHeader, certificateName);
				messagesSentCount += exportDocRequest.RequestMissingDocuments();
			}
			else if (entryHeader.IsExportUCC6 && !entryHeader.IsT2L)
			{
				var fileNamesDict = EDocHelper.FileNamesDictExportAES(entryHeader.MovementReferenceNumber, oldCSVClearance);
				EDocHelper.ChangeExistingEDocsFileNames(entryHeader, fileNamesDict, entryHeader);
				var exportDocRequest = new ExportAESDocumentRequest(entryHeader, certificateName);
				messagesSentCount += exportDocRequest.RequestMissingDocuments();
			}
			else if (declaration.IsExport && entryHeader.IsT2L)
			{
				var fileNamesDict = EDocHelper.FileNamesDictT2LExpeditionAmendment(entryHeader.MovementReferenceNumber, oldCSVClearance);
				EDocHelper.ChangeExistingEDocsFileNames(entryHeader, fileNamesDict, entryHeader);
				var t2lExpeditionDocRequest = new T2LExpeditionDocumentRequest(entryHeader, certificateName);
				messagesSentCount += t2lExpeditionDocRequest.RequestMissingDocuments();
			}
			else if (declaration.IsUCC6AndIsImport && !entryHeader.IsT2C && !entryHeader.IsT2L)
			{
				var fileNamesDict = EDocHelper.FileNamesDictImport(entryHeader.MovementReferenceNumber, oldCSVClearance);
				EDocHelper.ChangeExistingEDocsFileNames(entryHeader, fileNamesDict, entryHeader);
				var importDocRequest = new ImportH1DocumentRequest(entryHeader, certificateName);
				messagesSentCount += importDocRequest.RequestMissingDocuments();
			}
			else if (declaration.IsImport && entryHeader.IsH2Style)
			{
				var fileNamesDict = EDocHelper.FileNamesDictDVD(entryHeader.MovementReferenceNumber, oldCSVClearance);
				EDocHelper.ChangeExistingEDocsFileNames(entryHeader, fileNamesDict, entryHeader);
				var importDocRequest = new DVDDocumentRequest(entryHeader, certificateName);
				messagesSentCount += importDocRequest.RequestMissingDocuments();
			}
			else if (declaration.IsImport && !entryHeader.IsT2C && !entryHeader.IsT2L)
			{
				var fileNamesDict = EDocHelper.FileNamesDictImport(entryHeader.MovementReferenceNumber, oldCSVClearance);
				EDocHelper.ChangeExistingEDocsFileNames(entryHeader, fileNamesDict, entryHeader);
				var importDocRequest = new ImportDocumentRequest(entryHeader, certificateName);
				messagesSentCount += importDocRequest.RequestMissingDocuments();
			}
			else if (declaration.IsImport && entryHeader.IsT2C)
			{
				var fileNamesDict = EDocHelper.FileNamesDictT2C(entryHeader.T2CMovementReferenceNumber, oldCSVClearance);
				EDocHelper.ChangeExistingEDocsFileNames(entryHeader, fileNamesDict, entryHeader);
				var t2lRequestDocRequest = new T2LClearanceDocumentRequest(entryHeader, certificateName);
				messagesSentCount += t2lRequestDocRequest.RequestMissingDocuments();
			}
			else if (isImportT2LPOUS2)
			{
				var fileNamesDict = EDocHelper.FileNamesDictT2LReception(entryHeader.T2CMovementReferenceNumber, oldCSVClearance);
				EDocHelper.ChangeExistingEDocsFileNames(entryHeader, fileNamesDict, entryHeader);
				var t2lRequestDocRequest = new T2LReceptionDocumentRequest(entryHeader, certificateName);
				messagesSentCount += t2lRequestDocRequest.RequestMissingDocuments();
			}
		}

		return messagesSentCount;
	}

	void ShowCsvUpdateAndDocCaptureRequestSendingSuccessMessage(int messagesSentCount)
	{
		Globals.Message.Show(ResString.GetMultilingualString("452CF336-DC40-4617-BC73-FF4603AF9DC9", "{0} Document Capture request(s) created", messagesSentCount.ToString(Culture.Current)));
	}

	void ViewOnCustomsWebsite(object sender, EventArgs ev)
	{
		if (EntriesBoundGrid.SelectedElements.Length == 0)
		{
			Globals.Message.Show(SelectRowFirstMessage);
		}
		else
		{
			foreach (CusEntryHeader entryHeader in EntriesBoundGrid.SelectedElements)
			{
				var linkWebPage = entryHeader.GetUrlToLaunch();
				LaunchUrlIfNotEmpty(linkWebPage);
			}
		}
	}

	void ReenableAnnexesMessagesClick(object sender, EventArgs ev)
	{
		var selectedElements = EntriesBoundGrid.SelectedElements;

		if (selectedElements.Length != 1)
		{
			Globals.Message.Show(Res.GetString("1c60c360-9a6c-4926-81d2-79aa8110b189", "Please select a single row first"));
			return;
		}

		var entryHeaderSelected = (CusEntryHeader)selectedElements[0];

		if (entryHeaderSelected.ZG_RequestDispatch != Enterprise.Customs.Business.YesNoList.Codes.Yes)
		{
			Globals.Message.Show(ResString.GetMultilingualString("D79366B1-9DA7-4B5B-971E-710F5BB055D1", "You have not sent any Annex message with Request Dispatch flag in the selected entry ({0})", entryHeaderSelected.CH_BGMReference));
			return;
		}

		if (entryHeaderSelected.CH_Status == Customs.Common.EU.MessageStatusList.Codes.AwaitingResponse)
		{
			Globals.Message.Show(ResString.GetMultilingualString("B02766C7-C320-4507-BEF9-BB6C3D59B487", "Cannot trigger this action while entry {0} is awaiting response", entryHeaderSelected.CH_BGMReference));
			return;
		}

		if (SaveIfRequiredAndConfirmedByUser() && ResetRequestDispatchAndSaveInSeparateFactory(entryHeaderSelected.PK))
		{
			Globals.Message.Show(ResString.GetMultilingualString("A3DB1F74-FC24-4FC7-A79C-987315D7C1AB", "Annexes Messages have been enabled by removing the Request Dispatch flag. You can now send a new Annex message for entry {0}", entryHeaderSelected.CH_BGMReference));
		}
	}

	bool ResetRequestDispatchAndSaveInSeparateFactory(ZGuid entryHeaderPk)
	{
		var factory = new BusinessObjectFactory();
		var newFactoryEntryHeader = factory.Load<CusEntryHeader>(entryHeaderPk);

		newFactoryEntryHeader.ZG_RequestDispatch = ZString.Empty;

		try
		{
			factory.Save();
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			ZExceptionReporting.HandleSaveException(ex);
			return false;
		}

		return true;
	}

	void SendAnnexDocumentsPUE(object sender, EventArgs ev)
	{
		var linkWebPage = ESCustomsDataRegistry.Instance.PueAnnexDocumentsUrl.Value;
		LaunchUrlIfNotEmpty(linkWebPage);
	}

	void SendMessagePUE(object sender, EventArgs ev)
	{
		var linkWebPage = ESCustomsDataRegistry.Instance.PueSendMessageUrl.Value;
		LaunchUrlIfNotEmpty(linkWebPage);
	}

	void RequestCertificateROHSRAEE(object sender, EventArgs ev)
	{
		var linkWebPage = ESCustomsDataRegistry.Instance.RohsRequestUrl.Value;
		LaunchUrlIfNotEmpty(linkWebPage);
	}

	void SendAdditionalDataROHSRAEE(object sender, EventArgs ev)
	{
		var linkWebPage = ESCustomsDataRegistry.Instance.RohsAdditionalDataUrl.Value;
		LaunchUrlIfNotEmpty(linkWebPage);
	}

	void QueryExistingCertificatesROHSRAEE(object sender, EventArgs ev)
	{
		var linkWebPage = ESCustomsDataRegistry.Instance.RohsStatusUrl.Value;
		LaunchUrlIfNotEmpty(linkWebPage);
	}

	void RequestCertificateCOM(object sender, EventArgs ev)
	{
		var linkWebPage = ESCustomsDataRegistry.Instance.ComRequestUrl.Value;
		LaunchUrlIfNotEmpty(linkWebPage);
	}

	void SendAdditionalDataCOM(object sender, EventArgs ev)
	{
		var linkWebPage = ESCustomsDataRegistry.Instance.ComAdditionalDataUrl.Value;
		LaunchUrlIfNotEmpty(linkWebPage);
	}

	void QueryExistingCertificatesCOM(object sender, EventArgs ev)
	{
		var linkWebPage = ESCustomsDataRegistry.Instance.ComStatusUrl.Value;
		LaunchUrlIfNotEmpty(linkWebPage);
	}

	void RequestCertificateECO(object sender, EventArgs ev)
	{
		var linkWebPage = ESCustomsDataRegistry.Instance.EcoRequestUrl.Value;
		LaunchUrlIfNotEmpty(linkWebPage);
	}

	void SendAdditionalDataECO(object sender, EventArgs ev)
	{
		var linkWebPage = ESCustomsDataRegistry.Instance.EcoAdditionalDataUrl.Value;
		LaunchUrlIfNotEmpty(linkWebPage);
	}

	void QueryExistingCertificatesECO(object sender, EventArgs ev)
	{
		var linkWebPage = ESCustomsDataRegistry.Instance.EcoStatusUrl.Value;
		LaunchUrlIfNotEmpty(linkWebPage);
	}

	void NewRelatedDeclaration(object sender, EventArgs ev)
	{
		var caption = Res.GetString("2E5AC8E6-A7B6-44C2-8B06-59D44D2C6C2F", "New Related Declaration");
		var message = Res.GetString("5C4938FA-0DE1-4466-94F8-A624760C415F", "A new Customs Declaration will be generated for each selected and accepted simplified Entry (type B or C and status CLP). Do you want to continue?");
		if (Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.No) == DialogResult.Yes)
		{
			//This will be refactored in Workflow 5
			Globals.Message.Show(Res.GetString("51AF517C-B2E7-4BCA-92CA-0A0178BE2AFD", "Create related declaration will be done in another workflow"));
		}
	}

	void NewEntryAndInstruction(object sender, EventArgs ev)
	{
		var caption = Res.GetString("9E6292AB-9EB2-4C74-A14F-54A6906C804A", "New Entry and Instruction");
		var message = Res.GetString("9806EAA4-098A-498B-AF65-C84E899E2290", "A new Entry Instruction and its associated Invoice Lines will be generated for each selected and accepted simplified Entry (type B or C and status CLP). Do you want to continue?");
		if (Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.No) == DialogResult.Yes)
		{
			//This will be refactored in Workflow 5
			Globals.Message.Show(Res.GetString("076301B6-1D07-4C7C-B566-4BE43AD78A7D", "Adding new entry and instructions will be done in another workflow"));
		}
	}

	void LaunchUrlIfNotEmpty(string url)
	{
		if (!string.IsNullOrEmpty(url))
		{
			WebUrlLauncher.Launch(url);
		}
	}

	void RequestEffectiveDepCertClick(object sender, EventArgs ev)
	{
		if (EntriesBoundGrid.SelectedElements.Length == 0)
		{
			Globals.Message.Show(SelectRowFirstMessage);
		}

		else if (EntriesBoundGrid.SelectedElements.Cast<CusEntryHeader>().Any(x => !x.CanRequestEffectiveDepartureCertificate))
		{
			Globals.Message.Show(ResString.GetMultilingualString("51948BF7-7593-4EF9-A019-D1140B7FE9FE", "Certificate Request is only available for Effective Departures (Entry Status = EFD and Entry Instruction A, B, C, X, Y or Z)"));
		}
		else
		{
			if (SaveIfRequiredAndConfirmedByUser())
			{
				bool continueWithSend = CheckBrokerBeforeSending(out var messageSendingObject, out var declaration);

				if (continueWithSend)
				{
					int entries = 0;
					foreach (CusEntryHeader entryHeader in EntriesBoundGrid.SelectedElements)
					{
						CreateAndSaveEffectiveDepCertRequestMessage(entryHeader, ((ICertificateProvider)messageSendingObject).CertificateName);
						entries++;
					}

					if (entries == 1)
					{
						Globals.Message.Show(ResString.GetMultilingualString("1718AF06-268D-4271-A7F4-3A9EDC67C160", "1 Certificate Request created"));
					}
					else
					{
						Globals.Message.Show(ResString.GetMultilingualString("EFFBAEA7-5FAE-49E0-86A1-61911B0040D4", "{0} Certificate Requests created", entries.ToString(Culture.Current)));
					}
				}
			}
		}
	}
	void CreateAndSaveEffectiveDepCertRequestMessage(CusEntryHeader entryHeader, ZString certName)
	{
		try
		{
			var factory = new BusinessObjectFactory();
			MessageRequest.CreateEDIMessageForEffectiveDepCertRequest(factory, entryHeader, certName);
			factory.Save();
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
	}

	protected virtual UpdateCSVClearanceForm GetUpdateCSVClearanceForm(CsvCodeInfo csvCodeInfo) => new UpdateCSVClearanceForm(csvCodeInfo);

	void SetUpEntryLineGridColumns()
	{
		EntryLineGrid.ColumnStyles.AddRange(new []
		{
			new ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("0DBF41C6-B2E6-40C7-87EA-8FEFAA219B42", "Gross Weight"),
				ColumnName = CusEntryLine.Schema.TotalGrossWeightInKG,
				Decimals = 5,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			},
			new ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("AA2154B1-A1AA-4555-8611-9E07DD1D216C", "Packages"),
				ColumnName = CusEntryLine.Schema.VehiclesOrPackagesQty,
				Decimals = 0,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70)
			},
			new ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("FB19A568-5444-4A56-BC13-4F149B33162B", "CPC"),
				ColumnName = CusEntryLine.Schema.ProcedureCodeWithoutConcession,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70)
			},
			new ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("540F71FE-D581-438A-8FF8-4BFC85E0EE88", "Origin"),
				ColumnName = CusEntryLine.Schema.CountryOfOriginCode,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70)
			},
			new ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("D47B7DC1-D175-4D93-95F0-C5AB6E9D9E3B", "Preference"),
				ColumnName = CusEntryLine.Schema.PreferenceCode,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70)
			}
		});
	}

	protected void EntriesBoundGrid_AfterBind(object sender, EventArgs e)
	{
		EntriesBoundGrid.ListManager.PositionChanged += EntriesBoundGridListManager_PositionChanged;
		EntriesBoundGridListManager_PositionChanged(null, null);

		var header = CurrentEntryHeader;
		if (header != null)
		{
			header.ZG_RequestDispatchInfo.ValueChanged += ZG_RequestDispatchInfo_ValueChanged;
			ZG_RequestDispatchInfo_ValueChanged(null, null);
		}
	}

	protected void EntriesBoundGridListManager_PositionChanged(object sender, EventArgs e)
	{
		ControlTabVisibleAndReadOnly();
		SetCreateSupplementaryFromSimplifiedMenuItemVisibility();
	}

	void ControlTabVisibleAndReadOnly()
	{
		AnnexTabPage.TabVisible = RequiresTab;
		if (CurrentEntryHeader?.ShouldAnnexesBeReadOnly ?? false)
		{
			AnnexTabPage.Enabled = false;
			AnnexesTabUserControl.AnnexGrid.ReadOnly = true;
		}
		else
		{
			AnnexTabPage.Enabled = true;
			AnnexesTabUserControl.AnnexGrid.ReadOnly = false;
		}
	}

	CusEntryHeader CurrentEntryHeader => (CusEntryHeader)(EntriesBoundGrid.ListManager?.GetCurrent() ?? EntriesBoundGrid.List?.Cast<BusinessObject>().FirstOrDefault());

	ZString SelectRowFirstMessage => ResString.GetMultilingualString("F9B433B9-5581-403D-A2C5-929AE2A01355", "Please select a row first");

	protected override Type GetBaseMessagesTabUserControlType() => typeof(MessagesTabUserControl);

	public void RefreshTabVisibilty()
	{
		ControlTabVisibleAndReadOnly();
	}

	void ZG_RequestDispatchInfo_ValueChanged(object sender, EventArgs e)
	{
		ControlTabVisibleAndReadOnly();
	}

	void SetCreateSupplementaryFromSimplifiedMenuItemVisibility()
	{
		CreateSupplementaryFromSimplifiedMenuItem.Visible = CurrentEntryHeader != null && CurrentEntryHeader.IsImport && CurrentEntryHeader.ZG_UCC6Version >= UCC6VersionCodes.UCC6 && !CurrentEntryHeader.IsH2Style && CurrentEntryHeader.EntryInstruction.IsSubStyleAOrBOrCOrXOrYOrZ;
	}

	ZBool RequiresTab => (CurrentEntryHeader != null && (CurrentEntryHeader.RequiresT2LAnnexes() || CurrentEntryHeader.RequiresAESAnnexes || CurrentEntryHeader.RequiresT2LPOUSAnnexes || CurrentEntryHeader.RequiresH1Annexes));

	const string LineBreakForMessage = "\n\n";
}
