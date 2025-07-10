using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class CusExitControlHeader : EU.Business.CusExitControlHeader, Integration.Customs.ES.ICusExitControlHeader, ICustomsFileParent
	{
		public CusExitControlHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Schema

		public new partial class Schema : EU.Business.AutoCusExitControlHeader.Schema
		{
			public const string DeclEmailAddr = "DeclEmailAddr";
		}

		#endregion

		#region Implementation

		ForwardingShipment Shipment => (CEH_ParentTableCode == "JS") ? (CEH_Parent as ForwardingShipment) : null;

		#region ICustomsFileParent

		ZString ICustomsFileParent.DeclarationType => Declaration?.JE_MessageType ?? (Shipment != null && Shipment.IsExport() ? EUJobMessageTypeList.Codes.Export : string.Empty) ?? ZString.Empty;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names",
			Justification = "The param of GetZPropertyInfo will be 'DeclarationType' if use 'nameof(Enterprise.Customs.Business.ICustomsFileParent.DeclarationType)'  to replace it.")]
		ZPropertyInfo ICustomsFileParent.DeclarationTypeInfo => Declaration?.JE_MessageTypeInfo ?? GetZPropertyInfo("Enterprise.Customs.Business.ICustomsFileParent.DeclarationType");

		ZGuid ICustomsFileParent.BranchPk => Declaration?.JE_GB ?? GlbBranch.CurrentBranch.PK;

		ZBool ICustomsFileParent.IsLocked => Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit) != null;

		void ICustomsFileParent.LockFile(ZString reference) => this.AddLockEvent(reference);

		void ICustomsFileParent.UnlockFile(ZString reference) => this.AddUnlockEvent(reference);

		#endregion

		bool ExitHeaderLocked => CusExitDetails.Any(x => x.ReadOnly);

		public ZString LockExitHeader(ZString reference)
		{
			var fileParent = ((ICustomsFileParent)this);
			if (ExitHeaderLocked)
			{
				return Res.GetString("3CBBE0EF-7501-4CBB-9775-94405E625580", "The Exit Declaration is already locked.");
			}
			var config = CustomsDataRegistry.Instance.DeclarationLockForEdit.Value.Find(fileParent.DeclarationType);
			if (config != null)
			{
				if (!fileParent.IsLocked)
				{
					fileParent.LockFile(reference);

					foreach (CusExitDetail exitDetail in CusExitDetails)
					{
						if (CodesForUnlocking.Contains(exitDetail.CED_Status))
						{
							exitDetail.SetReadOnlyIncludingChildren(true);
						}
					}

					return Res.GetString("C9FDC639-8ED9-4330-9A5B-404787EE3861",
						"This tab page has been locked for edit\r\nExit Control\r\n\r\nYou can click the Exit Summary - Unlock Declaration to unlock it. \r\nYou can change the lock config in the System Registry under {0}.",
						CustomsDataRegistry.Instance.DeclarationLockForEdit.HumanReadableRegistryPath());
				}
			}
			return ZString.Empty;
		}

		public ZString UnlockExitHeader(ZString reference)
		{
			var fileParent = ((ICustomsFileParent)this);
			if (!ExitHeaderLocked)
			{
				return Res.GetString("6ECCA6D3-5E4B-479D-85FD-5FF4166C88E8", "The Exit Declaration is already unlocked.");
			}
			if (fileParent.IsLocked)
			{
				fileParent.UnlockFile(reference);

				foreach (CusExitDetail exitDetail in CusExitDetails)
				{
					if (CodesForUnlocking.Contains(exitDetail.CED_Status))
					{
						exitDetail.SetReadOnlyIncludingChildren(false);
					}
				}
			}
			return Res.GetString("872B6094-A6A1-43C1-A31D-FBC596B0E9CD", "The Exit Declaration is unlocked.");
		}

		public ImmutableList<ZString> CodesForUnlocking
			=> codesForUnlocking ??
				(codesForUnlocking = new List<ZString>() { MessageStatusList.Codes.AwaitingResponse, MessageProcessorConstants.EntryStatusCodes.Cleared, MessageProcessorConstants.EntryStatusCodes.CustomsDeclarationAccepted }.ToImmutableList());
		ImmutableList<ZString> codesForUnlocking;

		public new CusExitControlHeaderValidation Validation => (CusExitControlHeaderValidation)base.Validation;

		public new CusExitControlHeaderLookups Lookups => (CusExitControlHeaderLookups)base.Lookups;

		public new CusExitDetailCollection CusExitDetails => (CusExitDetailCollection)base.CusExitDetails;

		protected override EU.Business.CusExitControlHeaderValidation GetNewValidation() => new CusExitControlHeaderValidation(this);

		protected override EU.Business.CusExitControlHeaderLookups GetNewLookups() => new CusExitControlHeaderLookups(this);

		protected override EU.Business.CusExitDetailCollection GetNewCusExitDetailsCollectionCore() => new CusExitDetailCollection(this);

		#endregion

		#region CEH_CustomsProfile

		[List(nameof(Lookups) + "." + nameof(CusExitControlHeaderLookups.CertificateNames))]
		[ResourceStringData("8AAE589F-0567-48F4-9E8A-54218F4BA53F", Caption = "Certificate", MediumCaption = "Certif.", ShortCaption = "Cert.", FullDescription = "The certificate selected will be used to sign and communicate with Customs to declare all entries in this Job")]
		public override ZString CEH_CustomsProfile { get => base.CEH_CustomsProfile; set => base.CEH_CustomsProfile = value; }

		#endregion

		[ResourceStringData("EF7E851B-D3FD-4382-87C8-B16FA2ED58FB", Caption = "Arrival Date")]
		public override ZDateTime CEH_ArrivalNotificationDate { get => base.CEH_ArrivalNotificationDate; set => base.CEH_ArrivalNotificationDate = value; }

		[ResourceStringData("0E3E1E51-1958-4C05-93ED-32936689A5B4", Caption = "Arrival Place")]
		public override ZString CEH_ArrivalNotificationPlace { get => base.CEH_ArrivalNotificationPlace; set => base.CEH_ArrivalNotificationPlace = value; }

		[ResourceStringData("3101396C-6DA1-4259-A985-E46DE2006016", Caption = "Broker", MediumCaption = "Broker", ShortCaption = "Broker", FullDescription = "The broker selected will be the responsible of declarations to Customs in this Job")]
		public override ZString CEH_GS_NKCustomsAgent
		{
			get => base.CEH_GS_NKCustomsAgent;
			set
			{
				var oldValue = base.CEH_GS_NKCustomsAgent;
				base.CEH_GS_NKCustomsAgent = value;

				if (!IsCopying && oldValue != value)
				{
					SetDefaultCEH_CustomsProfile();
				}
			}
		}

		void SetDefaultCEH_CustomsProfile()
		{
			var certificateNamesList = Lookups.CertificateNames;
			var customsProfile = CEH_CustomsProfile;
			if (customsProfile.IsEmpty || !certificateNamesList.GetAllCodesZString().Contains(customsProfile))
			{
				if (certificateNamesList.Count == 1)
				{
					CEH_CustomsProfile = certificateNamesList[0].Code;
				}
				else
				{
					CEH_CustomsProfile = ZString.Empty;
				}
			}
		}

		#region DeclEmailAddr

		public ZString DeclEmailAddr
		{
			get
			{
				if (!declEmailAddr.HasValue)
				{
					declEmailAddr = EmailHelper.GetDeclEmailAddrFromRegistry();
				}
				return declEmailAddr.Value;
			}
		}
		ZString? declEmailAddr;

		public ZPropertyInfo DeclEmailAddrInfo => GetZPropertyInfo(Schema.DeclEmailAddr);

		#endregion
	}
}
