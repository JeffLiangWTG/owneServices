using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ExtendedHoursRequestNewForm : ZChildForm
	{
		public ExtendedHoursRequestNewForm(ExtendedHoursRequestHeader header)
			: base(header)
		{
			this.header = header;
			InitializeComponent();
			SetExtendedHoursRequestLineGrid();
			SetButtonCaption();
		}
		readonly ExtendedHoursRequestHeader header;

		public override string FormCaption
		{
			get
			{
				string caption = ZString.Empty;
				switch (header.MessageType)
				{
					case ElectronicDocumentTypeList.Codes._5AC:
						caption = Res.GetString("91F06618-2B70-485C-9D17-082A81063CE0", "(Exp) Application for Extended Office Hours");
						break;
					case ElectronicDocumentTypeList.Codes._5GW:
						caption = Res.GetString("4C5552AB-B650-4EBB-97D1-2D6C34704DB1", "(Imp) Application for Extended Office Hours");
						break;
				}
				return caption;
			}
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			try
			{
				header.Validation.ValidateAll();

				if (header.HasErrors)
				{
					using (ZMessageBox msgBox = new ZErrorMessageBox(BusinessEntityForValidation, false))
					{
						msgBox.Text = Res.GetString("5E33F22A-18CD-46FA-A943-1DA949998B86", "Unable to send...");
						msgBox.Message = Res.GetString("0C44C108-A88C-4212-AFB0-B531A87FE52C", "There are errors on the form. Please fix them first.");
						ZFormModaliser.ShowDialogWithoutDispose(msgBox);
					}
					return;
				}
				else
				{
					if (header.HasMessageErrors)
					{
						if (header.SendWithMessageErrorsIsAllowed)
						{
							var errorMessage = Res.GetString("7AFF1104-FE3F-4DED-B4A1-70FA38CCEB5D", "There are message errors on the form. Are you sure you want to send a message regardless of those message errors?");
							var caption = Res.GetString("AA217823-DEFA-43DB-93C0-204A57C83E4E", "Warning");
							using (var notification = new ZMessageBox(errorMessage, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2))
							{
								var result = ZFormModaliser.ShowDialogWithoutDispose(notification);
								if (result != DialogResult.Yes)
								{
									return;
								}
							}
						}
						else
						{
							Globals.Message.ShowInformation(Res.GetString("A8B0ABC5-428B-42B5-A98F-9439727490F4", "There are message errors on the form. Unless you fix them, you will not be able to send a message as you don't have the security right to send with message errors."));
							return;
						}
					}
				}

				var cusMiscRequestHeaderCreator = CreateCreator();
				var cusMiscRequestHeader = cusMiscRequestHeaderCreator.Create(header);
				var message = cusMiscRequestHeaderCreator.CreateEdiMessage(cusMiscRequestHeader, header);

				header.Factory.Save();

				DialogResult = System.Windows.Forms.DialogResult.OK;
				if (message != null)
				{
					Globals.Message.ShowInformation(Res.GetString("B1AB5801-7701-415A-9A30-6C05AC3FE9F9", "1 Message of # {0} is generated.", message.EM_MessageNum));
				}
				Close();
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		protected virtual CusMiscRequestHeaderCreator CreateCreator()
		{
			return new CusMiscRequestHeaderCreator();
		}

		void SetButtonCaption()
		{
			switch (header.MessageType)
			{
				case ElectronicDocumentTypeList.Codes._5AC:
					SearchButton.CaptionResourceString = Res.GetData("5520BAAA-4CB3-40C3-8AEF-CFFF8485989F", "Export Declaration Search");
					break;
				case ElectronicDocumentTypeList.Codes._5GW:
					SearchButton.CaptionResourceString = Res.GetData("BF1F8112-E1CF-43C2-A2AE-234408FF5F8B", "Import Declaration Search");
					break;
			}
		}

		void SearchButton_Click(object sender, EventArgs e)
		{
			var moduleID = header.MessageType == ElectronicDocumentTypeList.Codes._5AC ? ModuleIDs.Customs.KR.ExportEntryDetails : ModuleIDs.Customs.KR.ImportEntryDetails;
			var module = (ZFilterModule)ZModuleFactory.Instance.Create(moduleID);
			var popup = new EmbeddedModulePopup(module);
			var strategy = new KREntryDetailsPopupOKButtonStrategy(popup, header.ExtendedHoursRequestLines);
			popup.EmbeddedModulePopupOKButtonStrategy = strategy;
			module.OverrideModuleDecisionProvider(strategy);

			ZFormModaliser.Show(popup, this);
		}

		void SetExtendedHoursRequestLineGrid()
		{
			var lineGrid = extendedHoursRequestNewUserControl.ExtenedHoursRequestLinesBoundGrid;

			if (header.MessageType == ElectronicDocumentTypeList.Codes._5AC)
			{
				lineGrid.ColumnStyles.Add(
					new ZTextBoxColumnStyleInfo
					{
						CharacterCasing = CharacterCasing.Upper,
						ColumnName = nameof(ExtendedHoursRequestLine.SupplierName),
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300)
					}
				);
				lineGrid.ReOrderColumns(lineGridTypeIs5AC);
			}
			else if (header.MessageType == ElectronicDocumentTypeList.Codes._5GW)
			{
				lineGrid.ColumnStyles.AddRange(new ZTextBoxColumnStyleInfo[]
				{
					new ZTextBoxColumnStyleInfo
					{
						CharacterCasing = CharacterCasing.Upper,
						ColumnName = ExtendedHoursRequestLine.Schema.HSDescription,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300),
						IsMandatory = true
					},
					new ZCodeFindBoxColumnStyleInfo
					{
						CharacterCasing = CharacterCasing.Upper,
						ColumnName = ExtendedHoursRequestLine.Schema.BondedAreaCode,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115),
						IsMandatory = true
					},
					new ZTextBoxColumnStyleInfo
					{
						CharacterCasing = CharacterCasing.Upper,
						ColumnName = ExtendedHoursRequestLine.Schema.PayerCompanyName,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300),
						IsMandatory = true
					},
					new ZTextBoxColumnStyleInfo
					{
						CharacterCasing = CharacterCasing.Upper,
						ColumnName = nameof(ExtendedHoursRequestLine.CustomsEntryType),
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65),
						IsVisible = false
					}
				});
				lineGrid.ReOrderColumns(lineGridTypeIs5GW);
			}
		}

		readonly string[] lineGridTypeIs5AC =
		{
			nameof(ExtendedHoursRequestLine.FormattedReferenceNumber),
			ExtendedHoursRequestLine.Schema.CustomsValue,
			ExtendedHoursRequestLine.Schema.PackageCount,
			ExtendedHoursRequestLine.Schema.TotalWeight,
			ExtendedHoursRequestLine.Schema.SupplierName
		};

		readonly string[] lineGridTypeIs5GW =
		{
			nameof(ExtendedHoursRequestLine.FormattedReferenceNumber),
			ExtendedHoursRequestLine.Schema.HSDescription,
			ExtendedHoursRequestLine.Schema.CustomsValue,
			ExtendedHoursRequestLine.Schema.PackageCount,
			ExtendedHoursRequestLine.Schema.TotalWeight,
			ExtendedHoursRequestLine.Schema.BondedAreaCode,
			ExtendedHoursRequestLine.Schema.PayerCompanyName,
			nameof(ExtendedHoursRequestLine.CustomsEntryType)
		};
	}
}
