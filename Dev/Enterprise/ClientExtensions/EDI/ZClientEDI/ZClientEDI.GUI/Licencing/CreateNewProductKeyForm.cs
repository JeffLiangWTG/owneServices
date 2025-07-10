using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Licencing.GUI
{
	public partial class CreateNewProductKeyForm : ZChildForm
	{
		#region Constants

		protected const string LicenceEnterpriseWTL = "WTL";

		protected const int MaxProductKeysForUserInEnterprise = 10;

		#endregion

		#region Properties

		protected MultilingualString ErrorCreatingNewProductKeyMessage => ResString.GetMultilingualString("29664364-B432-4D0A-AB41-092517010A9E", "Error creating new product key.");

		protected MultilingualString InvalidServerCodeMessage => ResString.GetMultilingualString("CA653924-643D-4E02-AF04-5F634216093F", "Must be 3 characters beginning with a letter and containing only letter and numbers.");

		protected MultilingualString NumberOfProductKeysReachedLimitMessage => ResString.GetMultilingualString("9F5988A5-AFC7-4118-8C6E-DFBD41E4F23F", "Limit of 10 keys has been reached.");

		protected MultilingualString ProductKeySuccessfullyCreatedMessage => ResString.GetMultilingualString("51b303a0-fa4b-4edc-879c-4e55232af636", "Product key has been successfully created.");

		protected MultilingualString ReasonMessage => ResString.GetMultilingualString("3DB56320-EB95-47AF-9099-86B077ECBCF9", "Reason: ");

		protected MultilingualString ServerCodeAlreadyInUseMessage => ResString.GetMultilingualString("2F67C810-CF62-4FE1-9723-925D2B607916", "The Server Code entered is already in use. Please enter a different code.");

		#endregion

		#region Overrides

		protected override void OnLoad(EventArgs e)
		{
			if (NumberOfProductKeysForUserInEnterpriseHasReachedLimit())
			{
				Globals.Message.ShowError(NumberOfProductKeysReachedLimitMessage);
				Close();
				return;
			}

			base.OnLoad(e);

			ActiveControl = PreferredServerCodeTextBox;
		}

		#endregion

		#region Event Handlers

		internal void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		public void OkButton_Click(object sender, EventArgs e)
		{
			if (NumberOfProductKeysForUserInEnterpriseHasReachedLimit())
			{
				Globals.Message.ShowError(NumberOfProductKeysReachedLimitMessage);
				Close();
				return;
			}

			var serverCode = PreferredServerCodeTextBox.Text;

			if (!ServerCodeIsValid(serverCode, out string errorMessage))
			{
				Globals.Message.ShowError(errorMessage);
				PreferredServerCodeTextBox.Focus();
				return;
			}

			try
			{
				LicenceDatabase.CreateNewProductKey(LicenceEnterpriseWTL, serverCode, GlbStaff.CurrentUser.GS_Code);
			}
			catch (ZSaveException ex)
			{
				var dbErrorMatch = new DbErrorMatch(ex.InnerException.InnerException as SqlException);
				var exceptionMessage = dbErrorMatch.ExceptionType == DbErrorType.CannotInsertDuplicateUniqueIndexKey ? ServerCodeAlreadyInUseMessage : $"{ErrorCreatingNewProductKeyMessage}\r\n{ReasonMessage}{ex.FriendlyMessage}";

				Globals.Message.ShowError($"{exceptionMessage}");
				return;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError($"{ErrorCreatingNewProductKeyMessage}\r\n{ReasonMessage}{ex.Message}");
				return;
			}

			ServerCodeRegistered();
		}

		void PreferredServerCodeTextBox_EnterPressedLeavingControl(object sender, EventArgs e)
		{
			OkButton_Click(sender, e);
		}

		#endregion

		#region Methods

		internal int NumberOfProductKeysForUserInEnterprise(string staffCode)
		{
			return LicenceDatabase.GetAllProductKeysForUser(staffCode).Count(x => x.LicEnterprise.LE_EnterpriseCode == LicenceEnterpriseWTL);
		}

		[SuppressMessage("CargoWiseOne", "CW1040", Justification = "Non-UI related value. ZArchitecture DPI-awareness checks not done.")]
		internal bool NumberOfProductKeysForUserInEnterpriseHasReachedLimit()
		{
			return NumberOfProductKeysForUserInEnterprise(GlbStaff.CurrentUser.GS_Code) >= MaxProductKeysForUserInEnterprise;
		}

		internal static bool ServerCodeContainsValidCharacters(string serverCode)
		{
			return new Regex("^[A-Z]{1}[0-9A-Z]{2}$").Matches(serverCode).Count == 1;
		}

		internal bool ServerCodeIsValid(string serverCode, out string errorMessage)
		{
			errorMessage = string.Empty;

			if (!ServerCodeContainsValidCharacters(serverCode))
			{
				errorMessage = InvalidServerCodeMessage;
			}

			return string.IsNullOrEmpty(errorMessage);
		}

		internal void ServerCodeRegistered()
		{
			PreferredServerCodeTextBox.Text = string.Empty;
			Globals.Message.ShowInformation(ProductKeySuccessfullyCreatedMessage);
			Close();
		}

		#endregion
	}
}


