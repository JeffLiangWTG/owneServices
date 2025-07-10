using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public class QuarantineControlsManager
	{
		public void Initialize(ZTabControl tabControl, KBindingSource bindingSource, JobComInvoiceHeader invoice, CurrencyManager currencyManager = null)
		{
			var header = invoice?.QuarantineExDocHeader;
			var addPages = new List<QuarantineTabPage>();

			foreach (var tabPage in GetQuarantineTabPages(header, bindingSource, currencyManager).ToArray())
			{
				tabControl.TabPages.Add(tabPage);
				addPages.Add(tabPage);
			}

			var isNEXDOCSActive = header?.IsNEXDOCSActive ?? false;

			foreach (var tabPage in addPages)
			{
				tabPage.Text = tabPage.GetCaptionFunc(isNEXDOCSActive);
				tabPage.TabVisible = tabPage.IsVisibleFunc(isNEXDOCSActive, header);
			}
		}

		IEnumerable<QuarantineTabPage> GetQuarantineTabPages(QuarantineExDocHeader exDocHeader, KBindingSource bindingSource, CurrencyManager currencyManager)
		{
			yield return new QuarantineTabPage<RFPDetailsUserControl>(exDocHeader, bindingSource, currencyManager)
			{
				IsVisibleFunc = (isActive, _) => true,
				GetCaptionFunc = (isActive) => isActive ? Res.GetString("66733C23-2E64-448E-9499-D463EE7CF7F0", "REX Details") : Res.GetString("21620146-5928-4F37-8EE3-8FE6761074E0", "RFP Details")
			};

			yield return new QuarantineTabPage<REXDeclarationsUserControl>(exDocHeader, bindingSource, currencyManager)
			{
				IsVisibleFunc = (isActive, _) => isActive,
				GetCaptionFunc = (isActive) => Res.GetString("9F4A38E4-FE72-4FFE-AC89-04AA26E11CBE", "REX Declarations")
			};

			yield return new QuarantineTabPage<RFPIndicatorDeclarationsUserControl>(exDocHeader, bindingSource, currencyManager)
			{
				IsVisibleFunc = (isActive, _) => !isActive,
				GetCaptionFunc = (isActive) => Res.GetString("A462F55C-4E46-4BD9-A318-5B1BC5AA9414", "RFP Indicator Declarations")
			};

			yield return new QuarantineTabPageWithSubTabs(exDocHeader, bindingSource, currencyManager)
			{
				IsVisibleFunc = (isActive, _) => true,
				GetCaptionFunc = (isActive) => isActive ? Res.GetString("75B52A46-C8F4-4C4D-8D71-D119042F3F3A", "REX Inspection Details") : Res.GetString("4C4AC957-E47C-4E8E-8EF1-3F47EEBEDEBD", "RFP Inspection Details"),
				TabsPages = (exDocHeader, bindingSource, currencyManager) => new QuarantineTabPage[] {
					new QuarantineTabPage<RFPInspectionDetailsUserControl>(exDocHeader, bindingSource, currencyManager)
					{
						IsVisibleFunc = (isActive, _) => true,
						GetCaptionFunc = (isActive) => Res.GetString("Enterprise.Customs.AU.Declaration.GUI.QuarantineControls|Details", "Details")
					},
					new QuarantineTabPage<REXSkinsAndHidesUserControl>(exDocHeader, bindingSource, currencyManager)
					{
						IsVisibleFunc = (bool isActive, QuarantineExDocHeader header) => isActive && header.QH_ProduceType == EXDOCCommodityCodes.Codes.SkinsAndHides,
						GetCaptionFunc = (isActive) => Res.GetString("Enterprise.Customs.AU.Declaration.GUI.QuarantineControls|SkinsAndHides", "Skins && Hides")
					}
				}
			};

			yield return new QuarantineTabPage<RFPForwardTransferUserControl>(exDocHeader, bindingSource, currencyManager)
			{
				IsVisibleFunc = (isActive, _) => true,
				GetCaptionFunc = (isActive) => isActive ? Res.GetString("DB9EBA81-896D-49DD-A1FC-697C0AE2503E", "REX Forward/Transfer") : Res.GetString("B5089718-B6F6-4E0E-BBCA-8B5B70EA1B73", "RFP Forward/Transfer")
			};

			yield return new QuarantineTabPage<RFPShipsCompartmentsUserControl>(exDocHeader, bindingSource, currencyManager)
			{
				IsVisibleFunc = (isActive, _) => true,
				GetCaptionFunc = (isActive) => isActive ? Res.GetString("3E9EB218-B617-4B74-B0F2-A02FBDFA27A7", "REX Ships Compartments") : Res.GetString("44A2F605-9A04-4D50-9EB9-93795AFADEC5", "RFP Ships Compartments")
			};

			yield return new QuarantineTabPage<RFPEUTransitUserControl>(exDocHeader, bindingSource, currencyManager)
			{
				IsVisibleFunc = (isActive, _) => true,
				GetCaptionFunc = (isActive) => isActive ? Res.GetString("36EE2C57-D21F-41E7-8D33-982D4CCEAF47", "REX EU Details") : Res.GetString("424A5F5D-5C84-461F-B19F-5BE3E604D309", "RFP EU Details")
			};

			yield return new QuarantineTabPage<REXInvoiceProductAttachmentsUserControl>(exDocHeader, bindingSource, currencyManager)
			{
				IsVisibleFunc = (isActive, _) => isActive,
				GetCaptionFunc = (isActive) => Res.GetString("ED5633B0-6553-4F64-87B2-0404ED4676A6", "REX Invoice Attachments")
			};

			yield return new QuarantineTabPage<REXAcknowledgementUserControl>(exDocHeader, bindingSource, currencyManager)
			{
				IsVisibleFunc = (isActive, _) => isActive,
				GetCaptionFunc = (isActive) => Res.GetString("38695116-CF48-4C76-BD77-67C04E43A881", "REX Acknowledgement")
			};

			yield return new QuarantineTabPage<RFPMessagingUserControl>(exDocHeader, bindingSource, currencyManager)
			{
				IsVisibleFunc = (isActive, _) => true,
				GetCaptionFunc = (isActive) => isActive ? Res.GetString("C9A5C029-D123-4AB6-8D40-524DF652270F", "REX Messaging") : Res.GetString("9CAFBB72-E776-4ff8-A5F8-FFAD3B7F547D", "RFP Messaging")
			};
		}
	}
}
