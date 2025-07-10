using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.EU.NCTS.DataTransfer;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public abstract class NctsMessagingMenuProvider
	{
		protected NctsMessagingMenuProvider(NctsHeader header) : this(header, new NctsHeaderUniversalMessagingHelper())
		{
			Header = Argument.NotNull(header, nameof(header));
		}

		protected NctsMessagingMenuProvider(NctsHeader header, NctsHeaderUniversalMessagingHelper ntcsHeaderUniversalMessagingHelper)
		{
			Header = Argument.NotNull(header, nameof(header));
			HeaderUniversalMessagingHelper = Argument.NotNull(ntcsHeaderUniversalMessagingHelper, nameof(ntcsHeaderUniversalMessagingHelper));
		}

		protected virtual NctsHeader Header { get; set; }
		protected NctsHeaderUniversalMessagingHelper HeaderUniversalMessagingHelper { get; }

		public static NctsMessagingMenuProvider New(NctsHeader header, NctsHeaderUniversalMessagingHelper ntcsHeaderUniversalMessagingHelper, ZForm parentForm) => Loader.New(header, ntcsHeaderUniversalMessagingHelper, parentForm);

		public abstract IEnumerable<ZMenuItem> CreateMenuItems();

		public virtual void RefreshMenu() { }

		#region Implementation

		protected void SendMessageToNcts(NctsMessageFunctionSet messageFunction)
		{
			var mandatoryProperties = GetMandatoryPropertyInfosToValidate(messageFunction);

			if (PreSaveDeclaration() && ValidateAndDisplayMandatoryErrors(messageFunction, mandatoryProperties))
			{
				SendMessageToNctsCore(messageFunction);
			}
		}

		protected virtual void SendMessageToNctsCore(NctsMessageFunctionSet messageFunction)
		{
			var generatorFactoryChooser = new NctsMessageGeneratorFactoryChooser(Header, messageFunction);
			var generator = generatorFactoryChooser.GeneratorFactory?.Get(messageFunction);
			if (generator != null)
			{
				var messageManager = new NctsMessageManager(Header, generator);
				messageManager.SendNctsMessage(new Customs.GUI.SendsMessagesToCustomsGUI());
			}
			else
			{
				if (Header.IsTargetNctsSystemUsingNativeComms)
				{
					var nctsMessageSenderChooser = new NctsMessageSenderChooser(Header, messageFunction);
					if (!nctsMessageSenderChooser.CreateMessage(Header, new Customs.GUI.SendsMessagesToCustomsGUI(), messageFunction))
					{
						Globals.Message.ShowError(ResString.GetMultilingualString("118690F2-3F48-4EF4-BF3A-E809F9A63B91", "NCTS has not been implemented for the country of departure or destination selected.\r\nPlease select a departure or destination office in another country."));
					}
				}
				else // Universal XML to eHub
				{
					if (Header.IsUniversalXmlSupportedByTargetNctsCountry)
					{
						this.Header.MessageFunctionCode = messageFunction.Code;
						Globals.Message.Show(HeaderUniversalMessagingHelper.SendViaEHub(this.Header, messageFunction));
					}
					else
					{
						Globals.Message.ShowError(ResString.GetMultilingualString("B44DDA5E-484D-4DF7-9DCD-A9F99A4A396B", "NCTS is not supported by the target country.\r\nPlease select a departure or destination office that supports the NCTS."));
					}
				}
			}
		}

		protected Dictionary<ZPropertyInfo, Action> GetMandatoryPropertyInfosToValidate(NctsMessageFunctionSet messageFunction)
		{
			var propertiesToValidate = new Dictionary<ZPropertyInfo, Action>();

			if (messageFunction is NctsMessageFunctionSet.DeclarationDataMessage)
			{
				Header.GetMandatoryDeclarationPropertiesToValidate(propertiesToValidate);
			}
			else if (messageFunction is NctsMessageFunctionSet.ArrivalNotificationMessage)
			{
				Header.GetMandatoryArrivalPropertiesToValidate(propertiesToValidate);
			}
			else if (messageFunction is NctsMessageFunctionSet.UnloadingRemarksMessage)
			{
				Header.GetMandatoryUnloadingPropertiesToValidate(propertiesToValidate);
			}
			return propertiesToValidate;
		}

		bool ValidateAndDisplayMandatoryErrors(NctsMessageFunctionSet messageFunction, Dictionary<ZPropertyInfo, Action> mandatoryProperties)
		{
			var canSend = false;
			Header.LoadChildEditableObjects();
			Header.RunPreSaveValidation();

			var mandatoryErrors = Header.GetMandatoryMessageErrors(mandatoryProperties);
			ExtraValidation(messageFunction, mandatoryErrors);

			if (Header.StopFromSendingMessageWithMandatoryError && mandatoryErrors.ContainsError())
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("AE8A0639-42C8-4B08-9C11-E11697E4FC1E", "Please enter the following mandatory data before sending any messages:\r\n\r\n{0}", mandatoryErrors.NotificationsAsString()));
			}
			else if (Header.HasMessageErrors)
			{
				if (Env.Security.EuNctsSendWithMessageErrors.IsAllowed)
				{
					var warningForSuperUser = ResString.GetMultilingualString("7AEA4143-693A-43A7-81CB-485B2BD29FBA", "It is likely that your message(s) will be rejected by NCTS as they have the following message errors:\r\n\r\n{0}\r\nDo you want to send the message(s) despite these errors?",
						Header.GetNonMandatoryMessageErrors());
					canSend = Globals.Message.ShowConfirmation(warningForSuperUser, Res.GetString("9B4FEDFE-93CF-4671-A9AA-D630B541C3C0", "Send NCTS Message"),
						ResString.GetMultilingualString("955C1EFA-1DC3-4BD3-9032-54FDB13E27CC", "yes"), MessageBoxIcon.Question) == DialogResult.OK;
				}
				else
				{
					Globals.Message.ShowError(ResString.GetMultilingualString("D47B4D91-1735-43CC-B759-40767E4B393F", "Please fix the following message errors before sending any messages.\r\n\r\n{0}", Header.GetNonMandatoryMessageErrors()));
				}
			}
			else
			{
				canSend = true;
			}
			return canSend;
		}

		protected virtual void ExtraValidation(NctsMessageFunctionSet messageFunction, MessageSendingNotificationCollection notificationCollection)
		{
		}

		protected void CheckNctsHeaderIsNotNullAndThenDoSending(Action action)
		{
			if (Header == null)
			{
				Globals.Message.Show(ResString.GetMultilingualString("C27F2D47-A1F5-4F3B-8CD8-969A069A1987", "No NCTS record exists.  Please enter the NCTS tab to create one"));
			}
			else
			{
				action();
			}
		}

		protected void SetMenuItemVisibility(ZMenuItem menuItem, Func<bool> isVisible)
		{
			if (menuItem != null)
			{
				menuItem.Visible = isVisible();
			}
		}

		protected bool SaveAndContinue(ZMenuItem menuItem)
		{
			var canContinue = true;

			var form = (ZForm)menuItem?.GetMainMenu()?.GetForm();
			if (form != null && Header.HasChanges)
			{
				var messageBoxResult = Globals.Message.Show(
					Res.GetString("02DCA08D-AACA-4D2B-BE1C-0BC092C793BA", "The Job has not yet been saved, Do you want to save and proceed?"),
					Res.GetString("EB623D76-4B0E-4709-AF7C-5FB6D34657E3", "Save Job"),
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning,
					DialogResult.Yes);
				canContinue = messageBoxResult == DialogResult.Yes && form.FireSaveButton() == ContinueWithSave.Yes;
			}

			return canContinue && !Header.HasChanges;
		}

		protected bool PreSaveDeclaration()
		{
			if (ParentForm == null)
			{
				throw new InvalidOperationException("PreSaveDeclaration cannot be run without the parent NctsMovementForm");
			}

			MarkAsNeedToBeSavedIfNotInDatabase();
			return Customs.GUI.PlugIn.CustomsPlugIn.FormPreSaved(Header, ParentForm);
		}

		void MarkAsNeedToBeSavedIfNotInDatabase()
		{
			if (!Header.IsInDatabase)
			{
				Header.HasChanges = true;
			}
		}

		protected ZForm ParentForm { get; set; }

		#region Loader

		class Loader
		{
			public static NctsMessagingMenuProvider New(NctsHeader header, NctsHeaderUniversalMessagingHelper ntcsHeaderUniversalMessagingHelper, ZForm parentForm)
			{
				NctsMessagingMenuProvider result = null;
				if (header != null && ntcsHeaderUniversalMessagingHelper != null)
				{
					var messagingMenuProviderHandle = GetMessagingMenuProviderHandle(header);

					result = messagingMenuProviderHandle == null
						? NewDefaultNctsMessagingMenuProvider(header, ntcsHeaderUniversalMessagingHelper, result)
						: (NctsMessagingMenuProvider)messagingMenuProviderHandle.GetObject(header, ntcsHeaderUniversalMessagingHelper);

					SetParentForm();
				}

				return result;

				void SetParentForm()
				{
					if (result != null)
					{
						result.ParentForm = parentForm;
					}
				}
			}

			#region Implementation

			static ObjectHandle GetMessagingMenuProviderHandle(NctsHeader header)
			{
				var messagingMenuProviders = ObjectFactory.Get<Hashtable>("NCTSMessagingMenuProviders");
				var messagingMenuProviderHandle = (ObjectHandle)messagingMenuProviders[(header.CountryCode + header.BH_HeaderType)];
				return messagingMenuProviderHandle;
			}

			static NctsMessagingMenuProvider NewDefaultNctsMessagingMenuProvider(NctsHeader header, NctsHeaderUniversalMessagingHelper ntcsHeaderUniversalMessagingHelper, NctsMessagingMenuProvider result)
			{
				{
					if (header.IsDepartureAndArrivalMovement)
					{
						result = new NctsDepartureAndArrivalMovementMessagingMenuProvider(header, ntcsHeaderUniversalMessagingHelper);
					}
					else if (header.IsArrivalMovement)
					{
						result = new NctsArrivalMovementMessagingMenuProvider(header, ntcsHeaderUniversalMessagingHelper);
					}
					else if (header.IsDepartureMovement)
					{
						result = new NctsDepartureMovementMessagingMenuProvider(header, ntcsHeaderUniversalMessagingHelper);
					}
				}

				return result;
			}

			#endregion
		}

		#endregion

		#endregion
	}
}
