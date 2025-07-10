using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class NctsPlugin : Customs.GUI.CusInBondPlugIn<NctsHeader, CommonNctsMessagingMenuItem>
	{
		public NctsPlugin(ICusInBondParent host)
			: base(host)
		{
			WireVisibilityHandlers(host);
			Enabled = CalculateEnabled(host);
		}

		protected override Control GetNewUserControl()
		{
			return IsPhase5 ? new Phase5NctsUserControlForPlugin(InternalInBond) : new NctsUserControlForPlugin(InternalInBond);
		}

		protected override MenuItem GetNewTopLevelMenu() => SecurityCheckpointForMessagingMenu.IsAllowed ? base.GetNewTopLevelMenu() : InBondMenuStubWhenSecurityRightsDenied;

		protected override CommonNctsMessagingMenuItem CreateNewInBondMenu()
		{
			var applicationCode = string.IsNullOrEmpty(InBond?.BH_ApplicationCode) ? ApplicationCode : InBond.BH_ApplicationCode.ToString();
			fInBondMenu = applicationCode == CusInBondApplicationCodeList.Codes.NCTS5 ? new Phase5NctsMessagingMenuItem() : new NctsMessagingMenu(Form);
			fInBondMenu.NctsHeader = InBond;
			ChangeTheVisibility();
			return fInBondMenu;
		}

		public override string Name => Res.GetString("A1B03E89-2A38-40C0-80B6-26269855C644", "NCTS");

		protected override Security.SecurityCheckpoint SecurityCheckpointForMessagingMenu
		{
			get { return Env.Security.EuNctsMessaging; }
		}

		ZBool CalculateEnabled(ICusInBondParent host)
		{
			var nctsSettings = ObjectFactory.Get<Integration.Customs.Shared.INctsSettings>();
			var enabled = nctsSettings.IsNctsEnabled;
			if (enabled && host != null)
			{
				if (host is ForwardingShipment shipment)
				{
					enabled = IsInOrThroughRelevantNctsCountryAndCurrentLoginCompany((c) => shipment.ShipmentPassesThroughCountry(c));
				}
				else if (host is ForwardingConsol consol)
				{
					enabled = IsInOrThroughRelevantNctsCountryAndCurrentLoginCompany((c) => consol.ConsolPassesThroughCountry(c));
				}
				// Plug in to declaration not currently supported
			}
			return enabled;
		}

		protected override void SetHeaderOnMessagingMenu(NctsHeader value)
		{
			fInBondMenu.NctsHeader = value;
		}

		protected override NctsHeader GetInbondFromParent()
		{
			return GetInBondHeader(CusInBondApplicationCodeList.Codes.NCTS5) ?? GetInBondHeader(CusInBondApplicationCodeList.Codes.NCTS4);
		}

		protected override void InitiliseNewInbondAfterCreateNew(NctsHeader header)
		{
			base.InitiliseNewInbondAfterCreateNew(header);
			SetNCTSPhaseIfNeeded(header);
			header.SetMovementType(arrivalOrDeparture);
		}

		protected virtual void SetNCTSPhaseIfNeeded(NctsHeader header)
		{
			var nctsSettings = ObjectFactory.Get<Integration.Customs.Shared.INctsSettings>();
			header.BH_ApplicationCode = nctsSettings.IsUsingPhase5(header.DefaultDataGroupingCode) ? CusInBondApplicationCodeList.Codes.NCTS5 : CusInBondApplicationCodeList.Codes.NCTS4;
		}

		protected override string ApplicationCode
		{
			get
			{
				var nctsSettings = ObjectFactory.Get<Integration.Customs.Shared.INctsSettings>();
				return nctsSettings.IsUsingPhase5(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) ? CusInBondApplicationCodeList.Codes.NCTS5 : CusInBondApplicationCodeList.Codes.NCTS4;
			}
		}

		#region NCTS Licence logging

		void RegisterNctsLicenceLogin()
		{
			var checkPoint = Env.Licence.NCTS;
			var args = new LicenceLoginEventArgs(checkPoint);
			args.LoginHasBeenAttempted = true;
			args.LicenceCheckPoint.Login(this);
		}

		#endregion

		protected override bool QueryUser(string question, string caption)
		{
			bool result = true;
			using (var messageBox = new ZMessageBox(Res.GetString("4132C265-F96C-4535-A71A-68478208BD7F", "No NCTS Movement exists. What type of NCTS Movement would you like to create?"),
				caption,
				MessageBoxButtons.YesNoCancel,
				MessageBoxIcon.Question,
				Common.EU.NctsMoveHeaderType.Descriptions.Departure,
				Common.EU.NctsMoveHeaderType.Descriptions.Arrival))
			{
				var answer = ZFormModaliser.ShowMessageBoxWithoutDispose(messageBox);
				if (answer == DialogResult.Yes)
				{
					arrivalOrDeparture = NctsMovementType.Codes.Departure;
					RegisterNctsLicenceLogin();
				}
				else if (answer == DialogResult.No)
				{
					arrivalOrDeparture = NctsMovementType.Codes.Arrival;
					RegisterNctsLicenceLogin();
				}
				else
				{
					arrivalOrDeparture = "";
					result = false;
				}
			}

			return result;
		}

		bool IsInOrThroughRelevantNctsCountryAndCurrentLoginCompany(Func<string, bool> passesThroughCountryDelegate)
		{
			var isCurrentLoginCompanyInNctsContractingCountries = NctsHeaderValidationHelper.IsNctsContractingParty(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			if (isCurrentLoginCompanyInNctsContractingCountries)
			{
				foreach (var country in NctsHeaderValidationHelper.NctsContractingParties)
				{
					if (passesThroughCountryDelegate(country))   // this looks at shipments origin and dest and at any consol's origin, dest and routing too
					{
						return true;
					}
				}
			}
			return false;
		}

		void WireVisibilityHandlers(ICusInBondParent hostBusinessEntity)
		{
			if (hostBusinessEntity is IManifestProvider hbe)
			{
				hbe.CustomsManifestVisibilityChanged += OnChangeTheVisibility;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (HostBusinessEntity is IManifestProvider hbe)
				{
					hbe.CustomsManifestVisibilityChanged -= OnChangeTheVisibility;
				}
			}
			base.Dispose(disposing);
		}

		protected void OnChangeTheVisibility(object sender, EventArgs e)
		{
			ChangeTheVisibility();
		}

		protected override void ChangeTheVisibility()
		{
			Enabled = CalculateEnabled(HostBusinessEntity);
			if (fInBondMenu != null)
			{
				fInBondMenu.Visible = Enabled;
			}
		}

		protected override bool GetIsAllowed() => Env.Security.EuNctsMovementNew.IsAllowed;

		protected override string GetCreateNewMovementText() => Res.GetString("99AA1F26-AB82-40F9-B179-51221852E524", "Create NCTS Movement for {0}", HostBusinessEntity.ParentType);

		protected override MultilingualString GetErrorMessageForNotAllowed() => Env.Security.EuNctsMovementNew.ErrorMessageForNotAllowed;

		protected override string GetMovementAlreadyCreatedText() => Res.GetString("0cb1e7e8-8b31-465e-9e79-148adf5d86b4", "{0} Movement has already been created and linked to this {1}.", new NctsMovementType().GetDescriptionFromCode(InternalInBond.BH_HeaderType), HostBusinessEntity.ParentType);

		ZString arrivalOrDeparture;

		ZBool IsPhase5 => InternalInBond?.IsPhase5 ?? false;
	}
}
