using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public class EDIMenu : Customs.GUI.EDIMenu
	{
		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
			set { base.Declaration = value; }
		}

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();

			generateAsycudaXMLMenuItem = new ZMenuItem(ResString.GetMultilingualString("f0d10e0a-71e1-4d5e-b27b-0e2697934f6b", "Generate Asycuda XML"));
			generateAsycudaXMLMenuItem.Click += GenerateAsycudaXMLMenuItem_Click;
			MenuItems.Add(generateAsycudaXMLMenuItem);
			generateAsycudaXMLMenuItem.Visible = ShowGenerateAsycudaXMLMenuItem;

			allocateBGMReferenceMenuItem = new ZMenuItem(ResString.GetMultilingualString("66E53FA5-2FAE-441E-8ABE-14B0A2A48D90", "Allocate Entry Reference Number"));
			allocateBGMReferenceMenuItem.Click += AllocateBGMReferenceMenuItem_Click;
			MenuItems.Add(allocateBGMReferenceMenuItem);
			allocateBGMReferenceMenuItem.Visible = ShowAllocateBGMReferenceMenuItem;

			setEntryStatusMenuItem = new ZMenuItem(ResString.GetMultilingualString("7D254875-EFD2-4F10-ADE1-DC68B97BBB8A", "Set Entry Status"));
			setEntryStatusMenuItem.Click += SetEntryStatusMenuItem_Click;
			MenuItems.Add(setEntryStatusMenuItem);
		}

		void SetEntryStatusMenuItem_Click(object sender, EventArgs e)
		{
			SetEntryStatusDetail setEntryStatusHelper = new SetEntryStatusDetail(Declaration);
			using (var form = new SetEntryStatusForm(setEntryStatusHelper))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		protected void GenerateAsycudaXMLMenuItem_Click(object sender, EventArgs e)
		{
			var needMerge = !Declaration.IsMergeDone || Declaration.MergeManager.RequiresMerge;
			if ((!needMerge || PerformMerge()) && RunPreSaveValidationIfHasNoChanges(Declaration) && PreSaveDeclaration(Declaration))
			{
				var formWrapper = new JobDeclarationMessageSendingObjectParent<JobDeclarationMessageSendingObject>(Declaration);
				var messagingHelper = GetJobDeclarationUniversalMessagingHelper(formWrapper);
				var validateResult = messagingHelper.ValidateCanSubmit();

				if (validateResult.IsEmpty)
				{
					using (var form = new GenerateAsycudaXMLForm(formWrapper))
					{
						if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
						{
							var messagesCount = ((AsycudaJobDeclarationUniversalMessagingHelper)messagingHelper).SendAsycudaDeclarationUniversalMessage();
							if (messagesCount > 0)
							{
								Declaration.Messages.Load();
								foreach (Business.CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
								{
									entryHeader.Messages.Load();
								}

								Globals.Message.ShowInformation(Res.GetString("44423ed3-0040-4cc8-8451-f637292d478b", "{0} Asycuda Declaration message(s) queued for sending.", messagesCount));
							}
						}
					}
				}
				else
				{
					Globals.Message.ShowError(validateResult);
				}
			}
		}

		protected void AllocateBGMReferenceMenuItem_Click(object sender, EventArgs e)
		{
			if (RunPreSaveValidationIfHasNoChanges(Declaration) && PreSaveDeclaration(Declaration))
			{
				Declaration.AllocateAllBGMReferences();

				if (Declaration.HasChanges)
				{
					try
					{
						Declaration.Factory.Save();
					}
					catch (ZSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
			}
		}

		protected override JobDeclarationUniversalMessagingHelper GetJobDeclarationUniversalMessagingHelper(IJobDeclarationMessageSendingObjectParent wrapper)
			=> new AsycudaJobDeclarationUniversalMessagingHelper(wrapper);

		protected override bool DisplayGenerateEntriesMenuOption => true;

		ZMenuItem generateAsycudaXMLMenuItem;
		ZMenuItem allocateBGMReferenceMenuItem;
		ZMenuItem setEntryStatusMenuItem;

		public override void RefreshMenu()
		{
			base.RefreshMenu();
			allocateBGMReferenceMenuItem.Visible = ShowAllocateBGMReferenceMenuItem;
			generateAsycudaXMLMenuItem.Visible = ShowGenerateAsycudaXMLMenuItem;
		}

		protected override bool SupportShortCutBondedWarehouseMenus => true;

		bool ShowAllocateBGMReferenceMenuItem => Declaration?.ActiveEntryHeaders?.Cast<Business.CusEntryHeader>().Any(x => x.CH_BGMReference.IsEmpty) ?? false;

		bool ShowGenerateAsycudaXMLMenuItem
		{
			get
			{
				var result = false;
				if (Declaration != null)
				{
					if (asycudaCustomsCountryProvider == null)
					{
						asycudaCustomsCountryProvider = new AsycudaCustomsCountryProvider();
					}

					result = asycudaCustomsCountryProvider.IsAsycudaXMLCountry(Declaration.CountryCode);
				}

				return result;
			}
		}
		AsycudaCustomsCountryProvider asycudaCustomsCountryProvider;
	}
}
