using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.GUI
{
	public class MonthlyClosingMessagingMenu : ZMenuItem
	{
		public MonthlyClosingMessagingMenu(CusReconDeclaration declaration)
		{
			if (messageRoleList == null)
			{
				messageRoleList = new MonthlyClosingMessageRoleList();
			}
			enabledFuncs = new Dictionary<ZMenuItem, Func<bool>>(5);
			this.declaration = declaration;
			this.SetCaptionResourceString();
			AddMenuItems();
			UpdateMenuVisibility();

			Popup += (s, e) => UpdateMenuVisibility();
		}
		readonly CusReconDeclaration declaration;

		void AddMenuItems()
		{
			MenuItems.AddRange(new[]
			{
				CreateMenuItem(MonthlyClosingMessageRoleList.Codes.FinalMessage, () => DeclarationRegistrationNumberIsEmpty && DeclarationStatusIsNotSNT),
				CreateMenuItem(MonthlyClosingMessageRoleList.Codes.FirstPartialMessage, () => DeclarationRegistrationNumberIsEmpty && DeclarationStatusIsNotSNT),
				CreateMenuItem(MonthlyClosingMessageRoleList.Codes.AmendmentMessage, () => !DeclarationRegistrationNumberIsEmpty && DeclarationStatusIsNotSNT && !DeclarationFinalized),
				CreateMenuItem(MonthlyClosingMessageRoleList.Codes.ModificationMessage, () => !DeclarationRegistrationNumberIsEmpty && DeclarationStatusIsNotSNT && (DeclarationCustomsStatusRC2TX4 || DeclarationCustomsStatusTX5ForBondedWarehouse)),
				CreateMenuItem(MonthlyClosingMessageRoleList.Codes.FinalizationMessage, () => !DeclarationRegistrationNumberIsEmpty && DeclarationStatusIsNotSNT && DeclarationCustomsStatusRC2TX4)
			});
		}

		void UpdateMenuVisibility()
		{
			enabledFuncs.ForEach(m => m.Key.Enabled = m.Value());
		}

		bool DeclarationStatusIsNotSNT => declaration.CanSendMessage;

		bool DeclarationFinalized => declaration.IsFinalized;

		bool DeclarationCustomsStatusRC2TX4 => (allowedCustomsStatus ?? (allowedCustomsStatus = new HashSet<string>(new string[] { EntryStatus.RC2, EntryStatus.TX4 }))).Contains(declaration.CRD_CustomsStatus);

		bool DeclarationCustomsStatusTX5ForBondedWarehouse => (declaration.CRD_DeclarationType == MonthlyClosingDeclarationTypeList.Codes.AZL || declaration.CRD_DeclarationType == MonthlyClosingDeclarationTypeList.Codes.VZL)
															  && declaration.CRD_CustomsStatus == EntryStatus.TX5;

		[ThreadStatic]
		static HashSet<string> allowedCustomsStatus;
		readonly Dictionary<ZMenuItem, Func<bool>> enabledFuncs;

		bool DeclarationRegistrationNumberIsEmpty => CachedValueHelper.GetValue(ref declarationRegistrationNumberIsEmpty, () => declaration.RegistrationNumber.IsEmpty);
		CachedValue<bool> declarationRegistrationNumberIsEmpty;

		ZMenuItem CreateMenuItem(string messageRole, Func<bool> enabled)
		{
			var menuItem = new ZMenuItem(GetMenuCaption(messageRole));
			menuItem.Click += (sender, e) => { SendMonthlyClosingDeclaration(sender, e, messageRole); };
			enabledFuncs[menuItem] = enabled;
			return menuItem;
		}

		void SendMonthlyClosingDeclaration(object sender, EventArgs e, string messageRole)
		{
			var declarationSender = GetDeclarationSender(messageRole);
			if (declarationSender != null)
			{
				if (CustomsPlugIn.FormPreSaved(declaration, Form))
				{
					if (declarationSender.MessageHasInformationToSend)
					{
						declarationSender.Send();
						try
						{
							declaration.Factory.Save();
							Globals.Message.Show(Res.GetString("7085D352-7E7B-4F6B-B33B-787CE8DE8B38", "The message has been sent."));
						}
						catch (ZSaveException ex)
						{
							ZExceptionReporting.HandleSaveException(ex);
						}
					}
					else
					{
						Globals.Message.Show(Res.GetString("F49F3774-A3FA-4F9F-A17D-619389D411AB", "No items found that match this message function. No Message was sent."));
					}
				}
			}
		}

		MonthlyClosingDeclarationSender GetDeclarationSender(string messageRole)
		{
			switch (declaration.CRD_DeclarationType)
			{
				case MonthlyClosingDeclarationTypeList.Codes.AZ:
				case MonthlyClosingDeclarationTypeList.Codes.VZA:
					return new MonthlyClosingDeclarationFreeCirculationSender(declaration, messageRole);
				case MonthlyClosingDeclarationTypeList.Codes.AAV:
				case MonthlyClosingDeclarationTypeList.Codes.VAV:
					return new MonthlyClosingDeclarationInwardProcessingSender(declaration, messageRole);
				case MonthlyClosingDeclarationTypeList.Codes.AZL:
				case MonthlyClosingDeclarationTypeList.Codes.VZL:
					return new MonthlyClosingDeclarationBondedWarehouseSender(declaration, messageRole);
				default:
					return null;
			}
		}

		public static ResourceString GetMenuCaption(string code) => ResString.GetMultilingualString("919E8680-82DF-478C-9229-1156C7CF9F22", "{0} ({1})", messageRoleList.GetDescriptionFromCode(code), code);

		[ThreadStatic]
		static MonthlyClosingMessageRoleList messageRoleList;

		ZForm Form => (ZForm)GetMainMenu().GetForm();
	}
}
